using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.ServiceBus.Messaging;
using TwitterClient.Models;
using System.Diagnostics;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Cognitive.LUIS;
using System.Reactive.Concurrency;
using CommanderHelper.Services;
using CommanderHelper.EntityFramework;
using CommanderHelper.EntityFramework.Entities;

namespace TwitterClient.Services {
   

    public class TwitterService {

        private readonly EventHubClient _eventHubClient;
        private IDisposable _streamTweetSubscription { get; set; }

        public TwitterService() {
            EventHubConfig config = GetEventHubConfiguration();
            _eventHubClient = EventHubClient.CreateFromConnectionString(config.ConnectionString, config.EventHubName);
        }

        public void Start() {
            if (_streamTweetSubscription == null) {
                //Tweet.doStreamTweets = true;
                var twitterEventObserver = new TwitterEventObserver();
                var twitterData = Tweet.StreamStatuses().Select(tweet => TwitterPayload.GetPayloadFromTweet(tweet));
                _streamTweetSubscription = twitterData.ToObservable().SubscribeOn(NewThreadScheduler.Default)
                    .Subscribe(twitterEventObserver);
            }
        }

        //public void Stop() {
        //    Tweet.doStreamTweets = false;
        //    _streamTweetSubscription?.Dispose();
        //    _streamTweetSubscription = null;
        //}

        private static TwitterService instance;
        public static TwitterService Instance {
            get {
                if (instance == null)
                    instance = new TwitterService();
                return instance;
            }
        }

        #region Private Helpers
        private TwitterConfig GetTwitterConfiguration() {
            var toReturn = new TwitterConfig {
                OAuthToken = ConfigurationManager.AppSettings["oauth_token"],
                OAuthTokenSecret = ConfigurationManager.AppSettings["oauth_token_secret"],
                OAuthConsumerKey = ConfigurationManager.AppSettings["oauth_consumer_key"],
                OAuthConsumerSecret = ConfigurationManager.AppSettings["oauth_consumer_secret"],
                Keywords = ConfigurationManager.AppSettings["twitter_keywords"]
            };
            return toReturn;
        }

        private EventHubConfig GetEventHubConfiguration() {
            var toReturn = new EventHubConfig {
                ConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"],
                EventHubName = ConfigurationManager.AppSettings["EventHubName"]
            };
            return toReturn;
        }
        #endregion
    }

    public class TwitterEventObserver : IObserver<TwitterPayload> {
        private readonly EventHubClient _eventHubClient;
        private readonly LuisTwitterService _luisTwitterService = new LuisTwitterService();
        private readonly SharePointService _sharePointService = new SharePointService();
        private readonly InvaderContext _context = new InvaderContext();
        public TwitterEventObserver() {
            EventHubConfig config = GetEventHubConfiguration();
            _eventHubClient = EventHubClient.CreateFromConnectionString(config.ConnectionString, config.EventHubName);
        }
       
        public void OnCompleted() {
            Debug.WriteLine(" - OPERATION COMPLETED - ");
        }

        public void OnError(Exception error) {
            Debug.WriteLine(" - OPERATION ERROR - ");
            Debug.WriteLine(error.Message);
        }

        public async void OnNext(TwitterPayload tweet) {
            try {
                InvaderTweet invaderTweet = await _luisTwitterService.ParseTweetText(tweet.Text);
                if (invaderTweet != null) {
                    HandleInvaderTweet(invaderTweet);
                    SendToEventHub(invaderTweet);
                }
                else
                    Debug.WriteLine("Tweet did not match invader intent");
            }
            catch (Exception ex) {
                Debug.WriteLine("Exception occured:/n{0}", ex.Message);
            }
        }

        private void HandleInvaderTweet(InvaderTweet invaderTweet) {
            InvaderDetail invaderDetail = invaderTweet.ToInvaderDetail();
            invaderDetail.Location = _context.Locations.GetClosest(invaderTweet.GeoLocation);
            _context.InvaderDetails.Add(invaderDetail);
            _context.SaveChanges();
            string reportTitle = $"{invaderTweet.InvaderType} - {invaderTweet.InvaderCount}";
            _sharePointService.AddInvaderLocation(invaderDetail);
        }

        private void SendToEventHub(InvaderTweet invaderTweet) {
            var serializedTweet = JsonConvert.SerializeObject(invaderTweet);
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(serializedTweet));
            _eventHubClient.Send(eventData);
        }

        #region Private Helpers

        private EventHubConfig GetEventHubConfiguration() {
            var toReturn = new EventHubConfig {
                ConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"],
                EventHubName = ConfigurationManager.AppSettings["EventHubName"]
            };
            return toReturn;
        }
        #endregion
    }

}
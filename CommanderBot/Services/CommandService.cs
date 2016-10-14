using CommanderHelper.EntityFramework;
using CommanderHelper.EntityFramework.Entities;
using CommanderHelper.Models;
using CommanderHelper.Services;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CommanderBot.Services {
    public class CommandService {

        private readonly SharePointService _sharePointService = new SharePointService();
        private readonly MapService _mapService = new MapService();
        private readonly HeroService _heroService = new HeroService();
        private readonly TweetService _tweetService = new TweetService();
        private readonly ConnectorClient _connector;
        private readonly Activity _activity;

        public CommandService(ConnectorClient connector, Activity activityToReply) {
            _connector = connector;
            _activity = activityToReply;
        }

        public async Task<string> ExecuteDesiredWorkflow(LuisResult luisResult) {

            string response = "Sorry, the Justice League was unable to understand your request";
            JusticeLeagueIntent intent = DeriveIntent(luisResult.TopScoringIntent.Name); // GetIntent(luisResult.TopScoringIntent);
            CommandHeroModel commandModel = null;
            var intendedAction = luisResult.TopScoringIntent?.Actions?.FirstOrDefault();
            if (intendedAction != null && intendedAction.Triggered)
                commandModel = new CommandHeroModel(intendedAction.Parameters);
            if (commandModel != null || intent != JusticeLeagueIntent.None) 
                response = await PerformRequestedAction(commandModel, intent);
            return response;
        }

        public async Task<string> PerformRequestedAction(CommandHeroModel entityModel, JusticeLeagueIntent intent) {
            string response = "I'm sorry, I could not understand your request. Please rephrase that and try again.";
            switch (intent) {
                case JusticeLeagueIntent.CommandHero:
                    response = await ExecuteCommandHero(entityModel.Hero, entityModel.Location);
                    break;
                case JusticeLeagueIntent.CreateSite:
                    response = await ExecuteSiteCreation(entityModel.SiteName);
                    break;
                case JusticeLeagueIntent.GetReport:
                    response = $"Here's a link to the {entityModel.ReportName} report: ";
                    response += GetUrlForPowerBiReport(entityModel.ReportName);
                    break;
                case JusticeLeagueIntent.Greeting:
                    response = "Hello there! I'm able to create sites, command heroes, retrieve tasks, and retrieve reports";
                    break;
                case JusticeLeagueIntent.DeleteTweets:
                    response = "All of your tweets have been deleted successfully";
                    var tweets = await _tweetService.GetAllTweets();
                    foreach (var tweet in tweets) {
                        await _tweetService.DeleteTweet(tweet.id);
                    }
                    break;
                case JusticeLeagueIntent.SendTweets:
                    //response = "The distress sample tweets have been created!";
                    response = await ExecuteTweetCreation();
                    break;
            }
            return response;
        }

        #region Site Commands
        private async Task<string> ExecuteCommandHero(string heroName, string locationName) {
            string toReturn = "Sorry, I ran into an issue issue commanding the hero";

            var reply = _activity.CreateReply("Hang on a moment while I command to your hero...");
            await _connector.Conversations.ReplyToActivityAsync(reply);
            GeoLocation geoLocation = await _mapService.DetermineGeoLocation(locationName);
           
            _sharePointService.AddHeroTask(heroName, geoLocation);
            toReturn = $"{heroName} is defending {geoLocation.address}";
            
            await _heroService.UpdateHero(heroName, geoLocation);
            return toReturn;
        }

        private async Task<string> ExecuteSiteCreation(string siteName) {
            string toReturn = "Sorry, there was an issue creating that site. Please try again.";
            var reply = _activity.CreateReply("Hang on a moment while your site is being created. I'll let you know when it's done...");
            await _connector.Conversations.ReplyToActivityAsync(reply);
            string newSiteUrl = SharePointService.CreateSite(siteName);
            if (newSiteUrl != null) 
                toReturn = $"Your new team site has been created successfully! {newSiteUrl}";
            return toReturn;
        }

        private async Task<string> ExecuteTweetCreation() {
            string toReturn = "Sorry about this, I encountered an error while creating the tweets";
            var reply = _activity.CreateReply("Hang on a moment while I send the invader demo tweets for you...");
            await _connector.Conversations.ReplyToActivityAsync(reply);

            List<string> tweetList = new List<string>() {
                "Whoa! I just saw like 300 flying invaders in Houston Heights #sptfhoustoninvasion", // Superman // 
                "I can't believe it! There's 425 kryptonite invaders in downtown houston #sptfhoustoninvasion", // Wonder Woman
                "Oh my! I'm counting about 200 brute invaders at NRG Stadium #sptfhoustoninvasion", // Batman
                "Oh no! I can barely make them out, but there's like 253 ghost invaders at rice university #sptfhoustoninvasion" // Green Lantern
            };
            try {
                foreach (string tweetText in tweetList) {
                    await _tweetService.SendTweet2(tweetText);
                    await Task.Delay(2000);
                }
                toReturn = "The demo tweets have been created successfully!";
            }
            catch (Exception) { }
            return toReturn;
        }

        #endregion

       

        

        private string GetUrlForPowerBiReport(string reportName) {
            string toReturn = null;
            reportName = reportName.ToLower();
            if (reportName == "location")
                toReturn = "https://app.powerbi.com/groups/me/reports/73cc75c3-8afb-4ffb-886c-4f8d105ada2c/ReportSection";
            else if (reportName == "damage")
                toReturn = "https://app.powerbi.com/groups/me/reports/73cc75c3-8afb-4ffb-886c-4f8d105ada2c/ReportSection2";
            else if (reportName == "life")
                toReturn = "https://app.powerbi.com/groups/me/reports/73cc75c3-8afb-4ffb-886c-4f8d105ada2c/ReportSection1";
            return toReturn;
        }

        private JusticeLeagueIntent DeriveIntent(string intentName) {
            JusticeLeagueIntent toReturn = JusticeLeagueIntent.None;
            var intentEnumMembers = typeof(JusticeLeagueIntent).GetMembers().Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(DescriptionAttribute)));
            foreach (var intent in intentEnumMembers) {
                var attributes = intent.GetCustomAttributes(typeof(DescriptionAttribute), false);
                string description = ((DescriptionAttribute)attributes[0]).Description;
                if (description == intentName) {
                    toReturn = (JusticeLeagueIntent)Enum.Parse(typeof(JusticeLeagueIntent), intent.Name);
                    break;
                }
            }
            return toReturn;
        }
    }

    public class CommandHeroModel {
        public string Hero { get; set; }
        public string Location { get; set; }
        public string SiteName { get; set; }
        public string ReportName { get; set; }

        public CommandHeroModel(Parameter[] parameters) {
            foreach (var param in parameters) {
                if (param.Name == "hero")
                    this.Hero = param.FirstEntity();
                else if (param.Name == "location")
                    this.Location = param.FirstEntity();
                else if (param.Name == "sitename")
                    this.SiteName = param.FirstEntity();
                else if (param.Name == "reportname")
                    this.ReportName = param.FirstEntity();
            }
        }
    }

    public static class ParameterExtensions {
        public static string FirstEntity(this Parameter param) {
            return param.ParameterValues.FirstOrDefault()?.Entity;
        }
    }

    public enum JusticeLeagueIntent {
        [Description("None")]
        None,
        [Description("Command Hero")]
        CommandHero,
        [Description("Create Site")]
        CreateSite,
        [Description("Get Commands")]
        GetCommands,
        [Description("Get Report")]
        GetReport,
        [Description("Greeting")]
        Greeting,
        [Description("Delete Tweets")]
        DeleteTweets,
        [Description("Send Tweets")]
        SendTweets
    }
}
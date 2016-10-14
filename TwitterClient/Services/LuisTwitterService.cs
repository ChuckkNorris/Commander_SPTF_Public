using CommanderHelper.Models;
using CommanderHelper.Services;
using Microsoft.Cognitive.LUIS;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TwitterClient.Models;


namespace TwitterClient.Services {
    public class LuisTwitterService {
        private readonly LuisClient _luisClient;
        private readonly MapService _mapService;
        public LuisTwitterService() {
            _luisClient = new LuisClient(LUIS_APP_ID, LUIS_APP_KEY);
            _mapService = new MapService();
        }

        public async Task<InvaderTweet> ParseTweetText(string tweetText) {
            InvaderTweet toReturn = null;
            LuisResult luisResult = await _luisClient.Predict(tweetText);
            var intendedAction = luisResult?.TopScoringIntent?.Actions?.FirstOrDefault();
            if (intendedAction != null && intendedAction.Triggered) {
                var parameters = intendedAction.Parameters;
                string locationString = parameters.ParamValue("location");
                string invaderCountString = parameters.ParamValue("count");
                int count = 0;
                int.TryParse(invaderCountString, out count);
                toReturn = new InvaderTweet() {
                    InvaderType = parameters.ParamValue("invader type"),
                    Text = tweetText,
                    GeoLocation = await GetGeoFromTweetLocation(locationString),
                    InvaderCount = count,
                    OriginalTweet = tweetText
                };
            }
            return toReturn;
        }

        //public InvaderTweet ParseResultToInvaderTweet(LuisResult luisResult) {
        //    InvaderTweet toReturn = null;
        //    var intendedAction = luisResult?.TopScoringIntent?.Actions?.FirstOrDefault();
        //    if (intendedAction != null && intendedAction.Triggered) {
        //        var parameters = intendedAction.Parameters;
        //        string locationString = parameters.ParamValue("location");
        //        string invaderCountString = parameters.ParamValue("count");
        //        int count = 0;
        //        int.TryParse(invaderCountString, out count);
        //        toReturn = new InvaderTweet() {
        //            InvaderType = parameters.ParamValue("invader type"),
        //            Text = tweetText,
        //            GeoLocation = await GetGeoFromTweetLocation(locationString),
        //            InvaderCount = count
        //        };
        //    }
        //}

        private async Task<GeoLocation> GetGeoFromTweetLocation(string locationString) {
            GeoLocation toReturn = null;
            string address = await _mapService.GetLocationAddress(locationString);
            if (address != null) {
                toReturn = await _mapService.GetGeoLocation(address);
                toReturn.location = address;
            }
            return toReturn;
        }

        #region Constants

        private const string LUIS_APP_KEY = "YOUR_LUIS_API_KEY_HERE";
        private const string LUIS_APP_ID = "YOUR_LUIS_APP_ID_HERE";
        #endregion
    }

    public static class ParameterExtensions {
        public static string FirstEntity(this Parameter param) {
            return param?.ParameterValues?.FirstOrDefault()?.Entity;
        }

        public static string ParamValue(this Parameter[] parameters, string parameterName) {
            string toReturn = null;
            if (parameters != null && parameters.Count() > 0) {
                toReturn = parameters.FirstOrDefault(param => param.Name == parameterName)?.FirstEntity();
            }
            return toReturn;
        }
    }

}
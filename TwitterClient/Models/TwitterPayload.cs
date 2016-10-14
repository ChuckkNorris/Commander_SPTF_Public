using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterClient.Models {
    public class TwitterPayload {
        public Int64 ID;
        public DateTime CreatedAt;
        public string UserName;
        public string TimeZone;
        public string ProfileImageUrl;
        public string Text;
        public string Language;

        public override string ToString() {
            return new { ID, CreatedAt, UserName, TimeZone, ProfileImageUrl, Text, Language }.ToString();
        }

        public string GetTweet() {
            return ToString();
        }

        public static TwitterPayload GetPayloadFromTweet(Tweet tweet) {
            var tweetPayload = new TwitterPayload {
                ID = tweet.Id,
                CreatedAt = TwitterDateTime.ParseTwitterDateTime(tweet.CreatedAt),
                UserName = tweet.User?.Name,
                TimeZone = tweet.User != null ? (tweet.User.TimeZone ?? "(unknown)") : "(unknown)",
                ProfileImageUrl = tweet.User != null ? (tweet.User.ProfileImageUrl ?? "(unknown)") : "(unknown)",
                Text = tweet.Text,
                Language = tweet.Language ?? "(unknown)",
            };

            return tweetPayload;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommanderHelper.Models;
using Microsoft.Cognitive.LUIS;
using CommanderHelper.EntityFramework.Entities;

namespace TwitterClient.Models {
    public class InvaderTweet {

        public string Text { get; set; }
        public int InvaderCount { get; set; }
        public string InvaderType { get; set; }
        public GeoLocation GeoLocation { get; set; }
        public string OriginalTweet { get; set; }

        public InvaderDetail ToInvaderDetail() {
            InvaderDetail toReturn = new InvaderDetail {
                InvaderCount = InvaderCount,
                InvaderType = InvaderType,

                OriginalTweet = OriginalTweet
            };
            return toReturn;
        }

    }
}
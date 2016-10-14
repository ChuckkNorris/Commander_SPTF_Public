using CommanderHelper.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TwitterClient.Services;

namespace TwitterClient.Controllers
{
    public class TwitterController : ApiController
    {

        private readonly TwitterService _twitterService = TwitterService.Instance;
        private readonly ResetService _resetService = new ResetService();

        [HttpGet]
        public async Task<string> ResetDemo() {
            string toReturn = "Demo has been correctly reset";
            await _resetService.ResetDemo();
            Debug.Write(toReturn);
            return toReturn;
        }

        [HttpPost]
        public string Post(TwitterAction action) {
            string toReturn = "Action does not exist";
            if (action.Action == "start") {
                _twitterService.Start();
                toReturn = "Twitter Streaming Started";
            }
            //else if (action.Action == "stop") {
            //    _twitterService.Stop();
            //    toReturn = "Twitter Streaming Stopped";
            //}
            return toReturn;
        }
    }

    public class TwitterAction {
        public string Action { get; set; }
    }
}

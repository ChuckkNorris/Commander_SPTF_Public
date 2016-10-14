using CommanderHelper.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Services {
    public class TweetService : BaseRestService {
        private const string TWITTER_BASE_URL = "https://api.twitter.com/1.1/statuses/update.json"; // statuses/update.json
        private const string OAUTH_CONSUMER_KEY = "zgJCjYZKBg99DgKR46hD7HBwA";
        private const string OAUTH_CONSUMER_SECRET = "LHXQfGBkjdf2i8IRc5RBM0JIEkfMEy3DEllNWyx0pTcWgAQrjY";
        private const string OAUTH_TOKEN = "782662068553908225-f6qLBw4heBHYtmaYwOJyPLapd2Z29HI";
        private const string OAUTH_TOKEN_SECRET = "iPbqH3yFaEFw4pHRMbY2Qll7uB4dS29zSXiCpuBzTOmGJ";

        public async Task SendTweet2(string tweetText) {
            string toReturn = "";
            var postBody = "status=" + Uri.EscapeDataString(tweetText);
            ServicePointManager.Expect100Continue = false;
            string authHeader = GetAuthDos(TWITTER_BASE_URL, $"status={tweetText}", true);
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TWITTER_BASE_URL);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";
                using (Stream stream = request.GetRequestStream()) {
                    byte[] content = Encoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }

                WebResponse response = await request.GetResponseAsync();
                Stream reqStream = response.GetResponseStream();
                StreamReader reqStreamReader = new StreamReader(reqStream);
                string ret = reqStreamReader.ReadToEnd();
            }
            catch (Exception ex) {
                // ex + ":\n" + ex.Message;
            }
        }

        public async Task<IEnumerable<TweetModel>> GetAllTweets() {
            IEnumerable<TweetModel> toReturn = new List<TweetModel>();
            ServicePointManager.Expect100Continue = false;
            string authHeader = GetAuthDos("https://api.twitter.com/1.1/statuses/user_timeline.json", $"screen_name=houston_report");
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=houston_report");
                request.Headers.Add("Authorization", authHeader);
                request.Method = "Get";

                //request.ContentType = "application/x-www-form-urlencoded";
                //using (Stream stream = request.GetRequestStream()) {
                //    byte[] content = Encoding.ASCII.GetBytes(postBody);
                //    stream.Write(content, 0, content.Length);
                //}

                WebResponse response = await request.GetResponseAsync();
                Stream reqStream = response.GetResponseStream();
                StreamReader reqStreamReader = new StreamReader(reqStream);
                string jsonTweets = reqStreamReader.ReadToEnd();
                toReturn = JsonConvert.DeserializeObject<IEnumerable<TweetModel>>(jsonTweets);
            }
            catch (Exception ex) {
                // ex + ":\n" + ex.Message;
            }
            return toReturn;
        }

        public async Task DeleteTweet(long id) {
            ServicePointManager.Expect100Continue = false;
            string url = $"https://api.twitter.com/1.1/statuses/destroy/{id}.json";
            string authHeader = GetAuthDos(url, isPost: true);
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "Post";

                //request.ContentType = "application/x-www-form-urlencoded";
                //using (Stream stream = request.GetRequestStream()) {
                //    byte[] content = Encoding.ASCII.GetBytes(postBody);
                //    stream.Write(content, 0, content.Length);
                //}

                WebResponse response = await request.GetResponseAsync();
                //Stream reqStream = response.GetResponseStream();
                //StreamReader reqStreamReader = new StreamReader(reqStream);
                //string jsonTweets = reqStreamReader.ReadToEnd();
                //TweetResponse tweets = JsonConvert.DeserializeObject<TweetResponse>(jsonTweets);

            }
            catch (Exception ex) {
                // ex + ":\n" + ex.Message;
            }
        }

        //public async void SendTweet(string tweetText) {
        //    //HttpClient client = BuildHttpClient(TWITTER_BASE_URL);

            



        //    HttpClient client = new HttpClient();
        //    client.MaxResponseContentBufferSize = int.MaxValue;
        //    client.DefaultRequestHeaders.ExpectContinue = false;
        //    string statusToSend = Uri.EscapeDataString(tweetText);
        //    string fullUrl = $"{TWITTER_BASE_URL}?status={statusToSend}";
        //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, TWITTER_BASE_URL); // ?status={statusToSend}

        //    string authHeader = GetAuthDos(statusToSend);
        //    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authHeader);
        //    request.Content = new StringContent($"status={statusToSend}");
        //    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        //    //request.Content = new StringContent($"status={statusToSend}", Encoding.UTF8, "application/x-www-form-urlencoded");
        //    var response = await client.SendAsync(request);
        //}


        private string GetAuthDos(string endpointUrl, string queryString = null, bool isPost = false) {
            string httpMethod = isPost ? "POST" : "GET";
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var oauth_timestamp = Convert.ToInt64(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                    .TotalSeconds).ToString();

           // var resource_url = "https://api.twitter.com/1.1/statuses/update.json";

            // create oauth signature
            var baseString = string.Format(
                "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}&" +
                "oauth_timestamp={3}&oauth_token={4}&oauth_version={5}",
                OAUTH_CONSUMER_KEY,
                oauth_nonce,
                oauth_signature_method,
                oauth_timestamp,
                OAUTH_TOKEN,
                oauth_version);
            if (queryString != null) {
                foreach (var queryParam in queryString.Split('&')) {
                    var keyValue = queryParam.Split('=');
                    baseString += $"&{keyValue[0]}={Uri.EscapeDataString(keyValue[1])}";
                }
            }


            baseString = string.Concat(httpMethod, "&", Uri.EscapeDataString(endpointUrl), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(OAUTH_CONSUMER_SECRET),
            "&", Uri.EscapeDataString(OAUTH_TOKEN_SECRET));

            string oauth_signature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey))) {
                oauth_signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            // REMOVED from auth beginning: OAuth
            var authHeader = string.Format(
                "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\"," +
                " oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " +
                "oauth_timestamp=\"{4}\",  oauth_token=\"{5}\", " +
                "oauth_version=\"{6}\"",
                Uri.EscapeDataString(OAUTH_CONSUMER_KEY),
                Uri.EscapeDataString(oauth_nonce),
                Uri.EscapeDataString(oauth_signature),
                Uri.EscapeDataString(oauth_signature_method),
                Uri.EscapeDataString(oauth_timestamp),
                Uri.EscapeDataString(OAUTH_TOKEN),
                Uri.EscapeDataString(oauth_version)
            );
            return authHeader;
        }
        private string GetAuthorizationHeader(string formattedTweet) {
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var oauth_timestamp = GetTimeStamp();

            var baseString = $"POST&"+
                $"{Uri.EscapeDataString(TWITTER_BASE_URL)}&"+
                Uri.EscapeDataString($"oauth_consumer_key={OAUTH_CONSUMER_KEY}&oauth_nonce={oauth_nonce}&oauth_signature_method={oauth_signature_method}&oauth_timestamp={oauth_timestamp}&oauth_token={OAUTH_TOKEN}&oauth_version={oauth_version}&status={formattedTweet}");// &status={statusText}

            var compositeKey = $"{Uri.EscapeDataString(OAUTH_CONSUMER_SECRET)}&{Uri.EscapeDataString(OAUTH_TOKEN_SECRET)}";
            string oauth_signature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey))) {
                oauth_signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            }
            //OAuth
            var escapedAuth = Uri.EscapeDataString(oauth_signature);
            string authHeader = 
                $"oauth_consumer_key=\"{OAUTH_CONSUMER_KEY}\", " + 
                $"oauth_nonce=\"{oauth_nonce}\", " +
                $"oauth_signature=\"{escapedAuth}\", " +  
                $"oauth_signature_method=\"{oauth_signature_method}\", " + 
                $"oauth_timestamp=\"{oauth_timestamp}\", "+
                $"oauth_token=\"{OAUTH_TOKEN}\", "+
                $"oauth_version=\"{oauth_version}\"";

            return authHeader;
        }

        private string GetTimeStamp() {
            return Convert.ToInt64(
                (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                    .TotalSeconds).ToString();
        }


        private string getParamsBaseString(string queryParamsString, string nonce, string timeStamp) {
            // these parameters are required in every request api 
            var baseStringParams = new Dictionary<string, string>{
                {"oauth_consumer_key", OAUTH_CONSUMER_KEY},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_token", OAUTH_TOKEN},
             //   {"oauth_verifier", oAuthVerifier.Text},
                {"oauth_version", "1.0"},
            };

            // put each parameter into dictionary 
            var queryParams = queryParamsString
                                .Split('&')
                                .ToDictionary(p => p.Substring(0, p.IndexOf('=')), p => p.Substring(p.IndexOf('=') + 1));

            foreach (var kv in queryParams) {
                baseStringParams.Add(kv.Key, kv.Value);
            }

            // The OAuth spec says to sort lexigraphically, which is the default alphabetical sort for many libraries. 
            var ret = baseStringParams
                .OrderBy(kv => kv.Key)
                .Select(kv => kv.Key + "=" + kv.Value)
                .Aggregate((i, j) => i + "&" + j);

            return ret;
        }

      
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Services {
    public abstract class BaseRestService {
        protected HttpClient BuildHttpClient(string baseUrl) {
            HttpClient toReturn = new HttpClient();
            toReturn.BaseAddress = new Uri(baseUrl);
            toReturn.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return toReturn;
        }
    }
}

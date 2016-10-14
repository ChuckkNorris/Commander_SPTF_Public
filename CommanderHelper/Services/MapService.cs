using CommanderHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CommanderHelper.Services {
    public class MapService {

        #region Constants
        private const string GOOGLE_PLACES_API_KEY = "YOUR_PLACES_API_KEY_HERE";
        private const string GOOGLE_PLACES_BASE_URL = "https://maps.googleapis.com/maps/api/place/autocomplete/json";
        private const string GOOGLE_GEO_API_KEY = "YOUR_GEO_API_KEY_HERE";
        private const string GOOGLE_GEO_BASE_URL = "https://maps.googleapis.com/maps/api/geocode/json";
        #endregion

        public async Task<GeoLocation> DetermineGeoLocation(string locationToSearchFor) {
            var toReturn = new GeoLocation();
            string address = await GetLocationAddress(locationToSearchFor);

            if (address != null)
                toReturn = await GetGeoLocation(address);
            return toReturn;
        }

        public async Task<string> GetLocationAddress(string locationToSearchFor) {
            string toReturn = null;
            // &types=geocode
            if (locationToSearchFor != null) {
                string queryString = $"?input={locationToSearchFor.Replace(" ", "+")}";
                GMapRestClient client = new GMapRestClient(GOOGLE_PLACES_BASE_URL, GOOGLE_PLACES_API_KEY);
                PlaceSearchResult response = await client.Get<PlaceSearchResult>(queryString);
                toReturn = response?.predictions?.FirstOrDefault()?.description;
            }
            return toReturn;
        }

        public async Task<GeoLocation> GetGeoLocation(string address) {
            GeoLocation toReturn = null;
            string queryString = $"?address={address.Replace(" ", "+")}";
            GMapRestClient client = new GMapRestClient(GOOGLE_GEO_BASE_URL, GOOGLE_GEO_API_KEY);
            GeoResponse response = await client.Get<GeoResponse>(queryString);
            toReturn = response?.results?.FirstOrDefault()?.geometry?.location;
            if (toReturn != null) 
                toReturn.address = address;
            return toReturn;
        }
    }

    public class GMapRestClient : BaseRestService {
        public readonly string _baseUrl;
        public readonly string _apiKey;
        public GMapRestClient(string baseUrl, string apiKey) {
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }

        public async Task<T> Get<T>(string queryStringParameters) where T : new() {
            T toReturn = new T();
            HttpClient client = BuildHttpClient(_baseUrl);
            string queryString = BuildQueryString(queryStringParameters);
            HttpResponseMessage response = await client.GetAsync(queryString);
            if (response.IsSuccessStatusCode)
                toReturn = await response.Content.ReadAsAsync<T>();
            else
                Debug.Write($"{response.StatusCode} ({response.ReasonPhrase})");
            return toReturn;
        }

        private string BuildQueryString(string url) {
            return $"{url}&key={_apiKey}";
        }
    }



}

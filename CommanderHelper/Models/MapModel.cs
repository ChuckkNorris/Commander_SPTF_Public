using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Models {

        #region API Response Classes
        public class GeoResponse {
            public IEnumerable<GeoResult> results { get; set; }
        }

        public class GeoResult {
            public IEnumerable<AddressComponent> address_components { get; set; }
            public Geometry geometry { get; set; }
        }
        public class Geometry {
            public GeoLocation location { get; set; }
        }
        public class GeoLocation {
            public double lat { get; set; }
            public double lng { get; set; }
            public string location { get; set; }
            public string address { get; set; }
        }
        public class AddressComponent {
            public string long_name { get; set; }
            public string short_name { get; set; }
        }
        public class PlaceSearchResult {
            public string status { get; set; }
            public IEnumerable<Prediction> predictions { get; set; }
        }

        public class Prediction {
            public string description { get; set; }
            public string id { get; set; }
            public string place_id { get; set; }
            public string reference { get; set; }
            public IEnumerable<Term> terms { get; set; }
            public IEnumerable<string> types { get; set; }
        }

        public class Term {
            public int offset { get; set; }
            public string value { get; set; }
        }
        #endregion
    
}

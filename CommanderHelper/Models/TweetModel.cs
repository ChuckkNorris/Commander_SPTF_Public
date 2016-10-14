using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Models {

    public class TweetResponse {
        public IEnumerable<TweetModel> Tweets { get; set; }
    }
    public class TweetModel {
        public long id { get; set; }
        public string text { get; set; }

    }
}

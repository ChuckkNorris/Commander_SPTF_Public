using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TwitterClient.Models {
    [DataContract]
    public class TwitterUser {
        [DataMember(Name = "time_zone")]
        public string TimeZone;
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "profile_image_url")]
        public string ProfileImageUrl;
    }
}
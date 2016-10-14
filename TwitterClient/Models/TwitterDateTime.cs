using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TwitterClient.Models {
    public class TwitterDateTime {
        public static DateTime ParseTwitterDateTime(string p) {
            if (p == null)
                return DateTime.Now;
            p = p.Replace("+0000 ", "");
            DateTimeOffset result;

            if (DateTimeOffset.TryParseExact(p, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.GetCultureInfo("en-us").DateTimeFormat, DateTimeStyles.AssumeUniversal, out result))
                return result.DateTime;
            else
                return DateTime.Now;
        }
    }
}
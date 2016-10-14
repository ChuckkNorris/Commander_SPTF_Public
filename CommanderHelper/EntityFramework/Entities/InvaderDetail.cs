using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.EntityFramework.Entities {
    [Table("InvaderDetail")]
    public class InvaderDetail : BaseModel {
        public string InvaderType { get; set; }
        public int InvaderCount { get; set; }
        public Location Location { get; set; }
        public string OriginalTweet { get; set; }

    }

}

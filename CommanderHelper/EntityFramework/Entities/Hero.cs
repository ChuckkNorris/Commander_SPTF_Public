using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.EntityFramework.Entities {
    [Table("Hero")]
    public class Hero : BaseModel {
        public string Name { get; set; }
        public int NumberOfLivesSaved { get; set; }
        public decimal DamageCausedAtLocation { get; set; }
        public int NumberOfInvadersDefeated { get; set; }
    }
}

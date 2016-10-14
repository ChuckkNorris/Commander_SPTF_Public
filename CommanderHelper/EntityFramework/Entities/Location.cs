using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.EntityFramework.Entities {
    [Table("Location")]
    public class Location : BaseModel {
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal DamageCost { get; set; }

    }

}

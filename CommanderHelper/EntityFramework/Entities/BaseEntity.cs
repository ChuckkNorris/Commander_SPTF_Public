﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.EntityFramework.Entities {
    public class BaseModel {
        [Key]
        public long Id { get; set; }
    }
}

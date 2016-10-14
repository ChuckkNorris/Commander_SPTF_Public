using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommanderBot.Models {
    public class SharePointTask {
        public string Title { get; set; }
        public SharePointUser AssignedTo { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class SharePointUser {
        public string Name { get; set; }
    }
}
using CommanderHelper.EntityFramework;
using CommanderHelper.EntityFramework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Services {
    public class ResetService {
        private readonly InvaderContext _invaderContext = new InvaderContext();
        public async Task ResetDemo() {
            _invaderContext.Heroes.Clear();
            _invaderContext.InvaderDetails.Clear();
            _invaderContext.Locations.Clear();
            await _invaderContext.SaveChangesAsync();
           
        }

    }
}

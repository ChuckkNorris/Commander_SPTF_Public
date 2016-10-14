using CommanderHelper.EntityFramework.Entities;
using CommanderHelper.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.EntityFramework {
    public class InvaderContext : DbContext {
        public InvaderContext() : base("InvaderContext") {

        }

        public DbSet<InvaderDetail> InvaderDetails { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Hero> Heroes { get; set; }
    }

    public static class ContextExtensions {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class {
            dbSet.RemoveRange(dbSet);
        }
        private const double MaxCoordinateRange = .02;
        public static Location GetClosest(this DbSet<Location> locations, GeoLocation geoLocation) {
            double minLat = geoLocation.lat - MaxCoordinateRange;
            double maxLat = geoLocation.lat + MaxCoordinateRange;
            double minLng = geoLocation.lng - MaxCoordinateRange;
            double maxLng = geoLocation.lng + MaxCoordinateRange;
            Location toReturn = locations
               .FirstOrDefault(loc => loc.LocationName == geoLocation.address
               || 
               (loc.Latitude < maxLat && loc.Latitude > minLat && loc.Longitude > minLng && loc.Longitude < maxLng));
            if (toReturn == null) {
                toReturn = new Location() {
                    LocationName = geoLocation.address,
                    Longitude = geoLocation.lng,
                    Latitude = geoLocation.lat,
                };
            }
            return toReturn;
        }
    }
}

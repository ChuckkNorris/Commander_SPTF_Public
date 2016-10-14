using CommanderHelper.EntityFramework;
using CommanderHelper.EntityFramework.Entities;
using CommanderHelper.Models;
using Microsoft.BusinessData.MetadataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Services {
    public class HeroService {
        private readonly InvaderContext _invaderContext = new InvaderContext();
        
        public async Task UpdateHero(string heroName, GeoLocation geoLocation) {
            string formattedHeroName = FormatHeroName(heroName);
          
            // Get/Create Hero
            Hero myHero = _invaderContext.Heroes.FirstOrDefault(hero => hero.Name == formattedHeroName);
            if (myHero == null) {
                myHero = new Hero() { Name = formattedHeroName };
                _invaderContext.Heroes.Add(myHero);
            }

            // Increase Lives Saved
            var rand = new Random();
            myHero.NumberOfLivesSaved += rand.Next(300, 1253);

            // Increase Damage
            myHero.DamageCausedAtLocation = IncreaseDamageAtLocation(geoLocation);
            IEnumerable<InvaderDetail> invadersWithinRange = GetInvadersWithinRange(geoLocation);
            myHero.NumberOfInvadersDefeated = invadersWithinRange.Sum(x => x.InvaderCount);

            // Defeat Invaders
            foreach (var invader in invadersWithinRange) {
                invader.InvaderCount = 0;
            }


            await _invaderContext.SaveChangesAsync();
        }
       
        private decimal IncreaseDamageAtLocation(GeoLocation geoLocation) {
            Location matchingLocation = _invaderContext.Locations.GetClosest(geoLocation);
            Random rand = new Random();
            matchingLocation.DamageCost += rand.Next(100000, 34000000);
            return matchingLocation.DamageCost;
        }

        private const double MaxCoordinateRange = .02;

        private IEnumerable<InvaderDetail> GetInvadersWithinRange(GeoLocation location) {
            IEnumerable<InvaderDetail> toReturn = null;
            double minLat = location.lat - MaxCoordinateRange;
            double maxLat = location.lat + MaxCoordinateRange;
            double minLng = location.lng - MaxCoordinateRange;
            double maxLng = location.lng + MaxCoordinateRange;
            toReturn = _invaderContext.InvaderDetails
                .Where(invader =>
                    invader.Location.Latitude > minLat
                    && invader.Location.Latitude < maxLat
                    && invader.Location.Longitude > minLng
                    && invader.Location.Longitude < maxLng).ToList();
            return toReturn;

        }

        public static string FormatHeroName(string heroName) {
            string toReturn = heroName;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            toReturn = textInfo.ToTitleCase(heroName);
            return toReturn;
        }
    }

    
}

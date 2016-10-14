using CommanderHelper.EntityFramework.Entities;
using CommanderHelper.Models;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CommanderHelper.Services {
    public class SharePointService {


        public static string CreateSite(string siteName) {
            var siteToCreate = new SharePointSite(siteName, createSiteIfNotExist: true);
            siteToCreate.SetBingMapsKey();
            return siteToCreate._site?.Url;
        }

        public void AddInvaderLocation(InvaderDetail invaderDetail) {
            var latestSite = new SharePointSite(latestSite: true);
            var invaderList = latestSite.GetList(SharePointSite.INVADER_LOCATION_LIST);
            if (invaderList == null) 
                latestSite.CreateList(SharePointSite.INVADER_LOCATION_LIST, "Twitter reports about Invaders");

            string itemTitle = $"{invaderDetail.InvaderType} Invaders";
            GeoLocation geo = new GeoLocation { lat = invaderDetail.Location.Latitude, lng = invaderDetail.Location.Longitude };
            latestSite.AddListItem(SharePointSite.INVADER_LOCATION_LIST, itemTitle, invaderType: invaderDetail.InvaderType, count: invaderDetail.InvaderCount, location: geo);
        }

        public void AddHeroTask(string heroName, GeoLocation location) {
            var latestSite = new SharePointSite(latestSite: true);
            latestSite.AddHeroTask(heroName, location);
            latestSite.UpdateInvaderToDefeated(location);
        }

        //public void SetHeroTaskComplete(string heroName) {
        //    var latestSite = new SharePointSite(latestSite: true);
        //    latestSite.SetTaskComplete(heroName);
            
        //    latestSite.AddListItem(SharePointSite.HERO_COMMAND_LIST, "Status", );
        //}

        public IEnumerable<CommandListItem> GetHeroTasks() {
            var latestSite = new SharePointSite(latestSite: true);
            return latestSite.GetListItems(SharePointSite.HERO_COMMAND_LIST);
        }

    }

    public class SharePointSite {

        public readonly Web _site;
        private readonly ClientContext _context;
        public SharePointSite(string siteName, bool createSiteIfNotExist = false) {
            _context = GetDefaultSharePointSiteContext();
            _site = GetSiteByName(siteName);
            if (_site == null && createSiteIfNotExist) {
                _site = CreateSite(siteName);
                if (siteName.ToLower().Contains("command"))
                    siteName = COMMAND_CENTER_SITE;
                else {
                    CreateList(HERO_COMMAND_LIST, "Complete these quests!");
                    CreateList(INVADER_LOCATION_LIST, "Invader Locations");
                }
            }
            
        }

        public SharePointSite(bool latestSite) {
            _context = GetDefaultSharePointSiteContext();
            _site = GetLatestSite();
        }

        #region Site Actions

        private Web CreateSite(string siteName) {

            // Load RootSite e.g. JusticeLeague
            Web rootSite = _context.Web;
            _context.Load(rootSite);
            //build new project site 
            WebCreationInformation subsiteToCreationInfo = new WebCreationInformation() {
                Url = siteName.Replace(" ", ""),
                Title = siteName,
                Description = "For Justice!"
            };
            Web newSubsite = _context.Web.Webs.Add(subsiteToCreationInfo);
            _context.Load(newSubsite,
                site => site.Lists,
                Site => Site.Url,
                site => site.SiteUsers);

            _context.ExecuteQuery();
            return newSubsite;
        }

        #endregion



        #region List Actions
        public void UpdateInvaderToDefeated(GeoLocation geoLocation) {
            List invaderLocationList = GetList(INVADER_LOCATION_LIST);
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection listItems = invaderLocationList.GetItems(query);
            _context.Load(listItems);
            _context.ExecuteQuery();
            foreach (ListItem invaderLocation in listItems) {
                var location = (FieldGeolocationValue)invaderLocation["Location"];
                if (location.Latitude == geoLocation.lat && location.Longitude == geoLocation.lng) {
                    invaderLocation["Status"] = "Defeated";
                    invaderLocation.Update();
                }
            }
          
            _context.ExecuteQuery();

        }
        public void CreateList(string listName, string description) {

            ListCollection lists = _site.Lists;
            ListCreationInformation listCreationInfo = new ListCreationInformation() {
                Title = listName,
                TemplateType = (int)ListTemplateType.GenericList, // ListTemplateType.Tasks = 107,
                Description = description
            };
            lists.Add(listCreationInfo);
            _context.ExecuteQuery();
            AddGeolocationField(listName);
           
            if (listName == INVADER_LOCATION_LIST) {
                AddFieldToList(INVADER_LOCATION_LIST, "Text", "Status");
                AddCountField(listName);
                AddInvaderTypeField(listName);
            }
            else if (listName == HERO_COMMAND_LIST) {
                AddFieldToList(HERO_COMMAND_LIST, "Text", "Status");
                AddFieldToList(HERO_COMMAND_LIST, "Text", "Hero");
            }
        }

        public void SetTaskComplete(string heroName) {
            List list = _site.Lists.GetByTitle(HERO_COMMAND_LIST);
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection taskList = list.GetItems(query);
            foreach (ListItem task in taskList) {

            }
        }
        public void AddHeroTask(string heroName, GeoLocation location) {
            List list = GetList(HERO_COMMAND_LIST);
            ListItemCreationInformation itemCreationInfo = new ListItemCreationInformation();
            ListItem listItem = list.AddItem(itemCreationInfo);

            listItem["Title"] = $"Defend {location.address}";
            listItem["Hero"] = HeroService.FormatHeroName(heroName);
            listItem["Status"] = "Complete";

            if (location != null) {
                FieldGeolocationValue oGeolocationValue = new FieldGeolocationValue();
                oGeolocationValue.Latitude = (double)location.lat;
                oGeolocationValue.Longitude = (double)location.lng;
                listItem["Location"] = oGeolocationValue;
            }
            
            listItem.Update();
            _context.ExecuteQuery();
        }

       

        public void AddListItem(string listTitle, string listItemTitle, string userFirstNameToAssignTo = null, 
            int? count = null,
            string invaderType = null,
            GeoLocation location = null) {

            List list = GetList(listTitle);
            ListItemCreationInformation itemCreationInfo = new ListItemCreationInformation();
            ListItem listItem = list.AddItem(itemCreationInfo);

            listItem["Title"] = listItemTitle;
            if (userFirstNameToAssignTo != null)
                listItem["AssignedTo"] = GetUserByFirstName(userFirstNameToAssignTo);

            if (location != null) {
                FieldGeolocationValue oGeolocationValue = new FieldGeolocationValue();
                oGeolocationValue.Latitude = (double)location.lat;
                oGeolocationValue.Longitude = (double)location.lng;
                listItem["Location"] = oGeolocationValue;
            }

            if (count != null) 
                listItem["Count"] = count;

            if (invaderType != null)
                listItem["Type"] = invaderType;

            listItem.Update();
            _context.ExecuteQuery();
        }

        public IEnumerable<CommandListItem> GetListItems(string listTitle) {
            var toReturn = new List<CommandListItem>();
            List list = _site.Lists.GetByTitle(listTitle);
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection listItems = list.GetItems(query);
            _context.Load(listItems);
            _context.ExecuteQuery();

            foreach (ListItem item in listItems) {
                var toAdd = new CommandListItem() {
                    Title = (string)item["Title"],
                    Status = (string)item["Status"]
                };

                FieldUserValue assignedUser = item["AssignedTo"] as FieldUserValue;
                if (assignedUser != null)
                    toAdd.AssignedUser = assignedUser.LookupValue;

                toReturn.Add(toAdd);
            }
            return toReturn;

        }
        #endregion

        #region Retrieve Sites
        private Web GetLatestSite() {
            WebCollection webCollection = GetRootSite().Webs;
            Web toReturn = webCollection.OrderByDescending(subsite => subsite.Created).FirstOrDefault();
            _context.Load(toReturn,
                site => site.SiteUsers,
                Site => Site.Url,
                site => site.Lists);
            _context.ExecuteQuery();
            return toReturn;
        }

        private Web GetRootSite() {
            Web rootSite = _context.Web;
            _context.Load(rootSite,
                website => website.Webs,
                website => website.SiteUsers,
                Site => Site.Url,
                //  Site => Site.Webs.
                site => site.Lists);
            _context.ExecuteQuery();
            return rootSite;
        }

        private Web GetSiteByName(string siteName) {
            Web toReturn = null;
            WebCollection webCollection = GetRootSite().Webs;
            toReturn = webCollection.FirstOrDefault(site => site.Title == siteName);
            if (toReturn != null) {
                _context.Load(toReturn,
                   site => site.SiteUsers,
                   Site => Site.Url,
                   site => site.Lists);
                _context.ExecuteQuery();
            }

            return toReturn;
        }
        #endregion

        #region Private Helpers

        public void SetBingMapsKey() {
            _site.AllProperties["BING_MAPS_KEY"] = BING_MAPS_KEY;
            _site.Update();
            _context.ExecuteQuery();
        }

        private User GetUserByFirstName(string userFirstName) {
            User toReturn = _site.SiteUsers.FirstOrDefault(user => user.Title.ToLower().Contains(userFirstName.ToLower()));
            return toReturn;
        }

        public List GetList(string listTitle) {
            List toReturn = _site.Lists.FirstOrDefault(list => list.Title == listTitle);
            return toReturn;
        }

        private void AddCountField(string listName) {
            List oList = _site.Lists.GetByTitle(listName);
            oList.Fields.AddFieldAsXml("<Field Type='Integer' DisplayName='Count'/>", true, AddFieldOptions.AddToAllContentTypes);
            oList.Update();
            _context.ExecuteQuery();
        }

        private void AddInvaderTypeField(string listName) {
            List oList = _site.Lists.GetByTitle(listName);
            oList.Fields.AddFieldAsXml("<Field Type='Text' DisplayName='Type'/>", true, AddFieldOptions.AddToAllContentTypes);
            oList.Update();
            _context.ExecuteQuery();
        }

        private void AddFieldToList(string listName, string fieldType, string displayName) {
            List oList = _site.Lists.GetByTitle(listName);
            oList.Fields.AddFieldAsXml($"<Field Type='{fieldType}' DisplayName='{displayName}'/>", true, AddFieldOptions.AddToAllContentTypes);
            oList.Update();
            _context.ExecuteQuery();
        }

        public void AddGeoLocationItem(string listTitle, string listItemName, double latitude, double longitude) {

            List list = GetList(listTitle);
            ListItemCreationInformation itemCreationInfo = new ListItemCreationInformation();
            ListItem listItem = list.AddItem(itemCreationInfo);

            listItem["Title"] = listItemName;

            FieldGeolocationValue oGeolocationValue = new FieldGeolocationValue();
            oGeolocationValue.Latitude = (double)latitude;
            oGeolocationValue.Longitude = (double)longitude;
            listItem["Location"] = oGeolocationValue;

            listItem.Update();
            _context.ExecuteQuery();
        }

        private void AddGeolocationField(string listName) {
            List oList = _site.Lists.GetByTitle(listName);
            oList.Fields.AddFieldAsXml("<Field Type='Geolocation' DisplayName='Location'/>", true, AddFieldOptions.AddToAllContentTypes);
            oList.Update();
            _context.ExecuteQuery();
        }

        private string GetFullUrl(string siteName) {
            return $"{SHAREPOINT_SERVER_URL}/{siteName}";
        }

        private ClientContext GetDefaultSharePointSiteContext() {
            return new ClientContext(SHAREPOINT_SERVER_URL) {
                Credentials = new SharePointOnlineCredentials(USERNAME, CreateSecuredPassword(PASSWORD))
            };
        }

        private SecureString CreateSecuredPassword(string password) {
            SecureString toReturn = new SecureString();
            foreach (char c in password.ToCharArray()) toReturn.AppendChar(c);
            return toReturn;
        }
        #endregion

        #region Constants
        public const string HERO_COMMAND_LIST = "Hero Tasks";
        public const string INVADER_LOCATION_LIST = "Invader Locations";
        public const string COMMAND_CENTER_SITE = "CommandCenter";

        private const string BING_MAPS_KEY = "YOUR_BING_MAPS_API_KEY_HERE";
        private const string SHAREPOINT_SERVER_URL = "https://YOUR_DOMAIN.sharepoint.com";
        private const string USERNAME = "YOUR_EMAIL@YOURDOMAIN.onmicrosoft.com";
        private const string PASSWORD = "YOUR_SHAREPOINT_ONLINE_USER_PASSWORD_HERE";
        #endregion
    }
}

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Cognitive.LUIS;
//using Microsoft.SharePoint.Client;
using CommanderBot.Services;
using System.Collections.Generic;
using CommanderHelper.Services;

namespace CommanderBot {
    [BotAuthentication]
    public class MessagesController : ApiController {
        private const string LUIS_KEY = "YOUR_LUIS_KEY_HERE";
        private const string LUIS_APP_ID = "YOUR_LUIS_APP_ID_HERE";

        private readonly LuisClient _luisClient = new LuisClient(LUIS_APP_ID, LUIS_KEY);
    

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity) {
            if (activity.Type == ActivityTypes.Message) {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = null;
                try {
                   
                    LuisResult intendedActionResult = await _luisClient.Predict(activity.Text);
                    CommandService _commandService = new CommandService(connector, activity);
                    string commandResponse = await _commandService.ExecuteDesiredWorkflow(intendedActionResult);
                    reply = activity.CreateReply(commandResponse);
                   
                }
                catch (Exception ex) {
                    reply = activity.CreateReply("This is embarassing. I'm really sorry about an error during the demo! Here are the details: " + ex.Message);
                }
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message) {

            if (message.Type == ActivityTypes.DeleteUserData) {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate) {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate) {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing) {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping) {
            }

            return null;
        }
    }
}
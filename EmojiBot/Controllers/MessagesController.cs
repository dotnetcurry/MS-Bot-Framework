using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using System.Web;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace EmojiBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {

                //Get attachment
                if (message.Attachments.Count > 0)
                {
                    string imageUri = message.Attachments[0].ContentUrl;
                    return message.CreateReplyMessage(await Utilities.CheckEmotion(imageUri));
                }
                else
                {
                    return message.CreateReplyMessage("Send me a photo!");
                }

            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }

    public static class Utilities
    {
        public static async Task<string> CheckEmotion(string query)
        {

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            string responseMessage = string.Empty;
            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "");

            // Request parameters
            var uri = "https://api.projectoxford.ai/emotion/v1.0/recognize";
            HttpResponseMessage response = null;
            byte[] byteData = Encoding.UTF8.GetBytes("{ 'url': '" + query + "' }");
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content).ConfigureAwait(false);
            }

            string responseString = await response.Content.ReadAsStringAsync();

            EmotionResult[] faces = JsonConvert.DeserializeObject<EmotionResult[]>(responseString);
            if (faces.Count() > 0)
            {
                foreach (EmotionResult r in faces)
                {
                    string emotion = new[]
                    {
                Tuple.Create(r.scores.contempt,"Contempt"),
                Tuple.Create(r.scores.disgust,"Disgust"),
                Tuple.Create(r.scores.fear,"Fear"),
                Tuple.Create(r.scores.happiness,"Happiness"),
                Tuple.Create(r.scores.neutral,"Neutral"),
                Tuple.Create(r.scores.sadness,"Sadness"),
                Tuple.Create(r.scores.surprise,"Surprise"),
            }.Max().Item2;
                    responseMessage += $"![{emotion}](https://emojibot.azurewebsites.net/{emotion}.png)";
                }
            }
            else
            {

                responseMessage = "No faces detacted";
            }

                return responseMessage;
            }
        }


        public class Scores
        {
            public double anger { get; set; }
            public double contempt { get; set; }
            public double disgust { get; set; }
            public double fear { get; set; }
            public double happiness { get; set; }
            public double neutral { get; set; }
            public double sadness { get; set; }
            public double surprise { get; set; }
        }

        public class EmotionResult
        {
            public Scores scores { get; set; }
        }
    }
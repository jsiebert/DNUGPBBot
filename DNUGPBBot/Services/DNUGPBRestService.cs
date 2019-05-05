using DNUGPBBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DNUGPBBot.Services
{
    public class DNUGPBRestService
    {
        private const string ServiceUrl = "http://dotnet-paderborn.azurewebsites.net/api/experimental/Events";

        private readonly HttpClient _client;

        public DNUGPBRestService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<Event>> FetchEventListAsync(WaterfallStepContext stepContext = null)
        {
            var eventList = new List<Event>();
            var uri = new Uri(string.Format(ServiceUrl));

            stepContext?.Context.SendActivityAsync(new Activity()
            {
                Type = ActivityTypes.Typing
            });

            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(content);
                if (result["Events"] != null)
                {
                    eventList = JsonConvert.DeserializeObject<List<Event>>(
                        result["Events"].ToString(),
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });
                }
            }

            return eventList;
        }

        public async Task<IEnumerable<EventDetails>> FetchEventDetailsAsync(string eventId, WaterfallStepContext stepContext = null)
        {
            var eventDetailsList = new List<EventDetails>();
            var uri = new Uri(string.Format(ServiceUrl + "/" + eventId, string.Empty));

            stepContext?.Context.SendActivityAsync(new Activity()
            {
                Type = ActivityTypes.Typing
            });

            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content != null)
                {
                    eventDetailsList.Add(
                        JsonConvert.DeserializeObject<EventDetails>(
                            content,
                            new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            }));
                }
            }

            return eventDetailsList;
        }

        public async Task<IEnumerable<EventLocation>> FetchEventLocationAsync(string eventId, WaterfallStepContext stepContext = null)
        {
            var eventLocationList = new List<EventLocation>();
            var uri = new Uri(string.Format(ServiceUrl + "/" + eventId, string.Empty));

            stepContext?.Context.SendActivityAsync(new Activity()
            {
                Type = ActivityTypes.Typing
            });

            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(content);
                if (result["Location"] != null)
                {
                    eventLocationList.Add(
                        JsonConvert.DeserializeObject<EventLocation>(
                            result["Location"].ToString(),
                            new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            }));
                }
            }

            return eventLocationList;
        }
    }
}

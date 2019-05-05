using AdaptiveCards;
using DNUGPBBot.Resources;
using DNUGPBBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DNUGPBBot.Dialogs
{
    public class SearchDialogLuisExtendedUI : ComponentDialog
    {
        public SearchDialogLuisExtendedUI(string dialogId) : base(dialogId)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                EventFilterStep,
                EventSelectionStep,
                EventDetailsStep
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                Style = ListStyle.HeroCard
            });
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> EventFilterStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Welcome to LUIS event search!",
                cancellationToken: cancellationToken);
            await stepContext.Context.SendActivityAsync(
                "Here are some examples of sentences that I can recognize: 'Show all events' or 'Show me a list of upcoming events'",
                cancellationToken: cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Enter your text please...")
            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> EventSelectionStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var luisApplication =
                new LuisApplication(ApiKeys.LuisAppId, ApiKeys.LuisApiKey, "https://" + ApiKeys.LuisApiHostName);
            var luisRecognizer = new LuisRecognizer(luisApplication);
            var result = await luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            var (intent, score) = result.GetTopScoringIntent();

            if (intent.Equals("None"))
            {
                await stepContext.Context.SendActivityAsync(
                    "Sorry, I didn't get that. Here are some examples that I can recognize: 'Show all events' or 'Show me a list of upcoming events'",
                    cancellationToken: cancellationToken);
            }

            if (intent.Equals("Search"))
            {
                if (!result.Entities.HasValues)
                {
                    await stepContext.Context.SendActivityAsync(
                        "Sorry, I didn't get that. Here are some examples that I can recognize: 'Show all events' or 'Show me a list of upcoming events'",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    var service = new DNUGPBRestService();
                    var eventList = await service.FetchEventListAsync(stepContext);

                    var entity = result.Entities["SearchOption"].First().First().ToString();
                    var events = eventList.Where(e =>
                    {
                        if (entity.Equals("upcoming"))
                        {
                            return e.DateToDateTime() >= DateTime.Today;
                        }

                        if (entity.Equals("past"))
                        {
                            return e.DateToDateTime() < DateTime.Today;
                        }

                        return true;
                    });

                    var choices = events.Select(e => new Choice()
                    {
                        Value = e.EventId.ToString(),
                        Action = new CardAction()
                        {
                            Title = e.Name,
                            Value = e.Name,
                            Type = ActionTypes.ImBack
                        }
                    }).ToList();

                    if (choices.Count > 0)
                    {
                        return await stepContext.PromptAsync(nameof(ChoicePrompt),
                            new PromptOptions
                            {
                                Prompt = MessageFactory.Text("Select Event for details."),
                                Choices = choices,
                                RetryPrompt = MessageFactory.Text("Please select a valid option!")
                            }, cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(
                            "There are currently no events in this category. Please select another category!",
                            cancellationToken: cancellationToken);

                        return await stepContext.ReplaceDialogAsync(nameof(SearchDialogBasicExtendedUI),
                            cancellationToken: cancellationToken);
                    }
                }
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> EventDetailsStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;
            var reply = stepContext.Context.Activity.CreateReply();

            var service = new DNUGPBRestService();
            var eventDetailsList = await service.FetchEventDetailsAsync(selection, stepContext);
            var eventLocationList = await service.FetchEventLocationAsync(selection, stepContext);

            var eventDetails = eventDetailsList.First();
            var eventLocations = eventLocationList.ToList();
            var eventLocation = eventLocations.First();
            var latitude = eventLocations.First().Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = eventLocations.First().Longitude.ToString(CultureInfo.InvariantCulture);

            var converter = new Converter();

            var adaptiveCard = new AdaptiveCard("1.0");

            adaptiveCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"### {eventDetails.Name}",
            });

            adaptiveCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = converter.Convert(eventDetails.ShortDescription),
                Wrap = true
            });

            adaptiveCard.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(string.Format(
                    $"https://dev.virtualearth.net/REST/v1/Imagery/Map/CanvasLight/{latitude},{longitude}/15?pushpin={latitude},{longitude}&key={ApiKeys.BingMapsApiKey}")),
                Size = AdaptiveImageSize.Stretch
            });

            adaptiveCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"{eventLocation.Name}, {eventLocation.Street}, {eventLocation.PLZ} {eventLocation.City}"
            });

            var attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };

            reply.Attachments = new List<Attachment>() { attachment };

            await stepContext.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
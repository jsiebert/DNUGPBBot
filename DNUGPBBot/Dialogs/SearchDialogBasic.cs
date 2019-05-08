using DNUGPBBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DNUGPBBot.Dialogs
{
    public class SearchDialogBasic : ComponentDialog
    {
        public SearchDialogBasic(string dialogId) : base(dialogId)
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
                Style = ListStyle.List
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> EventFilterStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new[]
            {
                "All Events",
                "Upcoming Events",
                "Past Events"
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Which Events would you like too see?"),
                    Choices = ChoiceFactory.ToChoices(options),
                    RetryPrompt = MessageFactory.Text("Please select a valid option!")
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> EventSelectionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;

            var service = new DNUGPBRestService();
            var eventList = await service.FetchEventListAsync(stepContext);

            var events = eventList.Where(e =>
            {
                if (selection.ToLower().StartsWith("upcoming"))
                {
                    return e.DateToDateTime() >= DateTime.Today;
                }

                if (selection.ToLower().StartsWith("past"))
                {
                    return e.DateToDateTime() < DateTime.Today;
                }

                return true;
            });

            var choices = events.Select(e => new Choice()
            {
                Value = e.Name,
                Action = new CardAction()
                {
                    Title = e.Name.Split('.', 2).ElementAt(1).Trim(),
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
                await stepContext.Context.SendActivityAsync("There are currently no events in this category. Please select another category!", cancellationToken: cancellationToken);

                return await stepContext.ReplaceDialogAsync(nameof(SearchDialogBasic), cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> EventDetailsStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;
            var reply = stepContext.Context.Activity.CreateReply(selection);

            reply.TextFormat = "plain";

            await stepContext.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

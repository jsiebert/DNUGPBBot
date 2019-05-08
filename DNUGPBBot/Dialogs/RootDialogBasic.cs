using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Threading;
using System.Threading.Tasks;

namespace DNUGPBBot.Dialogs
{
    public class RootDialogBasic : ComponentDialog
    {
        public RootDialogBasic(string dialogId, UserState userState) : base(dialogId)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                OptionDisplayStep,
                OptionSelectionStep,
                RestartStep
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new SearchDialogBasic(nameof(SearchDialogBasic)));
            AddDialog(new UserProfileDialog(nameof(UserProfileDialog), userState));
            AddDialog(new RegistrationDialogBasic(nameof(RegistrationDialogBasic)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt))
            {
                Style = ListStyle.List
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> OptionDisplayStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new[]
            {
                "Search for Events",
                "Register for Event",
                "Create Profile"
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What would you like to do?"),
                    Choices = ChoiceFactory.ToChoices(options),
                    RetryPrompt = MessageFactory.Text("Please select a valid option!")
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> OptionSelectionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selection = ((FoundChoice)stepContext.Result).Value;

            if (selection.ToLower().StartsWith("search"))
            {
                return await stepContext.BeginDialogAsync(nameof(SearchDialogBasic), cancellationToken: cancellationToken);
            }

            if (selection.ToLower().StartsWith("create"))
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), cancellationToken: cancellationToken);
            }

            if (selection.ToLower().StartsWith("register"))
            {
                return await stepContext.BeginDialogAsync(nameof(RegistrationDialogBasic), cancellationToken: cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> RestartStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(nameof(RootDialogBasic), cancellationToken: cancellationToken);
        }
    }
}

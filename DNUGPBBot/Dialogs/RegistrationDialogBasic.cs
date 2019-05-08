using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace DNUGPBBot.Dialogs
{
    public class RegistrationDialogBasic : ComponentDialog
    {
        public RegistrationDialogBasic(string dialogId) : base(dialogId)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                RegistrationStep
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RegistrationStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Event registration is currently not implemented!", cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

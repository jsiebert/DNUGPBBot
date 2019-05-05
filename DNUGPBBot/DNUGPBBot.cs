using DNUGPBBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DNUGPBBot
{
    public class DNUGPBBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;

        private readonly DialogSet _dialogSet;
        public DNUGPBBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;

            _dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            _dialogSet.Add(new RootDialogBasic(nameof(RootDialogBasic), userState));
            _dialogSet.Add(new RootDialogExtendedUI(nameof(RootDialogExtendedUI), userState));
            _dialogSet.Add(new SearchDialogBasic(nameof(SearchDialogBasic)));
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await RunDialog(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessage(turnContext, cancellationToken);
            await RunDialog(turnContext, cancellationToken);
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id.Equals(turnContext.Activity.Recipient.Id))
                {
                    await SendWelcomeMessage(turnContext, cancellationToken);
                    await RunDialog(turnContext, cancellationToken);
                }
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                if (turnContext.Activity.Action.ToLower().Equals("add"))
                {
                    await SendWelcomeMessage(turnContext, cancellationToken);
                    await RunDialog(turnContext, cancellationToken);
                }
            }
        }

        private static async Task SendWelcomeMessage(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Welcome to the Chatbot of the .NET User Group Paderborn!",
                cancellationToken: cancellationToken);
        }

        private async Task RunDialog(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (results.Status == DialogTurnStatus.Empty)
            {
                // await dialogContext.BeginDialogAsync(nameof(SearchDialogBasic), null, cancellationToken);
                // await dialogContext.BeginDialogAsync(nameof(RootDialogBasic), null, cancellationToken);
                await dialogContext.BeginDialogAsync(nameof(RootDialogExtendedUI), null, cancellationToken);
            }
        }
    }
}

using AdaptiveCards;
using DNUGPBBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DNUGPBBot.Dialogs
{
    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        public UserProfileDialog(string dialogId, UserState userState) : base(dialogId)
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                FirstNameStep,
                LastNameStep,
                EmailStep,
                CompanyNameStep,
                ConfirmStep,
                SummaryStep
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("EmailPrompt", EmailPromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt))
            {
                Style = ListStyle.HeroCard
            });

            InitialDialogId = nameof(WaterfallDialog);
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private static async Task<DialogTurnResult> FirstNameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Please answer the following questions to create a profile...", cancellationToken: cancellationToken);
            await stepContext.Context.SendActivityAsync("The data entered here will not be saved in any way because Profile Creation is not yet implemented ;)", cancellationToken: cancellationToken);
            await stepContext.Context.SendActivityAsync("Type 'help' or '?' to get help, 'cancel' or 'quit' to leave this dialog.", cancellationToken: cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter you First Name: ")
            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> LastNameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["FirstName"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter your Last Name: ")
            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> EmailStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["LastName"] = (string)stepContext.Result;

            return await stepContext.PromptAsync("EmailPrompt", new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter your E-Mail Address: "),
                RetryPrompt = MessageFactory.Text("Please enter a valid E-Mail Address!")
            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> CompanyNameStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["EmailAddress"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please enter your Company Name: ")
            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["CompanyName"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Is the date entered above correct?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            var reply = stepContext.Context.Activity.CreateReply();

            if (result)
            {
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.FirstName = (string)stepContext.Values["FirstName"];
                userProfile.LastName = (string)stepContext.Values["LastName"];
                userProfile.EmailAddress = (string)stepContext.Values["EmailAddress"];
                userProfile.CompanyName = (string)stepContext.Values["CompanyName"];

                var adaptiveCard = new AdaptiveCard("1.0");

                adaptiveCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = "### Your User Profile:"
                });

                adaptiveCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Name: {userProfile.FirstName} {userProfile.LastName}"
                });

                adaptiveCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"E-Mail: {userProfile.EmailAddress}"
                });

                adaptiveCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = $"Company: {userProfile.CompanyName}"
                });

                var attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = adaptiveCard
                };

                reply.Attachments = new List<Attachment>() { attachment };
            }
            else
            {
                reply.Text = "Ok, your information won't be kept.";
            }

            await stepContext.Context.SendActivityAsync(reply, cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> EmailPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var regex = new Regex(@".+@.+\..+");
            return Task.FromResult(promptContext.Recognized.Succeeded && regex.IsMatch(promptContext.Recognized.Value));
        }

        private static async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLower();

                switch (text)
                {
                    case "help":
                    case "?":
                        await innerDc.Context.SendActivityAsync("This is not the help text you are looking for...", cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "cancel":
                    case "quit":
                        await innerDc.Context.SendActivityAsync("Cancelling", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}

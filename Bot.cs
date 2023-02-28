// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Bot.Api.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Api
{
    public class Bot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly DialogSet _dialogSet;
        private readonly BotState _userState;

        public Bot(UserState userState, ConversationState conversationState)
        {
            _userState = userState;
            _conversationState = conversationState;
            _dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>(nameof(DialogState)));

            // Adding the dialogs to the DialogSet.
            _dialogSet.Add(new RootDialog(_conversationState));
            _dialogSet.Add(new ObtenerSaldoPresupuestalDialog());
            _dialogSet.Add(new ObtenerCeCoDialog());
            _dialogSet.Add(new ObtenerSociedadDialog());
        }


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido al Chatbot de Presupuestos!\n"), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Escoja alguna opción que quiera preguntar o escriba su pregunta\n 1. Saber tu saldo presupuestal\n O escribe tu pregunta que tengas =)"), cancellationToken);

                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            /*
            var messageText = turnContext.Activity.Text;
            await turnContext.SendActivityAsync(MessageFactory.Text($"Dijiste: {messageText}"), cancellationToken);
            */
            var messageText = turnContext.Activity.Text;
            if(messageText == "tengo una duda")
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Cual es tu duda?"), cancellationToken);
            }
            

            var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);

            // Verify the user authentication
            

            // Use the DialogSet to start the dialog if it hasn't started yet.
            var result = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (result.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(typeof(RootDialog).Name, null, cancellationToken);
            }

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            
        }
    }
}
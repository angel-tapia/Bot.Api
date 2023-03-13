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
            _dialogSet.Add(new SaludoDialog());
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
                    await turnContext.SendActivityAsync(MessageFactory.Text($"<b>Bienvenido al Chatbot Presupuestal!</b>"), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Elije una opción o escribe tu pregunta"), cancellationToken);
                    var card = new HeroCard
                    {
                        Text = "Aquí hay una variedad de opciones de las cuales puedes escoger:",
                        Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.ImBack, title: "1) Consultar tu saldo presupuestal", value: "Consulta saldo presupuestal"),
                            new CardAction(ActionTypes.ImBack, title: "2) Solicitud de Traspaso", value: "Tengo una solicitud de traspaso"),
                            new CardAction(ActionTypes.ImBack, title: "3) FAQs (Preguntas frecuentes)", value: "Muestrame las preguntas frecuentes"),
                            new CardAction(ActionTypes.ImBack, title: "4) Solicitar Reporte Power BI", value: "Solicito un reporte Power BI")
                        }
                    };
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"O escribe tu propia pregunta =), solo está implementada la intención Consultar tu saldo presupuestal"), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
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
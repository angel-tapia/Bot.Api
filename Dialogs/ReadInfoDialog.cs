using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using System;
using Bot.Api.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using System.Collections.Generic;

namespace Bot.Api.Dialogs
{
    public class ReadInfoDialog : ComponentDialog
    {
        public ReadInfoDialog() : base(nameof(ReadInfoDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptForSociety,
                PrintCeCosUsuario,
                PromptForCeCo,
                PromptForNumCuenta
            }));
        }

        private async Task<DialogTurnResult> PromptForSociety(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "Aquí hay una variedad de opciones de las cuales puedes escoger:",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, title: "1) AC SAB", value: "AC SAB"),
                    new CardAction(ActionTypes.ImBack, title: "2) DAC", value: "DAC")
                }
            };
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa la sociedad a la que perteneces.")
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> PrintCeCosUsuario(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.NextAsync(cancellationToken : cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForCeCo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa el CeCo que quieres consultar.")
            };

            stepContext.Values["Society"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> PrintCuentasUsuario(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForNumCuenta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa el numero de cuenta que quieres consultar.")
            };

            stepContext.Values["CeCo"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        
    }
}

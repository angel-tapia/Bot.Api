using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Bot.Api.Dialogs
{
    public class SaludoDialog : ComponentDialog
    {
        public SaludoDialog() : base(nameof(SaludoDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SaludoAsync
            }));

        }

        private async Task<DialogTurnResult> SaludoAsync(WaterfallStepContext dialog, CancellationToken cancellationToken)
        {
            await dialog.Context.SendActivityAsync(MessageFactory.Text($"Hola! Estoy feliz por ayudarte, escribe tu pregunta o seleccionala del menu de arriba para que comencemos ;)"), cancellationToken);
            return await dialog.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> PromptForSociedadAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Prompt the user to provide the sociedad name
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor, dime el nombre de la sociedad:")
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
    }
}

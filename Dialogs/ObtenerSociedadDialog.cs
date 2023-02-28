using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Api.Dialogs
{
    public class ObtenerSociedadDialog : ComponentDialog
    {
        public ObtenerSociedadDialog() : base(nameof(ObtenerSociedadDialog))
        {
            // Define the steps of the dialog
            var obtenerSociedad = new WaterfallStep[]
            {
                PromptForSociedadAsync,
                DisplaySociedadAsync,
            };

            // Add the steps to the dialog
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), obtenerSociedad));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Set the initial dialog that will be run
            InitialDialogId = nameof(WaterfallDialog);
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

        private static async Task<DialogTurnResult> DisplaySociedadAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Display the sociedad name to the user
            var sociedad = stepContext.Result.ToString();
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"La sociedad que ingresaste es: {sociedad}"), cancellationToken);

            // End the dialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

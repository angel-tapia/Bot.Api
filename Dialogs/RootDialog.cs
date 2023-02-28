using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Azure.Core;
using Azure;
using Azure.AI.Language.Conversations;
using System.Text.Json;

namespace Bot.Api.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private readonly ConversationState _converstationState;
        public RootDialog(ConversationState converstationState)
        {
            _converstationState = converstationState;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetCluIntent,
                Redirect
            }));
        }

        
        private async Task<DialogTurnResult> GetCluIntent(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Uri endpoint = new Uri("https://acnaclu.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("7147a03774cb479cafd922b253ec26e3");

            ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credential);

            string projectName = "AC-NA-CLU";
            string deploymentName = "Prueba";

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = stepContext.Context.Activity.Text,
                        id = "1",
                        participantId = "user"
                    }
                },
                parameters = new
                {
                    projectName,
                    deploymentName,
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            Response response = await client.AnalyzeConversationAsync(RequestContent.Create(data));

            using JsonDocument result = JsonDocument.Parse(response.ContentStream);
            JsonElement conversationalTaskResult = result.RootElement;
            JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

            //The top intent
            var intent = conversationPrediction.GetProperty("topIntent").GetString();

            await stepContext.Context.SendActivityAsync($"The top intent detected is: {intent}", cancellationToken: cancellationToken);

            await stepContext.NextAsync(intent, cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private Task<DialogTurnResult> Redirect(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string intent = stepContext.Result as string;

            switch (intent)
            {
                case "ObtenerSaldoPresup":
                    return stepContext.ReplaceDialogAsync(nameof(ObtenerSaldoPresupuestalDialog), cancellationToken: cancellationToken);
                case "ObtenerCeCo":
                    return stepContext.ReplaceDialogAsync(nameof(ObtenerCeCoDialog), cancellationToken: cancellationToken);
                case "ObtenerSociedad":
                    return stepContext.ReplaceDialogAsync(nameof(ObtenerSociedadDialog), cancellationToken: cancellationToken);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

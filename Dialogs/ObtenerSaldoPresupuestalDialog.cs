using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using Azure.AI.Language.Conversations;
using Azure.Core;
using Azure;
using System.Text.Json;
using System;
using System.Diagnostics.Eventing.Reader;

namespace Bot.Api.Dialogs
{
    public class ObtenerSaldoPresupuestalDialog : ComponentDialog
    {
        public ObtenerSaldoPresupuestalDialog() : base(nameof(ObtenerSaldoPresupuestalDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                TipoSaldoAsync,
                GetCluIntent,
                Redirect
            }));
        }

        private static async Task<DialogTurnResult> TipoSaldoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "Aquí hay una variedad de opciones de las cuales puedes escoger:",
                Buttons = new List<CardAction>
        {
            new CardAction(ActionTypes.ImBack, title: "1) Importe disponible (liberado)", value: "ImporteDisponibleLiberado"),
            new CardAction(ActionTypes.ImBack, title: "2) Anual", value: "Anual"),
            new CardAction(ActionTypes.ImBack, title: "3) Disponible al momento", value: "DisponibleAlMomento"),
            new CardAction(ActionTypes.ImBack, title: "4) Gastado total", value: "GastadoTotal"),
            new CardAction(ActionTypes.ImBack, title: "5) Comprometido", value: "Comprometido")
        }
            };
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"O escribe tu mismo el que quieras"), cancellationToken);

            // Wait for user input

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> GetCluIntent(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string userText = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(userText, cancellationToken : cancellationToken);

            // Use the CLU service
            Uri endpoint = new Uri("https://acnaclu.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("7147a03774cb479cafd922b253ec26e3");

            ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credential);

            string projectName = "AC-CLU-SP";
            string deploymentName = "Prueba";

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = userText,
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

            return await stepContext.NextAsync(intent, cancellationToken);
        }

        private Task<DialogTurnResult> Redirect(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string intent = stepContext.Result as string;

            switch (intent)
            {
                case "ImporteDisponibleLiberado":
                    return stepContext.ReplaceDialogAsync(nameof(ImporteDisponibleLiberadoDialog), cancellationToken: cancellationToken);
                default:
                    throw new NotImplementedException();
            }
        }


    }
}


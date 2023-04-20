using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using System;
using Bot.Api.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;

namespace Bot.Api.Dialogs
{
    public class GastadoTotalDialog : ComponentDialog
    {
        public GastadoTotalDialog() : base(nameof(GastadoTotalDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptForSociety,
                PromptForCeCo,
                PromptForNumCuenta,
                ShowUsers,
                End
            }));
        }

        private async Task<DialogTurnResult> PromptForSociety(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa la sociedad a la que perteneces.")
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
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

        private async Task<DialogTurnResult> PromptForNumCuenta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa el numero de cuenta que quieres consultar.")
            };

            stepContext.Values["CeCo"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowUsers(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Creamos la instancia para la conexion 
            var db = new DatabaseService("sqlserverdac.database.windows.net", "databaseac", "usrteam1", "XW9ZEzoa");

            // Retrieve the saved parameters from the stepContext.Values dictionary
            var society = (string)stepContext.Values["Society"];
            var ceco = (string)stepContext.Values["CeCo"];
            var numCuenta = (string)stepContext.Result;

            var tableName = society switch
            {
                "DAC" => "[DummyDAC]",
                "AC SAB" => "[Dummy_AC_SAB]",
                "SAB" => "[Dummy_AC_SAB]",
                "AC_SAB" => "[Dummy_AC_SAB",
                _ => throw new ArgumentException($"Invalid society: {society}")
            };

            var tableColumn = society switch
            {
                "DAC" => "Real_Anual_Gastado_total",
                "AC SAB" => "Importe_Asignado_Anual",
                "SAB" => "Importe_Asignado_Anual",
                "AC_SAB" => "Importe_Asignado_Anual",
                _ => throw new ArgumentException($"Invalid society: {society}")
            };

            // Construct the query using the CeCo and NumCuenta parameters
            var query = $@"SELECT Centro_Gestor, Pos_Pre, {tableColumn}
                   FROM {tableName}
                   WHERE Centro_Gestor = '{ceco}'
                   AND Pos_Pre = '{numCuenta}'";


            try
            {
                //Ejecutamos la conexion
                var reader = db.ExecuteReader(query);

                while (reader.Read())
                {
                    var CeCo = reader["Centro_Gestor"].ToString();
                    var NumCuenta = reader["Pos_Pre"].ToString();
                    var sociedad = society;
                    var saldoPresupuestal = reader[tableColumn].ToString();

                    //await stepContext.Context.SendActivityAsync($"Centro de Costos: {CeCo}, Numero de cuenta: {NumCuenta}, Sociedad: {sociedad}, Saldo Presupuestal: {saldoPresupuestal}");

                    var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
                    var columnSet = new AdaptiveColumnSet();
                    var column1 = new AdaptiveColumn() { Width = "20%" };
                    var column2 = new AdaptiveColumn() { Width = "30%" };
                    var column3 = new AdaptiveColumn() { Width = "25%" };
                    var column4 = new AdaptiveColumn() { Width = "25%" };

                    column1.Items.Add(new AdaptiveTextBlock() { Text = "Centro de Costos" });
                    column1.Items.Add(new AdaptiveTextBlock() { Text = CeCo });

                    column2.Items.Add(new AdaptiveTextBlock() { Text = "Numero de cuenta" });
                    column2.Items.Add(new AdaptiveTextBlock() { Text = NumCuenta });

                    column3.Items.Add(new AdaptiveTextBlock() { Text = "Sociedad" });
                    column3.Items.Add(new AdaptiveTextBlock() { Text = sociedad });

                    column4.Items.Add(new AdaptiveTextBlock() { Text = "Saldo Presupuestal" });
                    column4.Items.Add(new AdaptiveTextBlock() { Text = saldoPresupuestal });

                    columnSet.Columns.Add(column1);
                    columnSet.Columns.Add(column2);
                    columnSet.Columns.Add(column3);
                    columnSet.Columns.Add(column4);

                    card.Body.Add(columnSet);

                    Attachment attachment = new Attachment()
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card
                    };

                    var reply = MessageFactory.Attachment(attachment);
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync("The bot encountered an error or bug.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync("To continue to run this bot, please fix the bot source code.", cancellationToken: cancellationToken);
                await stepContext.Context.SendActivityAsync(ex.Message, cancellationToken: cancellationToken);
            }


            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.NextAsync(cancellationToken: cancellationToken);
        }
    }
}

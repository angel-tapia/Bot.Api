using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using System;
using Bot.Api.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Teams;
using System.Net.Sockets;

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
                PrintCuentasUsuario,
                PromptForNumCuenta,
                End
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
            stepContext.Values["Society"] = (string)stepContext.Result;

            //Creamos la instancia para la conexion 
            var db = new DatabaseService("sqlserverdac.database.windows.net", "databaseac", "usrteam1", "XW9ZEzoa");

            var table = "users";
            var name = "Angel Manuel Tapia Avitia";

            string query = $@"SELECT CeCo, DescCeCo, Sociedad
                   FROM {table}
                   WHERE Name = '{name}'";

            try
            {
                //Ejecutamos la conexion
                var reader = db.ExecuteReader(query);

                while (reader.Read())
                {
                    var CeCo = reader["CeCo"].ToString();
                    var DescCeCo = reader["DescCeCo"].ToString();
                    var Sociedad = reader["Sociedad"].ToString();
                    //await stepContext.Context.SendActivityAsync($"Centro de Costos: {CeCo}, Numero de cuenta: {NumCuenta}, Sociedad: {sociedad}, Saldo Presupuestal: {saldoPresupuestal}");

                    var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
                    var columnSet = new AdaptiveColumnSet();
                    var column1 = new AdaptiveColumn() { Width = "20%" };
                    var column2 = new AdaptiveColumn() { Width = "70%" };
                    var column3 = new AdaptiveColumn() { Width = "10%" };

                    column1.Items.Add(new AdaptiveTextBlock() { Text = "Descripcion" });
                    column1.Items.Add(new AdaptiveTextBlock() { Text = CeCo });

                    column2.Items.Add(new AdaptiveTextBlock() { Text = "Centro de Costos" });
                    column2.Items.Add(new AdaptiveTextBlock() { Text = DescCeCo });

                    column3.Items.Add(new AdaptiveTextBlock() { Text = "Sociedad" });
                    column3.Items.Add(new AdaptiveTextBlock() { Text = Sociedad });

                    columnSet.Columns.Add(column1);
                    columnSet.Columns.Add(column2);
                    columnSet.Columns.Add(column3);

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
            return await stepContext.NextAsync(cancellationToken : cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForCeCo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa el CeCo que quieres consultar.")
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> PrintCuentasUsuario(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["CeCo"] = (string)stepContext.Result;

            //Creamos la instancia para la conexion 
            var db = new DatabaseService("sqlserverdac.database.windows.net", "databaseac", "usrteam1", "XW9ZEzoa");

            var table = "users";
            var name = "Angel Manuel Tapia Avitia";

            string query = $@"SELECT NumCuenta, DescCuenta, Sociedad
                   FROM {table}
                   WHERE Name = '{name}'";

            try
            {
                //Ejecutamos la conexion
                var reader = db.ExecuteReader(query);

                while (reader.Read())
                {
                    var NumCuenta = reader["NumCuenta"].ToString();
                    var DescCuenta = reader["DescCuenta"].ToString();
                    var Sociedad = reader["Sociedad"].ToString();

                    var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
                    var columnSet = new AdaptiveColumnSet();
                    var column1 = new AdaptiveColumn() { Width = "20%" };
                    var column2 = new AdaptiveColumn() { Width = "70%" };
                    var column3 = new AdaptiveColumn() { Width = "10%" };

                    column1.Items.Add(new AdaptiveTextBlock() { Text = "Descripcion" });
                    column1.Items.Add(new AdaptiveTextBlock() { Text = DescCuenta });

                    column2.Items.Add(new AdaptiveTextBlock() { Text = "Numero de cuenta" });
                    column2.Items.Add(new AdaptiveTextBlock() { Text = NumCuenta});

                    column3.Items.Add(new AdaptiveTextBlock() { Text = "Sociedad" });
                    column3.Items.Add(new AdaptiveTextBlock() { Text = Sociedad });

                    columnSet.Columns.Add(column1);
                    columnSet.Columns.Add(column2);
                    columnSet.Columns.Add(column3);

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

        private async Task<DialogTurnResult> PromptForNumCuenta(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Por favor ingresa el numero de cuenta que quieres consultar.")
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["NumCuenta"] = (string)stepContext.Result;

            return await stepContext.NextAsync(stepContext, cancellationToken: cancellationToken);
        }
    }
}

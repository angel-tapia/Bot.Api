using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using System;
using Bot.Api.Data;
using Microsoft.Bot.Builder;


namespace Bot.Api.Dialogs
{
    public class ImporteDisponibleLiberadoDialog : ComponentDialog
    {
        public ImporteDisponibleLiberadoDialog() : base(nameof(ImporteDisponibleLiberadoDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ReadInfo,
                ShowUsers,
                End
            }));
        }
        private Task<DialogTurnResult> ReadInfo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.PromptAsync(
             nameof(TextPrompt),
             new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa la sociedad a la que perteneces.") },
             cancellationToken);

            // Save the Society parameter in the stepContext.Values dictionary and prompt the user for the CeCo parameter
            stepContext.Values["Society"] = (string)stepContext.Result;

            stepContext.PromptAsync(
             nameof(TextPrompt),
             new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa el CeCo al que perteneces.") },
             cancellationToken);

            // Save the CeCo parameter in the stepContext.Values dictionary and prompt the user for the NumCuenta parameter
            stepContext.Values["CeCo"] = (string)stepContext.Result;

            stepContext.PromptAsync(
             nameof(TextPrompt),
             new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa el numero de cuenta al que perteneces.") },
             cancellationToken);

            // Save the NumCuenta parameter in the stepContext.Values dictionary and proceed to the next step in the waterfall
            stepContext.Values["NumCuenta"] = (string)stepContext.Result;

            return stepContext.NextAsync(cancellationToken: cancellationToken);
        }


        private Task<DialogTurnResult> ShowUsers(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Creamos la instancia para la conexion 
            var db = new DatabaseService("sqlserverdac.database.windows.net", "databaseac", "usrteam1", "XW9ZEzoa");

            // Retrieve the saved parameters from the stepContext.Values dictionary
            var society = (string)stepContext.Values["Society"];
            var ceco = (string)stepContext.Values["CeCo"];
            var numCuenta = (string)stepContext.Values["NumCuenta"];

            var tableName = society switch
            {
                "DAC" => "[Dummy_DAC]",
                "AC SAB" => "[Dummy_AC_SAB]",
                "SAB" => "[Dummy_AC_SAB]",
                "AC_SAB" => "[Dummy_AC_SAB",
                _ => throw new ArgumentException($"Invalid society: {society}")
            };

            // Construct the query using the CeCo and NumCuenta parameters
            var query = $@"SELECT Centro_Gestor, Pos_Pre, Importe_Disponible_Anual_Dipsonible_al_momento
                   FROM {tableName}
                   WHERE Centro_Gestor = '{ceco}'
                   AND Pos_Pre = '{numCuenta}'";


            try
            {
                //Ejecutamos la conexion
                var reader = db.ExecuteReader(query);

                while (reader.Read())
                {
                    var CeCo = reader["Centro_Gestor"];
                    var NumCuenta = reader["Pos_Pre"];
                    var sociedad = reader["Society"];
                    var saldoPresupuestal = reader["Importe_Disponible_Anual_Disponible_al_momento"];

                    stepContext.Context.SendActivityAsync($"Centro de Costos: {CeCo}, Numero de cuenta: {NumCuenta}, Sociedad: {sociedad}, Saldo Presupuestal: {saldoPresupuestal}");
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                stepContext.Context.SendActivityAsync("The bot encountered an error or bug.", cancellationToken: cancellationToken);
                stepContext.Context.SendActivityAsync("To continue to run this bot, please fix the bot source code.", cancellationToken: cancellationToken);
                stepContext.Context.SendActivityAsync(ex.Message, cancellationToken: cancellationToken);
            }


            return stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;

namespace Bot.Api.Dialogs
{
    public class ObtenerSaldoPresupuestalDialog : ComponentDialog
    {
        public ObtenerSaldoPresupuestalDialog() : base(nameof(ObtenerSaldoPresupuestalDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ShowSaldoPresup,
                End
            }));
        }

        private async Task<DialogTurnResult> ShowSaldoPresup(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Code to retrieve and show the saldo presupuestal information
            var saldoPresup = GetSaldoPresup();

            await stepContext.Context.SendActivityAsync("Tu saldo presupuestal es: " + saldoPresup);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private string GetSaldoPresup()
        {
            // Code to retrieve saldo presupuestal information
            return "1000";
        }
    }
}


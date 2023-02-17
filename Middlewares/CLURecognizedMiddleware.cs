using System;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Schema;
using Azure.AI.Language.Conversations;
using Azure;
using Microsoft.Bot.Builder.AI.Luis;

namespace Bot.Api.Middlewares
{
    public class CLURecognizedMiddleware : IMiddleware
    {
        IStatePropertyAccessor<RecognizerResult> _statePropertyAccessor;
        private readonly LuisRecognizer _cluRecognizer;
    
        public CLURecognizedMiddleware(ConversationState conversationState, LuisRecognizer cluRecognizer)
        {
            _statePropertyAccessor = conversationState.CreateProperty<RecognizerResult>(nameof(RecognizerResult));
            _cluRecognizer = cluRecognizer;
        }
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if(turnContext.Activity.Type == ActivityTypes.Message)
            {
                var utterance = turnContext.Activity.AsMessageActivity().Text;
                if(!string.IsNullOrEmpty(utterance))
                {
                    var cluRecognizer = await _cluRecognizer.RecognizeAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    await _statePropertyAccessor.SetAsync(turnContext, cluRecognizer, cancellationToken);
                }
            }
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
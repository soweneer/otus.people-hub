using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PeopleHub.Dialogs;

namespace PeopleHub.Filters;

public sealed class ChatsGatewayExceptionFilter(ILogger<ChatsGatewayExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ChatsGatewayException exception)
        {
            return;
        }

        logger.LogWarning(
            "Вызов сервиса диалогов завершился ошибкой: {Message} ({StatusCode})",
            exception.Message,
            exception.StatusCode);

        context.Result = new ObjectResult(exception.Message) { StatusCode = exception.StatusCode };
        context.ExceptionHandled = true;
    }
}

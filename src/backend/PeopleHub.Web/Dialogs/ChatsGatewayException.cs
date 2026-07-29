namespace PeopleHub.Dialogs;

public sealed class ChatsGatewayException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

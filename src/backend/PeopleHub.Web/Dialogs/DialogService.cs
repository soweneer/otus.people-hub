using Grpc.Core;
using PeopleHub.Chats.Grpc;
using PeopleHub.Domain.Repositories;
using PeopleHub.Model;
using ChatsDialogs = PeopleHub.Chats.Grpc.Dialogs;

namespace PeopleHub.Dialogs;

internal sealed class DialogService(ChatsDialogs.DialogsClient client, IUserRepository userRepository) : IDialogService
{
    public async Task<bool> SendMessageAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default)
    {
        var response = await CallAsync(() => client.SendAsync(
            new SendRequest { FromUserId = fromUserId, ToUserId = toUserId, Text = text },
            cancellationToken: cancellationToken));

        return response.MessageId != 0;
    }

    public async Task<IReadOnlyCollection<DialogMessageResponse>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        var response = await CallAsync(() => client.ListAsync(
            new ListRequest { UserId1 = userId1, UserId2 = userId2 },
            cancellationToken: cancellationToken));

        return response.Messages
            .Select(message => new DialogMessageResponse(
                message.FromUserId.ToString(),
                message.ToUserId.ToString(),
                message.Text))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<DialogPartnerResponse>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default)
    {
        var response = await CallAsync(() => client.GetPartnersAsync(
            new PartnersRequest { UserId = userId },
            cancellationToken: cancellationToken));

        var partners = new List<DialogPartnerResponse>(response.PartnerIds.Count);
        foreach (var partnerId in response.PartnerIds)
        {
            var user = await userRepository.GetAsync(partnerId, cancellationToken);
            var name = user is null
                ? $"Пользователь #{partnerId}"
                : $"{user.PersonalInfo.Surname} {user.PersonalInfo.Name}";

            partners.Add(new DialogPartnerResponse(partnerId, name));
        }

        return partners;
    }

    private static async Task<TResponse> CallAsync<TResponse>(Func<AsyncUnaryCall<TResponse>> call)
    {
        try
        {
            return await call();
        }
        catch (RpcException exception)
        {
            throw new ChatsGatewayException(MapStatusCode(exception.StatusCode), Describe(exception));
        }
    }

    private static int MapStatusCode(StatusCode statusCode) => statusCode switch
    {
        StatusCode.InvalidArgument or StatusCode.FailedPrecondition => StatusCodes.Status400BadRequest,
        StatusCode.NotFound => StatusCodes.Status404NotFound,
        StatusCode.Unauthenticated => StatusCodes.Status401Unauthorized,
        StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
        StatusCode.Unavailable or StatusCode.DeadlineExceeded => StatusCodes.Status503ServiceUnavailable,
        StatusCode.Cancelled => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status502BadGateway
    };

    private static string Describe(RpcException exception) => exception.StatusCode switch
    {
        StatusCode.InvalidArgument or StatusCode.FailedPrecondition or StatusCode.NotFound => exception.Status.Detail,
        StatusCode.Unavailable or StatusCode.DeadlineExceeded => "Сервис диалогов временно недоступен",
        _ => "Сервис диалогов вернул ошибку"
    };
}

using PeopleHub.Chats.Grpc;
using PeopleHub.Domain.Repositories;
using PeopleHub.Model;
using ChatsDialogs = PeopleHub.Chats.Grpc.Dialogs;

namespace PeopleHub.Dialogs;

internal sealed class DialogService(ChatsDialogs.DialogsClient client, IUserRepository userRepository) : IDialogService
{
    public async Task<bool> SendMessageAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default)
    {
        var response = await client.SendAsync(
            new SendRequest { FromUserId = fromUserId, ToUserId = toUserId, Text = text },
            cancellationToken: cancellationToken);

        return response.MessageId != 0;
    }

    public async Task<IReadOnlyCollection<DialogMessageResponse>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        var response = await client.ListAsync(
            new ListRequest { UserId1 = userId1, UserId2 = userId2 },
            cancellationToken: cancellationToken);

        return response.Messages
            .Select(message => new DialogMessageResponse(
                message.FromUserId.ToString(),
                message.ToUserId.ToString(),
                message.Text))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<DialogPartnerResponse>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default)
    {
        var response = await client.GetPartnersAsync(
            new PartnersRequest { UserId = userId },
            cancellationToken: cancellationToken);

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
}

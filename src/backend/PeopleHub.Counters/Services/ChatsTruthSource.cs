using PeopleHub.Chats.Grpc;
using PeopleHub.Counters.Storage;
using ChatsDialogs = PeopleHub.Chats.Grpc.Dialogs;

namespace PeopleHub.Counters.Services;

internal sealed class ChatsTruthSource(ChatsDialogs.DialogsClient client, ReconcilerOptions options) : IDialogTruthSource
{
    public async Task<IReadOnlyCollection<PartnerCount>> GetCountsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var response = await client.GetUnreadCountsAsync(
            new UnreadCountsRequest { UserId = userId },
            cancellationToken: cancellationToken);

        return response.Counts.Select(count => new PartnerCount(count.PartnerId, count.Count)).ToArray();
    }

    public async Task<IReadOnlyCollection<PartnerUnreadState>> GetStateAsync(long userId, CancellationToken cancellationToken = default)
    {
        var response = await client.GetUnreadStateAsync(
            new UnreadStateRequest { UserId = userId, LimitPerPartner = options.LimitPerPartner },
            cancellationToken: cancellationToken);

        return response.Partners
            .Select(partner => new PartnerUnreadState(
                partner.PartnerId,
                partner.LastReadMessageId,
                partner.UnreadMessageIds.ToArray()))
            .ToArray();
    }
}

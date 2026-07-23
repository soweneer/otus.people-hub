using Grpc.Core;
using PeopleHub.Chats.Services;

namespace PeopleHub.Chats.Grpc;

internal sealed class DialogsGrpcService(IDialogService dialogService) : Dialogs.DialogsBase
{
    public override async Task<SendResponse> Send(SendRequest request, ServerCallContext context)
    {
        var messageId = await dialogService.SendAsync(request.FromUserId, request.ToUserId, request.Text, context.CancellationToken);

        return new SendResponse { MessageId = messageId.ToString() };
    }

    public override async Task<ListResponse> List(ListRequest request, ServerCallContext context)
    {
        var messages = await dialogService.GetDialogAsync(request.UserId1, request.UserId2, context.CancellationToken);

        var response = new ListResponse();
        foreach (var message in messages)
        {
            response.Messages.Add(new DialogMessage
            {
                FromUserId = message.FromUserId,
                ToUserId = message.ToUserId,
                Text = message.Text
            });
        }

        return response;
    }

    public override async Task<PartnersResponse> GetPartners(PartnersRequest request, ServerCallContext context)
    {
        var partnerIds = await dialogService.GetPartnerIdsAsync(request.UserId, context.CancellationToken);

        var response = new PartnersResponse();
        response.PartnerIds.AddRange(partnerIds);

        return response;
    }
}

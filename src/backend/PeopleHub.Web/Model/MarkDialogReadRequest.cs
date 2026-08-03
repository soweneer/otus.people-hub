namespace PeopleHub.Model;

public sealed record MarkDialogReadRequest(string UpToMessageId);

public sealed record MarkDialogReadResponse(string LastReadMessageId);

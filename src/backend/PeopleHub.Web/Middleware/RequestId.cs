namespace PeopleHub.Middleware;

public static class RequestId
{
    public const string HeaderName = "x-request-id";
    private const string ItemKey = "RequestId";

    public static string New() => Guid.NewGuid().ToString("N");

    extension(HttpContext context)
    {
        public string GetRequestId() => context?.Items.TryGetValue(ItemKey, out var value) is true
            ? value as string
            : null;

        public void SetRequestId(string requestId) => context.Items[ItemKey] = requestId;
    }
}

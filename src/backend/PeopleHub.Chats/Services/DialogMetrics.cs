using Prometheus;

namespace PeopleHub.Chats.Services;

internal static class DialogMetrics
{
    public static readonly Counter MessagesSent = Metrics.CreateCounter(
        "chats_messages_sent_total",
        "Сколько сообщений отправлено");

    public static readonly Counter DialogsRead = Metrics.CreateCounter(
        "chats_dialogs_read_total",
        "Сколько раз диалог помечен прочитанным");

    public static readonly Histogram DialogPageSize = Metrics.CreateHistogram(
        "chats_dialog_page_size",
        "Сколько сообщений вернуло чтение диалога",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(1, 2, 12) });

    public static readonly Histogram UnreadReturned = Metrics.CreateHistogram(
        "chats_unread_messages_returned",
        "Сколько непрочитанных сообщений отдано за один запрос",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(1, 2, 12) });

    public static void Publish()
    {
        MessagesSent.IncTo(0);
        DialogsRead.IncTo(0);
    }
}

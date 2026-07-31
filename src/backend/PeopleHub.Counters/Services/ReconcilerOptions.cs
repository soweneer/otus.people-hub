namespace PeopleHub.Counters.Services;

public sealed class ReconcilerOptions
{
    public int LimitPerPartner { get; set; } = 1000;
    public int IntervalSeconds { get; set; } = 60;
    public int BatchSize { get; set; } = 100;
    public bool Enabled { get; set; } = true;
}

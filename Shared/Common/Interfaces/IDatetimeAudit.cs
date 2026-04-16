namespace Shared.Common.Interfaces
{
    public interface IDatetimeAudit
    {
        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset ModifiedAt { get; set; }
    }
}

namespace Shared.Common.Interfaces
{
    public interface IUserAudit
    {
        string? CreatedBy { get; set; }
        string? ModifiedBy { get; set; }
    }
}

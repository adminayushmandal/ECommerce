using Shared.Common.Interfaces;

namespace Domain.Common
{
    public class BaseAuditableEntity : BaseEntity, IUserAudit, IDatetimeAudit
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}

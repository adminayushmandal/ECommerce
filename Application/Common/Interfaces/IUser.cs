namespace Application.Common.Interfaces
{
    public interface IUser
    {
        string? Id { get; }
        string[] Roles { get; }
    }
}

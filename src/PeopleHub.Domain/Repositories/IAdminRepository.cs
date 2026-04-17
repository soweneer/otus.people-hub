namespace PeopleHub.Domain.Services;

public interface IAdminRepository
{
    Task MigrateAsync();
}
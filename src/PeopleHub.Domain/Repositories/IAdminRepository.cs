namespace PeopleHub.Domain.Repositories;

public interface IAdminRepository
{
    Task MigrateAsync();
}
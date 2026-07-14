namespace PeopleHub.Infrastructure.Db;

/// <summary>
/// Создание/пересоздание схемы БД. Инфраструктурная операция, к предметной области не относится.
/// </summary>
public interface IDbMigrator
{
    Task MigrateAsync();
}

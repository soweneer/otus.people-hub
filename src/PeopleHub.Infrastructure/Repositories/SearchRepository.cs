using System.Data;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class SearchRepository(DbClient dbClient) : ISearchRepository
{
    public async Task<IReadOnlyCollection<PersonInfo>> SearchAsync(SearchFilter searchFilter, CancellationToken cancellationToken)
    {
        var (firstName, lastName, skip, take) = searchFilter;

        var conditions = new List<string>();
        var parameters = new List<(string, object)>();
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            conditions.Add("p.surname like @surname");
            parameters.Add(("surname", lastName + "%"));
        }
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            conditions.Add("p.name like @name");
            parameters.Add(("name", firstName + "%"));
        }
        var whereClause = conditions.Count > 0
            ? $"where {string.Join(" and ", conditions)}"
            : string.Empty;

        var selectQuery = $"""
            select p.id, p.surname || ' ' || p.name as name, p.age, p.city
            from {DbClient.PersonsTable} p
            {whereClause}
            order by p.id
            limit {take} offset {skip};
            """;

        var dataTable = await dbClient.ExecuteDataTableAsync(selectQuery, parameters);
        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        return dataTable.Rows.Cast<DataRow>()
            .Select(row => new PersonInfo(
                new PersonLite(
                    int.Parse(row["id"].ToString()),
                    row["name"].ToString(),
                    int.Parse(row["age"].ToString()),
                    row["city"].ToString()
                ),
                FriendRequestStatus.None))
            .ToArray();
    }
}

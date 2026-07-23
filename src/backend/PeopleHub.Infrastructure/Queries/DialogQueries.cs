using System.Data;
using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Queries;

internal sealed class DialogQueries(DbClient dbClient) : IDialogQueries
{
    public async Task<IReadOnlyCollection<DialogPartner>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"""
             select p.partner_id, u.surname, u.name
             from (
                 select case when from_user_id = @userId then to_user_id else from_user_id end as partner_id,
                        max(id) as last_id
                 from {DbClient.DialogsTable}
                 where from_user_id = @userId or to_user_id = @userId
                 group by 1
             ) p
             join {DbClient.UsersTable} u on u.id = p.partner_id
             order by p.last_id desc
             """,
            [("userId", userId)],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>().Select(ExtractPartner).ToArray();
    }

    private static DialogPartner ExtractPartner(DataRow row) =>
        new(
            Convert.ToInt64(row["partner_id"]),
            $"{row["surname"]} {row["name"]}");
}

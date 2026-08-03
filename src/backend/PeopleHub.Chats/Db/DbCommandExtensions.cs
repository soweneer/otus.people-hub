using Npgsql;

namespace PeopleHub.Chats.Db;

internal static class DbCommandExtensions
{
    public static NpgsqlCommand Fill(this NpgsqlCommand command, string query, IEnumerable<(string, object)> parameters)
    {
        command.CommandText = query;

        if (parameters is null)
        {
            return command;
        }

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return command;
    }
}

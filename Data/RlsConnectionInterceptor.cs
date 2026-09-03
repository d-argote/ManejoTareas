using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ManejoTareas.Helpers;

namespace ManejoTareas.Data;

/// <summary>
/// Interceptor que inyecta las variables de sesion para RLS en cada conexion abierta.
/// Usa UserContext.AsyncLocal para obtener el usuario actual.
/// </summary>
public class RlsConnectionInterceptor : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await SetRlsVariablesAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetRlsVariablesAsync(connection, CancellationToken.None).GetAwaiter().GetResult();
        base.ConnectionOpened(connection, eventData);
    }

    private static async Task SetRlsVariablesAsync(DbConnection connection, CancellationToken ct)
    {
        var userId = UserContext.CurrentUserId;
        var isAdmin = UserContext.IsAdmin;

        var idStr = userId?.ToString() ?? "";
        var adminStr = isAdmin ? "1" : "0";

        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
                return;

            using var cmd1 = connection.CreateCommand();
            cmd1.CommandText = "SELECT set_config('app.current_user_id', @id, false)";
            var p1 = cmd1.CreateParameter();
            p1.ParameterName = "@id";
            p1.Value = idStr;
            cmd1.Parameters.Add(p1);
            await cmd1.ExecuteNonQueryAsync(ct);

            using var cmd2 = connection.CreateCommand();
            cmd2.CommandText = "SELECT set_config('app.is_admin', @adm, false)";
            var p2 = cmd2.CreateParameter();
            p2.ParameterName = "@adm";
            p2.Value = adminStr;
            cmd2.Parameters.Add(p2);
            await cmd2.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // Ignorar si la funcion no existe aun (antes de migracion RLS)
        }
    }
}

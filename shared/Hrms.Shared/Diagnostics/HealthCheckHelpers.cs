using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Hrms.Shared.Diagnostics;

public class SqlServerHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public SqlServerHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Kết nối SQL Server thành công.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Kết nối SQL Server thất bại.", ex);
        }
    }
}

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly string _host;

    public RabbitMqHealthCheck(string host)
    {
        _host = host;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            var result = client.BeginConnect(_host, 5672, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
            if (!success)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Kết nối TCP đến RabbitMQ (port 5672) quá thời gian."));
            }
            client.EndConnect(result);
            return Task.FromResult(HealthCheckResult.Healthy("Kết nối TCP đến RabbitMQ (port 5672) thành công."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Kết nối TCP đến RabbitMQ (port 5672) thất bại.", ex));
        }
    }
}

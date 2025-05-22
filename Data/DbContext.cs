using System.Data;
using HBYSClientApi.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace HBYSClientApi.Data;

public class DbContext : IDbContext
{
    private string _connectionString;

    public DbContext()
    {
        _connectionString = Environment.GetEnvironmentVariable("HBYS") ?? throw new ArgumentNullException("Connection string not found in environment variables.");
    }

    public IDbConnection GetDbConnection() => new OracleConnection(_connectionString);
}

using System.Data;

namespace HBYSClientApi.Interfaces;

public interface IDbContext
{
    IDbConnection GetDbConnection();
}

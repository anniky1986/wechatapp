using Agility.Zoey.Core.Entities;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace Agility.Zoey.Data.DbContext;

public static class SqlSugarDbContext
{
    private static readonly Type[] EntityTypes =
    [
        typeof(User),
        typeof(Role),
        typeof(Menu),
        typeof(Dept),
        typeof(Tenant),
        typeof(Dict),
        typeof(DictItem),
        typeof(SysFile),
        typeof(SysFileFolder),
        typeof(SystemSetting),
        typeof(LoginLog),
        typeof(OperationLog),
        typeof(DataPermission),
        typeof(UserRole),
        typeof(RoleMenu),
        typeof(Article),
        typeof(WorkflowDefinition)
    ];

    public static SqlSugarScope CreateClient(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        var dbTypeString = configuration["DatabaseType"] ?? "MySql";

        var dbType = dbTypeString.ToLower() switch
        {
            "mysql" => DbType.MySql,
            "sqlserver" => DbType.SqlServer,
            "sqlite" => DbType.Sqlite,
            _ => DbType.MySql
        };

        var client = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });

        Configure(client);
        InitDatabase(client);

        return client;
    }

    private static void Configure(SqlSugarScope client)
    {
        client.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine($"[SqlSugar SQL] {sql}");
        };

        foreach (var type in EntityTypes)
        {
            client.CodeFirst.InitTables(type);
        }
    }

    private static void InitDatabase(SqlSugarScope client)
    {
        client.DbMaintenance.CreateDatabase();
    }
}
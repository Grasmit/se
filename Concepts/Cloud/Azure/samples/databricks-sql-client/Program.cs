using System.Text.Json;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

var workspaceUrl = config["Databricks:WorkspaceUrl"] ?? Environment.GetEnvironmentVariable("DATABRICKS_WORKSPACE_URL");
var warehouseId = config["Databricks:WarehouseId"] ?? Environment.GetEnvironmentVariable("DATABRICKS_WAREHOUSE_ID");
var pat = config["Databricks:Pat"] ?? Environment.GetEnvironmentVariable("DATABRICKS_PAT");

if (string.IsNullOrEmpty(workspaceUrl) || string.IsNullOrEmpty(warehouseId))
{
    Console.Error.WriteLine("Set Databricks workspace URL and warehouse id via environment variables or appsettings.json.");
    return 1;
}

var client = new DatabricksSqlClient(workspaceUrl, warehouseId, pat);
var sql = config["Databricks:SampleSql"] ?? "SELECT 1 as value";

try
{
    var result = await client.ExecuteSqlAsync(sql);
    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 2;
}

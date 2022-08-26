using LeaveON.Models;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LeaveON.Utils
{
  public class SQLQueryManager
  {
    public static string GetCreateTableQuery(DynamicForm dynamicFormViewModel)
    {
      var table = new StringBuilder();
      table.Append($"CREATE TABLE {dynamicFormViewModel.TableName} (");
      var primaryKeyColumns = dynamicFormViewModel.DynamicFormColumns.Where(x => x.IsPrimaryKey);
      var excludeKeyIds = new List<int>();
      if (primaryKeyColumns?.Any() == true)
      {
        foreach (var primaryKeyColumn in primaryKeyColumns)
        {
          excludeKeyIds.Add(primaryKeyColumn.Id);
          switch (primaryKeyColumn.DataType)
          {
            case "int":
            case "bigint":
              table.Append($"{primaryKeyColumn.ColumnName} {primaryKeyColumn.DataType} NOT NULL PRIMARY KEY,");
              break;
            case "nchar":
            case "char":
            case "varchar":
            case "nvarchar":
              if (primaryKeyColumn.Length > 0)
              {
                table.Append($"{primaryKeyColumn.ColumnName} {primaryKeyColumn.DataType} ({primaryKeyColumn.Length}) NOT NULL PRIMARY KEY,");
              }
              else
              {
                table.Append($"{primaryKeyColumn.ColumnName} {primaryKeyColumn.DataType} NOT NULL PRIMARY KEY,");
              }
              break;
            default:
              table.Append($"{primaryKeyColumn.ColumnName} {primaryKeyColumn.DataType} NOT NULL PRIMARY KEY,");
              break;
          }
        }
      }
      var notNullColumns = dynamicFormViewModel.DynamicFormColumns.Where(x => x.IsNullable && !excludeKeyIds.Contains(x.Id));
      if (notNullColumns?.Any() == true)
      {
        foreach (var notNullColumn in notNullColumns)
        {
          excludeKeyIds.Add(notNullColumn.Id);
          switch (notNullColumn.DataType)
          {
            case "int":
            case "bigint":
              if (notNullColumn.Min != null && notNullColumn.Max != null)
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType} CHECK( {notNullColumn.ColumnName} >= {notNullColumn.Min} AND {notNullColumn.ColumnName} <= {notNullColumn.Max})  NOT NULL,");
              }
              else if (notNullColumn.Min != null)
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType} CHECK( {notNullColumn.ColumnName} >= {notNullColumn.Min}) NOT NULL,");
              }
              else if (notNullColumn.Max != null)
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType} CHECK( {notNullColumn.ColumnName} <= {notNullColumn.Max})  NOT NULL,");
              }
              else
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType}  NOT NULL,");
              }
              break;
            case "nchar":
            case "char":
            case "varchar":
            case "nvarchar":
              if (notNullColumn.Min != null && notNullColumn.Max != null)
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType}({notNullColumn.Length}) CHECK( {notNullColumn.ColumnName} >= {notNullColumn.Min} AND {notNullColumn.ColumnName} <= {notNullColumn.Max})  NOT NULL,");
              }
              else if (notNullColumn.Min != null)
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType}({notNullColumn.Length}) CHECK( {notNullColumn.ColumnName} >= {notNullColumn.Min}) NOT NULL ,");
              }
              else if (notNullColumn.Max != null)
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType}({notNullColumn.Length}) CHECK( {notNullColumn.ColumnName} <= {notNullColumn.Max}) NOT NULL ,");
              }
              else
              {
                table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType}({notNullColumn.Length}) NOT NULL ,");
              }
              break;
            default:
              table.Append($"{notNullColumn.ColumnName} {notNullColumn.DataType} ({notNullColumn.Length}) NOT NULL,");
              break;
          }
        }
      }

      var nullColumns = dynamicFormViewModel.DynamicFormColumns.Where(x => x.IsNullable == false && !excludeKeyIds.Contains(x.Id));
      if (nullColumns?.Any() == true)
      {
        foreach (var nullColumn in nullColumns)
        {
          excludeKeyIds.Add(nullColumn.Id);
          switch (nullColumn.DataType)
          {
            case "int":
            case "bigint":
              if (nullColumn.Min != null && nullColumn.Max != null)
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType} CHECK( {nullColumn.ColumnName} >= {nullColumn.Min} AND {nullColumn.ColumnName} <= {nullColumn.Max}),");
              }
              else if (nullColumn.Min != null)
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType} CHECK( {nullColumn.ColumnName} >= {nullColumn.Min}),");
              }
              else if (nullColumn.Max != null)
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType} CHECK( {nullColumn.ColumnName} <= {nullColumn.Max}),");
              }
              else
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType},");
              }              
              break;
            case "nchar":
            case "char":
            case "varchar":
            case "nvarchar":
              if (nullColumn.Min != null && nullColumn.Max != null)
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType}({nullColumn.Length}) CHECK( {nullColumn.ColumnName} >= {nullColumn.Min} AND {nullColumn.ColumnName} <= {nullColumn.Max}),");
              }
              else if (nullColumn.Min != null)
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType}({nullColumn.Length}) CHECK( {nullColumn.ColumnName} >= {nullColumn.Min}),");
              }
              else if (nullColumn.Max != null)
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType}({nullColumn.Length}) CHECK( {nullColumn.ColumnName} <= {nullColumn.Max}),");
              }
              else
              {
                table.Append($"{nullColumn.ColumnName} {nullColumn.DataType}({nullColumn.Length}),");
              }
              break;
            default:
              table.Append($"{nullColumn.ColumnName} {nullColumn.DataType},");
              break;
          }
        }
      }
      table.Append(")");
      return table.ToString();
    }

    public static bool ExecuteCreateTableQuery(string query, LeaveONEntities db, string tableName)
    {
      //Drop Table If Already Exists
      var dropCMD = $"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo') DROP TABLE {tableName}";
      db.Database.ExecuteSqlCommand(dropCMD);

      var cmd = db.Database.ExecuteSqlCommand(query);
      return true;
    }

    public static bool CheckTable(DynamicFormViewModel item, LeaveONEntities db)
    {
      return db.Database
                   .SqlQuery<int?>(@"
                         SELECT 1 FROM sys.tables AS T
                         INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                         WHERE S.Name = 'dbo' AND T.Name = '" + item.TName + "'")
                   .SingleOrDefault() != null;
    }
  }
}

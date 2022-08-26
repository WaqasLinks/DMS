using LeaveON.Models;
//using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveON.Controllers
{
  public class DataEntryFormController : Controller
  {

    public string myConnectionString = "Data Source=WQSLAPTOP\\MSSQLSERVER15_19;Initial Catalog=LeaveON;Integrated Security=True;";
    // GET: DataEntryForm
    public ActionResult Index()
    {
      var tableNames = GetAllDynamicTables();
      return View(tableNames);
    }

    [HttpPost]
    public List<TableName> GetAllDynamicTables()
    {
      var result = new List<TableName>();
      using (SqlConnection conn = new SqlConnection(myConnectionString))
      using (SqlCommand cmd = new SqlCommand("select Id,TableName,FormName from DynamicForm", conn))
      {
        SqlDataAdapter adapt = new SqlDataAdapter(cmd);
        adapt.SelectCommand.CommandType = CommandType.Text;

        DataTable dt = new DataTable();
        adapt.Fill(dt);

        if (dt.Rows.Count > 0)
        {
          for (int i = 0; i < dt.Rows.Count; i++)
          {
            var record = new TableName();
            record.Id = dt.Rows[i]["Id"].ToString();
            record.Name = dt.Rows[i]["TableName"].ToString();
            record.FormName = dt.Rows[i]["FormName"].ToString();
            result.Add(record);
          }

        }
        return result;
      }
    }

    [HttpPost]
    public JsonResult GetTableColumns(string tableName)
    {
      var result = new List<string>();
      using (SqlConnection conn = new SqlConnection(myConnectionString))
      using (SqlCommand cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' and UPPER(COLUMN_NAME) <> 'DC_ID' and UPPER(COLUMN_NAME) <> 'STATUS'", conn))
      {
        SqlDataAdapter adapt = new SqlDataAdapter(cmd);
        adapt.SelectCommand.CommandType = CommandType.Text;

        DataTable dt = new DataTable();
        adapt.Fill(dt);

        if (dt.Rows.Count > 0)
        {
          for (int i = 0; i < dt.Rows.Count; i++)
          {
            result.Add(dt.Rows[i][0].ToString());
          }

        }
        return Json(result, JsonRequestBehavior.AllowGet);
      }
    }

    [HttpPost]
    public ActionResult UploadExcelFile()
    {
      if (Request.Files.Count > 0)
      {
        List<string> allColumns = new List<string>();
        List<string> allRows = new List<string>();
        List<string> allErrors = new List<string>();
        var tableName = Convert.ToString(Request.Form["tableName"]);


        try
        {
          HttpFileCollectionBase postedFiles = Request.Files;
          HttpPostedFileBase postedFile = postedFiles[0];
          //List<AnnualOffDay> annualLeaves = new List<AnnualOffDay>();
          string filePath = string.Empty;
          if (postedFile != null)
          {
            string path = Server.MapPath("~/Uploads/");
            if (!Directory.Exists(path))
            {
              Directory.CreateDirectory(path);
            }

            filePath = path + Path.GetFileName(postedFile.FileName);
            string extension = Path.GetExtension(postedFile.FileName);
            postedFile.SaveAs(filePath);

            //Read the contents of CSV file.
            string csvData = System.IO.File.ReadAllText(filePath);
            int index = 0;

            //Execute a loop over the rows.

            var tableType = new List<TableType>();
            using (SqlConnection conn = new SqlConnection(myConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COLUMN_NAME,DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' and UPPER(COLUMN_NAME) <> 'DC_ID' and UPPER(COLUMN_NAME) <> 'STATUS'", conn))
            {
              SqlDataAdapter adapt = new SqlDataAdapter(cmd);
              adapt.SelectCommand.CommandType = CommandType.Text;

              DataTable dt = new DataTable();
              adapt.Fill(dt);

              if (dt.Rows.Count > 0)
              {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                  var tableTpe = new TableType();
                  tableTpe.ColumnName = dt.Rows[i][0].ToString();
                  tableTpe.DataType = dt.Rows[i][1].ToString();
                  tableType.Add(tableTpe);
                }

              }

              int count = 0;
              foreach (string row in csvData.Split('\r'))
              {
                if (count == 0)
                {
                  for (int z = 0; z < row.Split(',').Length; z++)
                  {
                    allColumns.Add(Convert.ToString(row.Split(',')[z]));
                  }
                }
                else
                {

                  if (row != "\n")
                  {
                    var newRow = row.Split('\n')[1];
                    allRows.Add(Convert.ToString(newRow));


                    for (int validation = 0; validation < newRow.Split(',').Length; validation++)
                    {
                      var dataType = tableType.FirstOrDefault(x => x.ColumnName.ToUpper() == allColumns[validation].ToUpper()).DataType;

                      if (dataType == "nvarchar")
                      {
                        try
                        {
                          string test = Convert.ToString(newRow.Split(',')[validation]);
                        }
                        catch (Exception ex)
                        {
                          allErrors.Add("Value must be of type string at Col: " + (validation + 1) + " Row: " + (count + 1));
                        }
                      }
                      else if (dataType == "int")
                      {
                        try
                        {
                          int test = Convert.ToInt32(newRow.Split(',')[validation]);
                        }
                        catch (Exception ex)
                        {
                          allErrors.Add("Value must be of type integer at Col: " + (validation + 1) + " Row: " + (count + 1));
                        }
                      }
                      else if (dataType == "datetime")
                      {
                        try
                        {
                          DateTime test = Convert.ToDateTime(newRow.Split(',')[validation]);
                        }
                        catch (Exception ex)
                        {
                          allErrors.Add("Value must be of type dateTime at Col: " + (validation + 1) + " Row: " + (count + 1));
                        }
                      }
                      else if (dataType == "bit")
                      {
                        try
                        {
                          Boolean test = Convert.ToBoolean(newRow.Split(',')[validation]);
                        }
                        catch (Exception ex)
                        {
                          allErrors.Add("Value must be of type boolean at Col: " + (validation + 1) + " Row: " + (count + 1));
                        }
                      }
                      else if (dataType == "decimal")
                      {
                        try
                        {
                          Decimal test = Convert.ToDecimal(newRow.Split(',')[validation]);
                        }
                        catch (Exception ex)
                        {
                          allErrors.Add("Value must be of type boolean at Col: " + (validation + 1) + " Row: " + (count + 1));
                        }
                      }


                    }

                  }
                }
                ++count;
              }

            }
          }




          ///DB
          if (allErrors.Count != 0)
          {


            for (int u = 0; u < allRows.Count; u++)
            {
              string query = "insert into " + tableName + " select ";
              for (int o = 0; o < allRows[u].Split(',').Length; o++)
              {
                query += "'" + allRows[u].Split(',')[o] + "' ,";
              }
              query += "0";
              //Inserting into DB

              try
              {
                using (SqlConnection con = new SqlConnection(myConnectionString))

                using (SqlCommand cmnd = new SqlCommand(query, con))
                {
                  SqlDataAdapter adapter = new SqlDataAdapter(cmnd);
                  adapter.SelectCommand.CommandType = CommandType.Text;

                  DataTable dtble = new DataTable();
                  adapter.Fill(dtble);
                 }

              }
              catch (Exception ex)
              {

              }
           }
          }


          return Json(new { allColumns = allColumns, allRows = allRows, allErrors = allErrors }, JsonRequestBehavior.AllowGet);

        }
        catch (Exception ex)
        {
          return Json("Error occurred. Error details: " + ex.Message);
        }
      }
      else
      {
        return Json("No file selected.");
      }
    }


    public class TableType
    {
      public string ColumnName { get; set; }
      public string DataType { get; set; }
    }

  }
}

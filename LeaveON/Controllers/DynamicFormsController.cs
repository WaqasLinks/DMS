using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Models;
using LeaveON.Models;
using LeaveON.Utils;

namespace LeaveON.Controllers
{
  public class DynamicFormsController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: DynamicForms
    public ActionResult Index()
    {
      var forms = db.DynamicForms.Select(x => new DynamicFormViewModel { Id = x.Id, FName = x.FormName, TName = x.TableName, IsAutoTruncate = x.IsAutoTruncate }).ToList();
      var formTables = new List<DynamicFormViewModel>();
      foreach (var item in forms)
      {
        bool exists = SQLQueryManager.CheckTable(item, db);
        if (exists)
        {
          formTables.Add(item);
        }
        else
        {
          var tableColumns = db.DynamicFormColumns.Where(x => x.DynamicFormId == item.Id).ToList();
          if (tableColumns != null && tableColumns.Count > 0)
          {
            foreach (var tcolumn in tableColumns)
            {
              db.DynamicFormColumns.Remove(tcolumn);
            }
            db.SaveChanges();
          }

          var table = db.DynamicForms.Find(item.Id);
          db.DynamicForms.Remove(table);
          db.SaveChanges();
        }
      }

      return View(formTables);
    }
   

    // GET: DynamicForms/Create
    public ActionResult Create()
    {
      ViewBag.srNo = 0;
      return View();
    }

    [HttpGet]
    public ActionResult AddNewRow(string IndexId)
    {
      ViewBag.DataTypes = new List<string>() { "nvarchar", "varchar", "nchar", "char", "bigint", "int", "date", "datetime", "binary" };
      return PartialView("AddNewRow", IndexId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,FName,TName,IsAutoTruncate")] DynamicFormViewModel dynamicFormViewModel,
       List<DynamicFormColumnViewModel> dynamicFormColumns)
    {
      if (dynamicFormViewModel != null && db.DynamicForms.Any(x => x.TableName == dynamicFormViewModel.TName))
      {
        ModelState.AddModelError("TName", $"Table with name {dynamicFormViewModel.TName} already exists");
      }

      if (dynamicFormColumns == null)
      {
        ModelState.AddModelError("TName", $"Please add some Table columns.");
      }

      bool isError = false;
      if (dynamicFormColumns != null && dynamicFormColumns.Count > 0)
      {
        if (dynamicFormColumns.Any(x => string.IsNullOrWhiteSpace(x.ColumnName)))
        {
          ModelState.AddModelError("", $"Column name can not be empty.");
          isError = true;
        }

        var lengthDT = new List<string>() { "nchar", "char", "varchar", "nvarchar" };

        foreach (var dynamicFormColumn in dynamicFormColumns)
        {
          if (lengthDT.Contains(dynamicFormColumn.DataType) && (dynamicFormColumn.Length == null || dynamicFormColumn.Length <= 0))
          {
            ModelState.AddModelError("", $" Please define length for {dynamicFormColumn.ColumnName} Column.");
            isError = true;
          }
        }

        var primaryKeyCount = dynamicFormColumns.Count(x => x.IsPrimaryKey);
        //if (primaryKeyCount == 0)
        //{
        //  ModelState.AddModelError("", $"At least one column should be selected as primary key.");
        //  isError = true;
        //}

        if (primaryKeyCount > 1)
        {
          ModelState.AddModelError("", $"Only one column can be selected as primary key.");
          isError = true;
        }

        foreach (var columnCount in dynamicFormColumns.GroupBy(info => info.ColumnName)
                       .Select(group => new {
                         Column = group.Key,
                         Count = group.Count()
                       }))
        {
          if (columnCount.Count > 1)
          {
            ModelState.AddModelError("", $"ColumnName must be unique. {columnCount.Column} is repeating {columnCount.Count} times.");
            isError = true;
          }
        }
      }

      if (ModelState.IsValid)
      {
        DynamicForm dynamic = new DynamicForm();
        dynamic.TableName = dynamicFormViewModel.TName;
        dynamic.FormName = dynamicFormViewModel.FName;
        dynamic.IsAutoTruncate = dynamicFormViewModel.IsAutoTruncate;
        dynamic.DynamicFormColumns = new List<DynamicFormColumn>();

        foreach (var dynamicFormColumn in dynamicFormColumns)
        {
          dynamic.DynamicFormColumns.Add(new DynamicFormColumn
          {
            IsNullable = dynamicFormColumn.IsNullable,
            ColumnName = dynamicFormColumn.ColumnName,
            DataType = dynamicFormColumn.DataType,
            IsPrimaryKey = dynamicFormColumn.IsPrimaryKey,
            Length = dynamicFormColumn.Length,
            Max = dynamicFormColumn.Max,
            Min = dynamicFormColumn.Min
          });
        }

        db.DynamicForms.Add(dynamic);
        db.SaveChanges();
        SQLQueryManager.ExecuteCreateTableQuery(SQLQueryManager.GetCreateTableQuery(dynamic), db, dynamic.TableName);

        return RedirectToAction("Index");
      }

      ViewBag.IsError = isError;
      ViewBag.DataTypes = new List<string>() { "nvarchar", "varchar", "nchar", "char", "bigint", "int", "date", "datetime", "binary" };
      ViewBag.DynamicFormColumns = dynamicFormColumns;
      ViewBag.srNo = dynamicFormColumns != null ? dynamicFormColumns.Count : 0;
      return View(dynamicFormViewModel);
    }

    // GET: DynamicForms/Edit/5
    public ActionResult Edit(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      DynamicForm dynamicForm = db.DynamicForms.Find(id);
      if (dynamicForm == null)
      {
        return HttpNotFound();
      }

      DynamicFormViewModel dynamicFormViewModel = new DynamicFormViewModel();
      dynamicFormViewModel.Id = dynamicForm.Id;
      dynamicFormViewModel.IsAutoTruncate = dynamicForm.IsAutoTruncate;
      dynamicFormViewModel.FName = dynamicForm.FormName;
      dynamicFormViewModel.TName = dynamicForm.TableName;

      var dynamicFormColumns = new List<DynamicFormColumnViewModel>();
      foreach (var item in dynamicForm.DynamicFormColumns)
      {
        dynamicFormColumns.Add(new DynamicFormColumnViewModel
        {
          ColumnName = item.ColumnName,
          DataType = item.DataType,
          IsNullable = item.IsNullable,
          IsPrimaryKey = item.IsPrimaryKey,
          Length = item.Length,
          Max = item.Max,
          Min = item.Min

        });
      }
      ViewBag.srNo = dynamicForm.DynamicFormColumns != null ? dynamicForm.DynamicFormColumns.Count : 0;
      ViewBag.DataTypes = new List<string>() { "nvarchar", "varchar", "nchar", "char", "bigint", "int", "date", "datetime", "binary" };
      ViewBag.DynamicFormColumns = dynamicFormColumns;
      return View(dynamicFormViewModel);
    }

    // POST: DynamicForms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "Id,FName,TName,IsAutoTruncate")] DynamicFormViewModel dynamicFormViewModel,
       List<DynamicFormColumnViewModel> dynamicFormColumns)
    {
      if (dynamicFormViewModel != null && db.DynamicForms.Any(x => x.TableName == dynamicFormViewModel.TName && x.Id != dynamicFormViewModel.Id))
      {
        ModelState.AddModelError("TName", $"Table with name {dynamicFormViewModel.TName} already exists");
      }

      if (dynamicFormColumns == null)
      {
        ModelState.AddModelError("TName", $"Please add some Table columns.");
      }

      bool isError = false;
      if (dynamicFormColumns != null && dynamicFormColumns.Count > 0)
      {
        if (dynamicFormColumns.Any(x => string.IsNullOrWhiteSpace(x.ColumnName)))
        {
          ModelState.AddModelError("", $"Column name can not be empty.");
          isError = true;
        }

        var lengthDT = new List<string>() { "nchar", "char", "varchar", "nvarchar" };

        foreach (var dynamicFormColumn in dynamicFormColumns)
        {
          if (lengthDT.Contains(dynamicFormColumn.DataType) && (dynamicFormColumn.Length == null || dynamicFormColumn.Length <= 0))
          {
            ModelState.AddModelError("", $" Please define length for {dynamicFormColumn.ColumnName} Column.");
            isError = true;
          }
        }

        var primaryKeyCount = dynamicFormColumns.Count(x => x.IsPrimaryKey);
        //if (primaryKeyCount == 0)
        //{
        //  ModelState.AddModelError("", $"At least one column should be selected as primary key.");
        //  isError = true;
        //}

        if (primaryKeyCount > 1)
        {
          ModelState.AddModelError("", $"Only one column can be selected as primary key.");
          isError = true;
        }

        foreach (var columnCount in dynamicFormColumns.GroupBy(info => info.ColumnName)
                        .Select(group => new {
                          Column = group.Key,
                          Count = group.Count()
                        }))
        {
          if (columnCount.Count > 1)
          {
            ModelState.AddModelError("", $"ColumnName must be unique. {columnCount.Column} is repeating {columnCount.Count} times.");
            isError = true;
          }
        }
      }

      if (ModelState.IsValid)
      {
        var alreadyExistedColumns = db.DynamicFormColumns.Where(x => x.DynamicFormId == dynamicFormViewModel.Id);
        if (alreadyExistedColumns?.Any() == true)
        {
          foreach (var alreadyExistedColumn in alreadyExistedColumns)
          {
            db.DynamicFormColumns.Remove(alreadyExistedColumn);
          }
          db.SaveChanges();
        }

        var dynamicForm = db.DynamicForms.Find(dynamicFormViewModel.Id);
        dynamicForm.FormName = dynamicFormViewModel.FName;
        dynamicForm.TableName = dynamicFormViewModel.TName;
        dynamicForm.IsAutoTruncate = dynamicFormViewModel.IsAutoTruncate;

        dynamicForm.DynamicFormColumns = new List<DynamicFormColumn>();

        foreach (var dynamicFormColumn in dynamicFormColumns)
        {
          dynamicForm.DynamicFormColumns.Add(new DynamicFormColumn
          {
            IsNullable = dynamicFormColumn.IsNullable,
            ColumnName = dynamicFormColumn.ColumnName,
            DataType = dynamicFormColumn.DataType,
            IsPrimaryKey = dynamicFormColumn.IsPrimaryKey,
            Length = dynamicFormColumn.Length,
            Max = dynamicFormColumn.Max,
            Min = dynamicFormColumn.Min
          });
        }


        db.Entry(dynamicForm).State = EntityState.Modified;

        try
        {
          db.SaveChanges();
          SQLQueryManager.ExecuteCreateTableQuery(SQLQueryManager.GetCreateTableQuery(dynamicForm), db, dynamicForm.TableName);
        }
        catch (Exception ex)
        {
          ViewBag.IsError = isError;
          ViewBag.DataTypes = new List<string>() { "nvarchar", "varchar", "nchar", "char", "bigint", "int", "date", "datetime", "binary" };
          ViewBag.DynamicFormColumns = dynamicFormColumns;
          ViewBag.srNo = dynamicFormColumns != null ? dynamicFormColumns.Count : 0;
          ModelState.AddModelError("", $"An error occured while saving chnages. Please contact to administrator.");
          return View(dynamicFormViewModel);
        }

        return RedirectToAction("Index");
      }

      ViewBag.IsError = isError;
      ViewBag.DataTypes = new List<string>() { "nvarchar", "varchar", "nchar", "char", "bigint", "int", "date", "datetime", "binary" };
      ViewBag.DynamicFormColumns = dynamicFormColumns;
      ViewBag.srNo = dynamicFormColumns != null ? dynamicFormColumns.Count : 0;
      return View(dynamicFormViewModel);
    }

    // GET: DynamicForms/Delete/5
    public async Task<ActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      DynamicForm dynamicForm = await db.DynamicForms.FindAsync(id);
      if (dynamicForm == null)
      {
        return HttpNotFound();
      }
      return View(dynamicForm);
    }

    // POST: DynamicForms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {

      var alreadyExistedColumns = db.DynamicFormColumns.Where(x => x.DynamicFormId == id);
      if (alreadyExistedColumns?.Any() == true)
      {
        foreach (var alreadyExistedColumn in alreadyExistedColumns)
        {
          db.DynamicFormColumns.Remove(alreadyExistedColumn);
        }
        db.SaveChanges();
      }

      DynamicForm dynamicForm = await db.DynamicForms.FindAsync(id);
      db.DynamicForms.Remove(dynamicForm);
      await db.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}

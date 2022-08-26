using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveON.Models
{
  public class DynamicFormColumnViewModel
  {
    public int Id { get; set; }
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public int? Length { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public Nullable<int> DynamicFormId { get; set; }
  }
}

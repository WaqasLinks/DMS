using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveON.Models
{
  public class DynamicFormViewModel
  {
    public int Id { get; set; }
    public string FName { get; set; }
    public string TName { get; set; }
    public bool IsAutoTruncate { get; set; }
  }
}

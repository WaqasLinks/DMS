using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveON.Models
{
  public class ApproverReview
  {
    public string Id { get; set; }
    public string BatchNo { get; set; }
    public string FormTitle { get; set; }
    public string ModifiedBy { get; set; }
    public string ModifiedDate { get; set; }
    public string NoOfRecords { get; set; }
    public string ReviewStatus { get; set; }

  }
}

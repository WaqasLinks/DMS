using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public interface ILeaveBalance_MetadataType
    {

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //System.DateTime StartDate { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        //System.DateTime EndDate { get; set; }
    }

    [MetadataType(typeof(ILeaveBalance_MetadataType))]
    public partial class LeaveBalance : ILeaveBalance_MetadataType
    {
        public LeaveBalance()
        {}
            public LeaveBalance(ref Leave leave)
        {
            int LeaveTypeId = leave.LeaveTypeId;
            decimal UserLeavePolicyId = leave.AspNetUser.UserLeavePolicyId.Value;
            //LeaveBalance leaveBalance= leave.AspNetUser.LeaveBalances.FirstOrDefault(x => x.LeaveTypeId == LeaveTypeId && x.UserId == UserId);
            UserLeavePolicyDetail userLeavePolicyDetail = leave.AspNetUser.UserLeavePolicy.UserLeavePolicyDetails.FirstOrDefault(x => x.UserLeavePolicyId == UserLeavePolicyId && x.LeaveTypeId == LeaveTypeId);
            Balance = userLeavePolicyDetail.Allowed;
            Taken = 0;
            Description = string.Empty;
        }

    }


}

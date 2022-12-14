using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Repository.Models
{

    public interface IAspNetUserLeavePolicy_Metadata
    {
        [DisplayName("Fiscal Year Start")]
        Nullable<System.DateTime> FiscalYearStart { get; set; }
        [DisplayName("Fiscal Year End")]
        Nullable<System.DateTime> FiscalYearEnd { get; set; }


    }

    [MetadataType(typeof(IAspNetUserLeavePolicy_Metadata))]
    public partial class UserLeavePolicy : IAspNetUserLeavePolicy_Metadata
    {
        /* Id property has already existed in the mapped class */
    }

    
}

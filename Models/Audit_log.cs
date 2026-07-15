using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Audit_log
{
    public int Log_Db_Key { get; set; }

    public string? table_name { get; set; }

    public int? Primary_key { get; set; }

    public string? Json_Data { get; set; }

    public int? Activity_Db_key { get; set; }

    public string? Event_Description { get; set; }

    public string? Remarks { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public int? Approval_Status { get; set; }

    public string? Changes_On_type { get; set; }

    public string? Previous_JsonData { get; set; }

    public int? User_Requested_Approver { get; set; }

    public long? RelationShipKey { get; set; }

    public virtual Master_Activity? Activity_Db_keyNavigation { get; set; }
}

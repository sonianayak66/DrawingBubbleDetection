using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Base_Line_Engines_Approver
{
    public int BL_Approvers_Dbkey { get; set; }

    public int? BL_Engine_Dbkey { get; set; }

    public int? Role_Dbkey { get; set; }

    public int? Person_Dbkey { get; set; }

    public int? Module_Dbkey { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }
}


using MPCRS.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MPCRS.ViewModels
{
    public partial class Base_Line_Engines_ApproverViewModel
    {
        public int BL_Approvers_Dbkey { get; set; }

        public int? BL_Engine_Dbkey { get; set; }

        public int? Role_Dbkey { get; set; }

        public int? Person_Dbkey { get; set; }

        public int? Module_Dbkey { get; set; }

        public int? Updated_By { get; set; }

        public DateTime? Updated_On { get; set; }

        public string? Updated_By_UserGuid { get; set; }
    }
    public class BaseLineEngineApproversVm
    {
        public int BL_Approvers_Dbkey { get; set; }
        public int BL_Engine_Dbkey { get; set; }
        public int Role_Dbkey { get; set; }
        public int Person_Dbkey { get; set; }
        public int Module_Dbkey { get; set; }
        public string Person_Name { get; set; }
        public string RoleName { get; set; }
        public string ModuleName { get; set; }
        public string Engine_Title { get; set; }
        public string? Updated_By_UserGuid { get; set; }
        public SelectList UserAddtionalRoles { get; set; }
        public SelectList ModuleResponsibility { get; set; }
        public SelectList PersonsList { get; set; }
        public List<BaseLineEngineApproversVm> BaseLineEngine { get; set; }
        public BaseLineEngineApproversVm()
        {
            UserAddtionalRoles = MPCRS.Utilities.Masters.GetMaster_General("Terminology");
            ModuleResponsibility = MPCRS.Utilities.Masters.GetMaster_General("Module_Responsibility");
            PersonsList = MPCRS.Utilities.Masters.GetPersons();
        }

    }

}


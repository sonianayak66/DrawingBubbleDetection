
using MPCRS.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.ViewModels
{
    public class BaseLineEngineVM
    {
        public int BL_Engine_Dbkey { get; set; }

        public string? Engine_Title { get; set; }

        public string? Engine_Description { get; set; }

        public string? is_active { get; set; }

        public int? Updated_By { get; set; }

        public DateTime? Updated_on { get; set; }

        public DateTime? Revision_date { get; set; }

        public string? Revision_title { get; set; }

    }
    // For Creation and Edit of BaseLineEngine
    public class Base_Line_EngineViewModel
    {
    
        public int BL_Engine_Dbkey { get; set; }

        public string? Engine_Title { get; set; }

        public string? Engine_Description { get; set; }

        public bool is_active { get; set; }

        public int? Updated_By { get; set; }

        public DateTime? Updated_on { get; set; }

        public DateTime? Revision_date { get; set; }

        public string? Revision_title { get; set; }

        public virtual ICollection<Engine_Parts_Usage> Engine_Parts_Usages { get; set; } = new List<Engine_Parts_Usage>();

        public virtual ICollection<Engine> Engines { get; set; } = new List<Engine>();
    }


}
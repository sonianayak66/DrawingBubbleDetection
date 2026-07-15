using MPCRS.Models;

namespace MPCRS.ViewModels
{
    public class Master_GeneralVM
    {
        public List<Master_General> master_Generals { get; set; }
    }

    public class ModuleUserMapping
    {
        public int Master_Dbkey { get; set; }

        public string? Master_Name { get; set; }

        public string[] users { get; set; }
    }

}

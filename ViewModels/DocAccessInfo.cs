namespace MPCRS.ViewModels
{
    public class DocAccessInfo
    {
        public int doc_config_dbkey { get; set; }
        public Nullable<int> UserDbkey { get; set; }
        public Nullable<int> Document_Dbkey { get; set; }
        public string UserName { get; set; }
        public bool ReadAccess { get; set; }
        public bool WriteAccess { get; set; }
        public bool DownloadAccess { get; set; }
        public string Refrence_Title { get; set; }
        public string Description { get; set; }
        public string employeeType { get; set; }
        public string departmentName { get; set; }
        public string Roles { get; set; }
    }
}

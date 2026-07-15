namespace MPCRS.ViewModels
{
    public class DocJsTreeViewModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public State state { get; set; }
        public List<DocJsTreeViewModel> children { get; set; }
        public object li_attr { get; set; }
        public A_attr a_attr { get; set; }
        public DocumentsViewModel data { get; set; }
    }

    public class Folders
    {
        public string id { get; set; }
        public string text { get; set; }
        public System.Nullable<int> Parent_id { get; set; }
        public int isactive { get; set; }
        public int isapprove { get; set; }
        public string item_type { get; set; }
        public DocumentsViewModel data { get; set; }
    }

    public class FolderAccessConfigs
    {
        public int UserDbkey { get; set; }
        public int Document_Dbkey { get; set; }
        public int doc_config_dbkey { get; set; }
        public int ReadAccess { get; set; }
        public int WriteAccess { get; set; }
        public int DownloadAccess { get; set; }
    }



}

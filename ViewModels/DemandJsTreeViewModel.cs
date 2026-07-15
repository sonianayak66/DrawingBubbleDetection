namespace MPCRS.ViewModels
{
    public class DemandJsTreeViewModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public DemandNodeState state { get; set; }
        public List<DemandJsTreeViewModel> children { get; set; }
        public object li_attr { get; set; }
        public A_attr_DemandNode a_attr { get; set; }
    }

    public class DemandNodeState
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
        public bool Checked { get; set; }
    }

    public class A_attr_DemandNode
    {
        public string id { get; set; }
        public string Class { get; set; }

        public A_attr_DemandNode()
        {
            id = null;
        }
    }

    public class DemandNodeCategory
    {
        public string id { get; set; }
        public string text { get; set; }
        public string Parent_id { get; set; }
        public int isactive { get; set; }

    }


    public class DemandTreeViewModel
    {
        public string RecordType { get; set; }
        public string Parent_id { get; set; }
        public string id { get; set; }
        public int? is_active { get; set; }
        public string Nodetext { get; set; }

    }
}

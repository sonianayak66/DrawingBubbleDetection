namespace MPCRS.ViewModels
{
    public class MaterialIssueJsTreeViewModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public MaterialIssueNodeState state { get; set; }
        public List<MaterialIssueJsTreeViewModel> children { get; set; }
        public object li_attr { get; set; }
        public A_attr_MaterialIssueNode a_attr { get; set; }
    }


    public class MaterialIssueNodeState
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
        public bool Checked { get; set; }
    }

    public class A_attr_MaterialIssueNode
    {
        public string id { get; set; }
        public string Class { get; set; }

        public A_attr_MaterialIssueNode()
        {
            id = null;
        }
    }

    public class MasterialIssueNodeCategory
    {
        public string id { get; set; }
        public string text { get; set; }
        public string Parent_id { get; set; }
        public int isactive { get; set; }

    }


    public class MasterialIssueTreeViewModel
    {
        public string RecordType { get; set; }
        public string Parent_id { get; set; }
        public string id { get; set; }
        public int? is_active { get; set; }
        public string Nodetext { get; set; }

    }
}

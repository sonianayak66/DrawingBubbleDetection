namespace MPCRS.ViewModels
{

    public class State
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
        public bool Checked { get; set; }
        public State()
        {
            opened = false;
            disabled = false;
            selected = false;

        }

    }


    public class MplJsTreeViewModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public State state { get; set; }
        public List<MplJsTreeViewModel> children { get; set; }
        public object li_attr { get; set; }
        public A_attr a_attr { get; set; }
        public MplTreeViewModel data { get; set; }
    }



    public class A_attr
    {
        public string id { get; set; }
        public string Class { get; set; }

        public A_attr()
        {
            id = null;
        }
    }

    public class Category
    {
        public string id { get; set; }
        public string text { get; set; }
        public System.Nullable<int> Parent_id { get; set; }
        public int isactive { get; set; }
        public bool? Isupdated { get; set; }
        public bool? ForSopOnly { get; set; }
        public bool? IsNewlyAdded { get; set; }
        public bool? IsRemoved { get; set; }
        public bool? IsReplaced { get; set; }
        public MplTreeViewModel data { get; set; }

    }
}


namespace MPCRS.ViewModels
{
    public class MplTreeViewModel
    {
        public string Type_Part_Name { get; set; }  
        public int Parent_id { get; set; }
        public int Engine_Part_Dbkey { get; set; }
        public int? is_active { get; set; }
        public int Part_relation_dbkey { get; set; }
        public float Qty_per_Engine { get; set; }
        public string Parent_Draw_part_no { get; set; }
        public string ModuleResponsibility { get; set; }
        public string FCBP { get; set; }
        public string Reporting_Type { get; set; }
        public string Execution_Resp { get; set; }      
        public string RMName { get; set; }
    }   
}

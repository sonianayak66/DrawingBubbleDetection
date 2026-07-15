namespace MPCRS.ViewModels
{
    public class MaterialIssueSplitMapping
    {
        public int Issue_Item_Dbkey { get; set; }
        public int Issue_Dbkey { get; set; }
        public int Raw_material_Dbkey { get; set; }
        public string Material_name { get; set; }
        public string UOM { get; set; }
        public string Outer_Dia_mm { get; set; }
        public string Inner_Dia_mm { get; set; }
        public string Thickness { get; set; }
        public int Qty { get; set; }
        public string MMG_File_No { get; set; }
        public string Receipt_No { get; set; }
        public string Material_Reference_No { get; set; }
        public string Heat_No { get; set; }
        public string Batch_No { get; set; }
        public int split_issue_id { get; set; }
        public int SplitId { get; set; }
        public string Master_Name { get; set; }
        public Nullable<double> Measurement { get; set; }
        public Nullable<double> Measurement_breadth { get; set; }
        public Nullable<double> Weight { get; set; }
    }
}

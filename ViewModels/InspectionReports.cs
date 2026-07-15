namespace MPCRS.ViewModels
{
    public class InspectionReports
    {  
        public int Inspect_Rpt_Dbkey { get; set; }
        public int File_No { get; set; }
        public string Part_relation_dbkey { get; set; }
        public string Revision { get; set; }
        public string Serial_No { get; set; }
        public string Job_No { get; set; }
        public string File_Name { get; set; }
        public string File_Location { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string UpdatedRevision { get; set; }
        public int? UpdatedQty { get; set; }
        public string Remarks { get; set; }       
        public string RMC_Number { get; set; }

        public string zipFileName { get; set; }
    }

    

}

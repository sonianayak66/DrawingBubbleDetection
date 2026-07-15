namespace MPCRS.ViewModels
{
    public class SaveInitialAssignmentVM
    {
        public string NCRGuid { get; set; }
        public int ModuleID { get; set; }
        public string UserGUIDs { get; set; }   
    }
    public class SaveAssignmentResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public string WorkflowGuid { get; set; }
        public string StageName { get; set; }
    }

}

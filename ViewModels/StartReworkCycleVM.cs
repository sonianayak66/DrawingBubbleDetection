namespace MPCRS.ViewModels
{
    public class StartReworkCycleVM
    {
        public string NCRWorkflowGUID { get; set; }
        public int ModuleID { get; set; }
        public string AssignedUserGUIDs { get; set; }
    }

    public class StartReworkCycleResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public string NewWorkflowGUID { get; set; }
        public int ItemCount { get; set; }
    }
}

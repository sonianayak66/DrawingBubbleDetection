namespace MPCRS.ViewModels
{
    public class MarkItemReworkVM
    {
        public int NCRItemKey { get; set; }
        public int StageID { get; set; }
        public string NCRWorkFlowGuid { get; set; }
        public bool IsRework { get; set; }
        public bool IsTrialAssembly { get; set; }
    }

    public class MarkReworkResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public string MarkedAs { get; set; }
    }

    public class UnmarkItemReworkVM
    {
        public int NCRItemKey { get; set; }
        public int StageID { get; set; }
    }

    public class UnmarkReworkResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
    }
}

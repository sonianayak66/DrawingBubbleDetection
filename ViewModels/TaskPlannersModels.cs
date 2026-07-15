namespace MPCRS.ViewModels
{
    public class ProjectModel
    {
        public Guid? ProjectGUID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectStatus { get; set; }
        public string Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

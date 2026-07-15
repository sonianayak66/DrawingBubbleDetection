namespace MPCRS.ViewModels
{
    public class DistinctBuildAssignmentData
    {
        public List<DistinctBuildNames> DistinctBuildNames { get; set; }
        public List<DistinctBuildParts> DistinctBuildParts { get; set; }
    }

    public class DistinctBuildNames
    {
        public string? engine_build_name { get; set; }
        public int distinct_part_count { get; set; }
    }
    public class DistinctBuildParts
    {
        public string? engine_build_name { get; set; }
        public string? draw_part_no { get; set; }
        public string? parent_part_no { get; set; }
        public string? revision { get; set; }
        public string? serial_number { get; set; }
        public string? notes { get; set; }
        public int quantity { get; set; }
    }

}

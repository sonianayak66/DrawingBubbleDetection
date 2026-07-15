using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MPCRS.ViewModels
{
    public class TestDataRepositoryVM
    {

        public int TestdataDbKey { get; set; }
        [Required]
        public string? CellNo { get; set; }
        [Required]
        public string? EngineName { get; set; }
        [Required]
        public string? BuildNo { get; set; }
        [Required]
        public string? RunNo { get; set; }
        public string? NH { get; set; }
        public string? NL { get; set; }
        public string? AtmosphericPressure { get; set; }
        public string? RoomTemperature { get; set; }
        public string? DecuSWBuildNumber { get; set; }
    }
}

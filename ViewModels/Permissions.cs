using MPCRS.Utilities;

namespace MPCRS.ViewModels
{
    public class Permissions
    {
        public PermissionDescription PermissionInfo { get; set; }
        public string PermissionString { get; set; }
        public bool selected { get; set; } = false;
    }
}

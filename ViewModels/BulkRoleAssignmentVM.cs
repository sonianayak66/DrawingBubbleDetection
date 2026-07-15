namespace MPCRS.ViewModels
{
    public class BulkRoleAssignmentVM
    {
        public List<PersonVM> Users { get; set; } = new();
        public List<RoleItem> Roles { get; set; } = new();
        public Dictionary<string, List<UserRoleInfo>> UserRoleMappings { get; set; } = new();

        // Add to BulkRoleAssignmentVM
        public List<MetaMasterItem> Departments { get; set; } = new();
        public List<MetaMasterItem> PersonTypes { get; set; } = new();
    }

    public class UserRoleInfo
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class RoleItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }



    //Added
    public class MetaMasterItem
    {
        public string MasterGUID { get; set; }
        public string DisplayText { get; set; }
        public string UseValue { get; set; }
    }

}

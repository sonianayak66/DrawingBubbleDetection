using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NavigationMenuControl
{
    public int MenuItemKey { get; set; }

    public string? DisplayName { get; set; }

    public int? ParentKey { get; set; }

    public string? LandingPage { get; set; }

    public string? ClaimRequirement { get; set; }

    public double? DisplayOrder { get; set; }

    public bool? isActive { get; set; }
}

using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_EmailTaskRelationship
{
    public int RelationshipId { get; set; }

    public Guid RelationshipGUID { get; set; }

    public Guid EmailGUID { get; set; }

    public Guid TaskGUID { get; set; }

    public string RelationshipType { get; set; } = null!;

    public decimal? ConfidenceScore { get; set; }

    public bool IsConfirmed { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }
}

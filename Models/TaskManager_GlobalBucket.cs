using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_GlobalBucket
{
    public int BucketId { get; set; }

    public Guid BucketGUID { get; set; }

    public string BucketName { get; set; } = null!;

    public string? BucketDescription { get; set; }

    public int SortOrder { get; set; }

    public string? BucketColor { get; set; }

    public bool IsSystemDefault { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }
}

using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_NoteRecipient
{
    public int Id { get; set; }

    public string IONGUID { get; set; } = null!;

    public string GroupGUID { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}

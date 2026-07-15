using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class OTPVerification
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? OTP { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int Validity { get; set; }
}

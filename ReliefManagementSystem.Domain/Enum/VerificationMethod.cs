using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum VerificationMethod
    {
        None =0,
        ManualReview = 1,
        PhoneCall = 2,
        PhotoEvidence = 3,
        FieldVerification = 4,
        SystemAutoCheck = 5
    }
}

using System.Collections.Generic;

namespace MergeApprover.Models
{
    public class ConfigurationOptions
    {
        public string Host { get; set; }

        public string PrivateToken { get; set; }

        public IEnumerable<ApprovalConfiguration> AutoApprovedRequests { get; set; }
    }

    public class ApprovalConfiguration
    {
        public string Source { get; set; }

        public string Destination { get; set; }
    }
}

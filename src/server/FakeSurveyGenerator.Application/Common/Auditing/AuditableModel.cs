using System;

namespace FakeSurveyGenerator.Application.Common.Auditing
{
    public class AuditableModel
    {
        public string CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTimeOffset? ModifiedOn { get; set; }
    }
}

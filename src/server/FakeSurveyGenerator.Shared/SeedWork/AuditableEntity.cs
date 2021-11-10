using System;

namespace FakeSurveyGenerator.Shared.SeedWork;

public class AuditableEntity : Entity
{
    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string ModifiedBy { get; set; }

    public DateTimeOffset? ModifiedOn { get; set; }
}
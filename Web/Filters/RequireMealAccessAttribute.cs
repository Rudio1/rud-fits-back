using Microsoft.AspNetCore.Mvc;

namespace RudFitAI.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireMealAccessAttribute : ServiceFilterAttribute
{
    public RequireMealAccessAttribute()
        : base(typeof(RequireMealAccessFilter))
    {
    }
}

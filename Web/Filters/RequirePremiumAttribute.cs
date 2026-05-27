using Microsoft.AspNetCore.Mvc;

namespace RudFitAI.Web.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequirePremiumAttribute : ServiceFilterAttribute
{
    public RequirePremiumAttribute()
        : base(typeof(RequirePremiumFilter))
    {
    }
}

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ClientAccountRecovery
{
    [UsedImplicitly]
    internal class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation == null || context == null)
            {
                return;
            }

            var hasHeader = context.ApiDescription.ActionAttributes()
                .OfType<ApiKeyAuthAttribute>()
                .Any();

            if (!hasHeader)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    { "CustomScheme", new string[]{ } }
                }
            };
        }
    }
}

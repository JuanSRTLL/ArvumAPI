using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoAPI.Filters
{
    public class AddApiKeyHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

           
            var existingApiKeyParameter = operation.Parameters.FirstOrDefault(p => p.Name == "X-API-KEY");

            if (existingApiKeyParameter == null)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-API-KEY",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}
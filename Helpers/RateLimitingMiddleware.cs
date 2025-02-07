using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using ProyectoAPI.Models;

namespace ProyectoAPI.Helpers
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly int _maxRequests;
        private readonly TimeSpan _interval;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, int maxRequests, TimeSpan interval)
        {
            _next = next;
            _cache = cache;
            _maxRequests = maxRequests;
            _interval = interval;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = context.Request.Headers["X-API-KEY"].ToString() ?? context.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(key))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("No se proporcionó una clave API");
                return;
            }

            var counter = _cache.GetOrCreate(key, entry =>
            {
                entry.SetAbsoluteExpiration(_interval);
                return new CounterModel { Count = 0, LastReset = DateTime.UtcNow };
            });

            if (counter.Count >= _maxRequests)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Demasiadas solicitudes. Por favor, intente más tarde.");
                return;
            }

            counter.Count++;
            _cache.Set(key, counter, _interval);

            await _next(context);
        }
    }
}
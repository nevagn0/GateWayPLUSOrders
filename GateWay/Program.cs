using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<CustomTransformProvider>();

var app = builder.Build();

app.MapReverseProxy();

app.Run();

public class CustomTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            if (transformContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Authorization", authHeader.ToString());
            }
            else if (transformContext.HttpContext.Request.Cookies.TryGetValue("access_token", out var cookieToken))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {cookieToken}");
            }
            
            // Проброс всех кук
            var cookies = transformContext.HttpContext.Request.Cookies;
            foreach (var cookie in cookies)
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Cookie", $"{cookie.Key}={cookie.Value}");
            }
            
            // X-Forwarded заголовки
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Forwarded-Host", transformContext.HttpContext.Request.Host.Host);
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Forwarded-Proto", transformContext.HttpContext.Request.Scheme);
            
            // Добавляем Request-ID для трассировки
            var requestId = Guid.NewGuid().ToString();
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Request-ID", requestId);
            transformContext.HttpContext.Response.Headers.TryAdd("X-Request-ID", requestId);
        });
    }
}
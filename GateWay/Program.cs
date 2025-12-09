using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapReverseProxy();

app.Run();

public class CustomTransformProvider : ITransformProvider
{
    private ITransformProvider _transformProviderImplementation;

    public void ValidateRoute(TransformRouteValidationContext context)
    {
        _transformProviderImplementation.ValidateRoute(context);
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        _transformProviderImplementation.ValidateCluster(context);
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
            
            var cookies = transformContext.HttpContext.Request.Cookies;
            foreach (var cookie in cookies)
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Cookie", $"{cookie.Key}={cookie.Value}");
            }
            
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Forwarded-Host", transformContext.HttpContext.Request.Host.Host);
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Forwarded-Proto", transformContext.HttpContext.Request.Scheme);
        });
    }
}
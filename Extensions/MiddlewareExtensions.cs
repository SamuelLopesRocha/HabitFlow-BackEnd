public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthMiddleware>();
    }
}
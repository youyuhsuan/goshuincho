public class CookieService : ICookieService
{
    private readonly IWebHostEnvironment _environment;

    public CookieService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void SetAuthCookies(HttpResponse response, string accessToken, string? refreshToken = null, DateTime? expiresAt = null)
    {
        var isDevelopment = _environment.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Path = "/",
            Domain = isDevelopment ? "localhost" : null
        };

        response.Cookies.Append("access_token", accessToken, cookieOptions);

        if (!string.IsNullOrEmpty(refreshToken))
        {
            response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                Path = "/",
                Domain = isDevelopment ? "localhost" : null
            });
        }
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete("access_token");
        response.Cookies.Delete("refresh_token");
    }
}
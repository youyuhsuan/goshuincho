public class CookieService : ICookieService
{
    private readonly IWebHostEnvironment _environment;

    public CookieService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void SetAuthCookies(HttpResponse response, string accessToken, string? refreshToken = null, int expiresIn = 3600)
    {
        var isDevelopment = _environment.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromSeconds(expiresIn),
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
                MaxAge = TimeSpan.FromDays(30),
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
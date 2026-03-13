public class CookieService : ICookieService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CookieService> _logger;

    public CookieService(
        IWebHostEnvironment environment,
        ILogger<CookieService> logger)
    {
        _environment = environment;
        _logger = logger;
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
        _logger.LogInformation("SetAuthCookies: access_token set, Expires={Expires}, Domain={Domain}",
            cookieOptions.Expires, cookieOptions.Domain);

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
            _logger.LogInformation("SetAuthCookies: refresh_token set,refreshToken={RefreshToken}, Expires={Expires}", refreshToken, expiresAt);
        }
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete("access_token");
        response.Cookies.Delete("refresh_token");
    }
}
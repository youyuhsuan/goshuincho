public interface ICookieService
{
    void SetAuthCookies(HttpResponse response, string accessToken, string? refreshToken = null, DateTime? expiresAt = null);
    void ClearAuthCookies(HttpResponse response);
}
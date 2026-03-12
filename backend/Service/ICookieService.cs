public interface ICookieService
{
    void SetAuthCookies(HttpResponse response, string accessToken, string? refreshToken = null, int expiresIn = 3600);
    void ClearAuthCookies(HttpResponse response);
}
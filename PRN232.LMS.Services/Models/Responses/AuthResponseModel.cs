namespace PRN232.LMS.Services.Models.Responses;

public class AuthResponseModel
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }
}

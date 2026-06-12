namespace Large_Scale_CommunityPlatform.Models.Dtos.User;

//Registration Model : Email, Password, FullName
public class UserRegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DoB { get; set; }

    //    public string Role { get; set; } = string.Empty;
}
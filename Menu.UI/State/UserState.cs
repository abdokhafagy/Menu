using Menu.UI.Models.User;

namespace Menu.UI.State;

public sealed class UserState
{
    public UserDto? CurrentUser { get; private set; }

    public void SetUser(UserDto? user)
    {
        CurrentUser = user;
    }
}

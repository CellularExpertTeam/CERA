using CommunityToolkit.Mvvm.Messaging.Messages;
using Defencev1.Models;

namespace Defencev1.Messages;

public class UserProfileChangedMsg : ValueChangedMessage<UserProfile>
{
    public UserProfileChangedMsg(UserProfile user) : base(user)
    {
    }
}

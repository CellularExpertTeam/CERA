using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defencev1.Messages;

public class ProfileToolStatusMsg : ValueChangedMessage<bool>
{
    public ProfileToolStatusMsg(bool status) : base(status)
    {
    }
}

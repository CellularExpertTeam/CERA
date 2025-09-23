using CommunityToolkit.Mvvm.Messaging.Messages;
using Defencev1.Enums;

namespace Defencev1.Messages;

public class ControlPanelToolSelectedMsg : ValueChangedMessage<Tools>
{
    public ControlPanelToolSelectedMsg(Tools tool) : base(tool)
    {
    }
}

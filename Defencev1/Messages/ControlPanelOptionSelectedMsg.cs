using CommunityToolkit.Mvvm.Messaging.Messages;
namespace Defencev1.Messages;

public class ControlPanelOptionSelectedMsg : ValueChangedMessage<string>
{
    public ControlPanelOptionSelectedMsg(string option) : base(option)
    {
    }
}

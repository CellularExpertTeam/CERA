using CommunityToolkit.Mvvm.Messaging.Messages;
using Defencev1.Models;

namespace Defencev1.Messages
{
    public class NewWorkspaceOpenedMsg : ValueChangedMessage<Workspace>
    {
        public NewWorkspaceOpenedMsg(Workspace workspace) : base(workspace)
        {
        }
    }
}

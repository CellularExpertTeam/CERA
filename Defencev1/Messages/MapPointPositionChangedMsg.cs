using CommunityToolkit.Mvvm.Messaging.Messages;
using Esri.ArcGISRuntime.Geometry;

namespace Defencev1.Messages;

public class MapPointPositionChangedMsg : ValueChangedMessage<MapPoint>
{
    public MapPointPositionChangedMsg(MapPoint value) : base(value)
    {
    }
}

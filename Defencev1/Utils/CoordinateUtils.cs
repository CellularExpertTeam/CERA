using Esri.ArcGISRuntime.Geometry;

namespace Defencev1.Utils;

public static class CoordinateUtils
{
    public static Tuple<double, double> GetLatLong(MapPoint point)
    {
        if (point is null)
            return new Tuple<double, double>(0, 0);

        var wgs84 = (MapPoint)GeometryEngine.Project(point, SpatialReferences.Wgs84);
        return new Tuple<double, double>(wgs84.Y, wgs84.X);
    }
    public static async Task<MapPoint> GetUserCoordinates()
    {

        Location location = await Geolocation.GetLastKnownLocationAsync();
        if (location is null)
        {
            return new MapPoint(54.686571, 25.290571);
        }

        return new MapPoint(location.Latitude, location.Longitude);
    }
    public static async Task<bool> EnsureLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }
}

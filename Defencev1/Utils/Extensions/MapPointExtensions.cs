using Esri.ArcGISRuntime.Geometry;

namespace Defencev1.Utils.Extensions;

public static class MapPointExtensions
{
    /// <summary>
    /// Converts a MapPoint's X/Y to WGS84 and returns the latitude.
    /// </summary>
    public static double ToLatitude(this MapPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        // Project to WGS84 if needed
        var wgs84Point = point.SpatialReference != null &&
                         point.SpatialReference != SpatialReferences.Wgs84
            ? (MapPoint)GeometryEngine.Project(point, SpatialReferences.Wgs84)
            : point;

        return wgs84Point.Y; // Latitude
    }

    /// <summary>
    /// Converts a MapPoint's X/Y to WGS84 and returns the longitude.
    /// </summary>
    public static double ToLongitude(this MapPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        var wgs84Point = point.SpatialReference != null &&
                         point.SpatialReference != SpatialReferences.Wgs84
            ? (MapPoint)GeometryEngine.Project(point, SpatialReferences.Wgs84)
            : point;

        return wgs84Point.X; // Longitude
    }
}

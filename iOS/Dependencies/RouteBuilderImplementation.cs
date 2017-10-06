using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using MapKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

[assembly: Dependency(typeof(Libmemo.iOS.Dependencies.RouteBuilderImplementation))]
namespace Libmemo.iOS.Dependencies
{
    public class RouteBuilderImplementation : Libmemo.Dependencies.IRouteBuilder
    {

        public async Task<List<Position>> GetRoute(Position from, Position to)
        {

            MKDirectionsRequest req = new MKDirectionsRequest();
            req.Source = new MKMapItem(new MKPlacemark(new CLLocationCoordinate2D(from.Latitude, from.Longitude), new NSDictionary()));
            req.Destination = new MKMapItem(new MKPlacemark(new CLLocationCoordinate2D(to.Latitude, to.Longitude), new NSDictionary()));
            req.TransportType = MKDirectionsTransportType.Walking;

            var directions = new MKDirections(req);
            MKDirectionsResponse res;
            try
            {
                res = await directions.CalculateDirectionsAsync();
            }
            catch
            {
                return null;
            }

            if (!res.Routes.Any()) return null;

            var pl = res.Routes.First().Polyline;
            return pl.GetCoordinates(0, pl.Points.Length).Select(i => new Position(i.Latitude, i.Longitude)).ToList();
        }

    }

}

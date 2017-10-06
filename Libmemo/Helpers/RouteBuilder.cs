using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo.Helpers
{
    public class RouteBuilder
    {
        public static async Task<List<Position>> GetRoute(Position from, Position to)
        {
            if (Device.RuntimePlatform == Device.Android) {
				var routeData = await TK.CustomMap.Api.Google.GmsDirection.Instance.CalculateRoute(from, to, TK.CustomMap.Api.Google.GmsDirectionTravelMode.Walking);
                if (routeData != null && routeData.Status == TK.CustomMap.Api.Google.GmsDirectionResultStatus.Ok)
                {
                    var r = routeData.Routes.FirstOrDefault();
                    if (r != null && r.Polyline.Positions != null && r.Polyline.Positions.Any())
                    {
                        return r.Polyline.Positions.ToList();
                    }
                }

                return null;
            } else if (Device.RuntimePlatform == Device.iOS) {
                return await DependencyService.Get<Dependencies.IRouteBuilder>().GetRoute(from, to);
            } else {
                throw new NotSupportedException();
            }
        }
    }
}

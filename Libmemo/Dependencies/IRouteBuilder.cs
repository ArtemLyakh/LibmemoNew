using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace Libmemo.Dependencies
{
    public interface IRouteBuilder
    {
        Task<List<Position>> GetRoute(Position from, Position to);
    }
}

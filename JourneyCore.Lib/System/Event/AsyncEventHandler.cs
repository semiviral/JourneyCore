using System;
using System.Threading.Tasks;

namespace JourneyCore.Lib.System.Event
{
    public delegate Task AsyncEventHandler<in T>(object source, T args);
}

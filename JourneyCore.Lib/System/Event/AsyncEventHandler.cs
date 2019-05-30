using System.Threading.Tasks;

namespace JourneyCore.Lib.System.Event
{
    public delegate Task AsyncEventHandler<in TOne>(object source, TOne args);

    public delegate Task AsyncEventHandler<in TOne, in TTwo>(object source, TOne args, TTwo args2);
}
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JourneyCoreLib.System
{
    public interface IStatedObject : INotifyPropertyChanged
    {
        string Guid { get; }

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
    }
}

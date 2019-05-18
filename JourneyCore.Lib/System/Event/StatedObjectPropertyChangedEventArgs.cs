using System.ComponentModel;

namespace JourneyCoreLib.System.Event
{
    public class StatedObjectPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public string Guid { get; }

        public StatedObjectPropertyChangedEventArgs(string guid, string propertyName) : base(propertyName)
        {
            Guid = guid;
        }
    }
}

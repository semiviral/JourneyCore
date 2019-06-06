using System.ComponentModel;

namespace JourneyCore.Lib.System.Event
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
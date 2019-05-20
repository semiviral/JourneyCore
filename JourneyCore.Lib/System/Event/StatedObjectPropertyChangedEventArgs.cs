using System.ComponentModel;

namespace JourneyCore.Lib.System.Event
{
    public class StatedObjectPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public StatedObjectPropertyChangedEventArgs(string guid, string propertyName) : base(propertyName)
        {
            Guid = guid;
        }

        public string Guid { get; }
    }
}
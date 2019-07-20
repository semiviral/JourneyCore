namespace JourneyCore.Lib.System.Event
{
    public class UpdatedProperty
    {
        public UpdatedProperty(string parentId, string propertyName, object value)
        {
            ParentId = parentId;
            PropertyName = propertyName;
            Value = value;
        }

        public string ParentId { get; }
        public string PropertyName { get; }
        public object Value { get; }
    }
}
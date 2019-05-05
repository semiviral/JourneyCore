using JourneyCoreLib.Core.Context.Entities.Attribute;

namespace JourneyCoreLib.Event
{
    public class EntityAttributeUpdatedEventArgs
    {
        public EntityAttribute Attribute { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public EntityAttributeUpdatedEventArgs(EntityAttribute attribute, object oldValue, object newValue)
        {
            Attribute = attribute;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}

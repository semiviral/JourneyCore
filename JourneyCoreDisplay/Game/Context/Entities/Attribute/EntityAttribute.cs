using JourneyCoreLib.Core.Context.Entities.Attribute;
using JourneyCoreLib.Event;
using JourneyCoreLib.Exceptions;
using System;

namespace JourneyCoreLib.Game.Context.Entities.Attribute
{
    public class EntityAttribute
    {
        public string Id { get; }
        public EntityAttributeType Type { get; }
        public object Value {
            get => _value; set {
                if (value.GetType() == _value.GetType())
                {
                    throw new AttributeTypeMismatchException(this, $"Attribute '{value.GetType().AssemblyQualifiedName}' does not match actual type of '{_value.GetType().AssemblyQualifiedName}'");
                }

                EntityAttributeUpdatedEvent?.Invoke(this, new EntityAttributeUpdatedEventArgs(this, _value, value));

                _value = value;
            }
        }
        private object _value;

        public bool IsNative { get; }

        public event EventHandler<EntityAttributeUpdatedEventArgs> EntityAttributeUpdatedEvent;

        public EntityAttribute(EntityAttributeType type, object value, bool isNative = false)
        {
            Id = new Guid().ToString();

            _value = new object();

            Type = type;
            Value = value;
            IsNative = isNative;
        }
    }
}

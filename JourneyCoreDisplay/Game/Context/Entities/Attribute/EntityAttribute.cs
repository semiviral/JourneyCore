using JourneyCoreLib.Exceptions;
using JourneyCoreLib.System.Event;
using System;

namespace JourneyCoreLib.Game.Context.Entities.Attribute
{
    public class EntityAttribute
    {
        public string Id { get; }
        public EntityAttributeType Type { get; }
        public object Value {
            get => _value; set {
                _value = value;

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

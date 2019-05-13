using System;
using JourneyCoreLib.Game.Context.Entities.Attribute;

namespace JourneyCoreLib.Exceptions
{
    public class AttributeTypeMismatchException : Exception
    {
        public EntityAttribute Attribute { get; }

        public AttributeTypeMismatchException(EntityAttribute attribute, string innerException) : base(innerException)
        {
            Attribute = attribute;
        }
    }
}

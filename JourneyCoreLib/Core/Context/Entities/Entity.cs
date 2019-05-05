using System.Collections.Generic;
using System.Linq;
using JourneyCoreLib.Core.Context.Entities.Attribute;

namespace JourneyCoreLib.Core.Context.Entities
{
    public class Entity : Context
    {
        public List<EntityAttribute> EntityAttributes { get; }

        public Entity(Context owner, string name, string primaryTag, params EntityAttribute[] attributes) : base(owner, name, primaryTag)
        {
            EntityAttributes = new List<EntityAttribute>();

            foreach (EntityAttribute attribute in attributes)
            {
                EntityAttributes.Add(attribute);
            }

            InitialiseBasicAttributes();
        }

        public EntityAttribute GetNativeAttribute(EntityAttributeType attributeType)
        {
            return EntityAttributes.FirstOrDefault(attribute => attribute.Type.Equals(attributeType) && attribute.IsNative);
        }

        public IEnumerable<EntityAttribute> GetAllAttributesByName(string attributeName)
        {
            return EntityAttributes.Where(attribute => attribute.Type.Equals(attributeName));
        }

        public EntityAttribute SetNativeAttribute(EntityAttributeType attributeType, object newAttributeValue)
        {
            EntityAttribute targetAttribute = GetNativeAttribute(attributeType);

            if (targetAttribute == null)
            {
                EntityAttributes.Add(new EntityAttribute(attributeType, newAttributeValue, true));
            }
            else
            {
                targetAttribute.Value = newAttributeValue;
            }

            return targetAttribute;
        }

        public float AttributeSum(EntityAttributeType attributeType)
        {
            return EntityAttributes.Where(attribute => attribute.Type == attributeType).Sum(attribute => (float)attribute.Value);
        }

        private void InitialiseBasicAttributes()
        {
            SetNativeAttribute(EntityAttributeType.Strength, 0);
            SetNativeAttribute(EntityAttributeType.Intelligence, 0);
            SetNativeAttribute(EntityAttributeType.Defense, 0);
            SetNativeAttribute(EntityAttributeType.Attack, 0);
            SetNativeAttribute(EntityAttributeType.Dexterity, 0);
            SetNativeAttribute(EntityAttributeType.Speed, 0);
            SetNativeAttribute(EntityAttributeType.Fortitude, 0);
            SetNativeAttribute(EntityAttributeType.Insight, 0);
            SetNativeAttribute(EntityAttributeType.HP, (float)GetNativeAttribute(EntityAttributeType.Strength).Value * 12f);
            SetNativeAttribute(EntityAttributeType.HPRegen, (float)GetNativeAttribute(EntityAttributeType.Fortitude).Value * 0.12f);
            SetNativeAttribute(EntityAttributeType.Mana, (float)GetNativeAttribute(EntityAttributeType.Intelligence).Value * 5f);
            SetNativeAttribute(EntityAttributeType.ManaRegen, (float)GetNativeAttribute(EntityAttributeType.Insight).Value * 0.05f);

            GetNativeAttribute(EntityAttributeType.Strength).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.HP, (float)args.NewValue * 12f); };
            GetNativeAttribute(EntityAttributeType.Insight).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.Mana, (float)args.NewValue * 5f); };
            GetNativeAttribute(EntityAttributeType.Fortitude).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.HPRegen, (float)args.NewValue * 0.12f); };
            GetNativeAttribute(EntityAttributeType.Insight).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.ManaRegen, (float)args.NewValue * 0.05f); };

        }
    }
}

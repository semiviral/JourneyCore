using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCoreLib.Core.Context.Entities.Attribute;
using JourneyCoreLib.Game.Context.Entities.Attribute;
using SFML.Graphics;
using SFML.System;

namespace JourneyCoreLib.Game.Context.Entities
{
    public class Entity : Context
    {
        public Sprite Graphic { get; private set; }
        public EntityView EntityView { get; private set; }
        public List<EntityAttribute> EntityAttributes { get; }

        public event EventHandler<Vector2f> PositionChanged;
        public event EventHandler<float> RotationChanged;

        public Entity(Context owner, WindowManager wManager, string name, string primaryTag, Texture texture, params EntityAttribute[] attributes) : base(owner, name, primaryTag)
        {
            EntityAttributes = new List<EntityAttribute>();

            foreach (EntityAttribute attribute in attributes)
            {
                EntityAttributes.Add(attribute);
            }

            InitialiseSprite(texture);
            InitialiseView(Graphic);
            InitialiseBasicAttributes();

            Graphic.Position = new Vector2f(0, 0);
        }

        private void InitialiseSprite(Texture texture)
        {
            Graphic = new Sprite(texture, new IntRect(0, 0, (int)texture.Size.X, (int)texture.Size.Y))
            {
                Origin = new Vector2f(texture.Size.X / 2, texture.Size.Y / 2),

            };

        }

        private void InitialiseView(Sprite sprite)
        {
            EntityView = new EntityView(sprite.Position, new Vector2f(200f, 200f));

            PositionChanged += (sender, args) =>
            {
                EntityView.SetPosition(args);
            };

            RotationChanged += (sender, args) =>
            {
                EntityView.SetRotation(args);
            };
        }

        #region ATTRIBUTES

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
            //SetNativeAttribute(EntityAttributeType.Strength, 0);
            //SetNativeAttribute(EntityAttributeType.Intelligence, 0);
            //SetNativeAttribute(EntityAttributeType.Defense, 0);
            //SetNativeAttribute(EntityAttributeType.Attack, 0);
            //SetNativeAttribute(EntityAttributeType.Dexterity, 0);
            SetNativeAttribute(EntityAttributeType.Speed, 50);
            //SetNativeAttribute(EntityAttributeType.Fortitude, 0);
            //SetNativeAttribute(EntityAttributeType.Insight, 0);
            //SetNativeAttribute(EntityAttributeType.HP, (float)GetNativeAttribute(EntityAttributeType.Strength).Value * 12f);
            //SetNativeAttribute(EntityAttributeType.HPRegen, (float)GetNativeAttribute(EntityAttributeType.Fortitude).Value * 0.12f);
            //SetNativeAttribute(EntityAttributeType.Mana, (float)GetNativeAttribute(EntityAttributeType.Intelligence).Value * 5f);
            //SetNativeAttribute(EntityAttributeType.ManaRegen, (float)GetNativeAttribute(EntityAttributeType.Insight).Value * 0.05f);

            //GetNativeAttribute(EntityAttributeType.Strength).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.HP, (float)args.NewValue * 12f); };
            //GetNativeAttribute(EntityAttributeType.Insight).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.Mana, (float)args.NewValue * 5f); };
            //GetNativeAttribute(EntityAttributeType.Fortitude).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.HPRegen, (float)args.NewValue * 0.12f); };
            //GetNativeAttribute(EntityAttributeType.Insight).EntityAttributeUpdatedEvent += (sender, args) => { SetNativeAttribute(EntityAttributeType.ManaRegen, (float)args.NewValue * 0.05f); };

        }

        #endregion



        #region MOVEMENT

        private Vector2f GetSpeedModifiedVector(Vector2f vector)
        {
            return vector * ((int)GetNativeAttribute(EntityAttributeType.Speed).Value / 10f);
        }

        public void MoveEntity(Vector2f direction)
        {
            Graphic.Position += GetSpeedModifiedVector(direction) * GameLoop.MapTileSize.X * WindowManager.ElapsedTime; ;
            PositionChanged?.Invoke(this, Graphic.Position);
        }

        public void RotateEntity(float rotation, bool isClockwise)
        {
            if (isClockwise)
            {
                Graphic.Rotation += rotation * WindowManager.ElapsedTime;
            }
            else
            {
                Graphic.Rotation -= rotation * WindowManager.ElapsedTime;
            }

            RotationChanged(this, Graphic.Rotation);
        }

        #endregion
    }
}

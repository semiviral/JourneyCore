using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCoreLib.Core.Context.Entities.Attribute;
using JourneyCoreLib.Drawing;
using JourneyCoreLib.Game.Context.Entities.Attribute;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCoreLib.Core.Context.Entities
{
    public class Entity : Game.Context.Context
    {
        public Sprite Graphic { get; set; }
        private List<Keyboard.Key> RegisteredKeys { get; }

        public List<EntityAttribute> EntityAttributes { get; }
        public Vector2f Movement {
            get => _movement;
            set {
                if (_movement == value)
                {
                    return;
                }

                _movement = value;
                MovementVectorChanged?.Invoke(this, _movement);
            }
        }
        private Vector2f _movement;

        public Entity(Game.Context.Context owner, WindowManager wManager, string name, string primaryTag, Sprite sprite, params EntityAttribute[] attributes) : base(owner, name, primaryTag)
        {
            EntityAttributes = new List<EntityAttribute>();
            Movement = new Vector2f(0f, 0f);
            RegisteredKeys = new List<Keyboard.Key>();
            Graphic = sprite;

            foreach (EntityAttribute attribute in attributes)
            {
                EntityAttributes.Add(attribute);
            }

            InitialiseBasicAttributes();
            InitialiseRendering(wManager);
        }

        private void InitialiseRendering(WindowManager wManager)
        {
            Graphic.Position = new Vector2f(0, 0);

            wManager.KeyPressed += OnKeyPressed;
            wManager.KeyReleased += OnKeyReleased;

            wManager.DrawPersistent(new DrawQueueItem(DrawPriority.Foreground, (fTime, window) =>
            {
                if (Movement != new Vector2f(0f, 0f))
                {
                    Graphic.Position = GetEntityMovement(Graphic.Position, fTime);
                }

                window.Draw(Graphic, new RenderStates(Graphic.Texture));
            }));
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

        public Vector2f AddMovementVector(Vector2f direction)
        {
            Movement += GetSpeedModifiedVector(direction);
            return Movement;
        }

        public Vector2f RemoveMovementVector(Vector2f direction)
        {
            Movement -= GetSpeedModifiedVector(direction);
            return Movement;
        }

        private Vector2f GetSpeedModifiedVector(Vector2f vector)
        {
            return vector * ((int)GetNativeAttribute(EntityAttributeType.Speed).Value / 20);
        }

        public Vector2f GetEntityMovement(Vector2f vector, int frameTime)
        {
            Vector2f speedVector = GetSpeedModifiedVector(Movement);

            vector.X += speedVector.X * (1 / frameTime);
            vector.Y += speedVector.Y * (1 / frameTime);

            return vector;
        }

        private bool IsMovementKey(Keyboard.Key key)
        {
            return key.Equals(Keyboard.Key.W) || key.Equals(Keyboard.Key.A) || key.Equals(Keyboard.Key.S) || key.Equals(Keyboard.Key.D);
        }


        private void RegisterMovmentKey(Keyboard.Key key, bool isPressed)
        {
            switch (key)
            {
                case Keyboard.Key.W:
                    if (isPressed)
                    {
                        if (RegisteredKeys.Contains(Keyboard.Key.W))
                        {
                            break;
                        }

                        RegisteredKeys.Add(Keyboard.Key.W);

                        Movement += new Vector2f(0f, -1f);
                    }
                    else
                    {
                        RegisteredKeys.Remove(Keyboard.Key.W);

                        Movement -= new Vector2f(0f, -1f);
                    }
                    break;
                case Keyboard.Key.A:
                    if (isPressed)
                    {
                        if (RegisteredKeys.Contains(Keyboard.Key.A))
                        {
                            break;
                        }

                        RegisteredKeys.Add(Keyboard.Key.A);

                        Movement += new Vector2f(-1f, 0f);
                    }
                    else
                    {
                        RegisteredKeys.Remove(Keyboard.Key.A);

                        Movement -= new Vector2f(-1f, 0f);
                    }
                    break;
                case Keyboard.Key.S:
                    if (isPressed)
                    {
                        if (RegisteredKeys.Contains(Keyboard.Key.S))
                        {
                            break;
                        }

                        RegisteredKeys.Add(Keyboard.Key.S);

                        Movement += new Vector2f(0f, 1f);
                    }
                    else
                    {
                        RegisteredKeys.Remove(Keyboard.Key.S);

                        Movement -= new Vector2f(0f, 1f);
                    }
                    break;
                case Keyboard.Key.D:
                    if (isPressed)
                    {
                        if (RegisteredKeys.Contains(Keyboard.Key.D))
                        {
                            break;
                        }

                        RegisteredKeys.Add(Keyboard.Key.D);

                        Movement += new Vector2f(1f, 0f);
                    }
                    else
                    {
                        RegisteredKeys.Remove(Keyboard.Key.D);

                        Movement -= new Vector2f(1f, 0f);
                    }
                    break;
            }
        }

        public event EventHandler<Vector2f> MovementVectorChanged;

        #endregion



        public void OnKeyPressed(object sender, KeyEventArgs args)
        {
            if (IsMovementKey(args.Code))
            {
                RegisterMovmentKey(args.Code, true);
            }
        }

        public void OnKeyReleased(object sender, KeyEventArgs args)
        {
            if (IsMovementKey(args.Code))
            {
                RegisterMovmentKey(args.Code, false);
            }
        }
    }
}

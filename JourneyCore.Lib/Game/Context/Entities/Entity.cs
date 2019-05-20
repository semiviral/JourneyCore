using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Event;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Context.Entities
{
    public class Entity : Context, IDisposable
    {
        public Entity(Context owner, string name, string primaryTag, DateTime lifetime, Sprite sprite) : base(owner,
            name, primaryTag)
        {
            Guid = new Guid().ToString();

            Lifetime = lifetime;

            InitialiseSprite(sprite);
            InitialiseDefaultAttributes();

            // TODO set up entity anchors
        }

        public string Guid { get; }

        public Sprite Graphic { get; private set; }
        public DateTime Lifetime { get; }
        public DateTime ProjectileCooldown { get; set; }


        #region DISPOSE

        public void Dispose()
        {
            Graphic.Dispose();
        }

        #endregion

        public event AsyncEventHandler<Vector2f> PositionChanged;
        public event AsyncEventHandler<float> RotationChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public Entity GetProjectile()
        {
            //{
            //    if (DateTime.Now < ProjectileCooldown)
            //    {
            //        return null;
            //    }

            //    ProjectileCooldown = DateTime.Now.AddSeconds(1);

            //    Entity projectile = new Entity(this, "playerProjectile", "projectile", DateTime.Now.AddSeconds(2), GameLoop.Projectiles.GetSprite(0, 0));
            //    projectile.Graphic.Rotation = Graphic.Rotation;
            //    projectile.Graphic.Position = Graphic.Position;
            //    projectile.SetNativeAttribute(EntityAttributeType.Speed, 50);

            //    return projectile;

            return null;
        }


        #region EVENT

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new StatedObjectPropertyChangedEventArgs(Guid, propertyName));
        }

        #endregion

        #region ATTRIBUTES

        public int Strength { get; private set; }
        public int Intelligence { get; private set; }
        public int Defense { get; private set; }
        public int Attack { get; private set; }
        public int Speed { get; private set; }
        public int Dexterity { get; private set; }
        public int Fortitude { get; private set; }
        public int Insight { get; private set; }

        #endregion


        #region INIT

        private void InitialiseSprite(Sprite sprite)
        {
            Graphic = sprite;
            Graphic.Origin = new Vector2f(Graphic.TextureRect.Width / 2, Graphic.TextureRect.Height / 2);
            Graphic.Position = new Vector2f(0f, 0f);
        }


        private void InitialiseDefaultAttributes()
        {
            Strength = Intelligence = Defense = Attack = Speed = Dexterity = Fortitude = Insight = 1;

            Speed = 100;
        }

        #endregion


        #region MOVEMENT

        private Vector2f GetSpeedModifiedVector(Vector2f vector)
        {
            return vector * (Speed / 5f);
        }

        public void Move(Vector2f direction, int mapTileSize, float elapsedFrameTime)
        {
            Graphic.Position += GetSpeedModifiedVector(direction) * mapTileSize * elapsedFrameTime;
            PositionChanged?.Invoke(this, Graphic.Position);
        }

        public async Task RotateEntity(float elapsedTime, float rotation, bool isClockwise)
        {
            rotation *= elapsedTime;

            if (!isClockwise) rotation *= -1;

            if (Graphic.Rotation + rotation > 360)
                rotation -= 360;
            else if (Graphic.Rotation + rotation < 0) rotation += 360;

            Graphic.Rotation += rotation;

            if (RotationChanged == null) return;

            await RotationChanged?.Invoke(this, Graphic.Rotation);
        }

        #endregion
    }
}
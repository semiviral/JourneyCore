using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Event;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Context.Entities
{
    public class Entity : IDisposable, INotifyPropertyChanged
    {
        public Entity(Texture projectilesTexture, int lifetime, Sprite sprite)
        {
            Guid = new Guid().ToString();

            Lifetime = lifetime;
            ProjectileCooldown = DateTime.MinValue;
            ProjectilesTexture = projectilesTexture;

            InitialiseSprite(sprite);
            InitialiseDefaultAttributes();

            MinimapObject = new Transformable();

            // TODO set up entity anchors
        }

        public string Guid { get; }

        public Sprite Graphic { get; private set; }
        public int Lifetime { get; }
        public DateTime ProjectileCooldown { get; set; }
        public Texture ProjectilesTexture { get; set; }
        public Transformable MinimapObject { get; set; }


        #region DISPOSE

        public void Dispose()
        {
            Graphic.Dispose();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public event AsyncEventHandler<Vector2f> PositionChanged;
        public event AsyncEventHandler<float> RotationChanged;


        #region EVENT

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new StatedObjectPropertyChangedEventArgs(Guid, propertyName));
        }

        #endregion

        public Vertex[] GetVertices()
        {
            Vertex[] vertices = new Vertex[4];
            int pixelRadiusX = Graphic.TextureRect.Width / 2;
            int pixelRadiusY = Graphic.TextureRect.Height / 2;

            vertices[0] = new Vertex(new Vector2f(Graphic.Position.X - pixelRadiusX, Graphic.Position.Y + pixelRadiusY),
                new Vector2f(Graphic.TextureRect.Left, Graphic.TextureRect.Top));
            vertices[1] = new Vertex(new Vector2f(Graphic.Position.X + pixelRadiusX, Graphic.Position.Y + pixelRadiusY),
                new Vector2f(Graphic.TextureRect.Left + Graphic.TextureRect.Width, Graphic.TextureRect.Top));
            vertices[2] = new Vertex(new Vector2f(Graphic.Position.X + pixelRadiusX, Graphic.Position.Y - pixelRadiusY),
                new Vector2f(Graphic.TextureRect.Left + Graphic.TextureRect.Width,
                    Graphic.TextureRect.Top + Graphic.TextureRect.Height));
            vertices[3] = new Vertex(new Vector2f(Graphic.Position.X - pixelRadiusX, Graphic.Position.Y - pixelRadiusY),
                new Vector2f(Graphic.TextureRect.Left, Graphic.TextureRect.Top + Graphic.TextureRect.Height));

            return vertices;
        }


        public DrawItem GetFiredProjectile(double centerRelativeMouseX, double centerRelativeMouseY, int tileWidth)
        {
            if (DateTime.Now < ProjectileCooldown)
            {
                return null;
            }

            double angle = 180 / Math.PI * Math.Atan2(centerRelativeMouseY, centerRelativeMouseX) + Graphic.Rotation +
                           90d;

            ProjectileCooldown = DateTime.Now.AddMilliseconds(100);

            Entity projectile = new Entity(null, 2000,
                new Sprite(ProjectilesTexture, new IntRect(0, 0, 8, 8)))
            {
                Graphic = { Rotation = (float)angle, Position = Graphic.Position },
                Speed = 75
            };

            DrawItem projectileDrawItem = new DrawItem(projectile.Guid, projectile.Lifetime, (window, frameTime) =>
            {
                Vector2f movement = new Vector2f((float)GraphMath.SinFromDegrees(projectile.Graphic.Rotation),
                    (float)GraphMath.CosFromDegrees(projectile.Graphic.Rotation) * -1f);
                projectile.Move(movement, tileWidth, frameTime);

                window.Draw(projectile.Graphic);
            });

            return projectileDrawItem;
        }

        #region ATTRIBUTES

        public int Strength
        {
            get => strength;
            set
            {
                if (strength == value)
                {
                    return;
                }

                strength = value;

                NotifyPropertyChanged();
            }
        }

        private int strength;
        public int Intelligence { get; set; }
        public int Defense { get; set; }
        public int Attack { get; set; }
        public int Speed { get; set; }
        public int Dexterity { get; set; }
        public int Fortitude { get; set; }
        public int Insight { get; set; }

        public float CurrentHP
        {
            get => currentHp;
            set
            {
                if (Math.Abs(currentHp - value) < 0.01)
                {
                    return;
                }

                currentHp = value;

                NotifyPropertyChanged();
            }
        }

        private float currentHp;

        #endregion


        #region INIT

        private void InitialiseSprite(Sprite sprite)
        {
            Graphic = sprite;
            Graphic.Origin = new Vector2f(Graphic.TextureRect.Width / 2f, Graphic.TextureRect.Height / 2f);
            Graphic.Position = new Vector2f(0f, 0f);
        }


        private void InitialiseDefaultAttributes()
        {
            Strength = Intelligence = Defense = Attack = Speed = Dexterity = Fortitude = Insight = 1;

            Strength = 20;

            CurrentHP = Strength;

            Speed = 100;
        }

        #endregion


        #region MOVEMENT

        private Vector2f GetSpeedModifiedVector(Vector2f vector)
        {
            return vector * (Speed / 10f);
        }

        public void Move(Vector2f direction, int mapTileSize, float elapsedFrameTime)
        {
            Graphic.Position += GetSpeedModifiedVector(direction) * mapTileSize * elapsedFrameTime;

            MinimapObject.Position = Graphic.Position;

            PositionChanged?.Invoke(this, Graphic.Position);
        }

        public async Task RotateEntity(float elapsedTime, float rotation, bool isClockwise)
        {
            rotation *= elapsedTime;

            if (!isClockwise)
            {
                rotation *= -1;
            }

            if (Graphic.Rotation + rotation > 360)
            {
                rotation -= 360;
            }
            else if (Graphic.Rotation + rotation < 0)
            {
                rotation += 360;
            }

            Graphic.Rotation += rotation;

            if (RotationChanged == null)
            {
                return;
            }

            MinimapObject.Rotation = Graphic.Rotation;

            await RotationChanged?.Invoke(this, Graphic.Rotation);
        }

        #endregion
    }
}
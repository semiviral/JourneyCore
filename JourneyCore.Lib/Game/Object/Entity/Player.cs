using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JourneyCore.Lib.Display.Drawing;
using JourneyCore.Lib.System.Event;
using JourneyCore.Lib.System.Loaders;
using JourneyCore.Lib.System.Math;
using JourneyCore.Lib.System.Static;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public class Player : IEntity, IEntityLiving, IEntityAttacker, IAnchor
    {
        public Player(Sprite graphic, Texture projectilesTexture, long lifetime)
        {
            Guid = global::System.Guid.NewGuid().ToString();

            Lifetime = lifetime;
            ProjectileCooldown = DateTime.MinValue;
            ProjectileRenderStates = new RenderStates(projectilesTexture);
            CurrentChunk = new Vector2f(0f, 0f);

            InitialiseSprite(graphic);
            InitialiseDefaultAttributes();

            // todo set up inventory stat stuff

            AttackCooldownValue = 100;

            PositionChanged += CheckChunkChanged;
        }

        public DrawItem FireProjectile(double centerRelativeMouseX, double centerRelativeMouseY, int tileWidth)
        {
            if (!CanAttack)
            {
                return null;
            }

            ProjectileCooldown = DateTime.Now.AddMilliseconds(AttackCooldownValue);

            double angle = (180 / Math.PI * Math.Atan2(centerRelativeMouseY, centerRelativeMouseX) + Graphic.Rotation +
                            DrawView.DefaultPlayerViewRotation + 90d) % 360;

            Projectile projectile = new Projectile(
                new Sprite(ProjectileRenderStates.Texture, new IntRect(0, 0, 32, 32)),
                25, 1000);
            projectile.Graphic.Origin = new Vector2f(projectile.Graphic.TextureRect.Width / 2f,
                projectile.Graphic.TextureRect.Height / 2f);
            projectile.Graphic.Position = Graphic.Position;
            projectile.Graphic.Rotation = (float)angle + 180f % 360;
            projectile.Graphic.Scale = new Vector2f(0.35f, 0.35f);

            DrawItem projectileDrawItem = new DrawItem(projectile.TriggerAlive(), frameTime =>
                {
                    Vector2f movement = new Vector2f((float)GraphMath.SinFromDegrees(angle),
                        (float)GraphMath.CosFromDegrees(angle) * -1f);

                    projectile.MoveEntity(movement, tileWidth, frameTime);
                }, new DrawObject(projectile.Graphic, projectile.Graphic.GetVertices),
                ProjectileRenderStates);

            return projectileDrawItem;
        }

        #region VARIABLES

        private DateTime ProjectileCooldown { get; set; }

        public string Guid { get; }
        public Sprite Graphic { get; private set; }
        public long Lifetime { get; }
        public int AttackCooldownValue { get; }
        public bool CanAttack => ProjectileCooldown < DateTime.Now;
        public RenderStates ProjectileRenderStates { get; }
        public Vector2f CurrentChunk { get; set; }

        public Vector2f Position
        {
            get => Graphic.Position;
            set
            {
                if (Graphic.Position == value)
                {
                    return;
                }

                Graphic.Position = value;

                NotifyPropertyChanged();
                PositionChanged?.Invoke(this, Graphic.Position);
            }
        }

        public float Rotation {
            get => Graphic.Rotation;
            set {
                if (Math.Abs(Graphic.Rotation - value) < 0.0001)
                {
                    return;
                }

                Graphic.Rotation = value;

                NotifyPropertyChanged();
                RotationChanged?.Invoke(this, Graphic.Rotation);
            }
        }

        #endregion


        #region ATTRIBUTES

        public int Strength
        {
            get => _Strength;
            set
            {
                if (_Strength == value)
                {
                    return;
                }

                _Strength = value;

                NotifyPropertyChanged();
            }
        }

        private int _Strength;
        public int Intelligence { get; set; }
        public int Defense { get; set; }
        public int Attack { get; set; }
        public int Speed { get; set; }
        public int Dexterity { get; set; }
        public int Fortitude { get; set; }
        public int Insight { get; set; }

        public double CurrentHp
        {
            get => _CurrentHp;
            set
            {
                if (Math.Abs(_CurrentHp - value) < 0.01)
                {
                    return;
                }

                _CurrentHp = value;

                NotifyPropertyChanged();
            }
        }

        private double _CurrentHp;

        #endregion


        #region EVENT

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Vector2f> PositionChanged;
        public event EventHandler<float> RotationChanged;
        public event EventHandler<Vector2f> ChunkChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new StatedObjectPropertyChangedEventArgs(Guid, propertyName));
        }

        private void CheckChunkChanged(object sender, Vector2f newPosition)
        {
            Vector2f chunkPosition = new Vector2f((int)newPosition.X / MapLoader.ChunkSize,
                (int)newPosition.Y / MapLoader.ChunkSize);

            if (!(Math.Abs(CurrentChunk.X - chunkPosition.X) > 1) && !(Math.Abs(CurrentChunk.Y - chunkPosition.Y) > 1))
            {
                return;
            }

            // rounds float values towards zero, ensuring remainders are dropped
            CurrentChunk = new Vector2f((int)chunkPosition.X, (int)chunkPosition.Y);

            ChunkChanged?.Invoke(sender, CurrentChunk);
        }

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

            CurrentHp = Strength;

            Speed = 100;
        }

        #endregion


        #region MOVEMENT

        public void MoveEntity(Vector2f direction, int mapTileSize, float elapsedFrameTime)
        {
            Position = Graphic.TryMovement(direction, Speed, mapTileSize, elapsedFrameTime);
        }

        public void RotateEntity(float rotation, float elapsedTime, bool isClockwise)
        {
            Rotation = Graphic.TryRotation(rotation, elapsedTime, isClockwise);
        }

        public void AnchorItem(IAnchorable anchorableItem)
        {
            AnchorItemPosition(anchorableItem);
            AnchorItemRotation(anchorableItem);
        }

        public void AnchorItemPosition(IAnchorable anchorableItem)
        {
            anchorableItem.Position = Graphic.Position;

            PositionChanged += (sender, position) => { anchorableItem.Position = position; };
        }

        public void AnchorItemRotation(IAnchorable anchorableItem)
        {
            anchorableItem.Rotation = Graphic.Rotation;

            RotationChanged += (sender, rotation) => { anchorableItem.Rotation = rotation; };
        }

        #endregion
    }
}
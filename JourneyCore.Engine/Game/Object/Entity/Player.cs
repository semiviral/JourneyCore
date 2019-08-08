using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JourneyCore.Lib.Display.Drawing;
using JourneyCore.Lib.Game.Object.Collision;
using JourneyCore.Lib.System.Event;
using JourneyCore.Lib.System.Loaders;
using JourneyCore.Lib.System.Math;
using JourneyCore.Lib.System.Static;
using Newtonsoft.Json;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public class Player : IEntity, IEntityLiving, IEntityAttacker, IAnchor
    {
        public Player(byte[] humanAvatarTextureBytes, byte[] projectilesTextureBytes, long lifetime)
        {
            Guid = global::System.Guid.NewGuid().ToString();

            Lifetime = lifetime;
            ProjectileCooldown = DateTime.MinValue;
            ProjectileTextureBytes = projectilesTextureBytes;
            CurrentChunk = new Vector2f(0f, 0f);

            AvatarTextureBytes = humanAvatarTextureBytes;
            Graphic = new Sprite();

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

            double _angle = ((((180 / Math.PI) * Math.Atan2(centerRelativeMouseY, centerRelativeMouseX)) + Graphic.Rotation) - 90d) % 360;

            Projectile _projectile = new Projectile(
                new Sprite(ProjectileRenderStates.Texture, new IntRect(0, 0, 32, 32)),
                25, 1000);
            _projectile.Graphic.Origin = new Vector2f(_projectile.Graphic.TextureRect.Width / 2f,
                _projectile.Graphic.TextureRect.Height / 2f);
            _projectile.Graphic.Position = Graphic.Position;
            _projectile.Graphic.Rotation = (float) _angle;
            _projectile.Graphic.Scale = new Vector2f(0.35f, 0.35f);

            DrawItem _projectileDrawItem = new DrawItem(
                new DrawObject(_projectile.Graphic, _projectile.Graphic.GetVertices), ProjectileRenderStates, frameTime =>
                {
                    Vector2f _movement = new Vector2f((float) GraphMath.SinFromDegrees(_angle),
                        (float) GraphMath.CosFromDegrees(_angle) * -1f);

                    _projectile.MoveEntity(_movement, tileWidth, frameTime);
                }, _projectile.TriggerAlive());

            return _projectileDrawItem;
        }

        #region VARIABLES

        private double _CurrentHp;
        private DateTime ProjectileCooldown { get; set; }

        public string Guid { get; }
        [JsonIgnore] public Sprite Graphic { get; private set; }
        public byte[] AvatarTextureBytes { get; set; }
        public byte[] ProjectileTextureBytes { get; set; }
        [JsonIgnore] public RenderStates ProjectileRenderStates { get; private set; }
        public long Lifetime { get; }
        public int AttackCooldownValue { get; }
        public bool CanAttack => ProjectileCooldown < DateTime.Now;
        public Vector2f CurrentChunk { get; set; }
        public CollisionQuad Collider { get; set; }

        public GetCollisionAdjustments GetCollisionAdjustments { get; set; }


        public Vector2f Position
        {
            get => Graphic?.Position ?? new Vector2f(0f, 0f);
            set
            {
                if (Graphic == null)
                {
                    return;
                }

                if (Graphic.Position == value)
                {
                    return;
                }

                Vector2f _oldPosition = new Vector2f(Graphic.Position.X, Graphic.Position.Y);

                Collider.Position = value;

                if (GetCollisionAdjustments != null)
                {
                    value += GetCollisionAdjustments(Collider).Sum();
                }

                Graphic.Position = value;

                NotifyPropertyChanged();
                PositionChanged?.Invoke(this, new EntityPositionChangedEventArgs(_oldPosition, Graphic.Position));
            }
        }

        public float Rotation
        {
            get => Graphic?.Rotation ?? 0f;
            set
            {
                if (Graphic == null)
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

        #endregion


        #region EVENT

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EntityPositionChangedEventArgs> PositionChanged;
        public event EventHandler<float> RotationChanged;
        public event EventHandler<Vector2f> ChunkChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new StatedObjectPropertyChangedEventArgs(Guid, propertyName));
        }

        private void CheckChunkChanged(object sender, EntityPositionChangedEventArgs args)
        {
            if (((args.NewPosition.X - args.OldPosition.X) < 16) && ((args.NewPosition.Y - args.OldPosition.Y) < 16))
            {
                return;
            }

            Vector2f _chunkPosition = new Vector2f(args.NewPosition.X / MapLoader.ChunkSize, args.NewPosition.Y / MapLoader.ChunkSize);

            // rounds float values towards zero, ensuring remainders are dropped
            CurrentChunk = new Vector2f((int) _chunkPosition.X, (int) _chunkPosition.Y);

            ChunkChanged?.Invoke(sender, CurrentChunk);
        }

        #endregion


        #region INIT

        public void ClientSideInitialise(Vector2i spriteSize)
        {
            InitialiseGraphics(spriteSize);
            TryInitialiseCollider();
        }

        private void InitialiseGraphics(Vector2i spriteSize)
        {
            Graphic = new Sprite(new Texture(AvatarTextureBytes), new IntRect(3 * spriteSize.X, 1 * spriteSize.Y, spriteSize.X, spriteSize.Y))
            {
                Scale = new Vector2f(0.5f, 0.5f)
            };
            Graphic.Origin = new Vector2f(Graphic.TextureRect.Width, Graphic.TextureRect.Height) / 2f;

            ProjectileRenderStates = new RenderStates(new Texture(ProjectileTextureBytes));
        }

        private void TryInitialiseCollider()
        {
            if (Graphic == null)
            {
                return;
            }

            Collider.Origin = Collider.Size / 2f;
            Collider.Scale = Graphic.Scale;
            Collider.Mobile = true;
            
            AnchorItem(Collider);
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
            anchorableItem.OnAnchorPositionChanged(this, new EntityPositionChangedEventArgs(Position, Position));

            PositionChanged += anchorableItem.OnAnchorPositionChanged;
        }

        public void AnchorItemRotation(IAnchorable anchorableItem)
        {
            anchorableItem.OnAnchorRotationChanged(this, Graphic.Rotation);

            RotationChanged += anchorableItem.OnAnchorRotationChanged;
        }

        #endregion
    }
}
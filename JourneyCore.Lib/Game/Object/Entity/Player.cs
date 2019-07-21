using System;
using System.ComponentModel;
using System.Linq;
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

            double angle = (((180 / Math.PI) * Math.Atan2(centerRelativeMouseY, centerRelativeMouseX)) +
                            Graphic.Rotation +
                            DrawView.DefaultPlayerViewRotation + 90d) % 360;

            Projectile projectile = new Projectile(
                new Sprite(ProjectileRenderStates.Texture, new IntRect(0, 0, 32, 32)),
                25, 1000);
            projectile.Graphic.Origin = new Vector2f(projectile.Graphic.TextureRect.Width / 2f,
                projectile.Graphic.TextureRect.Height / 2f);
            projectile.Graphic.Position = Graphic.Position;
            projectile.Graphic.Rotation = (float) angle + (180f % 360);
            projectile.Graphic.Scale = new Vector2f(0.35f, 0.35f);

            DrawItem projectileDrawItem = new DrawItem(
                new DrawObject(projectile.Graphic, projectile.Graphic.GetVertices), ProjectileRenderStates, frameTime =>
                {
                    Vector2f movement = new Vector2f((float) GraphMath.SinFromDegrees(angle),
                        (float) GraphMath.CosFromDegrees(angle) * -1f);

                    projectile.MoveEntity(movement, tileWidth, frameTime);
                }, projectile.TriggerAlive());

            return projectileDrawItem;
        }

        public void PositionModify(Vector2f offset)
        {
            Position += offset;
        }

        #region VARIABLES

        private double _CurrentHp;
        private CollisionQuad _Collider;
        private DateTime ProjectileCooldown { get; set; }
        private CollisionQuad _DummyCollider { get; set; }

        public string Guid { get; }
        [JsonIgnore] public Sprite Graphic { get; private set; }
        public byte[] AvatarTextureBytes { get; set; }
        public byte[] ProjectileTextureBytes { get; set; }
        [JsonIgnore] public RenderStates ProjectileRenderStates { get; private set; }
        public long Lifetime { get; }
        public int AttackCooldownValue { get; }
        public bool CanAttack => ProjectileCooldown < DateTime.Now;
        public Vector2f CurrentChunk { get; set; }

        public CollisionQuad Collider
        {
            get => _Collider;
            set
            {
                _Collider = value;

                TryInitialiseCollider();
            }
        }

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

                Vector2f oldPosition = new Vector2f(Graphic.Position.X, Graphic.Position.Y);

                _DummyCollider.Position = value;

                if (GetCollisionAdjustments != null)
                {
                    value = GetCollisionAdjustments(_DummyCollider)
                        .Aggregate(value, (current, adjustment) => current + adjustment);
                }

                Graphic.Position = value;

                NotifyPropertyChanged();
                PositionChanged?.Invoke(this, new EntityPositionChangedEventArgs(oldPosition, Graphic.Position));
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

                if (Graphic.Rotation == value)
                {
                    return;
                }

                _DummyCollider.Rotation = value;
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

            Vector2f chunkPosition = new Vector2f((int) args.NewPosition.X / MapLoader.ChunkSize,
                (int) args.NewPosition.Y / MapLoader.ChunkSize);

            // rounds float values towards zero, ensuring remainders are dropped
            CurrentChunk = new Vector2f((int) chunkPosition.X, (int) chunkPosition.Y);

            ChunkChanged?.Invoke(sender, CurrentChunk);
        }

        #endregion


        #region INIT

        public void ClientSideInitialise()
        {
            InitialiseGraphics();
            TryInitialiseCollider();
        }

        private void InitialiseGraphics()
        {
            Graphic = new Sprite(new Texture(AvatarTextureBytes), new IntRect(3 * 64, 1 * 64, 64, 64))
            {
                Scale = new Vector2f(0.5f, 0.5f)
            };
            Graphic.Origin = new Vector2f(Graphic.TextureRect.Width / 2f, Graphic.TextureRect.Height / 2f);
            Graphic.Position = new Vector2f(0f, 0f);

            Collider.Scale = Graphic.Scale;

            ProjectileRenderStates = new RenderStates(new Texture(ProjectileTextureBytes));
        }

        private void TryInitialiseCollider()
        {
            if (Graphic == null)
            {
                return;
            }

            _Collider.Scale = Graphic.Scale;
            _Collider.Mobile = true;
            _Collider.Colliding += (sender, args) => { Position += args; };
            AnchorItem(_Collider, _Collider.Origin - _Collider.Position);

            _DummyCollider = new CollisionQuad(_Collider);
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
            AnchorItem(anchorableItem, new Vector2f(0f, 0f));
        }

        public void AnchorItem(IAnchorable anchorableItem, Vector2f positionOffset)
        {
            AnchorItemPosition(anchorableItem, positionOffset);
            AnchorItemRotation(anchorableItem);
        }

        public void AnchorItemPosition(IAnchorable anchorableItem, Vector2f positionOffset)
        {
            anchorableItem.Position = Graphic.Position + positionOffset;

            PositionChanged += (sender, args) => { anchorableItem.Position = args.NewPosition + positionOffset; };
        }

        public void AnchorItemRotation(IAnchorable anchorableItem)
        {
            anchorableItem.Rotation = Graphic.Rotation;

            RotationChanged += (sender, rotation) => { anchorableItem.Rotation = rotation; };
        }

        #endregion
    }
}
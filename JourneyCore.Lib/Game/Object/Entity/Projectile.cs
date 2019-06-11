using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public class Projectile : IEntity, IEntityTemporary
    {
        public int Speed { get; }

        public Projectile(Sprite graphic, int speed, long lifetime = 0, string guid = "")
        {
            Graphic = graphic;
            Speed = speed;
            Lifetime = lifetime;
            MaximumLifetime = DateTime.MinValue;
            Guid = string.IsNullOrWhiteSpace(guid) ? global::System.Guid.NewGuid().ToString() : guid;
        }

        public string Guid { get; }
        public Sprite Graphic { get; }
        public long Lifetime { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void MoveEntity(Vector2f direction, int mapTileSize, float elapsedFrameTime)
        {
            Graphic.Move(direction, Speed, mapTileSize, elapsedFrameTime);
        }

        public void RotateEntity(float elapsedTime, float rotation, bool isClockwise)
        {
            Graphic.Rotate(rotation, elapsedTime, isClockwise);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DateTime MaximumLifetime { get; private set; }

        public DateTime TriggerAlive()
        {
            return MaximumLifetime = DateTime.Now.AddMilliseconds(Lifetime);
        }
    }
}
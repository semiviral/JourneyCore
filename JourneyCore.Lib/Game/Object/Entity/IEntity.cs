using System.ComponentModel;
using System.Runtime.CompilerServices;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public interface IEntity : INotifyPropertyChanged
    {
        string Guid { get; }
        Sprite Graphic { get; }
        long Lifetime { get; }

        void MoveEntity(Vector2f direction, int mapTileSize, float elapsedFrameTime);
        void RotateEntity(float elapsedTime, float rotation, bool isClockwise);
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
    }
}
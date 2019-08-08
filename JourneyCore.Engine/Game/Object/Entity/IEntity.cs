using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JourneyCore.Lib.Game.Object.Collision;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public delegate IEnumerable<Vector2f> GetCollisionAdjustments(CollisionQuad subjectQuad);

    public interface IEntity : INotifyPropertyChanged
    {
        string Guid { get; }
        Sprite Graphic { get; }
        long Lifetime { get; }
        Vector2f Position { get; set; }
        float Rotation { get; set; }
        CollisionQuad Collider { get; set; }
        GetCollisionAdjustments GetCollisionAdjustments { get; set; }

        void MoveEntity(Vector2f direction, int mapTileSize, float elapsedFrameTime);
        void RotateEntity(float elapsedTime, float rotation, bool isClockwise);
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
    }
}
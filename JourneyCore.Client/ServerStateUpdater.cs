using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using SFML.System;

namespace JourneyCore.Client
{
    public class ServerStateUpdater
    {
        private readonly object _MovementLock;
        private readonly GameServerConnection _NetManager;
        private readonly object _RotationsLock;
        private Queue<Vector2f> _Movements;
        private Queue<float> _Rotations;


        public ServerStateUpdater(GameServerConnection netManager, int tickRate)
        {
            _MovementLock = new object();
            Positions = new Queue<Vector2f>();
            _RotationsLock = new object();
            Rotations = new Queue<float>();
            _NetManager = netManager;

            ServerTickClock = new AutoResetTimer(tickRate);
            ServerTickClock.ElapsedAsync += DeallocatePositions;
            ServerTickClock.ElapsedAsync += DeallocateRotations;
        }

        public Queue<Vector2f> Positions
        {
            get
            {
                lock (_MovementLock)
                {
                    return _Movements;
                }
            }
            private set
            {
                lock (_MovementLock)
                {
                    _Movements = value;
                }
            }
        }

        public Queue<float> Rotations
        {
            get => _Rotations;
            private set
            {
                lock (_RotationsLock)
                {
                    _Rotations = value;
                }
            }
        }

        public AutoResetTimer ServerTickClock { get; }

        private async Task DeallocatePositions(object sender, float args)
        {
            if (Positions.Count <= 0)
            {
                return;
            }

            List<Vector2f> positions = new List<Vector2f>();

            while (Positions.Count > 0)
            {
                positions.Add(Positions.Dequeue());
            }

            await _NetManager.Connection.InvokeAsync("ReceivePlayerPositions", positions);
        }


        private async Task DeallocateRotations(object sender, float args)
        {
            if (Rotations.Count <= 0)
            {
                return;
            }

            List<float> rotations = new List<float>();

            while (Rotations.Count > 0)
            {
                rotations.Add(Rotations.Dequeue());
            }

            //await _NetManager.Connection.InvokeAsync("ReceivePlayerRotations", rotations);
        }
    }
}
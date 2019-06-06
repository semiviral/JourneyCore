using System;
using System.Collections.Generic;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Event;

namespace JourneyCore.Server
{
    public class Instance
    {
        public Instance()
        {
            Id = Guid.NewGuid().ToString();

            Entities = new List<Player>();
            UpdatedProperties = new List<UpdatedProperty>();
        }

        public string Id { get; }

        public List<Player> Entities { get; }
        public List<UpdatedProperty> UpdatedProperties { get; }
    }
}
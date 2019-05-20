using System;
using System.Collections.Generic;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.System.Event;

namespace JourneyCore.Server
{
    public class Instance
    {
        public Instance()
        {
            Id = Guid.NewGuid().ToString();

            Entities = new List<Entity>();
            UpdatedProperties = new List<UpdatedProperty>();
        }

        public string Id { get; }

        public List<Entity> Entities { get; }
        public List<UpdatedProperty> UpdatedProperties { get; }
    }
}
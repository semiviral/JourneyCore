using System;
using System.Collections.Generic;
using JourneyCore.Lib.System.Event;
using JourneyCoreLib.Game.Context.Entities;

namespace JourneyCore.Server
{
    public class Instance
    {
        public string Id { get; }

        public List<Entity> Entities { get; }
        public List<UpdatedProperty> UpdatedProperties { get; }

        public Instance()
        {
            Id = Guid.NewGuid().ToString();

            Entities = new List<Entity>();
            UpdatedProperties = new List<UpdatedProperty>();
        }
    }
}

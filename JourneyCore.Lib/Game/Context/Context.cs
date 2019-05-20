using System.Collections.Generic;

namespace JourneyCore.Lib.Game.Context
{
    public class Context
    {
        public Context(Context owner, string name, string primaryTag, params string[] tags)
        {
            Owner = owner;
            Name = name;
            PrimaryTag = primaryTag;

            Tags = new List<string>
            {
                PrimaryTag
            };
            Tags.AddRange(tags);

            Initialised = owner != null && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(PrimaryTag);
        }

        public Context Owner { get; }
        public string Name { get; }
        public string PrimaryTag { get; }
        public List<string> Tags { get; }
        public bool Initialised { get; set; }
    }
}
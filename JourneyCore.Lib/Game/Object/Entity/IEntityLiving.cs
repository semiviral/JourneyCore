namespace JourneyCore.Lib.Game.Object.Entity
{
    public interface IEntityLiving
    {
        // todo implement some sort of automatic minimap draw object creation

        double CurrentHp { get; set; }
        int Strength { get; set; }
        int Intelligence { get; set; }
        int Defense { get; set; }
        int Attack { get; set; }
        int Speed { get; set; }
        int Dexterity { get; set; }
        int Fortitude { get; set; }
        int Insight { get; set; }
    }
}
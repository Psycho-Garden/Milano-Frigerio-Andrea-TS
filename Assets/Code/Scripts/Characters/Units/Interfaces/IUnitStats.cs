namespace AF.TS.Characters
{
    public interface IUnitStats : IUnitCommon
    {
        float GetStat(string name);
        void SetStat(string name, float value);
    }

}

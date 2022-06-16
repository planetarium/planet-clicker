namespace _Script.Data
{
    public interface ITable<TRow> where TRow : new()
    {
        void Load(string filename);
    }
}

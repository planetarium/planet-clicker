using System.Collections.Generic;

namespace Scripts.Data
{
    public interface IRow
    {
        public int Id { get; set; }

        public void Load(List<string> headerInfo, List<string> words);
    }
}

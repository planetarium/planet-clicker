using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace Scripts.Data
{
    public class Table<T> : Dictionary<int, T>, ITable<T>
        where T : IRow, new()
    {
        public void Load(string text)
        {
            ImmutableList<string> lines = text.Split('\n').ToImmutableList();
            List<string> headerInfo = GetHeaderInfo(lines[0]).ToList();

            foreach (var line in lines.Skip(1).Where(x => !string.IsNullOrEmpty(x)))
            {
                T row = new T();
                row.Load(headerInfo, line.Trim().Split(',').ToList());

                Add(row.Id, row);
            }
        }

        private IEnumerable<string> GetHeaderInfo(string line)
        {
            return line.Trim().Split(',').Select(x => x.ToLower().Replace(" ", "_"));
        }
    }
}

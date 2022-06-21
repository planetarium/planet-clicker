using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Scripts.Data
{
    public class Level : IRow
    {
        public Level()
        {
        }

        public int Id { get; set; } = 0;
        public long Exp { get; set; } = 0;

        public void Load(List<string> headerInfo, List<string> words)
        {
            if (headerInfo.Count != words.Count)
            {
                throw new ArgumentException(
                    $"Lengths of {nameof(headerInfo)} and {nameof(words)} should match.");
            }

            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                int index = headerInfo.FindIndex(x => x == propertyInfo.Name.ToLower());
                if (index < 0)
                {
                    throw new ArgumentException("Invalid header info.");
                }

                Type type = propertyInfo.PropertyType;
                string word = words[index];

                if (type == typeof(bool))
                {
                    propertyInfo.SetValue(this, bool.Parse(word));
                }
                else if (type == typeof(int))
                {
                    propertyInfo.SetValue(this, int.Parse(word));
                }
                else if (type == typeof(long))
                {
                    propertyInfo.SetValue(this, long.Parse(word));
                }
                else if (type == typeof(float))
                {
                    propertyInfo.SetValue(this, float.Parse(word));
                }
                else if (type == typeof(string))
                {
                    propertyInfo.SetValue(this, word);
                }
                else if (type == typeof(decimal))
                {
                    propertyInfo.SetValue(this, decimal.Parse(word));
                }
                else
                {
                    throw new NotSupportedException($"Type {type} is not supported.");
                }
            }
        }
    }
}

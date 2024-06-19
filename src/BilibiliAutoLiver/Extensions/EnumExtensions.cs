using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BilibiliAutoLiver.Extensions
{
    public class EnumExtensions
    {
        public static Dictionary<int, string> GetEnumDescriptions<T>(params int[] except) where T : Enum
        {
            var enumDescriptions = new Dictionary<int, string>();

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                string description = GetEnumDescription(value);
                int key = Convert.ToInt32(value);
                if (except != null && except.Length > 0 && except.Contains(key))
                {
                    continue;
                }
                enumDescriptions.Add(Convert.ToInt32(value), description);
            }

            return enumDescriptions;
        }

        public static string GetEnumDescription<T>(T value) where T : Enum
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}

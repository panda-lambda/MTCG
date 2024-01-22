﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Utilities
{
    public class EnumHelper
    {
        public static TEnum? GetEnumByNameFront<TEnum>(string name) where TEnum : Enum
        {
            foreach (var enumValue in Enum.GetValues(typeof(TEnum)))
            {
                if (name.StartsWith(enumValue.ToString()))
                {
                    return (TEnum)enumValue;
                }
            }
            return default;
        }

        public static TEnum? GetEnumByNameEnd<TEnum>(string name) where TEnum : Enum
        {
            foreach (var enumValue in Enum.GetValues(typeof(TEnum)))
            {
                if (name.EndsWith(enumValue.ToString()))
                {
                    return (TEnum)enumValue;
                }
            }
            return default;
        }
    }

}

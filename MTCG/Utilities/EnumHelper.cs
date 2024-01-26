namespace MTCG.Utilities
{
    public class EnumHelper
    {
        public static TEnum? GetEnumByNameFront<TEnum>(string name) where TEnum : Enum
        {
            foreach (Enum enumValue in Enum.GetValues(typeof(TEnum)))
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
            foreach (Enum enumValue in Enum.GetValues(typeof(TEnum)))
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

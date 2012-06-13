using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Metro.Controls.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetTypeInfo().GetDeclaredField(name);
                if (field != null)
                {
                    //investigate use of system.componentmodel.dataannotations.displayattribute
                    var attr = field.GetCustomAttribute<LocalisableDescriptionAttribute>(false);
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }

        public static Enum GetEnumValue<T>(this string description)
        {
            foreach (Enum value in Enum.GetValues(typeof(T)))
            {
                var generatedDescription = value.GetDescription();
                if (description == generatedDescription)
                {
                    return value;
                }
            }
            return default(Enum);
        }

    }
}

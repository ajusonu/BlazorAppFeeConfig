using System;
using System.ComponentModel;
using System.Reflection;

namespace FeesAutomationWebsite.Models
{
    public enum BookingType
    {
        [Description("Not Applicable")]
        NA,
        [Description("Online")]
        Online,
        [Description("Online Assisted")]
        OnlineAssisted,
        [Description("Offline")]
        Offline,
        [Description("All")]
        All
    };

    public static class Extensions
    {
        public static string ToDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}

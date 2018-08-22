// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines values for Progress.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Progress
    {
        [EnumMember(Value = "Ongoing")]
        Ongoing,
        [EnumMember(Value = "WaitingForSupport")]
        WaitingForSupport,
        [EnumMember(Value = "Frozen")]
        Frozen,
        [EnumMember(Value = "Suspended")]
        Suspended,
        [EnumMember(Value = "Allowed")]
        Allowed,
        [EnumMember(Value = "Undefined")]
        Undefined
    }
    internal static class ProgressEnumExtension
    {
        internal static string ToSerializedValue(this Progress? value)
        {
            return value == null ? null : ((Progress)value).ToSerializedValue();
        }

        internal static string ToSerializedValue(this Progress value)
        {
            switch( value )
            {
                case Progress.Ongoing:
                    return "Ongoing";
                case Progress.WaitingForSupport:
                    return "WaitingForSupport";
                case Progress.Frozen:
                    return "Frozen";
                case Progress.Suspended:
                    return "Suspended";
                case Progress.Allowed:
                    return "Allowed";
                case Progress.Undefined:
                    return "Undefined";
            }
            return null;
        }

        internal static Progress? ParseProgress(this string value)
        {
            switch( value )
            {
                case "Ongoing":
                    return Progress.Ongoing;
                case "WaitingForSupport":
                    return Progress.WaitingForSupport;
                case "Frozen":
                    return Progress.Frozen;
                case "Suspended":
                    return Progress.Suspended;
                case "Allowed":
                    return Progress.Allowed;
                case "Undefined":
                    return Progress.Undefined;
            }
            return null;
        }
    }
}

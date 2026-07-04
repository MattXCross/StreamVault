using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StreamVault.Models.Shared;

public static class EnumExtensions
{
    public static string DisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).First();
        return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
    }
}

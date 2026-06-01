using System.Reflection;
using System.Runtime.Serialization;

namespace frontend.Features.Shared;

public static class ApiEnumWire
{
    public static string GetValue<T>(T value) where T : struct, Enum
    {
        var name = Enum.GetName(typeof(T), value);
        if (name is null)
            return value.ToString()!;

        var field = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static);
        var attr = field?.GetCustomAttribute<EnumMemberAttribute>();
        return attr?.Value ?? name;
    }
}

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OliverBooth.Data;

internal sealed class PermissionListConverter : ValueConverter<IReadOnlyList<Permission>, string>
{
    public PermissionListConverter() : this(';')
    {
    }

    public PermissionListConverter(char separator) :
        base(v => ToProvider(v, separator),
            s => FromProvider(s, separator))
    {
    }

    private static IReadOnlyList<Permission> FromProvider(string source, char separator = ';')
    {
        var permissions = new List<Permission>();

        foreach (string permission in source.Split(separator))
        {
            string name = permission;
            var allowed = true;

            if (name.Length > 1 && name[0] == '-')
            {
                name = name[1..];
                allowed = false;
            }

            permissions.Add(new Permission(name, allowed));
        }

        return permissions.AsReadOnly();
    }

    private static string ToProvider(IEnumerable<Permission> permissions, char separator = ';')
    {
        return string.Join(separator, permissions.Select(p => $"{(p.IsAllowed ? "-" : "")}{p.Name}"));
    }
}

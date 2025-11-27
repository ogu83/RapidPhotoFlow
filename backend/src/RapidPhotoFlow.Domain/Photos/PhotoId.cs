using System.Diagnostics.CodeAnalysis;

namespace RapidPhotoFlow.Domain.Photos;

/// <summary>
/// Strongly-typed identifier for Photo entity.
/// </summary>
public readonly record struct PhotoId(Guid Value)
{
    public static PhotoId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static bool TryParse(string value, [NotNullWhen(true)] out PhotoId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new PhotoId(guid);
            return true;
        }

        id = default;
        return false;
    }

    public static implicit operator Guid(PhotoId id) => id.Value;
    public static implicit operator PhotoId(Guid guid) => new(guid);
}


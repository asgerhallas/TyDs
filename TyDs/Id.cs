namespace TyDs;

public abstract record Id
{
    protected Id(string prefix, string? identifier = null)
    {
        Prefix = prefix.ToLowerInvariant();
        Identifier = identifier ?? Guid.NewGuid().ToString("N").ToLowerInvariant();
    }

    public string Prefix { get; init; }
    public string Identifier { get; private set; }

    public sealed override string ToString()
    {
        return $"{Prefix}-{Identifier}";
    }

    public static implicit operator string(Id id)
    {
        return id.ToString();
    }

    public static Id Create(Type type, string identifier)
    {
        // Invoke the ctor having the identifier param if one exists.
        var id = type.GetConstructor([typeof(string)]) != null
            ? (Id)Activator.CreateInstance(type, identifier)
            : (Id)Activator.CreateInstance(type);

        id.Identifier = identifier.ToLowerInvariant();

        return id;
    }
}
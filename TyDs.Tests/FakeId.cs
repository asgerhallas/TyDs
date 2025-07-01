// ReSharper disable once CheckNamespace
namespace TyDs.Tests.Faky;

/// <summary>
/// This is not the right Id base type
/// </summary>
public record Id(string Prefix);

public record FakeId() : Id("fake");
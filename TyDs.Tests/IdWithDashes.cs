namespace TyDs.Tests;

public record IdWithDashes(string? Identifier = null) : Id("faky-fake", Identifier);
public record IdWithDots() : Id("faky.fake");

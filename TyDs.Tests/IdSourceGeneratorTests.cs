using System;
using Shouldly;
using Xunit;

namespace TyDs.Tests;

public class IdSourceGeneratorTests
{
    [Fact]
    public void Construct()
    {
        var id = new AId();

        id.ShouldBeAssignableTo<AId>().ShouldNotBe(null);
    }

    [Fact]
    public void Construct_IdWithFileScopedNamespace()
    {
        var idInFileScopedNamespace = new IdInFileScopedNamespace();

        IdParser.TryParse(idInFileScopedNamespace.ToString(), out var id).ShouldBe(true);

        id.ShouldBeAssignableTo<IdInFileScopedNamespace>()
            .ShouldBe(idInFileScopedNamespace);
    }

    [Fact]
    public void Construct_NestedId()
    {
        var nestedId = new NestedId();

        IdParser.TryParse(nestedId.ToString(), out var id).ShouldBe(true);

        id.ShouldBeAssignableTo<NestedId>()
            .ShouldBe(nestedId);
    }

    [Fact]
    public void Construct_Qualified()
    {
        var qualifiedId = new QualifiedId();

        IdParser.TryParse(qualifiedId.ToString(), out var id).ShouldBe(true);

        id.ShouldBeAssignableTo<QualifiedId>()
            .ShouldBe(qualifiedId);
    }

    [Fact]
    public void Construct_FakeId()
    {
        IdParser.TryParse("fake/123", out var id).ShouldBe(false);

        id.ShouldBe(null);
    }

    [Fact]
    public void ParseId()
    {
        IdParser.TryParse("b/123", out var b).ShouldBe(true);

        var bId = b.ShouldBeAssignableTo<BId>()!;
        bId.Prefix.ShouldBe("b");
        bId.Identifier.ShouldBe("123");

        IdParser.TryParse("a/abc", out var a).ShouldBe(true);

        var aId = a.ShouldBeAssignableTo<AId>()!;
        aId.Prefix.ShouldBe("a");
        aId.Identifier.ShouldBe("abc");
    }

    [Fact]
    public void ParseId_NotExiting()
    {
        IdParser.TryParse("c/123", out var b).ShouldBe(false);

        b.ShouldBe(null);
    }

    [Fact]
    public void ParseId_CaseInsensitive()
    {
        IdParser.TryParse("A/123", out var a).ShouldBe(true);

        var aId = a.ShouldBeAssignableTo<AId>()!;
        aId.Prefix.ShouldBe("a");
        aId.Identifier.ShouldBe("123");
    }

    [Fact]
    public void Equality_SameId()
    {
        var id1 = new AId();
        IdParser.TryParse(id1.ToString(), out var id1_2).ShouldBe(true);

        id1.ShouldBe(id1_2);
    }

    [Fact]
    public void Equality_OtherId()
    {
        var id1 = new AId();

        id1.ShouldNotBe(new AId());
    }

    [Fact]
    public void Equality_OtherIdType()
    {
        IdParser.TryParse("a/123", out var id1).ShouldBe(true);
        IdParser.TryParse("b/123", out var id2).ShouldBe(true);

        id1.ShouldNotBe(id2);
    }

    [Fact]
    public void Equality_CaseInsensitive()
    {
        IdParser.TryParse("a/abc", out var id1).ShouldBe(true);
        IdParser.TryParse("a/ABC", out var id2).ShouldBe(true);

        id1.ShouldBe(id2);
    }

    [Fact]
    public void TryParse_InvalidId()
    {
        IdParser.TryParse("dummy", out var result).ShouldBe(false);

        result.ShouldBe(null);
    }

    [Fact]
    public void Parse()
    {
        var aId = new AId();

        IdParser.Parse<AId>(aId.ToString()).ShouldBe(aId);
    }

    [Fact]
    public void ParseNotAnId() =>
        Should.Throw<InvalidOperationException>(() =>
                IdParser.Parse<AId>("dummy"))
            .Message.ShouldBe("Invalid id.");

    [Fact]
    public void ParseWrongType() =>
        Should.Throw<InvalidOperationException>(() =>
                IdParser.Parse<AId>(new BId().ToString()))
            .Message.ShouldBe("Id is not of type AId.");

    [Fact]
    public void Parse_Null() =>
        Should.Throw<ArgumentNullException>(() =>
                IdParser.Parse<AId>(null))
            .Message.ShouldBe("Value cannot be null. (Parameter 'id')");

    [Fact]
    public void TryParse_Null()
    {
        IdParser.TryParse(null, out var result).ShouldBe(false);

        result.ShouldBe(null);
    }

    public record NestedId() : Id("nested");
}

public record AId() : Id("a");
public record BId() : Id("b");
public record QualifiedId() : Id("q");
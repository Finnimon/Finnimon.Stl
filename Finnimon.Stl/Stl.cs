namespace Finnimon.Stl;

public sealed record Stl(string? Name, string Header, IReadOnlyList<StlFacet> Facets);
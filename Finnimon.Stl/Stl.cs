using System.Runtime.InteropServices;
using Finnimon.Math;

namespace Finnimon.Stl;

public sealed record Stl(string? Name, string Header, IReadOnlyList<StlFacet> Facets);
[StructLayout(LayoutKind.Explicit, Size = 12+2)]
public readonly record struct StlFacet([field: FieldOffset(0)] Triangle3D Triangle, [field: FieldOffset(12)] ushort Attribute);
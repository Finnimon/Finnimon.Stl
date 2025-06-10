using System.Runtime.InteropServices;
using Finnimon.Math;

namespace Finnimon.Stl;

[StructLayout(LayoutKind.Explicit, Size = 36+2)]
public readonly record struct StlFacet([field: FieldOffset(0)] Triangle3D Triangle, [field: FieldOffset(36)] ushort Attribute);
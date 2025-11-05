using Application.Features.Objects.Enums;
using MemoryPack;

namespace Application.Features.Objects.Models;

[MemoryPackable]
public partial class ObjectInfo
{
    public long? Id { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public ObjectTypeEnum Type { get; set; }
}
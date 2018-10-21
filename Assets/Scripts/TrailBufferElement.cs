using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
[System.Serializable]
public struct TrailBufferElement : IBufferElementData
{
    public float3 Value;

    public TrailBufferElement(float3 value)
    {
        Value = value;
    }
}

using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Velocity : IComponentData
{
    // 速度
    public float3 Value;

    public Velocity(float3 value)
    {
        Value = value;
    }
}
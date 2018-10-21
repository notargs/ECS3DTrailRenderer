using System;
using Unity.Entities;

[Serializable]
public struct LifeTime : IComponentData
{
    public float Time;

    public LifeTime(float time)
    {
        Time = time;
    }
}


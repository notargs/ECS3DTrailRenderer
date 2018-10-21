using System;
using Unity.Collections;
using UnityEngine;

public class MeshInstancedArgs : IDisposable
{
    public ComputeBuffer Buffer { get; }
    NativeArray<uint> args;

    public MeshInstancedArgs()
    {
        Buffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        args = new NativeArray<uint>(5, Allocator.Persistent);
    }

    public void SetData(Mesh mesh, uint instanceCount, int subMeshIndex = 0)
    {
        args[0] = mesh.GetIndexCount(subMeshIndex);
        args[1] = instanceCount;
        args[2] = mesh.GetIndexStart(subMeshIndex);
        args[3] = mesh.GetBaseVertex(subMeshIndex);

        Buffer.SetData(args);
    }

    public void Dispose()
    {
        args.Dispose();
        Buffer?.Dispose();
    }
}
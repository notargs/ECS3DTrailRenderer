using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GpuTrailRendererSystem : ComponentSystem
{
    public static Material Material;

    MeshInstancedArgs meshInstancedArgs;
    Mesh trailMesh;
    MaterialPropertyBlock materialPropertyBlock;

    ComputeBuffer trailElementBuffer;
    ComputeBuffer segmentBuffer;

    NativeArray<float3> trailElements;
    NativeArray<int3> segments;

    ComponentGroup componentGroup;
    
    protected override void OnCreateManager()
    {
        componentGroup = GetComponentGroup(ComponentType.Create<TrailBufferElement>());
        meshInstancedArgs = new MeshInstancedArgs();
        trailMesh = TrailMeshGenerator.CreateMesh();
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    protected override void OnDestroyManager()
    {
        meshInstancedArgs.Dispose();
        trailElementBuffer?.Dispose();
        segmentBuffer?.Dispose();

        if (trailElements.IsCreated) trailElements.Dispose();
        if (segments.IsCreated) segments.Dispose();
    }

    // 2のn乗、かつ、渡された数値を上回る値を返すメソッド
    static int CalcWrappingArraySize(int length) =>
        Mathf.Max(1 << Mathf.CeilToInt(Mathf.Log(length, 2)), 1048);

    protected override unsafe void OnUpdate()
    {
        // Segmentの個数を計算
        var segmentCount = componentGroup.CalculateLength();
        
        // TrailElementの個数を計算
        var trailBufferArray = componentGroup.GetBufferArray<TrailBufferElement>();
        var trailElementCount = 0;
        for (var i = 0; i < segmentCount; ++i)
        {
            trailElementCount += trailBufferArray[i].Length;
        }

        // 配列のサイズが足りなかったら新しく作り直す
        if (!trailElements.IsCreated || trailElements.Length < trailElementCount)
        {
            if (trailElements.IsCreated) trailElements.Dispose();
            trailElements = new NativeArray<float3>(CalcWrappingArraySize(trailElementCount), Allocator.Persistent);

            trailElementBuffer?.Dispose();
            trailElementBuffer = new ComputeBuffer(trailElements.Length, sizeof(TrailBufferElement));
        }

        if (!segments.IsCreated || segments.Length < segmentCount)
        {
            if (segments.IsCreated) segments.Dispose();
            segments = new NativeArray<int3>(CalcWrappingArraySize(segmentCount), Allocator.Persistent);

            segmentBuffer?.Dispose();
            segmentBuffer = new ComputeBuffer(segments.Length, sizeof(TrailBufferElement));
        }

        // UnsafePtrを取得する
        var offset = 0;
        var trailElementsPtr = (float3*) trailElements.GetUnsafePtr();
        var segmentsPtr = (int3*) segments.GetUnsafePtr();

        var entityArray = componentGroup.GetEntityArray();
        
        // あらかじめ計算してあったSegmentの数(=パーティクルの数)だけループを回す
        for (var i = 0; i < segmentCount; ++i)
        {
            var trailBuffer = trailBufferArray[i];
            var entity = entityArray[i];

            var bufferLength = trailBuffer.Length;

            // TrailBufferの値をNativeArrayに複製していく
            UnsafeUtility.MemCpy(trailElementsPtr, trailBuffer.GetBasePointer(), sizeof(float3) * bufferLength);
            *segmentsPtr = new int3(offset, bufferLength, entity.Index); // Segmentにヘッダ情報を書き込む

            // ヘッダ用のオフセットを再計算しておく
            offset += bufferLength;

            // ポインタを進める
            segmentsPtr++;
            trailElementsPtr += bufferLength;
        }

        // ComputeBufferにNativeArrayの情報を渡す
        trailElementBuffer.SetData(trailElements);
        segmentBuffer.SetData(segments);

        // MaterialPropertyBlockに生成したBufferを渡す
        materialPropertyBlock.SetBuffer("_Positions", trailElementBuffer);
        materialPropertyBlock.SetBuffer("_Segments", segmentBuffer);

        // Segmentの数を渡しておく
        meshInstancedArgs.SetData(trailMesh, (uint) segmentCount);

        // Meshを描画する
        Graphics.DrawMeshInstancedIndirect(
            trailMesh, 0, Material,
            new Bounds(Vector3.zero, Vector3.one * 1000),
            meshInstancedArgs.Buffer, 0, materialPropertyBlock
        );
    }
}
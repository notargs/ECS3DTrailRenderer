using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class VelocitySystem : JobComponentSystem
{
    Job job;
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Jobを作り、deltaTimeを渡して実行
        job.Time = Time.time;
        job.DeltaTime = Time.deltaTime;
        return job.Schedule(this, inputDeps);
    }
        
    // Jobの本体
    [BurstCompile]
    struct Job : IJobProcessComponentData<Velocity, Position>
    {
        public float Time;
        public float DeltaTime;

        public void Execute(ref Velocity velocity, ref Position position)
        {
            // 空気抵抗を加える
            var pos = position.Value * 0.4f;
            var t = Time * 0.5f;
            var drag = velocity.Value * DeltaTime * 2.0f;
            if (math.length(velocity.Value) < math.length(velocity.Value - drag))
            {
                velocity.Value = 0;
            }
            velocity.Value -= drag;
                
            // ノイズを加える
            var n1 = new float3(
                noise.snoise(new float4(pos, 0 + t)),
                noise.snoise(new float4(pos, 100 + t)),
                noise.snoise(new float4(pos, 200 + t))
            );
            var n2 = new float3(
                noise.snoise(new float4(pos, 300 + t)),
                noise.snoise(new float4(pos, 400 + t)),
                noise.snoise(new float4(pos, 500 + t))
            );
            velocity.Value += math.cross(n1, n2) * DeltaTime * 40.0f;
            
            // 速度をもとに位置を更新
            position.Value += velocity.Value * DeltaTime;
        }
    }
}
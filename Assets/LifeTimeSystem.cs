using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class LifeTimeSystem : JobComponentSystem
{
    Job job;
    // Job終了後にまとめてEntityに対する処理を行うためのBarrierSystem
    [Inject] EndFrameBarrier endFrameBarrier;
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // BarrierSystemからCommandBufferを取得し、Jobに渡す
        job.CommandBuffer = endFrameBarrier.CreateCommandBuffer().ToConcurrent();
        job.DeltaTime = Time.deltaTime;
        
        return job.Schedule(this, inputDeps);
    }

    [BurstCompile]
    struct Job : IJobProcessComponentDataWithEntity<LifeTime>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float DeltaTime;
        
        public void Execute(Entity entity, int index, ref LifeTime lifeTime)
        {
            // LifeTimeを減らし、0以下なら削除する
            lifeTime.Time -= DeltaTime;
            if (lifeTime.Time < 0)
            {
                CommandBuffer.DestroyEntity(index, entity);
            }
        }
    }
}

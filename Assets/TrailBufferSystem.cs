using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class TrailBufferSystem : JobComponentSystem
{
    ComponentGroup componentGroup;
        
    Job job;

    protected override void OnCreateManager()
    {
        // ComponentGroupを取得
        componentGroup = GetComponentGroup(
            ComponentType.Create<TrailBufferElement>(),
            ComponentType.Create<Position>()
        );
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // GetBufferArrayでBufferArrayを取得できる
        job.Buffers = componentGroup.GetBufferArray<TrailBufferElement>();
        job.Position = componentGroup.GetComponentDataArray<Position>();
        return job.Schedule(componentGroup.CalculateLength(), 32, inputDeps);
    }

    [BurstCompile]
    struct Job : IJobParallelFor
    {
        public BufferArray<TrailBufferElement> Buffers;
        public ComponentDataArray<Position> Position;
            
        public void Execute(int index)
        {
            var buffer = Buffers[index];
                
            if (buffer.Length > 20) buffer.RemoveAt(0);
            buffer.Add(new TrailBufferElement(Position[index].Value));
        }
    }
}
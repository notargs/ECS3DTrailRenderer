using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    [SerializeField] Material lineMaterial;

    EntityManager entityManager;
    Entity prefab;
    Random random = new Random(114514);
    
    void Start()
    {
        // EntityManagerを取得
        var world = World.Active;
        entityManager = world.GetOrCreateManager<EntityManager>();
        
        // Prefabを作成
        prefab = entityManager.CreateEntity(
            ComponentType.Create<Position>(), // 位置
            ComponentType.Create<Scale>(), // サイズ
            ComponentType.Create<Velocity>(), // 速度
            ComponentType.Create<Prefab>(), // Prefab（これがついているEntityはSystemから無視される）
            ComponentType.Create<LifeTime>(), // 生存時間
            ComponentType.Create<TrailBufferElement>() // これまでの位置情報の配列
        );
        
        entityManager.SetComponentData(prefab, new Scale{Value = new float3(1, 1, 1) * 0.1f});
        
        // 生存時間を設定
        entityManager.SetComponentData(prefab, new LifeTime(2));
        
        // Prefabに描画用のComponentを追加
        /*
        entityManager.AddSharedComponentData(prefab, new MeshInstanceRenderer {
            castShadows = ShadowCastingMode.On, receiveShadows = true,
            material = material, mesh = mesh
        });*/

        GpuTrailRendererSystem.Material = lineMaterial;
    }

    void Update()
    {
        for (int i = 0; i < 20; ++i)
        {
            // PrefabをInstance化
            var entity = entityManager.Instantiate(prefab);
        
            // Velocityを設定
            entityManager.SetComponentData(entity, new Velocity(random.NextFloat3Direction()));
        }
    }
}
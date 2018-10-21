using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class TrailRendererSystem : ComponentSystem
{
    Mesh mesh; // 動的に構築するMesh
    
    readonly List<Vector3> vertices = new List<Vector3>(); // 頂点配列
    readonly List<int> indices = new List<int>(); // インデックス配列

    ComponentGroup componentGroup;

    public static Material Material;

    protected override void OnCreateManager()
    {
        mesh = new Mesh(); // Meshを作成
        mesh.MarkDynamic(); // 動的なMeshとしてマーク

        // ComponentGroupを取得しておく
        componentGroup = GetComponentGroup(typeof(TrailBufferElement));
    }

    protected override void OnUpdate() {
        vertices.Clear(); // 頂点配列を初期化
        indices.Clear(); // インデックス配列を初期化

        var bufferArray = componentGroup.GetBufferArray<TrailBufferElement>();
        for (var i = 0; i < componentGroup.CalculateLength(); ++i) {
            var buffer = bufferArray[i];
            if (buffer.Length < 2) continue; // 頂点が2個以下なら、線を作れないためスキップ

            for (var j = 0; j < buffer.Length; ++j) {
                // 頂点をつなげて線を作っていく
                if (j > 0) {
                    indices.Add(vertices.Count - 1);
                    indices.Add(vertices.Count);
                }
                vertices.Add(buffer[j].Value);
            }
        }
            
        // Meshの更新
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            
        Graphics.DrawMesh(mesh, Matrix4x4.identity, Material, 0); // Meshの描画
    }
}
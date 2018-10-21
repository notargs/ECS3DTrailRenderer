using System.Collections.Generic;
using UnityEngine;

public static class TrailMeshGenerator
{
    // 円柱の頂点位置を計算
    static Vector3 CalcPosition(int i, int j) => new Vector3(i, Mathf.Sin(j * Mathf.PI * 2 / 8.0f), Mathf.Cos(j * Mathf.PI * 2 / 8.0f));

    public static Mesh CreateMesh()　{
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        // 横幅20、縦幅8の円柱を作成
        for (var i = 0; i < 20 - 1; ++i) {
            for (var j = 0; j < 8; ++j)  {
                triangles.AddRange(new [] {
                    vertices.Count + 0, vertices.Count + 1, vertices.Count + 2,vertices.Count + 2, vertices.Count + 1, vertices.Count + 3,
                });
                vertices.AddRange(
                    new []  {
                        CalcPosition(i, j),  CalcPosition(i+1, j),　CalcPosition(i, j+1), CalcPosition(i+1, j+1),
                    }
                );
        } }
        
        // Meshの作成
        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }
}


using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Ngin;
using TB;
using Unity.Collections;

[UnityEditor.AssetImporters.ScriptedImporter(1, "nmsh")]
public class MeshAssetImporter : UnityEditor.AssetImporters.ScriptedImporter {
    private Vector2 RotateUV(Vector2 uv, float angle) {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            cos * uv.x - sin * uv.y,
            sin * uv.x + cos * uv.y
        );
    }

    public int VertexCheck(Vector3 position, List<Vector3> vertices) {
        int i = 0;
        foreach (Vector3 vert in vertices) {
            if (Vector3.Distance(position, vert) < 0.0001f) {
                return i;
            }
            i++;
        }
        return -1;
    }

    public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
        Lexicon lexicon = Lexicon.FromLexiconFilePath(ctx.assetPath);
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        List<List<float>> boneWeights = new List<List<float>>();
        List<List<int>> boneIndices = new List<List<int>>();

        var faces = lexicon.Get<Lexicon>("faces");
        bool smoothShading = lexicon.Get<bool>("smoothShading", true);
        bool isAnim = lexicon.Get<bool>("isAnim", false);

        foreach (var face in faces.Objects) {
            var faceLexicon = face.Value as Lexicon;
            var faceVertices = faceLexicon.Get<Lexicon>("vertices");

            int baseIndex = vertices.Count;
            int vertexCount = faceVertices.Objects.Count;

            for (int i = 0; i < vertexCount; i++) {
                var vertex = faceVertices.Get<Lexicon>(i.ToString());
                var positionBag = vertex.Get<List<float>>("position");
                var normalBag = vertex.Get<List<float>>("normal");
                var uvBag = vertex.Get<List<float>>("uv");

                Vector3 vertPos = new Vector3(positionBag[0], positionBag[1], positionBag[2]);
                Vector3 vertNorm = new Vector3(normalBag[0], normalBag[1], normalBag[2]);
                Vector2 uv = new Vector2(uvBag[0], uvBag[1]);

                vertices.Add(vertPos);
                normals.Add(vertNorm);
                uvs.Add(uv);

                if (isAnim) {
                    var boneWeightsBag = vertex.Get<List<float>>("boneWeights", new List<float>());
                    var boneIndicesBag = vertex.Get<List<int>>("boneIndices", new List<int>());

                    boneWeights.Add(boneWeightsBag);
                    boneIndices.Add(boneIndicesBag);
                }
            }

            // Triangulate the face
            if (vertexCount == 3) {
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex);
            } else if (vertexCount == 4) {
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 2);
            }
        }

        Debug.Log("Imported mesh with " + vertices.Count + " vertices and " + faces.Length + " faces.");

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        if (isAnim) {
            byte[] bonesPerVertex = new byte[vertices.Count];
            int weightCount = 0;
            for (int i = 0; i < vertices.Count; i++) {
                bonesPerVertex[i] = (byte)boneWeights[i].Count;
                weightCount += boneWeights[i].Count;
            }

            BoneWeight1[] weights = new BoneWeight1[weightCount];
            int weightCounter = 0;
            for (int i = 0; i < vertices.Count; i++) {
                byte boneCount = bonesPerVertex[i];
                for (int j = 0; j < boneCount; j++) {
                    weights[weightCounter].boneIndex = boneIndices[i][j];
                    weights[weightCounter].weight = boneWeights[i][j];
                    weightCounter++;
                }
            }

            var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
            var weightsArray = new NativeArray<BoneWeight1>(weights, Allocator.Temp);

            // Set the bone weights on the mesh
            mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);
            bonesPerVertexArray.Dispose();
            weightsArray.Dispose();

            //mesh.boneWeights = boneWeights.ToArray();
        }

        float angle = 0.0f;
        if (smoothShading) {
            angle = 60.0f;
        }

        MeshNormalSolver.RecalculateNormals(mesh, 60.0f);

        ctx.AddObjectToAsset("mesh", mesh);
        ctx.SetMainObject(mesh);
    }
}

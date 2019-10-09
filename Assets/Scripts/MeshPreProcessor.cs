/**
 * 1. Reset Mesh`s UV
 * 2. change Material for ReplaceShader
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPreProcessor : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer smr;
    [SerializeField] private Material placeholder;

    public int VertexCount
    {
        get {
            return smr.sharedMesh.vertexCount;
        }
    }

    private void Awake()
    {
        ReplaceMaterial();
    }
    void Start()
    {
        ReBuildMesh();
    }

    void Update()
    {

    }

    //uvの書き換え
    void ReBuildMesh()
    {
        Mesh mesh = new Mesh(); //bufffer
        Mesh baseMesh = smr.sharedMesh;

        var vertices = new List<Vector3>(baseMesh.vertices);
        //var normals = new List<Vector3>(baseMesh.normals);
        //var tangents = new List<Vector4>(baseMesh.tangents);
        //var boneWeights = new List<BoneWeight>(baseMesh.boneWeights);
        int[] indices = new int[baseMesh.vertexCount];
        List<Vector2> uv = new List<Vector2>();

        for(int i = 0; i < baseMesh.vertexCount; i++)
        {
            uv.Add(new Vector2(((float)i + 0.5f) / (float)baseMesh.vertexCount, 0));
            indices[i] = i;
        }

        mesh.subMeshCount = 1;
        mesh.SetVertices(vertices);
        //mesh.SetNormals(normals);
        //mesh.SetTangents(tangents);
        //mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, uv);
        //mesh.bindposes = baseMesh.bindposes;
        //mesh.boneWeights = boneWeights.ToArray();
        mesh.UploadMeshData(true);
        smr.sharedMesh.SetUVs(0, uv);
        smr.sharedMesh.SetIndices(indices, MeshTopology.Points, 0);

    }

    void ReplaceMaterial()
    {
        smr.material = placeholder;
    }
}

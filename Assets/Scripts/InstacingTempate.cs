using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class InstacingTempate : MonoBehaviour
{

    #region private Data
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    [SerializeField] GameObject EmitterModel;
    [SerializeField] GameObject cam;
    [SerializeField] ComputeShader cs;
    RenderTexture positionRenderTexture;
    ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5];
    private uint instanceCount;
    AttributeBaker attributeBaker;

    #endregion


    void Start()
    {
        attributeBaker = cam.GetComponent<AttributeBaker>();
        instanceCount = (uint)EmitterModel.GetComponent<MeshPreProcessor>().VertexCount;
        initBufferes();
        positionRenderTexture = attributeBaker.MrtTexturees[0];
    }

    void Update()
    {
        ProcessComputeShader();
        material.SetBuffer("positionBuffer", positionBuffer);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 100.0f), argsBuffer);
    }

    private void initBufferes()
    {

        positionBuffer = new ComputeBuffer((int)(instanceCount), 3 * Marshal.SizeOf(typeof(float)));
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = mesh.GetIndexCount(0);
        args[1] = instanceCount;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        args[4] = 0;
        argsBuffer.SetData(args);
    }

    private void ProcessComputeShader()
    {
        var kernelID = cs.FindKernel("MainCulc");
        cs.SetTexture(kernelID, "positionRenderTexture", positionRenderTexture);
        cs.SetBuffer(kernelID, "positionBuffer", positionBuffer);
        cs.SetInt("instanceNum", (int)instanceCount);

        //１つ多めで実行しておいてComputeShader内部で歯止めを掛ける
        cs.Dispatch(kernelID, (int)(instanceCount / 8) + 1, 1, 1);
    }


    private void OnDisable()
    {
        positionBuffer.Release();
        argsBuffer.Release();
    }
}

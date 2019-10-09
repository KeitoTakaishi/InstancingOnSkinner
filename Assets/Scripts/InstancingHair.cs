using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class InstancingHair : MonoBehaviour
{
    #region data for Instancing
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    ComputeBuffer argsBuffer;
    uint[] args = new uint[5];
    private uint instanceCount;
    #endregion

    #region private Data
    [SerializeField] GameObject emitterModel;
    [SerializeField] GameObject cam;
    [SerializeField] ComputeShader cs;
    RenderTexture positionRenderTexture;
    RenderTexture normalRenderTexture;
    ComputeBuffer positionBuffer;
    ComputeBuffer axisBuffer;
    ComputeBuffer rotateBuffer;
    AttributeBaker attributeBaker;
    #endregion


    void Start()
    {
        attributeBaker = cam.GetComponent<AttributeBaker>();
        instanceCount = (uint)emitterModel.GetComponent<MeshPreProcessor>().VertexCount;
        initBufferes();
        positionRenderTexture = attributeBaker.MrtTexturees[0];
        normalRenderTexture = attributeBaker.MrtTexturees[1];
    }

    void Update()
    {
        ProcessComputeShader();
        material.SetBuffer("positionBuffer", positionBuffer);
        material.SetBuffer("axisBuffer", axisBuffer);
        material.SetBuffer("rotateBuffer", rotateBuffer);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 100.0f), argsBuffer);
    }

    void initBufferes()
    {
        positionBuffer = new ComputeBuffer((int)(instanceCount), 3 * Marshal.SizeOf(typeof(float)));
        axisBuffer = new ComputeBuffer((int)(instanceCount), 3 * Marshal.SizeOf(typeof(float)));
        rotateBuffer = new ComputeBuffer((int)(instanceCount), Marshal.SizeOf(typeof(float)));

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
        cs.SetTexture(kernelID, "normalRenderTexture", normalRenderTexture);
        cs.SetBuffer(kernelID, "positionBuffer", positionBuffer);
        cs.SetBuffer(kernelID, "axisBuffer", axisBuffer);
        cs.SetBuffer(kernelID, "rotateBuffer", rotateBuffer);

        cs.SetInt("instanceNum", (int)instanceCount);
        cs.Dispatch(kernelID, (int)(instanceCount / 64) + 1, 1, 1);
    }

    private void OnDisable()
    {
        positionBuffer.Release();
        argsBuffer.Release();
    }
}

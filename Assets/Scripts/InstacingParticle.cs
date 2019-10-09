using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class InstacingParticle : MonoBehaviour
{
    struct Params
    {
        Vector3 life;     // x = time elapsed, y = life time, z = isActive 1 is active, -1 is disactive.
        Vector2 size; //cur, default
        public Params(float life, float size)
        {
            this.life = new Vector3(0, life, -1.0f);
            this.size = new Vector2(0, size);
        }
    }

    [Serializable]
    public struct Lives
    {
        [Range(0f, 60f)]
        public float minLife;
        [Range(0f, 60f)]
        public float maxLife;
    }

    #region private Data
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    [SerializeField] GameObject EmitterModel;
    [SerializeField] GameObject cam;
    [SerializeField] ComputeShader cs;
    RenderTexture positionRenderTexture;
    ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer paramsBuffer;
    private uint[] args = new uint[5];
    private uint instanceCount;
    AttributeBaker attributeBaker;
    #endregion

    #region ComputeShader uniform 
    [SerializeField] List<Lives> lives = new List<Lives>();
    //[Serializable] [Range(0.1f,)] 
    private float timer;
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
        material.SetBuffer("paramsBuffer", paramsBuffer);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 100.0f), argsBuffer);
    }

    private void initBufferes()
    {

        positionBuffer = new ComputeBuffer((int)(instanceCount), 3 * Marshal.SizeOf(typeof(float)));
        paramsBuffer = new ComputeBuffer((int)(instanceCount), Marshal.SizeOf(typeof(Params)));
        Params[] parameters = new Params[paramsBuffer.count];
        for(int i = 0; i < instanceCount; i++)
        {
            var life = lives[UnityEngine.Random.Range(0, lives.Count)]; //inspectorで保存された値の中から探索
            var size =UnityEngine.Random.Range(0.01f, 0.075f);
            parameters[i] = new Params(UnityEngine.Random.Range(life.minLife, life.maxLife), size);
        }
        paramsBuffer.SetData(parameters);


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
        cs.SetBuffer(kernelID, "buffer", paramsBuffer);
        timer += Time.deltaTime;
        cs.SetVector("times", new Vector2(Time.deltaTime, timer));
        //１つ多めで実行しておいてComputeShader内部で歯止めを掛ける
        cs.Dispatch(kernelID, (int)(instanceCount / 8) + 1, 1, 1);
    }


    private void OnDisable()
    {
        positionBuffer.Release();
        argsBuffer.Release();
    }
}

/*
 this script is bake skinner`s attribtue.
 1. create virtual camera
 2. create rt
 3. replace shader :  (Placeholder -> MRTPass)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeBaker : MonoBehaviour
{
    #region private
    [SerializeField] private GameObject renderTargetObject;
    [SerializeField] private SkinnedMeshRenderer model;
    [SerializeField] Shader MRTPass;
    private int RenderTargetNum = 2;
    //[SerializeField] private RenderTexture[] renderTexture;
    private RenderTexture[] renderTexture;
    private RenderBuffer[] renderBuffer;
    private RenderBuffer depthBuffer;
    private Camera subCam;
    #endregion

    public bool isDebug = false;

    #region Accesor
    public RenderTexture[] MrtTexturees
    {
        get { return renderTexture; }
    }
    #endregion


    void Awake()
    {
        initBuffer();  
    }

    void Start()
    {
        BuildCamera();
        subCam.SetReplacementShader(MRTPass, "MRT");
    }

    void Update()
    {
        subCam.SetReplacementShader(MRTPass, "MRT");
        subCam.SetTargetBuffers(renderBuffer, depthBuffer);
        subCam.Render();
    }


    void BuildCamera()
    {
        var obj = new GameObject("SubCam");
        obj.transform.parent = renderTargetObject.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.hideFlags = HideFlags.DontSave;
        subCam = obj.AddComponent<Camera>();
        subCam.clearFlags = CameraClearFlags.SolidColor;
        subCam.depth = -1000;
        subCam.renderingPath = RenderingPath.Forward;
        subCam.cullingMask = 1 << LayerMask.NameToLayer("Bake");
        subCam.nearClipPlane = -10;
        subCam.farClipPlane = 100;
        subCam.orthographic = true;
        subCam.orthographicSize = 100;
        subCam.enabled = false;
    }

    RenderTexture CreateRenderTexture(Vector2 res)
    {
        RenderTexture rt = new RenderTexture((int)res.x, (int)res.y, 0, RenderTextureFormat.ARGBFloat);
        rt.format = RenderTextureFormat.ARGBFloat;
        rt.filterMode = FilterMode.Point;
       // rt.name = "RenderTexture";
        rt.Create();
        return rt;
    }

    void initBuffer()
    {
        renderTexture = new RenderTexture[RenderTargetNum];
        renderBuffer = new RenderBuffer[RenderTargetNum];
        var vertexCount = model.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertexCount;
        var res = new Vector2(vertexCount, 1);
        for(int i = 0; i < RenderTargetNum; i++)
        {
            renderTexture[i] = CreateRenderTexture(res);
            //renderTexture[i] = new RenderTexture((int)res.x, (int)res.y, 0, RenderTextureFormat.ARGBFloat);
            //renderTexture[i].format = RenderTextureFormat.ARGBFloat;
            //renderTexture[i].filterMode = FilterMode.Point;
            //renderTexture[i].name = "RenderTexture" + i.ToString();
            renderBuffer[i] = renderTexture[i].colorBuffer;
        }
        depthBuffer = new RenderTexture((int)res.x, (int)res.y, 24, RenderTextureFormat.Depth).depthBuffer;
    }


    void OnGUI()
    {
        float w = Screen.width / 6;
        float h = Screen.height / 6;
        var vertexCount = model.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertexCount;
        /*
        for(int i = 0; i < renderTexture.Length; i++)
        {
            if(renderTexture[i] != null)
            {
                GUI.DrawTexture(new Rect(0,  - i * 60, 300, 30), renderTexture[i]);
                Debug.Log(i);
            }
        }*/
        if(isDebug)
        {
            GUI.DrawTexture(new Rect(0, 0, 300, 30), renderTexture[0]);
            GUI.DrawTexture(new Rect(0, 60, 300, 30), renderTexture[1]);
        }
    }
}

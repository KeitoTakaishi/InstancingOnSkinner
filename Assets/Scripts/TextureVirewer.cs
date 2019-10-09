using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class TextureVirewer : MonoBehaviour
{
    [SerializeField] private  GameObject cam;
    [SerializeField] private GameObject[] viewer;
    void Start()
    {
        var baker = cam.GetComponent<AttributeBaker>();
        var textureNum = baker.MrtTexturees.Length;
        for(int i = 0; i < baker.MrtTexturees.Length; i++)
        {
           viewer[i].GetComponent<RawImage>().texture = baker.MrtTexturees[i];
        }
    }
}

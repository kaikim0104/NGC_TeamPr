using System;
using System.Collections.Generic;
using Google.FlatBuffers;
using InputData.Platform;
using Unity.VisualScripting;
using UnityEngine;

public class Map : MonoBehaviour
{
    //¾À¿¡ ÀÖ´Â ¸ðµç ÇÃ·§ÆûÀ» ´ã´Â ¹è¿­
    private Dictionary<byte, Platform> _platfomrs;
    private List<Transform> _startPos = new List<Transform>(2);
    private void Start()
    {
        //¾À¿¡ ÀÖ´Â ¸ðµç ÇÃ·§ÆûÀ» °¡Á®¿È
        Platform[] platforms = GetComponentsInChildren<Platform>();

        //ÇÃ·§Æû °³¼ö ¸¸Å­ µñ¼Å³Ê¸® °ø°£ ¸¶·Ã
        _platfomrs = new Dictionary<byte, Platform>(platforms.Length);

        //Debug.Log($"{platforms} != null : {platforms != null}");
        //Debug.Log(platforms);

        //¸ðµç ÇÃ·§ÆûÀ» ÃÊ±âÈ­ÇÔ
        foreach (var platform in platforms)
        {
            platform.Init();
            _platfomrs.Add(platform.Id, platform);
        }
    }
    public Platform FindPlatform(byte id)
    {
        //¾ÆÀÌµð °ªÀ» ÅëÇØ ÇÃ·§ÆûÀ» Ã£À½
        Platform platform = _platfomrs[id];
        return platform;

    }
    internal void SetPos()
    {
        if(BackEnd.Backend.Match.IsSuperGamer())
        {
            transform.position = _startPos[0].position;
        }
        else
        {
            transform.position = _startPos[1].position;
        }
    }
}

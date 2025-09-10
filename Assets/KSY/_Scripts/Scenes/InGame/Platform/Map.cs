using System;
using System.Collections.Generic;
using BackEnd;
using Google.FlatBuffers;
using InputData.Map;
using Unity.VisualScripting;
using UnityEngine;

public class Map : MonoBehaviour
{
    //¾À¿¡ ÀÖ´Â ¸ðµç ÇÃ·§ÆûÀ» ´ã´Â ¹è¿­

    [field:SerializeField]public Spawner SpawnerCompo { get; private set; }
    [SerializeField] private Transform[] _startPos = new Transform[2];

    private Dictionary<byte, Platform> _platfomrs;
    #region Unity Event Function
    private void Start()
    {
        //¾À¿¡ ÀÖ´Â ½ºÆ÷³Ê¸¦ °¡Á®¿È
        this.SpawnerCompo = GetComponentInChildren<Spawner>();
        //¾À¿¡ ÀÖ´Â ¸ðµç ÇÃ·§ÆûÀ» °¡Á®¿È
        Platform[] platforms = GetComponentsInChildren<Platform>();

        //ÇÃ·§Æû °³¼ö ¸¸Å­ µñ¼Å³Ê¸® °ø°£ ¸¶·Ã
        _platfomrs = new Dictionary<byte, Platform>(platforms.Length);

        //¸ðµç ÇÃ·§ÆûÀ» ÃÊ±âÈ­ÇÔ
        foreach (var platform in platforms)
        {
            platform.Init();
            _platfomrs.Add(platform.Id, platform);
        }
    }
    private void OnValidate()
    {

    }
    #endregion
    public Platform FindPlatform(byte id)
    {
        //¾ÆÀÌµð °ªÀ» ÅëÇØ ÇÃ·§ÆûÀ» Ã£À½
        Platform platform = _platfomrs[id];
        return platform;

    }
    public void SetPlayerStartPos(Player p)
    {
        if(Server.IsSuperGamer)
        {
            if(_startPos[0] != null)
            {
                p.transform.position = _startPos[0].position;
                _startPos[0] = null;
            }
            else
            {
                p.transform.position = _startPos[1].position;
            }
        }
        else
        {
            if (_startPos[1] != null)
            {
                p.transform.position = _startPos[1].position;
                _startPos[1] = null;
            }
            else
            {
                p.transform.position = _startPos[0].position;
            }
        }
    }
}

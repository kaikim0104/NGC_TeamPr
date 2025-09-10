using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static UnityEditor.Progress;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour, IReceiver, ISender
{
    [Header("스폰 포인트 & 아이템")]
    [SerializeField] private GameObject[] SpawnerPoints;
    [SerializeField] private GameObject[] ItemPrefabs;

    [Header("스폰 타이머 & 최대 아이템 갯수")]
    [SerializeField] private float Spawnertimer = 2;
    [SerializeField] private int MaxCount = 5;

    [SerializeField] private GameObject spawnObject;


    public Dictionary<ushort, GameObject> Items = new Dictionary<ushort, GameObject>();
    private int _itemCount;
    private float _currentTimer;

    public static Action OnItemSpawned;
    public static Action OnItemCollected;

    //network data
    private ushort _spawnItemId;
    private byte _spawnPointIndex;
    private byte _spawnItemIndex;

    private void Start()
    {
        _itemCount = 0;
        _currentTimer = 0;
    }

    private void OnEnable()
    {
        OnItemSpawned += IncreaseItemCount;
        OnItemCollected += DecreaseItemCount;
    }

    private void OnDisable()
    {
        OnItemSpawned -= IncreaseItemCount;
        OnItemCollected -= DecreaseItemCount;
    }

    private void Update()
    {
        if (Server.IsSuperGamer && Game.Instance.AllUserReady)
        {
            CreateItem();
        }
    }
    private void Receive()
    {
        Debug.Log("Receive");
        if ((_itemCount < MaxCount))
        {
            if (SpawnerPoints[_spawnPointIndex] != null)
            {
                //0번부터 아이템 배열의 길이까지 인덱스를 랜덤하게 구해서 랜덤한 아이템 객체를 가져옴
                GameObject itemPrefab = ItemPrefabs[_spawnItemIndex];
                //랜덤한 아이템 스폰 포인트의 위치를 가져옴
                Vector2 spawnPos = SpawnerPoints[_spawnPointIndex].transform.position;
                //가져온 아이템을 스폰 포인트의 위치로 생성시킴.
                spawnObject = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                Items.Add(_spawnItemId, spawnObject);
            }
        }
    }
    private void CreateItem()
    {
        if (_itemCount > MaxCount)
        {
            _currentTimer = 0;
        }
        if (_itemCount < MaxCount)
        {
            _currentTimer += Time.deltaTime;
            if (_currentTimer >= Spawnertimer)
            {
                _currentTimer = 0;
                _spawnPointIndex = (byte)Random.Range(0, SpawnerPoints.Length);

                while (SpawnerPoints[_spawnPointIndex] == null)
                {
                    _spawnPointIndex = (byte)Random.Range(0, SpawnerPoints.Length);
                }

                if (SpawnerPoints[_spawnPointIndex] != null)
                {
                    //0번부터 아이템 배열의 길이까지 인덱스를 랜덤하게 구해서 랜덤한 아이템 객체를 가져옴
                    _spawnItemIndex = (byte)Random.Range(0, ItemPrefabs.Length);
                    GameObject itemPrefab = ItemPrefabs[_spawnItemIndex];

                    //랜덤한 아이템 스폰 포인트의 위치를 가져옴
                    Vector3 spawnPos = SpawnerPoints[_spawnPointIndex].transform.position;
                    //가져온 아이템을 스폰 포인트의 위치로 생성시킴.
                    spawnObject = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                    _spawnItemId = spawnObject.GetComponent<Item>().Id;
                    Items.Add(_spawnItemId, spawnObject);
                    //어떤 아이템을 어떤 위치로 생성시켰는지 전송.
                    Send();
                }
            }
        }
    }
    public GameObject FindItem(ushort id)
    {
        GameObject item = Items[id];
        return item;
    }
    private void IncreaseItemCount()
    {
        _itemCount++;
    }
    private void DecreaseItemCount()
    {
        _itemCount--;
    }

    public void ApplyByteData(byte byteData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public void ApplyByteData(byte spawnItemIndex, byte spawnPointIndex)
    {
        _spawnItemIndex = spawnItemIndex;
        _spawnPointIndex = spawnPointIndex;
        Receive();
    }
    public void ApplySbyteData(sbyte sbyteData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public void ApplySbyteData(sbyte sbyteData1, sbyte sbyteData2, sbyte sbyteData3)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public void Send()
    {
        byte[] bff = Server.Instance.SerializationSpawnerInfoData(_spawnItemId ,_spawnItemIndex, _spawnPointIndex);
        Server.Instance.Send(bff);
    }
    public void ApplyUShortData(ushort id)
    {
        _spawnItemId = id;
    }
    public void ApplySbyteData(sbyte sbyteData1, sbyte sbyteData2)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
}

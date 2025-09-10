using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour, IReceiver, ISender
{
    [Header("SO")]
    [SerializeField] SpawnerDataSO _spawnerData;


    [Header("SO Data")]
    //스폰 시간
    [SerializeField] private float _spawnSecconds = 0f;
    //아이템 최대 스폰 갯수를 설정하는 변수
    [SerializeField] private int _maxCount;
    //생성할 아이템의 종류
    [SerializeField] private GameObject[] _itemPrefabs;


    [Header("Spawned Item")]
    [SerializeField] private GameObject spawnObject;


    public Dictionary<ushort, GameObject> _items = new Dictionary<ushort, GameObject>();

    //스폰 시간을 체크하는 변수
    private float _spawnTimmer = 0f;

    //아이템의 현재 갯수를 가지는 변수
    private int _itemCount = 0;

    public static Action OnItemSpawned;
    public static Action OnItemCollected;

    private Transform[] _spawnerPoints;

    //network data
    private ushort _spawnItemId;
    private byte _spawnPointIndex;
    private byte _spawnItemIndex;

    private void Awake()
    {
        _spawnerPoints = GetComponentsInChildren<Transform>();
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
        if (Server.IsSuperGamer && Game.Instance.IsAllReady)
        {
            CreateItem();
        }
    }
    private void OnValidate()
    {
        _itemPrefabs = _spawnerData.ItemPrefabs;
        _maxCount = _spawnerData.MaxCount;
        _spawnSecconds = _spawnerData.SpawnSecconds;
    }
    private void Receive()
    { 
        if ((_itemCount < _maxCount))
        {
            if (_spawnerPoints[_spawnPointIndex] != null)
            {
                //0번부터 아이템 배열의 길이까지 인덱스를 랜덤하게 구해서 랜덤한 아이템 객체를 가져옴
                GameObject itemPrefab = _itemPrefabs[_spawnItemIndex];
                //랜덤한 아이템 스폰 포인트의 위치를 가져옴
                Vector2 spawnPos = _spawnerPoints[_spawnPointIndex].transform.position;
                //가져온 아이템을 스폰 포인트의 위치로 생성시킴.
                spawnObject = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                _items.Add(_spawnItemId, spawnObject);
            }
        }
    }
    private void CreateItem()
    {
        if (_itemCount > _maxCount)
        {
            _spawnTimmer = 0;
        }
        if (_itemCount < _maxCount)
        {
            _spawnTimmer += Time.deltaTime;
            if (_spawnTimmer >= _spawnSecconds)
            {
                _spawnTimmer = 0;
                _spawnPointIndex = (byte)Random.Range(0, _spawnerPoints.Length);

                while (_spawnerPoints[_spawnPointIndex] == null)
                {
                    _spawnPointIndex = (byte)Random.Range(0, _spawnerPoints.Length);
                }

                if (_spawnerPoints[_spawnPointIndex] != null)
                {
                    //0번부터 아이템 배열의 길이까지 인덱스를 랜덤하게 구해서 랜덤한 아이템 객체를 가져옴
                    _spawnItemIndex = (byte)Random.Range(0, _itemPrefabs.Length);
                    GameObject itemPrefab = _itemPrefabs[_spawnItemIndex];

                    //랜덤한 아이템 스폰 포인트의 위치를 가져옴
                    Vector3 spawnPos = _spawnerPoints[_spawnPointIndex].transform.position;
                    //가져온 아이템을 스폰 포인트의 위치로 생성시킴.
                    spawnObject = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                    _spawnItemId = spawnObject.GetComponent<Item>().Id;
                    _items.Add(_spawnItemId, spawnObject);
                    //어떤 아이템을 어떤 위치로 생성시켰는지 전송.
                    Send();
                }
            }
        }
    }
    public GameObject FindItem(ushort id)
    {
        GameObject item = _items[id];
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

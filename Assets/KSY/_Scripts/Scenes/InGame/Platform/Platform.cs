using System;
using SDW;
using UnityEditor;
using UnityEngine;
using static BackendFunctionInGame;

public class Platform : MonoBehaviour, IReceiver, ISender
{
    public static byte Count = 0;
    [field: SerializeField] public byte Id { get; private set; } = 0;

    //송/수신 가능한 플랫폼 중 하나를 할당 받음
    private FallingPlatform _timmerPlatform;
    private BreakablePlatform _breakablePlatform;
    public void Init()
    {
        //초기화 설정 당시 플랫폼의 개수값을 id 값으로 지정
        Debug.Log($"Platform Id : {Id}");
        Id = Count;
        Count++;

        //자기 자신의 플랫폼 타입을 받아옴
        TryGetComponent(out _timmerPlatform);
        TryGetComponent(out _breakablePlatform);
    }

    //Network
    public virtual void ApplySbyteData(sbyte sbyteData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public void ApplySbyteData(sbyte sbyteData1, sbyte sbyteData2, sbyte sbyteData3)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public void ApplyByteData(byte byteData)
    {
        //플랫폼 상태 적용
        if(_timmerPlatform != null)
        {
            //플랫폼의 상태 데이터 가져오기
            byte state = byteData;

            //다른 플레이어가 플랫폼 위에 있는지 확인 여부
            bool isOnStep = (state & (byte)flagPlatformState.IsOnPlatform) != 0;

            //확인 여부를 적용.
            _timmerPlatform.isOnPlatform = isOnStep;
        }
        else if(_breakablePlatform != null)
        {
            //플랫폼의 상태 데이터 가져오기
            byte state = byteData;

            //다른 플레이어가 플랫폼을 부쉈는지 확인 여부
            bool isBroken = (state & (byte)flagPlatformState.IsBrokenPlatform) != 0;

            //확인 여부를 적용.
            _breakablePlatform.IsBreaking = isBroken;
        }
    }
    public void SendData(bool stateInfo)
    {
        if (_timmerPlatform != null)
        {
            byte[] bff = Server.Instance.SerializationPlatformStateData(Id, stateInfo, false);
            Server.Instance.SnedData(bff);
        }
        else if (_breakablePlatform != null)
        {
            byte[] bff = Server.Instance.SerializationPlatformStateData(Id, false, stateInfo);
            Server.Instance.SnedData(bff);
        }
    }
    public void SendData()
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");

    }
}


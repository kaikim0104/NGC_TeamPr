using System;
using BackEnd;
using BackEnd.Tcp;
using Google.FlatBuffers;
using InputData.Map;
using InputData.Player;
using UnityEngine;
public enum MatchEventType
{
    None = 0,
    //매칭 시작시
    OnEnterFindingMatch,
    //매칭 성사시
    OnFindedMatch,
    //매칭 캔슬시
    OnMatchCanceled
}
public class Server : SingletonBehaviour<Server>
{
    private BackendFunctionInGame _bfInGame;
    private BackendFunctionsAccount _bfAccount;
    private BackendFunctionMatch _bfMatch;
    private UserData _myData = new UserData();
    private UserData _otherData = new UserData();
    public static bool IsSuperGamer { get; private set; } = false;
    private void Awake()
    {
        base.Awake();
        InitBF();
    }
    private void InitBF()
    {
        if (_bfInGame == null && !TryGetComponent(out _bfInGame))
        {
            gameObject.AddComponent<BackendFunctionInGame>();
            _bfInGame = GetComponent<BackendFunctionInGame>();
        }

        if (_bfAccount == null && !TryGetComponent(out _bfAccount))
        {
            gameObject.AddComponent<BackendFunctionsAccount>();
            _bfAccount = GetComponent<BackendFunctionsAccount>();
        }

        if (_bfMatch == null && !TryGetComponent(out _bfMatch))
        {
            gameObject.AddComponent<BackendFunctionMatch>();
            _bfMatch = GetComponent<BackendFunctionMatch>();
        }

        _bfMatch.EnterRoom += () =>
        {
            IsSuperGamer = Backend.Match.IsSuperGamer();
        };
    }
    public void InitOtherData(MatchUserGameRecord otherInfo)
    {
        string nickname = otherInfo.m_nickname;

        //받아왔던 데이터를 할당.
        _otherData.nickname = nickname;

        //데이터 초기화를 표시
        _otherData.hasInit = true;
    }
    public void InitMyData()
    {
        //서버에서 내 계정에 맞는 데이터를 가져옴
        var bro_GetUserInfo = Backend.BMember.GetUserInfo();

        //받아온 데이터에서 닉네임을 가져옴
        string nickname = bro_GetUserInfo.GetReturnValuetoJSON()["row"]["nickname"].ToString();

        //받아온 데이터를 할당.
        _myData.nickname = nickname;

        //자주 데이터를 쓰는 곳에 자원 절약을 위해 할당해놈
        _bfMatch.myNickname = nickname;

        //데이터 초기화를 표시
        _myData.hasInit = true;
    }
    public UserData? GetMyData()
    {
        //초기화된 값일 경우 반환. 아닐 경우 null 반환
        if (_myData.hasInit == false)
        {
            return null;
        }
        else
        {
            return _myData;
        }
    }
    public UserData? GetOtherData()
    {
        //초기화된 값일 경우 반환. 아닐 경우 null 반환
        if (_otherData.hasInit == false) 
        { 
            return null; 
        }
        else
        {
            return _otherData;
        }
    }
    public void Send(byte[] bff)
    {
        _bfInGame.Send(bff);
    }
    public void ApplyData(PlayerMessage message, IReceiver Receiver, PlayerMessageType tpye)
    {
        _bfInGame.ApplyData(message, Receiver, tpye);
    }
    public void ApplyData(MapMessage message, IReceiver Receiver, MapMessageType type)
    {
        _bfInGame.ApplyData(message, Receiver, type);
    }
    public bool TryInitialize()
    {
        //초기화 시도
        BackendReturnObject bro_Initialize = Backend.Initialize();
        //초기화 성공 처리
        if (bro_Initialize.IsSuccess())
        {
            return true;
        }
        // 초기화 실패 처리
        else
        {
            return false;
        }
    }
    public bool TryReconnect()
    {
        Debug.Log("TryReconnect");
        return _bfMatch.TryReconnect();
    }
    public void Login(string id, string pw)
    {
        _bfAccount.Login(id,pw);
    }
    public void Login(string id, string pw, Action<bool> OnTryMatchServer, Action<int> OnTryLogin)
    {
        _bfAccount.Login(id, pw, OnTryMatchServer, OnTryLogin);
    }
    public int TrySignup(string id, string pw)
    {
        return _bfAccount.Signup(id,pw);
    }
    public int TryUpdateNickname(string nickName)
    {
        return _bfAccount.UpdateNickname(nickName);
    }
    public void FindMatch()
    {
        _bfMatch.FindMatch();
    }
    public void AddMatchEvent(MatchEventType t, Action eventHandler)
    {
       switch(t)
        {
            case MatchEventType.None:
                {
                    Debug.Log("Error");
                    break;
                }
            case MatchEventType.OnEnterFindingMatch:
                {
                    _bfMatch.EnterMatch += eventHandler;
                    break;
                }
            case MatchEventType.OnFindedMatch:
                {
                    _bfMatch.SuccessMatch += eventHandler;
                    break;
                }
            case MatchEventType.OnMatchCanceled:
                {
                    _bfMatch.CanceledMatch += eventHandler;
                    break;
                }
        }
    }
    public byte[] SerializationPlatformStateData(byte id, bool isOnPlatform, bool isBrokenPlatform)
    {
        return _bfInGame.SerializationPlatformStateData(id, isOnPlatform, isBrokenPlatform);
    }
    public byte[] SerializationSpawnerInfoData(ushort spawnItemId, byte spawnItemIndex,byte spawnPotinIndex)
    {
        return _bfInGame.SerializationSpawnerInfoData(spawnItemId, spawnItemIndex, spawnPotinIndex);
    }
    public byte[] SerializationActionData(ushort itemId, bool isHolding, bool isThrowing, byte chargeGauge, Vector2 throwDir)
    {
        return _bfInGame.SerializationActionData(itemId, isHolding, isThrowing, chargeGauge, throwDir);
    }
    public byte[] SerializationPlayerMovementData(Vector2 dashDir,sbyte moveX, bool usingJump, bool usingDash, bool isDashing, bool usingDownDash)
    {
        return _bfInGame.SerializationPlayerMovementData(dashDir, moveX, usingJump, usingDash, isDashing, usingDownDash);
    }
}



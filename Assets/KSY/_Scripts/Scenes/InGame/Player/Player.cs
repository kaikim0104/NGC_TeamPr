using System;
using BackEnd;
using BackEnd.Tcp;
using Google.FlatBuffers;
using InputData.Map;
using InputData.Player;
using UnityEngine;
using UnityEngine.InputSystem;

//Main Receiver
public class Player : MonoBehaviour, IReceiver, ISender
{
    //User Data
    private UserData _myData = new UserData();
    [SerializeField] private string Nickname;

    //My palyer
    [SerializeField] private InputActionAsset InputSetting;
    private PlayerInput _playerInput;
    private string _actionMap = "Player";
    private MyMovement _myMovement;
    private MyAction _myAction;

    byte[] receiveBff = new byte[64];

    //Other player
    private OtherMovement _otherMovement;
    private OtherAction _otherAction;

    public void Init()
    {
        //나의 플레이어라면
        if (Nickname == Server.Instance.GetMyData().Value.nickname)
        {
            //송신용 플레이어 스크립트 추가
            if (!TryGetComponent(out _myMovement)) _myMovement = gameObject.AddComponent<MyMovement>();
            if (!TryGetComponent(out _myAction)) _myAction = gameObject.AddComponent<MyAction>();

            //인풋 시스템 세팅
            if(!TryGetComponent(out _playerInput)) _playerInput = gameObject.AddComponent<PlayerInput>();
            _playerInput.actions = InputSetting;
            _playerInput.defaultActionMap = _actionMap;
            _playerInput.actions.Enable();
        }
        //다른 사람의 플레이어라면
        else
        {
            //수신용 플레이어 스크립트 추가
            if (!TryGetComponent(out _otherMovement)) _otherMovement = gameObject.AddComponent<OtherMovement>();
            if (!TryGetComponent(out _otherAction)) _otherAction = gameObject.AddComponent<OtherAction>();

            //수신 이벤트 추가
            Backend.Match.OnMatchRelay += ReceiveData;
        }
    }
    private void ReceiveData(MatchRelayEventArgs args)
    {
        if (args.From.NickName == Nickname)
        {
            Debug.Log("Receive");

            //수신 받은 데이터를 버퍼에 담기
            receiveBff = args.BinaryUserData;
            ByteBuffer _receiveBff = new ByteBuffer(receiveBff);

            //플레이어 관련 데이터가 맞다면 수신 시도
            if (PlayerMessage.PlayerMessageBufferHasIdentifier(_receiveBff))
            {
                PlayerMessage message = PlayerMessage.GetRootAsPlayerMessage(_receiveBff);
                PlayerMessageType messageType = message.DataType;

                switch (messageType)
                {
                    case PlayerMessageType.movement:
                        {
                            Server.Instance.ApplyData(message, _otherMovement, messageType);
                            break;
                        }
                    case PlayerMessageType.item_action:
                        {
                            Server.Instance.ApplyData(message, _otherAction, messageType);
                            break;
                        }
                    default:
                        {
                            Debug.LogError("this enum value is nonexistent");
                            break;
                        }
                }

            }
            //플랫폼 관련 데이터라면 넘겨주기;
            else if (MapMessage.MapMessageBufferHasIdentifier(_receiveBff))
            {
                MapMessage message = MapMessage.GetRootAsMapMessage(_receiveBff);
                MapMessageType messageType = message.MapMessageTypeType;

                //찾은 플랫폼에 수신받은 데이터를 적용함.
                Server.Instance.ApplyData(message, null, messageType);
            }
        }
    }
    public void GetUserData(UserData userData)
    {
        //userData 할당
        _myData = userData;

        //할당된 userData를 기반으로 인스턴스 멤버 초기화
        Nickname = _myData.nickname;

        //플레이어 객체에 userData가 할당되었음을 표시
        _myData.hasInit = true;
    }

    //IReceiver
    public virtual void ApplyByteData(byte byteData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public virtual void ApplyByteData(byte byteData1, byte byteData2)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public virtual void ApplySbyteData(sbyte sbyteData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public virtual void ApplySbyteData(sbyte sbyteData1, sbyte sbyteData2, sbyte sbyteData3)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public virtual void ApplySbyteData(sbyte dirX, sbyte dirY)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public virtual void Send()
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
    public virtual void ApplyUShortData(ushort ushortData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");
    }
}

using System;
using BackEnd;
using BackEnd.Tcp;
using InputData.Platform;
using InputData.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.GPUSort;

//Main Receiver
public class Player : MonoBehaviour, IReceiver, ISender
{
    //User Data
    private UserData _myData = new UserData();
    [SerializeField] private string _nickname;

    //Player
    [SerializeField] private InputActionAsset _inputSetting;
    private MyMovement _myMovement;
    private OtherMovement _otherMovement;
    private string _actionMap = "Player";
    public void Init()
    {
        //나의 플레이어라면
        if (_nickname == Server.Instance.GetMyData().Value.nickname)
        {
            //입력을 받는 movement 추가
            _myMovement = gameObject.AddComponent<MyMovement>();
            Debug.Log($"_myMovement is not null : {_myMovement != null}");

            //인풋 시스템 세팅
            PlayerInput input = gameObject.AddComponent<PlayerInput>();
            input.actions = _inputSetting;
            input.defaultActionMap = _actionMap;
            input.actions.Enable();
            input.actions.Enable();
        }
        else
        {
            //아니라면 수신받는 movement 추가
            _otherMovement = gameObject.AddComponent<OtherMovement>();
        }

        //만약 내가 other (수신만 받는 객체)라면
        if (_otherMovement != null)
        {
            //메세지가 브로드 캐스팅 되었을 때 호출 (자기자신 포함)
            Backend.Match.OnMatchRelay += ReceiveData;
        }
    }

    private void ReceiveData(MatchRelayEventArgs args)
    {
        if (args.From.NickName == _nickname)
        {
            Debug.Log("Receive");

            //수신 받은 데이터를 버퍼에 담기
            byte[] receiveBff = args.BinaryUserData;
            var _receiveBff = new Google.FlatBuffers.ByteBuffer(receiveBff);

            //Debug.Log($"PlayerMessageBufferHasIdentifier: {PlayerMessage.PlayerMessageBufferHasIdentifier(_receiveBff)}");
            //Debug.Log($"PlatformMessageBufferHasIdentifier: {PlatformMessage.PlatformMessageBufferHasIdentifier(_receiveBff)}");

            //플레이어 관련 데이터가 맞다면 수신 시도
            if (PlayerMessage.PlayerMessageBufferHasIdentifier(_receiveBff))
            {
                Server.Instance.ApplyData(_receiveBff, _otherMovement);
            }
            //플랫폼 관련 데이터라면 넘겨주기;
            else if (PlatformMessage.PlatformMessageBufferHasIdentifier(_receiveBff))
            {
                //(송신한)수신 받을 플랫폼의 아이디를 찾음
                var message = PlatformMessage.GetRootAsPlatformMessage(_receiveBff);
                byte senderId = message.SenderInfo.Value.Id;

                //(송신한) 수신 받을 플랫폼을 아이디로 찾음
                Platform platform = Game.Instance.Map.FindPlatform(senderId);

                //찾은 플랫폼에 수신받은 데이터를 적용함.
                Server.Instance.ApplyData(_receiveBff, platform);
            }
        }
    }

    //플레이어 시작 위치를 결정하고 정보를 넘김
    public void SetPos()
    {
        Game.Instance.Map.SetPos();
    }
    public void GetUserData(UserData userData)
    {
        //Debug.Log($"Player Data Set : {userData}");
        //userData 할당
        _myData = userData;

        //할당된 userData를 기반으로 인스턴스 멤버 초기화
        _nickname = _myData.nickname;

        //플레이어 객체에 userData가 할당되었음을 표시
        _myData.hasInit = true;
    }

    //IReceiver
    public virtual void ApplyByteData(byte byteData)
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
    public virtual void SendData()
    {
        if (_myMovement == null) 
        { 
            _myMovement = GetComponent<MyMovement>();
            Debug.Log($"_myMovement is not null : {_myMovement != null}");
            Debug.Log($"_myMovement.DashDir is not null : {_myMovement.DashDir != null}");
        }

        Vector2 dashDir = _myMovement.DashDir;
        sbyte moveX = _myMovement.MoveX;
        bool usingJump = _myMovement.UsingJump;
        bool usingDash = _myMovement.UsingDash;
        bool isDashing = _myMovement.IsDashing;

        byte[] bff = Server.Instance.SerializationPlayerMovementData(dashDir, moveX, usingJump, usingDash, isDashing);
        Server.Instance.SnedData(bff);
    }
    public void SendData(bool boolenData)
    {
        throw new NotImplementedException("If you want to use this method, you must override it.");

    }
}

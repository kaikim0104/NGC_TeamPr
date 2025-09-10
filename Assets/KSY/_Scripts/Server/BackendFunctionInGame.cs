using System;
using BackEnd;
using Google.FlatBuffers;
using InputData.Platform;
using InputData.Player;
using UnityEngine;

public class BackendFunctionInGame : MonoBehaviour
{
    private readonly FlatBufferBuilder _movementBuilder = new FlatBufferBuilder(32);
    private readonly FlatBufferBuilder _platformStateBuilder = new FlatBufferBuilder(32);
    private readonly FlatBufferBuilder _itemActionBuilder = new FlatBufferBuilder(32);

    [Flags]
    public enum flagPlayerMovementState : byte
    {
        //0
        None = 0b0000,

        //1 
        IsJumping = 0b0001,

        //2
        IsDashing = 0b0010,

        //4
        UsingDash = 0b0100
    }
    [Flags]
    public enum flagPlatformState : byte
    {
        //0
        None = 0b0000,

        //1
        IsOnPlatform = 0b0001,

        //2
        IsBrokenPlatform = 0b0010
    }
    [Flags]
    public enum flagPlayerItemState : byte
    {
        //0
        None = 0b0000,

        //1 
        hasItem = 0b0001,

        //2
        isShootingItem = 0b0010,
    }

    //데이터 직렬화
    public byte[] SerializationPlatformStateData(byte id, bool isOnPlatform, bool isBrokenPlatform)
    {
        //버퍼 재사용
        _platformStateBuilder.Clear();

        //비트 마스킹
        byte platformState = 0b0000;

        if (isOnPlatform) platformState |= (byte)flagPlatformState.IsOnPlatform;
        if (isBrokenPlatform) platformState |= (byte)flagPlatformState.IsBrokenPlatform;

        //오프셋 세팅
        Offset<PlatformState> offsetPlatformState = PlatformState.CreatePlatformState(_platformStateBuilder, platformState);

        //StartPlatformMessage, 데이터 할당
        PlatformMessage.StartPlatformMessage(_platformStateBuilder);
        PlatformMessage.AddDataType(_platformStateBuilder, PlatformMessageType.state);
        PlatformMessage.AddData(_platformStateBuilder, offsetPlatformState.Value);
        PlatformMessage.AddSenderInfo(_platformStateBuilder, PlatformInfo.CreatePlatformInfo(_platformStateBuilder,id));

        //EndPlatformMessage, 총 데이터 오프셋 세팅
        Offset<PlatformMessage> offsetResultData = PlatformMessage.EndPlatformMessage(_platformStateBuilder);

        //스키마 버퍼화
        //PlatformMessage.FinishPlatformMessageBuffer(_platformStateBuilder, offsetResultData);
        _platformStateBuilder.Finish(offsetResultData.Value, "PLFM");
        byte[] bff = _platformStateBuilder.SizedByteArray();

        return bff;
    }
    public byte[] SerializationPlayerItemData(bool hasItem, bool isShootingItem)
    {
        //버퍼 재사용
        _itemActionBuilder.Clear();

        //비트 마스킹
        byte State = 0b0000;

        if (hasItem) State |= (byte)flagPlayerItemState.hasItem;
        if (isShootingItem) State |= (byte)flagPlayerItemState.isShootingItem;

        //오프셋 세팅 + 데이터 할당
        Offset<ItemAction> offsetActionData = ItemAction.CreateItemAction(_itemActionBuilder, State);
        Offset<PlayerMessage> offsetResultData = PlayerMessage.CreatePlayerMessage(_itemActionBuilder, PlayerMessageType.item_action, offsetActionData.Value);

        //스키마 버퍼화
        _itemActionBuilder.Finish(offsetResultData.Value, "PLYR");
        byte[] bff = _itemActionBuilder.SizedByteArray();

        return bff;
    }
    public byte[] SerializationPlayerMovementData(Vector2 dashDir, sbyte dataMoveX, bool dataIsGrounded, bool dataCanDash, bool dataIsDashing)
    {
        //버퍼 재사용
        _movementBuilder.Clear();

        //이동 방향값
        sbyte moveX = dataMoveX;
        sbyte dashX = (sbyte)dashDir.x;
        sbyte dashY = (sbyte)dashDir.y;

        //비트 마스킹
        byte State = 0b0000;

        if (dataIsGrounded) State |= (byte)flagPlayerMovementState.IsJumping;
        if (dataIsDashing) State |= (byte)flagPlayerMovementState.IsDashing;
        if (dataCanDash) State |= (byte)flagPlayerMovementState.UsingDash;

        //오프셋 세팅 + 데이터 할당
        Offset<Movement> offsetMovementData = Movement.CreateMovement(_movementBuilder, State, moveX, dashX, dashY);
        Offset<PlayerMessage> offsetResultData = PlayerMessage.CreatePlayerMessage(_movementBuilder, PlayerMessageType.movement, offsetMovementData.Value);

        //스키마 버퍼화
        _movementBuilder.Finish(offsetResultData.Value, "PLYR");

        byte[] bff = _movementBuilder.SizedByteArray();

        Debug.Log($"PlayerMessageBufferHasIdentifier: {PlayerMessage.PlayerMessageBufferHasIdentifier(new ByteBuffer(bff))}");

        return bff;
    }

    //데이터 송신
    public void SnedData(byte[] bff)
    {
        Backend.Match.SendDataToInGameRoom(bff);
    }

    //데이터 수신
    public void ApplyData(ByteBuffer bff, IReceiver Receiver) 
    {
        //버퍼가 비어있다면 반환
        if (bff == null) return;

        //만약 플레이어 관련 메세지라면 처리
        if (PlayerMessage.PlayerMessageBufferHasIdentifier(bff))
        {
            Debug.Log("PlayerMessage.PlayerMessageBufferHasIdentifier == true");

            var message = PlayerMessage.GetRootAsPlayerMessage(bff);
            PlayerMessageType playerMessageType = message.DataType;

            //플레이어 관련 메세지일 시 처리
            switch (playerMessageType)
            {
                case PlayerMessageType.movement:
                    {
                        //데이터를 버퍼에서 꺼내옴 (역직렬화)
                        Movement data = message.DataAsmovement();
                        sbyte moveX = data.MoveX;
                        sbyte dashX = data.DashX;
                        sbyte dashY = data.DashY;
                        byte movementState = data.MovementState;

                        //데이터를 수신자에게 적용
                        Debug.Log("Start Receiver");
                        Receiver.ApplySbyteData(moveX, dashX, dashY);
                        Receiver.ApplyByteData(movementState);
                        Debug.Log("End Receiver");
                        break;
                    }

                //플레이어 아이템 액션 수신
                case PlayerMessageType.item_action:
                    {

                        break;
                    }
                default:
                    {
                        Debug.Log("Error");
                        break;
                    }
            }
        }
        //만약 플랫폼 관련 메세지라면 처리
        else if (PlatformMessage.PlatformMessageBufferHasIdentifier(bff))
        {
            Debug.Log("PlatformMessage.PlatformMessageBufferHasIdentifier == true");
            var message = PlatformMessage.GetRootAsPlatformMessage(bff);
            PlatformMessageType platfomrMessageType = message.DataType;

            //플랫폼 관련 메세지일 시 처리
            switch (platfomrMessageType)
            {
                case PlatformMessageType.state:
                    {
                        Debug.Log("Start PlatformMessageType.state");
                        PlatformState data = message.DataAsstate();
                        byte platformState = data.PlatformState_;
                        Receiver.ApplyByteData(platformState);
                        Debug.Log("End PlatformMessageType.state");
                        break;
                    }
                default:
                    {
                        Debug.Log("Error");
                        break;
                    }
            }
        }
        else
        {
            Debug.Log("Error");
        }
    }
}

using System;
using System.Runtime.InteropServices;
using BackEnd;
using Google.FlatBuffers;
using InputData.Map;
using InputData.Player;
using UnityEngine;

public class BackendFunctionInGame : MonoBehaviour
{
    private readonly FlatBufferBuilder _movementBuilder = new FlatBufferBuilder(32);
    private readonly FlatBufferBuilder _itemActionBuilder = new FlatBufferBuilder(32);

    private readonly FlatBufferBuilder _platformStateBuilder = new FlatBufferBuilder(32);
    private readonly FlatBufferBuilder _spawnerInfoBuilder = new FlatBufferBuilder(32);

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
    public enum flagActionState : byte
    {
        //0
        None = 0b0000,

        //1 
        IsHolding = 0b0001,

        //2
        IsThrowing = 0b0010,
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
        Offset<PlatformState> offsetPlatformState = PlatformState.CreatePlatformState(_platformStateBuilder, id, platformState);

        ////데이터 할당
        Offset<MapMessage> offsetResult = MapMessage.CreateMapMessage(_platformStateBuilder, MapMessageType.platform_state, offsetPlatformState.Value);

        //스키마 버퍼화
        _platformStateBuilder.Finish(offsetResult.Value, "MAPP");
        byte[] bff = _platformStateBuilder.SizedByteArray();

        return bff;
    }
    public byte[] SerializationSpawnerInfoData(ushort spawnItemId, byte spawnItemIndex, byte spawnPotinIndex)
    {
        //버퍼 재사용
        _spawnerInfoBuilder.Clear();

        //오프셋 세팅
        Offset<SpawnerInfo> offsetSpawnerInfo = SpawnerInfo.CreateSpawnerInfo(_spawnerInfoBuilder, spawnItemId, spawnItemIndex, spawnPotinIndex);

        //StartPlatformMessage, 데이터 할당
        Offset<MapMessage> offsetResult = MapMessage.CreateMapMessage(_spawnerInfoBuilder, MapMessageType.spawner_info, offsetSpawnerInfo.Value);

        //스키마 버퍼화
        _spawnerInfoBuilder.Finish(offsetResult.Value, "MAPP");
        byte[] bff = _spawnerInfoBuilder.SizedByteArray();

        return bff;
    }
    public byte[] SerializationActionData(ushort itemId, bool isHolding, bool isThrowing, byte chargeGauge, Vector2 throwDir)
    {
        //버퍼 재사용
        _itemActionBuilder.Clear();

        ushort id = itemId;
        sbyte x = (sbyte)throwDir.x;
        sbyte y = (sbyte)throwDir.y;

        //비트 마스킹
        byte state = 0b0000;

        if (isHolding) state |= (byte)flagActionState.IsHolding;
        if (isThrowing) state |= (byte)flagActionState.IsThrowing;

        //오프셋 세팅 + 데이터 할당
        Offset<ItemAction> offsetActionData = ItemAction.CreateItemAction(_itemActionBuilder, state, id, x, y, chargeGauge);
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

        return bff;
    }

    //데이터 송신
    public void Send(byte[] bff)
    {
        if (!Game.Instance.AllUserReady)
        {
            Debug.LogError("map un loaded.");
            return;
        }
        Backend.Match.SendDataToInGameRoom(bff);
    }

    //데이터 수신
    public void ApplyData(MapMessage message, IReceiver Receiver, MapMessageType type)
    {
        switch (type)
        {
            //플랫폼의 상태와 관련된 메세지 처리
            case MapMessageType.platform_state:
                {
                    //(송신한)수신 받을 플랫폼의 아이디를 찾음
                    byte senderId = message.MapMessageTypeAsplatform_state().Id;

                    //(송신한) 수신 받을 플랫폼을 아이디로 찾음
                    Platform platform = Game.Instance.Map.FindPlatform(senderId);

                    //MapMessage에서 데이터를 꺼내서 적용함.
                    PlatformState data = message.MapMessageTypeAsplatform_state();
                    byte platformState = data.PlatformState_;
                    platform.ApplyByteData(platformState);
                    break;
                }
            //아이템 스포너의 정보와 관련된 메세지 처리
            case MapMessageType.spawner_info:
                {
                    //씬에 있는 스포너를 가져옴
                    Receiver = Game.Instance.Map.Spawner;

                    SpawnerInfo data = message.MapMessageTypeAsspawner_info();
                    ushort spawnItemId = data.SpawnItemId;
                    byte spawnPointIndex = data.SpawnPointIndex;
                    byte spawnItemIndex = data.SpawnItemIndex;

                    Receiver.ApplyUShortData(spawnItemId);
                    Receiver.ApplyByteData(spawnItemIndex, spawnPointIndex);
                    break;
                }
            default:
                {
                    Debug.Log("Error");
                    break;
                }
        }
    }
    public void ApplyData(PlayerMessage message, IReceiver Receiver, PlayerMessageType type) 
    {
        //플레이어 관련 메세지일 시 처리
        switch (type)
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
                    Receiver.ApplySbyteData(moveX, dashX, dashY);
                    Receiver.ApplyByteData(movementState);
                    break;
                }
            //플레이어 아이템 액션 수신
            case PlayerMessageType.item_action:
                {
                    Debug.Log("Start item Action receive");

                    //데이터를 버퍼에서 꺼내옴 (역직렬화)
                    ItemAction data = message.DataAsitem_action();
                    byte state = data.ActionState;
                    byte charge = data.Charge;
                    ushort itemdID = data.ActionItemId;
                    sbyte x = data.ShotDirX;
                    sbyte y = data.ShotDirY;

                    //데이터를 수신자에게 적용
                    Receiver.ApplyUShortData(itemdID);
                    Receiver.ApplySbyteData(x, y);
                    Receiver.ApplyByteData(state, charge);

                    Debug.Log("End item Action receive");
                    break;
                }
            default:
                {
                    Debug.Log("Error");
                    break;
                }
        }
    }
}

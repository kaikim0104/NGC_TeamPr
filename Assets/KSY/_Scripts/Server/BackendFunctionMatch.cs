using System;
using System.Linq;
using BackEnd;
using BackEnd.Tcp;
using UnityEngine;
public class BackendFunctionMatch : MonoBehaviour
{
    //event
    public string myNickname = null; 
    public event Action EnterMatch, SuccessMatch, CanceledMatch;
    public event Action SuccessCreateRoom, FailedCreateRoom, EnterRoom;
    public void FindMatch()
    {
        //유저가 매칭을 신청, 취소 했을 때 그리고 매칭이 성사되었을 때 호출되는 이벤트 핸들러입니다.
        Backend.Match.OnMatchMakingResponse = (MatchMakingResponseEventArgs args) => {

            //매칭 신청 처리
            if (args.ErrInfo == ErrorCode.Match_InProgress) 
            {
                Debug.Log("매칭이 시작되었습니다.");
                EnterMatch?.Invoke();
            }

            //매칭 성사 처리
            if (args.MatchCardIndate != null && args.RoomInfo != null) 
            {
                Debug.Log("매칭이 성사되었습니다.");
                SuccessMatch?.Invoke();

                string serverAddress = args.RoomInfo.m_inGameServerEndPoint.m_address;
                ushort serverPort = args.RoomInfo.m_inGameServerEndPoint.m_port;
                string roomToken = args.RoomInfo.m_inGameRoomToken;
                bool isReconnecting = false;

                //인게임 서버 접속 
                JoinInGameServer(serverAddress, serverPort, roomToken, isReconnecting);
            }

            //매칭 취소 처리
            if (args.ErrInfo == ErrorCode.Match_MatchMakingCanceled) 
            {
                CanceledMatch.Invoke();
            }
        };

        //대기방을 생성하였을 때 호출되는 이벤트 핸들러입니다.
        Backend.Match.OnMatchMakingRoomCreate = (MatchMakingInteractionEventArgs args) =>
        {
            if(args.ErrInfo == ErrorCode.Success)
            {
                //대기방 생성 성공 이벤트 호출
                SuccessCreateRoom?.Invoke();

                //대기실이 생성되면 매칭을 곧바로 시작.
                MatchType random = MatchType.Random;
                MatchModeType oneOnOne = MatchModeType.OneOnOne;
                //매칭 카드 inDate  
                var matchCard = "2025-08-17T13:27:17.823Z";

                //매칭 요청
                Backend.Match.RequestMatchMaking(random, oneOnOne, matchCard);
            }
            else
            {
                //대기방 생성 실패 이벤트 호출
                FailedCreateRoom?.Invoke();
                Debug.LogError("대기방을 만드는 것에 실패했습니다.");
            }
        };

        //대기방 생성 시도
        Backend.Match.CreateMatchRoom();
    }
    public void JoinInGameServer(string serverAddress, ushort serverPort, string roomToken, bool isReconnecting)
    {
        //유저가 게임방에 입장할 때마다 호출되는 이벤트입니다.
        //+ 자기 자신에게도 호출됨.
        Backend.Match.OnMatchInGameAccess += (MatchInGameSessionEventArgs args) => {
            //나의 입장 수신이라면 반환
            if (args.GameRecord.m_nickname == myNickname)
            {
                return;
            }

            //Debug.Log($"{args.GameRecord.m_nickname} != {myNickname}");

            //아니라면 상대방 정보를 가져옴
            MatchUserGameRecord otherInfo = args.GameRecord;
            Server.Instance.InitOtherData(otherInfo);
        };

        //유저가 게임방 접속에 성공했을 때 입장한 유저에게만 최초 1회 호출되는 이벤트 핸들러입니다.
        //자신을 포함하여 현재 게임방에 접속해 있는 유저들의 세션 정보와 매칭 기록이 포함되어 있습니다.
        //인게임 서버에 재접속했을 때도 호출됩니다.
        Backend.Match.OnSessionListInServer += (MatchInGameSessionListEventArgs args) => {

            //오류 방지를 위해 중복된 값을 제거한 채 리스트로 데이터를 반환 
            var Gamerecords = args.GameRecords.Distinct().ToList<MatchUserGameRecord>();

            //게임방 접속에 성공했을 때 이벤트 호출
            EnterRoom?.Invoke();

            //만약 가져온 유저 데이터의 개수가 2개가 아닐 경우, 오류가 발생한 것이기 때문에 실행하지 않고 넘어감.
            if (Gamerecords.Count == 2)
            {
                //상대방 정보를 가져옴
                MatchUserGameRecord otherInfo = Gamerecords.Find((r) => r.m_nickname != myNickname);
                Server.Instance.InitOtherData(otherInfo);
            }

            //게임방 접속 성공 처리
            if (args.ErrInfo == ErrorCode.Success)
            {
                //게임방에서 접속이 끊겼을 경우 처리
                Backend.Match.OnSessionOffline = (MatchInGameSessionEventArgs args) => {
                    Game.Instance.EnterAccountMenu();
                };

                //게임방의 게임이 종료되었을 때 호출되는 이벤트입니다.
                //서버에서 결과 종합이 끝난 후 모든 클라이언트에서 호출되는 이벤트입니다.
                //게임 시간이 초과되거나, 모든 클라이언트가 게임에 접속되지 못해 게임방이 파기되는 등 게임 자체가 끝나는 경우에도 호출됩니다.
                Backend.Match.OnMatchResult = (MatchResultEventArgs args) => {
                    switch(args.ErrInfo)
                    {
                        //결과 종합 성공
                        case ErrorCode.Success:
                            {

                                break;
                            }
                        //1. 게임 시간 초과(콘솔에서 설정한 매치 제한 시간을 초과한 경우)
                        //2. 게임 시작 실패(룸 생성 후 모든 유저가 게임에 접속하지 않은 경우)
                        case ErrorCode.Match_InGame_Timeout:
                            {
                                switch(args.Reason)
                                {
                                    case "Some gamers are not connected.(0)":
                                        {
                                            //게임 시작 실패(룸 생성 후 모든 유저가 게임에 접속하지 않은 경우)
                                            break;
                                        }
                                    case "Timeout":
                                        {
                                            //게임 시간 초과(콘솔에서 설정한 매치 제한 시간을 초과한 경우)
                                            break;
                                        }
                                }
                                break;
                            }
                        //1. 결과 종합 실패(모든 유저가 결괏값을 서버로 전송하지 않은 경우)
                        //2. 결과 종합 실패(결과에 포함되어 있는 승/패 유저 리스트와 실제 팀 유저들이 일치하지 않는 경우)
                        case ErrorCode.Exception:
                            {
                                switch(args.Reason)
                                {
                                    case "error: Success, status: 400, reason: {\"errorCode\":\"BadParameterException\",\"message\":\"bad headCount, 잘못된 headCount 입니다\",\"statusCode\":400}":
                                        {

                                            break;
                                        }
                                    case "Success, status: 400, reason: {\"errorCode\":\"BadParameterException\",\"message\":\"bad invalid team infomation, 잘못된 invalid team infomation 입니다\",\"statusCode\":400}":
                                        {

                                            break;
                                        }

                                }
                                break;
                            }
                        default:
                            {

                                break;
                            }

                    }
                };

                Game.Instance.EnterInGame();
            }
            //게임방 접속 실패 처리
            else
            {
                Debug.LogError("인게임 룸 접속에 실패했습니다.");
            }
        };

        //인게임 서버에 접속을 성공/실패했을 때 호출되는 이벤트입니다.
        Backend.Match.OnSessionJoinInServer += (JoinArgs) => {
            if (JoinArgs.ErrInfo == ErrorInfo.Success)
            //인게임 서버 접속 성공 처리
            {
                //게임 룸 진입 시도
                Backend.Match.JoinGameRoom(roomToken);
            }
            //인게임 서버 재접속 성공 처리
            else if (JoinArgs.ErrInfo.Category == ErrorCode.Success && JoinArgs.ErrInfo.Detail == ErrorCode.NetworkOnline && JoinArgs.ErrInfo.Reason == "Reconnect Success")
            {
                Debug.Log("인게임 서버 재접속에 성공했습니다.");
            }
            //인게임 서버 접속 실패 처리
            else if (JoinArgs.ErrInfo.Category == ErrorCode.Exception)
            {
                Debug.LogError("인게임 서버 접속에 실패했습니다.");
            }
            //예외 에러처리
            else
            {
                Debug.LogError("Error : failed enter in gamer server");
            }
        };

        //인게임 서버 접속 시도
        Backend.Match.JoinGameServer(serverAddress, serverPort, isReconnecting, out ErrorInfo successInfo);      
    }
    public bool TryReconnect()
    {
        Debug.Log("Start TryReconnect");

        bool isReconnecting = false;
        //재접속 여부 확인
        BackendReturnObject bro_isGameRoomActivate = Backend.Match.IsGameRoomActivate();

        //Null 예외처리
        if (bro_isGameRoomActivate == null) { return isReconnecting; }
        //진행중이었던 게임이 있었을 경우 처리
        else if (bro_isGameRoomActivate.StatusCode == 200)
        {
            //진행중이었던 게임방의 정보를 획득
            LitJson.JsonData roomInfo = bro_isGameRoomActivate.GetReturnValuetoJSON();
            string serverAddress = roomInfo["serverPublicHostName"].ToString();
            string roomToken = roomInfo["roomToken"].ToString();
            UInt16 serverPort = Convert.ToUInt16(roomInfo["serverPort"].ToString());
            isReconnecting = true;
            //=> unsigned 16-bit integer

            //획득한 정보를 토대로 재접속 시도
            JoinInGameServer(serverAddress, serverPort, roomToken, isReconnecting);

            Debug.Log("End TryReconnect");
            return isReconnecting;
        }
        //진행중이었던 게임이 없었을 경우 처리
        else
        {
            Debug.Log("End TryReconnect");
            return isReconnecting;
        }
    }
}

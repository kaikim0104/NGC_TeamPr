using System;
using BackEnd;
using BackEnd.Tcp;
using UnityEngine;

public class BackendFunctionsAccount : MonoBehaviour
{
    public void Login(string id, string pw)
    {
        //매칭 서버에 접속을 성공/실패했을 때 호출되는 이벤트입니다.
        Backend.Match.OnJoinMatchMakingServer = (JoinChannelEventArgs joinChannelEventArgs) =>
        {
            if (joinChannelEventArgs.ErrInfo == ErrorInfo.Success)
            {
                //매칭 서버 접속 성공 처리
                //서버로부터 계정의 정보를 가져와 UserData에 할당
                //MainMenu로 이동
                Server.Instance.InitMyData();
                Game.Instance.EnterMainMenu();
            }
            else
            {
                //매칭 서버 접속 실패 처리
            }
        };

        //로그인 시도
        BackendReturnObject bro_customLogin = Backend.BMember.CustomLogin(id, pw);

        int statusCode = bro_customLogin.StatusCode;

        //로그인 성공 처리
        if (statusCode == 200)
        {
            //매칭 서버 접속 시도
            Backend.Match.JoinMatchMakingServer(out ErrorInfo isSuccess);
        }
        //로그인 실패 처리
        else 
        {
            Debug.LogError("로그인에 실패했습니다");
        }
    }
    public void Login(string id, string pw, Action<bool> OnTryEnterMatchServer, Action<int> OnTryLogin)
    {
        //매칭 서버에 접속을 성공/실패했을 때 호출되는 이벤트입니다.
        Backend.Match.OnJoinMatchMakingServer = (JoinChannelEventArgs joinChannelEventArgs) =>
        {
            //매칭 서버 접속 시도 이벤트 시작
            OnTryEnterMatchServer?.Invoke(joinChannelEventArgs.ErrInfo == ErrorInfo.Success);
        };

        //로그인 시도
        BackendReturnObject bro_customLogin = Backend.BMember.CustomLogin(id, pw);

        int statusCode = bro_customLogin.StatusCode;

        //로그인 시도 이벤트 시작
        OnTryLogin?.Invoke(statusCode);

        //로그인 성공시 매칭 서버 접속 시도
        if(statusCode == 200)
        {
            //매칭 서버 접속 시도
            Backend.Match.JoinMatchMakingServer(out ErrorInfo isSuccess);
        }

    }
    public int Signup(string id, string pw)
    {
        //회원 가입 시도
        BackendReturnObject bro_customSignUp = Backend.BMember.CustomSignUp(id, pw);

        return bro_customSignUp.StatusCode;
    }
    public int UpdateNickname(string nickname)
    {
        //닉네임 양쪽 공백 제거
        nickname = nickname.Trim();

        //닉네임 중복 체크
        BackendReturnObject bro_checkNickname = Backend.BMember.CheckNicknameDuplication(nickname);

        //닉네임 중복 체크 성공 처리
        if(bro_checkNickname.StatusCode == 204)
        {
            //닉네임 변경 시도
            BackendReturnObject bro_updateNickname = Backend.BMember.UpdateNickname(nickname);

            //닉네임 변경 성공 처리
            if (bro_updateNickname.IsSuccess() || bro_updateNickname.StatusCode == 204)
            {
                Debug.Log("닉네임 변경에 성공했습니다 : " + bro_checkNickname);
                return bro_updateNickname.StatusCode;
            }
            //닉네임 변경 실패 처리
            else
            {
                Debug.LogError("닉네임 변경에 실패했습니다 : " + bro_checkNickname);
            }
        }
        return bro_checkNickname.StatusCode;
    }
}


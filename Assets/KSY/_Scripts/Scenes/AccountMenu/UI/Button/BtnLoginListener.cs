using System;
using TMPro;
using UnityEngine;
public class BtnLoginListener : MonoBehaviour
{
    private Action<int> OnTryLogin;
    private Action<bool> OnTryEnterMatchServer;

    [SerializeField] private TMP_InputField input_ID;
    [SerializeField] private TMP_InputField input_Pw;
    private void Awake()
    {
        //로그인 시도 이벤트 할당
        if (OnTryLogin == null)
        {
            OnTryLogin += SuccessLogin;
            OnTryLogin += FailedLogin;
        }
        if (OnTryEnterMatchServer == null)
        {
            OnTryEnterMatchServer += FailedEnterMatchServer;
        }
    }
    public void Login()
    {
        string id = input_ID.text;
        string pw = input_Pw.text;

        //만약 재접속 할 게임이 있다면 재접속 시도
        Debug.Log($"Server.Instance : {Server.Instance == null}");

        if (Server.Instance.TryReconnect()) return;

        //없다면 로그인 시도
        Server.Instance.Login(id, pw, OnTryEnterMatchServer, OnTryLogin);
    }

    //정확한 기능을 메소드 이름으로 명시할 것
    private void SuccessLogin(int statusCode)
    {
        //로그인 성공 처리
        if(statusCode == 200)
        {
            Server.Instance.InitMyData();
            Game.Instance.EnterScene(SceneType.MainMenu);
        }

        //이벤트 할당 해제
        OnTryLogin -= SuccessLogin;
    }

    //정확한 기능을 메소드 이름으로 명시할 것
    private void FailedLogin(int statusCode)
    {
        //로그인 실패 처리
        switch(statusCode)
        {
            //아이디나 비밀번호가 틀렸을 시 처리
            case 401:
                {
                    //UIManager.Instance.UpdateText("Login/ErrorInfo", "Invalid id or password.");
                    break;
                }
            //차단당한 아이디일 경우 처리
            case 403:
                {
                    //UIManager.Instance.UpdateText("Login/ErrorInfo", "This account has been banned.");
                    break;
                }
            //그 밖에 예외처리
            default:
                {
                    //성공이 아닐 경우
                    if(statusCode != 200)
                    {
                        //UIManager.Instance.UpdateText("Login/ErrorInfo", "Login failed");
                    }
                    break;
                }
        }

        //이벤트 할당 해제
        OnTryLogin -= FailedLogin;
    }

    //정확한 기능을 메소드 이름으로 명시할 것
    private void FailedEnterMatchServer(bool isConnected)
    {
        if(isConnected == false)
        {
            //UIManager.Instance.UpdateText("Login/ErrorInfo", "Enter MatchServer failed");
        }
    }
}

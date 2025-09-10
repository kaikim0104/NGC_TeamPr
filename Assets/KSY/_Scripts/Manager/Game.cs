using System;
using BackEnd;
using BackEnd.Tcp;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;

public class Game : SingletonBehaviour<Game>
{
    public event Action OnEnterAccountMenu;
    public event Action OnEnterMainMenu;
    public event Action OnEnterInGame;

    public Map Map;

    string[] mapNames = { "SDW_Map_1", "SDW_Map_2", "SDW_Map_3" };

    //씬이 다 로드되었다면 true
    private bool _accountMenuLoaded;
    private bool _mainMenuLoaded;
    private bool _mapLoaded;
    private void Awake()
    {
        BackendReturnObject Initialize = Backend.Initialize();

        if (!Initialize.IsSuccess())
        {
            //초기화에 실패했을 때 처리
            UIManager.Instance.ShowUI("Retry");
            UIManager.Instance.UpdateText("Retry/Text_ErrorInfo", "Connection failed. \nPlease try again");
        }
    }
    private void Start()
    {
        //씬이 완료되었을 떄 호출되는 이벤트 등록.
        SceneManager.sceneLoaded += (Scene s, LoadSceneMode lsm) =>
        {
            //로딩을 완료한 씬의 이름.
            var sceneName = s.name;

            //씬의 이름을 통해 어떤 씬의 로드를 완료했는지 판단함.
            switch (sceneName)
            {
                case "AccountMenu":
                    {
                        _accountMenuLoaded = true;
                        break;
                    }
                case "MainMenu":
                    {
                        _mainMenuLoaded = true;
                        break;
                    }
                case "InGame":
                    {
                        //맵에 있는 플랫폼 모음 가져오기
                        //if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        _mapLoaded = true;
                        break;
                    }
                case "SDW_Map_1":
                    {
                        _mapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        break;
                    }
                case "SDW_Map_2":
                    {
                        _mapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        break;
                    }
                case "SDW_Map_3":
                    {
                        _mapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        break;
                    }
                default:
                    {
                        Debug.Log("해당 씬은 등록되지 않은 씬입니다.");
                        break;
                    }
            }
        };

        //모든 유저가 준비되었을 때 호출되는 이벤트
        Backend.Match.OnMatchInGameStart = () => {

            //플레이어 데이터 초기화
            Server.Instance.InitOtherData();

            //씬이 다 로드되고나서 실행되도록 이벤트 등록
            if (_mapLoaded)
            {
                InitPlayer();
            }
            else
            {
                SceneManager.sceneLoaded += InitPlayer;
            }
        };
    }
    private void InitPlayer()
    {
        //********내 데이터 처리********

        //씬에서 플레이어 오브젝트 P1을 찾음
        Player p1;  GameObject.Find("P1").TryGetComponent(out p1);
        
        //서버로부터 불러왔던 나의 데이터를 가져옴
        UserData? myData = Server.Instance.GetMyData();
        
        //데이터가 제대로 불러와지지 않았다면 return;
        if (myData == null)
        {
            Debug.LogError("Error");
            return;
        }

        //플레이어 객체에 데이터 할당
        p1.GetUserData((UserData)myData);

        //나의 플레이어 객체 세팅
        p1.Init();

        //********상대방 데이터 처리********

        //씬에서 플레이어 오브젝트 P2를 찾음
        Player p2; GameObject.Find("P2").TryGetComponent(out p2);

        //서버로부터 불러왔던 상대방 데이터를 가져옴
        UserData? otherData = Server.Instance.GetOtherData();

        //데이터가 제대로 불러와지지 않았다면 return;
        if (otherData == null)
        {
            Debug.LogError("Error");
            return;
        }

        //플레이어 객체에 데이터 할당
        p2.GetUserData((UserData)otherData);

        //나의 플레이어 객체 세팅
        p2.Init();
    }
    private void InitPlayer(Scene s, LoadSceneMode lsm)
    {
        //********내 데이터 처리********

        //씬에서 플레이어 오브젝트 P1을 찾음
        Player p1; GameObject.Find("P1").TryGetComponent(out p1);

        //서버로부터 불러왔던 나의 데이터를 가져옴
        UserData? myData = Server.Instance.GetMyData();

        //데이터가 제대로 불러와지지 않았다면 return;
        if (myData == null)
        {
            Debug.LogError("Error");
            return;
        }

        //플레이어 객체에 데이터 할당
        p1.GetUserData((UserData)myData);

        //나의 플레이어 객체 세팅
        p1.Init();

        //********상대방 데이터 처리********

        //씬에서 플레이어 오브젝트 P2를 찾음
        Player p2; GameObject.Find("P2").TryGetComponent(out p2);

        //서버로부터 불러왔던 상대방 데이터를 가져옴
        UserData? otherData = Server.Instance.GetOtherData();

        //데이터가 제대로 불러와지지 않았다면 return;
        if (otherData == null)
        {
            Debug.LogError("Error");
            return;
        }

        //플레이어 객체에 데이터 할당
        p2.GetUserData((UserData)otherData);

        //나의 플레이어 객체 세팅
        p2.Init();
    }
    private void Update()
    {
       Backend.Match.Poll();
    }        //컴포넌트가 있다면 가져오고 아니라면 추가
    public void EnterAccountMenu()
    {
        SceneManager.LoadScene("AccountMenu");
    }
    public void EnterMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void EnterInGame()
    {
        //씬 로드
        //SceneManager.LoadScene("InGame");

        //int mapIndex = UnityEngine.Random.Range(0, 4);
        //SceneManager.LoadScene(_maps[mapIndex]); 
        SceneManager.LoadScene("SDW_Map_1"); 
    }
    public void ExitAccountMenu()
    {

    }
    public void ExitMainMenu()
    {

    }
    public void ExitInGame()
    {

    }
}

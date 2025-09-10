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
    public bool MapLoaded;
    public bool AllUserReady { get; private set; }

    private void Awake()
    {
        BackendReturnObject Initialize = Backend.Initialize();

        if (!Initialize.IsSuccess())
        {
            //초기화에 실패했을 때 처리
            //UIManager.Instance.ShowUI("Retry");
            //UIManager.Instance.UpdateText("Retry/Text_ErrorInfo", "Connection failed. \nPlease try again");
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
                case "KSY_Map_1":
                    {
                        MapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        OnEnterInGame?.Invoke();
                        break;
                    }
                case "SDW_Map_1":
                    {
                        MapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        OnEnterInGame?.Invoke();

                        break;
                    }
                case "SDW_Map_2":
                    {
                        MapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        OnEnterInGame?.Invoke();

                        break;
                    }
                case "SDW_Map_3":
                    {
                        MapLoaded = true;
                        if (!GameObject.Find("Map").TryGetComponent(out Map)) Map = GameObject.Find("Map").AddComponent<Map>();
                        OnEnterInGame?.Invoke();

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

            //씬이 다 로드되고나서 실행되도록 이벤트 등록
            if (MapLoaded)
            {
                AllUserReady = true;
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

        //플레이어 위치 세팅
        Map.SetPlayerStartPos(p1);

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

        //상대 플레이어 객체 세팅
        p2.Init();

        //플레이어 위치 세팅
        Map.SetPlayerStartPos(p2);
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

        //상대 플레이어 객체 세팅
        p1.Init();

        //플레이어 위치 세팅
        Map.SetPlayerStartPos(p1);

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

        //상대 플레이어 객체 세팅
        p2.Init();

        //플레이어 위치 세팅
        Map.SetPlayerStartPos(p2);
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
        ////랜덤한 맵을 선정함.
        //int mapIndex = UnityEngine.Random.Range(0, 4);
        ////선정한 맵의 이름을 가져옴
        //string mapName = mapNames[mapIndex];
        ////가져온 이름의 씬(맵)을 로드함.
        //SceneManager.LoadScene(mapName);  

        SceneManager.LoadScene("KSY_Map_1");
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

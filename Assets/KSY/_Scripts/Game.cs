using System;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    None = 0,
    Account,
    MainMenu,
    InGame
}
public class Game : SingletonBehaviour<Game>
{
    [SerializeField] private GameDataSO _gameData;
    
    //씬이 다 로드된 후에 호출됨 
    public event Action EnterAccountMenu;
    public event Action EnterMainMenu;
    public event Action EnterInGame;

    public bool IsAllReady { get; private set; }
    public Map MapCompo { get; private set; }
    [SerializeField] private string[] _mapNames;
    [SerializeField] private byte _mapCount = 0;

    #region Unity Event Function
    private void Awake()
    {
        BackendReturnObject Initialize = Backend.Initialize();

        if (!Initialize.IsSuccess())
        {
            //초기화 실패 처리
        }
    }
    private void Start()
    {
        EnterInGame += InitPlayer;

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
                        EnterAccountMenu?.Invoke();
                        break;
                    }
                case "MainMenu":
                    {
                        EnterMainMenu?.Invoke();
                        break;
                    }
                //In Game Loaded
                default:
                    {
                        MapCompo = GameObject.Find("Map").GetComponent<Map>();
                        EnterInGame?.Invoke();
                        break;
                    }
            }
        };

        //모든 유저가 준비되었을 때 호출되는 이벤트
        Backend.Match.OnMatchInGameStart = () => {
                IsAllReady = true;
        };
    }
    private void Update()
    {
        Backend.Match.Poll();
    }
    private void OnValidate()
    {
        _mapNames = _gameData.MapNames;
        _mapCount = _gameData.MapCount;

        if (_mapNames.Length > _gameData.MapCount)
        {
            Debug.Log("<color=red>맵의 이름의 개수가 지정된 맵의 개수보다 많습니다!</color>");
        }
    }
    #endregion
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
        MapCompo.SetPlayerStartPos(p1);

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
        MapCompo.SetPlayerStartPos(p2);
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
        MapCompo.SetPlayerStartPos(p1);

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
        MapCompo.SetPlayerStartPos(p2);
    }
    public void EnterScene(SceneType t)
    {
        switch(t)
        {
            case SceneType.Account:
                {
                    SceneManager.LoadScene("AccountMenu");
                    break;
                }
            case SceneType.MainMenu:
                {
                    SceneManager.LoadScene("MainMenu");
                    break;
                }
            case SceneType.InGame:
                {
                    ////랜덤한 맵을 선정함.
                    //int mapIndex = UnityEngine.Random.Range(0, _mapNames.Length - 1);
                    ////선정한 맵의 이름을 가져옴
                    //string mapName = _mapNames[mapIndex];
                    ////가져온 이름의 씬(맵)을 로드함.
                    //SceneManager.LoadScene(mapName);

                    SceneManager.LoadScene("KSY_Map_1");
                    break;
                }
        }   
    }
}

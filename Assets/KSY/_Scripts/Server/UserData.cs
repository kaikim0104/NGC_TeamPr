using BackEnd.Tcp;
using UnityEngine;

public struct UserData
{
    public string nickname;
    //playerNume은 게임방에 들어갈 때 정해지며 슈퍼 게이머(방장,호스트)일 경우 1,
    //아닐 경우 2를 할당 받는다.
    public bool hasInit;
}

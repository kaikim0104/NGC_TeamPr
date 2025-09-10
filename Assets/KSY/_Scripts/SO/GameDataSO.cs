using UnityEngine;

[CreateAssetMenu(fileName = "GameDataSO", menuName = "SO/GameDataSO")]
public class GameDataSO : ScriptableObject
{
    public readonly byte MapCount;
    public string[] MapNames;
}

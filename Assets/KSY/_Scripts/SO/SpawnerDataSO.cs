using UnityEngine;

[CreateAssetMenu(fileName = "SpawnerDataSO", menuName = "SO/SpawnerDataSO")]
public class SpawnerDataSO : ScriptableObject
{
    public GameObject[] ItemPrefabs;

    [Range(2, 4)]
    public float SpawnSecconds = 2;

    [Range(3,8)]
    public int MaxCount = 5;
}

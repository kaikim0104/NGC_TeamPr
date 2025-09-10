using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("스폰 포인트 & 아이템")]
    [SerializeField] private GameObject[] spawnerPoints;
    [SerializeField] private GameObject[] Objects;

    [Header("스폰 타이머 & 최대 아이템 갯수")]
    [SerializeField] private float spawnertimer;
    [SerializeField] private int maxCount = 5;

    static public int itemCount;
    private int maxItemCount;
    private float currentTimer;
    private int randomPoint;

    // network data
    static public sbyte itemX;
    private sbyte spawnX;

    private void Start()
    {
        maxItemCount = maxCount;
        itemCount = 0;
        currentTimer = 0;
    }

    private void Update()
    {
        if (itemCount < maxItemCount)
        {
            currentTimer += Time.deltaTime;
            if (currentTimer >= spawnertimer)
            {
                currentTimer = 0;
                randomPoint = Random.Range(0, spawnerPoints.Length);

                // 랜덤한 위치에 오브젝트 생성
                GameObject Obj = Instantiate(Objects[Random.Range(0, Objects.Length)],
                            spawnerPoints[randomPoint].transform.position,
                            Quaternion.identity);
                Obj.AddComponent<Object>();
                #region network data
                spawnX = (sbyte)Mathf.RoundToInt(spawnerPoints[randomPoint].transform.position.x);
                #endregion
                itemCount++;
            }
        }
    }
}

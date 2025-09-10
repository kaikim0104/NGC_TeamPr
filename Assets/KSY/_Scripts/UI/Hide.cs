using UnityEngine;

public class Hide : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
}

using BackEnd;
using UnityEngine;

public class BtnRetryListener : MonoBehaviour
{
    public void RetryInitialize()
    {
        if(Server.Instance.TryInitialize())
        {
            UIManager.Instance.HideUI("RetryInitialize");
        }
    }
}

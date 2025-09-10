using UnityEngine;

public class BtnFindMatchListener : MonoBehaviour
{
   public void FindMatch()
    {
        Server.Instance.FindMatch();
    }
}

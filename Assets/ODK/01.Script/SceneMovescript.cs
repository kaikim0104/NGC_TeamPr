using UnityEngine;

public class SceneMovescript : MonoBehaviour
{
    public void MoveScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}

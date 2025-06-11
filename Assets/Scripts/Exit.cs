using UnityEngine;

public class Exit : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QuitGame");
    }
}

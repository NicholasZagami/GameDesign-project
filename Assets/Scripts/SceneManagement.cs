using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    [Header("Indice di default della scena da caricare")]
    public int defaultSceneIndex = 1;

    // Metodo usato per avviare una scena con indice predefinito (da Inspector)
    public void PlayDefaultScene()
    {
        LoadSceneByIndex(defaultSceneIndex);
    }

    // Metodo chiamabile da altri script o da pulsanti con parametro
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadSceneAsync(sceneIndex);
        }
        else
        {
            Debug.LogWarning("Indice scena non valido: " + sceneIndex);
        }
    }
}

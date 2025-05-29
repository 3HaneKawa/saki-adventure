using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void LoadGameScene()
    { 
        SceneManager.LoadScene("SampleScene");
    }
}

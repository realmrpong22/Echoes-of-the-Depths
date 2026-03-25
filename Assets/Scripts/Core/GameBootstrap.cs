using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] string levelName = "YourLevel";

    void Start()
    {
        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
    }
}
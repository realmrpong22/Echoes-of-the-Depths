using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class SceneInitializer : MonoBehaviour
    {
        [Header("Manager Scene")]
        [Tooltip("Name of the persistent manager scene")]
        public string managerSceneName = "_Manager";

        void Awake()
        {
            // Check if manager scene is already loaded
            Scene managerScene = SceneManager.GetSceneByName(managerSceneName);

            if (!managerScene.isLoaded)
            {
                Debug.Log($"Loading {managerSceneName} scene...");
                SceneManager.LoadSceneAsync(managerSceneName, LoadSceneMode.Additive);
            }
            else
            {
                Debug.Log($"{managerSceneName} scene already loaded");
            }
        }
    }
}
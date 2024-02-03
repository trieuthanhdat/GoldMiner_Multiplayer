using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoreGame
{
    public class SceneLoader : NetworkSceneManagerBase
    {
        [Header("Scenes Setting")]
        public System.Action<List<NetworkObject>> OnSceneLoaded = null;

        protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
        {
            Debug.Log($"Switching Scene from {prevScene} to {newScene}");
            if (prevScene != SceneRef.None && prevScene == newScene)
                yield break;

            int sceneIndex = (int)newScene;
            yield return SceneManager.LoadSceneAsync(newScene/*path*/, LoadSceneMode.Single);
            List<NetworkObject> sceneObjects = new List<NetworkObject>();
            //var loadedScene = SceneManager.GetSceneByPath(path);
            var loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            //Debug.Log($"Loaded scene {path}: {loadedScene}");
            sceneObjects = FindNetworkObjects(loadedScene, disable: false);
            // Delay one frame
            yield return null;
            finished(sceneObjects);
        }
    }
}
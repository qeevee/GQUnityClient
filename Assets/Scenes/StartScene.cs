﻿using System.Collections;
using Code.GQClient.Err;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public string SceneName;
    
    public void StartTheScene()
    {
        StartCoroutine(LoadScene(SceneName));
    }

    private static IEnumerator LoadScene(string sceneName)
    {
        // if (SceneManager.GetSceneByName(sceneName).IsValid())
        // {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        // }
        // else
        // {
        //     Log.SignalErrorToAuthor($"Scene '{sceneName}' not found. Couldn't load it.");
        // }
        yield break;
    }

}
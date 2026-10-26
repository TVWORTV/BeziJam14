using System;
using System.Collections.Generic;

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagerController : PersistentInstantiable<SceneManagerController>
{
    [Header("Boot")]
    [SerializeField] bool onBootScene = true;

    [Header("Visuals")]
    [SerializeField] Image loadingImage;
    [SerializeField] Animator loadingAnimator;
    
    [SerializeField] string loadTrigger;
    [SerializeField] string unloadTrigger;

    [Header("Scenes")]
    public string Gameplay;

    float transitionDuration = 0.5f;

    string currentScene; 
    string pendingScene;
    bool isLoading;

    void Start()
    {
        if(onBootScene)
        {
            // LoadScene(Selection);
        }
    }

#region Public API

    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        pendingScene = sceneName;
        StartUnloadVisuals();
    }

#endregion

#region Animation Callvack 
    
    public void OnUnloadVisualsFinished()
    {
        StartCoroutine(SwitchSceneRoutine());
    }
    public void OnLoadVisualsFinished()
    {
        loadingImage.gameObject.SetActive(false); 
    }

    IEnumerator SwitchSceneRoutine()
    {
        isLoading = true;

        if (!string.IsNullOrEmpty(currentScene))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);

            while (!unloadOp.isDone)
                yield return null;
        }

        AsyncOperation loadOp =
            SceneManager.LoadSceneAsync(pendingScene, LoadSceneMode.Additive);

        while (!loadOp.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(pendingScene);

        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);

        currentScene = pendingScene;
        pendingScene = null;

        StartLoadVisuals();

        isLoading = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

     void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //StartLoadVisuals();
    }

#endregion

#region Visuals 

    void StartUnloadVisuals()
    {
        loadingImage.gameObject.SetActive(true); 
        loadingAnimator.SetTrigger(unloadTrigger);
    }

    void StartLoadVisuals()
    {
        loadingAnimator.SetTrigger(loadTrigger);
    }

    IEnumerator Fade(Image image, float from, float to, float duration, System.Action onComplete)
    {
        if (image == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        image.gameObject.SetActive(true);

        Color c = image.color;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);

            c.a = a;
            image.color = c;

            yield return null;
        }

        c.a = to;
        image.color = c;

        onComplete?.Invoke();
    }

#endregion

}

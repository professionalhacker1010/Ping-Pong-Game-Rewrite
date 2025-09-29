using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public List<GameObject> transitionObjects;
    private List<Image> images;
    private List<Animator> animators;

    public bool isTransitioning = false; //for pause menu - don't wanna be able to pause while transitioning
    public event Action OnTransitionIn, OnTransitionOut;
    public string activeScene;

    #region
    private static TransitionManager _instance;
    public static TransitionManager Instance
    {
        get
        {
            if (_instance == null) Debug.Log("The TransitionManager is NULL");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;

        images = new List<Image>();
        transitionObjects.ForEach(i => images.Add(i.GetComponent<Image>()));

        animators = new List<Animator>();
        transitionObjects.ForEach(i => animators.Add(i.GetComponent<Animator>()));
    }
    #endregion

    public void StartIn()
    {
        StartCoroutine(PlayTransition(false, "StartIn"));

        StartCoroutine(DeactivateObject());
    }

    public void QuickIn()
    {
        StartCoroutine(PlayTransition(false, "QuickIn"));

        StartCoroutine(DeactivateObject());
    }

    public void SlowIn()
    {
        StartCoroutine(PlayTransition(false, "SlowIn"));

        StartCoroutine(DeactivateObject());
    }

    public void QuickOut(string scene)
    {
        Debug.Log("quick out called");
        animators.ForEach(i => i.speed = 1f);
        StartCoroutine(PlayTransition(true, "QuickOut", scene));
    }

    public void SlowOut(string scene)
    {
        animators.ForEach(i => i.speed = 1f);
        StartCoroutine(PlayTransition(true, "SlowOut", scene));
    }

    private IEnumerator DeactivateObject()
    {
        yield return new WaitForSecondsRealtime(3.0f);
/*        yield return new WaitForSecondsRealtime(animators[0].GetCurrentAnimatorStateInfo(0).length);
        animators.ForEach(i => i.speed = 0f);*/
    }

    private IEnumerator PlayTransition(bool value, string trigger, string scene = "")
    {
        animators.ForEach(i => i.SetTrigger(trigger));
        StartCoroutine(ResetSize(3.0f));

        if (value == true && OnTransitionOut != null) OnTransitionOut();

        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(animators[0].GetCurrentAnimatorStateInfo(0).length);

        if (scene != "")
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(activeScene);
            op.completed += (_op) => { 
                SceneManager.LoadScene(scene, LoadSceneMode.Additive);
                activeScene = scene;
            };
        }

        if (value == false && OnTransitionIn != null) OnTransitionIn();

        isTransitioning = value;
        animators.ForEach(i => i.ResetTrigger(trigger));
    }

    private IEnumerator ResetSize(float time)
    {
        float t = 0f;
        while (t <= time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.unscaledDeltaTime;
            images.ForEach(i => { if (i.gameObject.name != "Mask") i.SetNativeSize(); });
        }
    }
}

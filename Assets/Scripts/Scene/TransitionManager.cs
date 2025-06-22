using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class TransitionManager : MonoBehaviour
{
    private Animator animator;
    public GameObject transitionObject;

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
        animator = transitionObject.GetComponent<Animator>();
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
        animator.speed = 1f;
        StartCoroutine(PlayTransition(true, "QuickOut", scene));
    }

    public void SlowOut(string scene)
    {
        animator.speed = 1f;
        StartCoroutine(PlayTransition(true, "SlowOut", scene));
    }

    private IEnumerator DeactivateObject()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.speed = 0f;
    }

    private IEnumerator PlayTransition(bool value, string trigger, string scene = "")
    {
        animator.SetTrigger(trigger);

        if (value == true && OnTransitionOut != null) OnTransitionOut();

        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length);

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
        animator.ResetTrigger(trigger);
    }
}

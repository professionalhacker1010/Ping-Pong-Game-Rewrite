using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class QuickGame : MonoBehaviour
{
    [SerializeField] Camera cam;
    public PaddleControls paddleControls;

    [Header("Background Transition")]
    [SerializeField] Material quickGamePixelMaterial;
    [SerializeField] Material quickGameBlendMaterial;
    [SerializeField] SpriteRenderer baseGameOverlay;
    [SerializeField] float endSampleSize;
    [SerializeField] float endBlendOpacity;
    [SerializeField] float startRotation;
    [SerializeField] float backgroundTransitionTime;
    [SerializeField] int transitionFrameRate;

    public event Action OnQuickGameWon;

    void Start()
    {
        Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(cam);
        baseGameOverlay.gameObject.SetActive(true);
        StartCoroutine(OpenGame());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(CloseGame(false));
        }
    }

    IEnumerator OpenGame()
    {
        Vector3 startPos = cam.transform.localPosition, endPos = new Vector3(0, 0, -15);

        cam.transform.localRotation = Quaternion.Euler(0, 0, startRotation);

        float t = 0, lerpExpo = 0, lerpCubic = 0, lerp = 0;
        DOTween.To(() => lerpExpo, f => lerpExpo = f, 1, backgroundTransitionTime).SetEase(Ease.OutExpo);
        DOTween.To(() => lerpCubic, f => lerpCubic = f, 1, backgroundTransitionTime).SetEase(Ease.OutCubic);
        DOTween.To(() => lerp, f => lerp = f, 1, backgroundTransitionTime);
        while (t < backgroundTransitionTime)
        {
            cam.transform.localPosition = Vector3.Lerp(startPos, endPos, lerpExpo);
            cam.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(startRotation, 0, lerpExpo));
            quickGamePixelMaterial.SetFloat("_ResampleSize", Mathf.Lerp(1, endSampleSize, lerpCubic));
            quickGameBlendMaterial.SetFloat("_AddOpacity", Mathf.Lerp(0, endBlendOpacity, lerp));
            baseGameOverlay.color = Color.Lerp(Color.white, Color.clear, lerp);

            float startTime = Time.time;
            yield return new WaitForSeconds(1f/ transitionFrameRate);
            t += Time.time - startTime;
        }
        cam.transform.localPosition = endPos;
        cam.transform.localRotation = Quaternion.identity;
        baseGameOverlay.color = Color.clear;
    }

    public IEnumerator CloseGame(bool won)
    {
        if (won && OnQuickGameWon != null)
        {
            OnQuickGameWon();
        }

        Vector3 startPos = new Vector3(0, 0, -15), endPos = new Vector3(0, 0, -13);

        float t = 0, lerpCubic = 0, lerp = 0;
        DOTween.To(() => lerpCubic, f => lerpCubic = f, 1, backgroundTransitionTime).SetEase(Ease.InCubic);
        DOTween.To(() => lerp, f => lerp = f, 1, backgroundTransitionTime);
        while (t < backgroundTransitionTime)
        {
            cam.transform.localPosition = Vector3.Lerp(startPos, endPos, lerp);
            quickGamePixelMaterial.SetFloat("_ResampleSize", Mathf.Lerp(endSampleSize, 1, lerpCubic));
            quickGameBlendMaterial.SetFloat("_AddOpacity", Mathf.Lerp(endBlendOpacity, 0, lerp));
            baseGameOverlay.color = Color.Lerp(Color.clear, Color.white, lerp);

            float startTime = Time.time;
            yield return new WaitForSeconds(1f / transitionFrameRate);
            t += Time.time - startTime;
        }

        quickGamePixelMaterial.SetFloat("_ResampleSize", 1);
        quickGameBlendMaterial.SetFloat("_AddOpacity", 0f);

        var overworldManager = OverworldManager.Instance;
        if (overworldManager)
        {
            overworldManager.DestroyQuickGame();
        }
    }

    private void OnDestroy()
    {
        quickGamePixelMaterial.SetFloat("_ResampleSize", 1);
        quickGameBlendMaterial.SetFloat("_AddOpacity", 0f);
        if (!Camera.main) return;
        var camData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        if (camData) camData.cameraStack.Remove(cam);
    }
}

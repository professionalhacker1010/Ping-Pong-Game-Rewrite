using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
public class QuickGame : MonoBehaviour
{
    [SerializeField] Camera cam;
    public PaddleControls paddleControls;

    [Header("Background Transition")]
    [SerializeField] Material quickGamePixelMaterial;
    [SerializeField] Material quickGameBlendMaterial;
    [SerializeField] float backgroundTransitionTime;
    [SerializeField] float endSampleSize;

    protected virtual void Start()
    {
        Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(cam);
        paddleControls.OnHit += OnPaddleHit;
        StartCoroutine(OpenGame());
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(CloseGame());
        }
    }

    IEnumerator OpenGame()
    {
        DOTween.To(() => quickGamePixelMaterial.GetFloat("_ResampleSize"), f => quickGamePixelMaterial.SetFloat("_ResampleSize", f), endSampleSize, backgroundTransitionTime).SetEase(Ease.OutCubic);
        DOTween.To(() => quickGameBlendMaterial.GetFloat("_AddOpacity"), f => quickGameBlendMaterial.SetFloat("_AddOpacity", f), .5f, backgroundTransitionTime);
        cam.transform.DOLocalMove(new Vector3(0, 0, -15), backgroundTransitionTime).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(backgroundTransitionTime);
    }

    IEnumerator CloseGame()
    {
        DOTween.To(() => quickGamePixelMaterial.GetFloat("_ResampleSize"), f => quickGamePixelMaterial.SetFloat("_ResampleSize", f), 1, backgroundTransitionTime).SetEase(Ease.InCubic);
        DOTween.To(() => quickGameBlendMaterial.GetFloat("_AddOpacity"), f => quickGameBlendMaterial.SetFloat("_AddOpacity", f), 0f, backgroundTransitionTime);
        cam.transform.DOLocalMove(new Vector3(0, 100, -155), backgroundTransitionTime).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(backgroundTransitionTime);

        quickGamePixelMaterial.SetFloat("_ResampleSize", 1);
        quickGameBlendMaterial.SetFloat("_AddOpacity", 0f);

        var overworldManager = OverworldManager.Instance;
        if (overworldManager)
        {
            overworldManager.DestroyQuickGame();
        }
    }

    protected virtual void OnPaddleHit(IHittable hittable)
    {

    }

    private void OnDestroy()
    {
        paddleControls.OnHit -= OnPaddleHit;
        quickGamePixelMaterial.SetFloat("_ResampleSize", 1);
        quickGameBlendMaterial.SetFloat("_AddOpacity", 0f);
    }
}

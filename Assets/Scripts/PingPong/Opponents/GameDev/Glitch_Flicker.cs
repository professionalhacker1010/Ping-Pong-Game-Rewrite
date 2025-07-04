using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch_Flicker : GameDevGlitch
{
    //flickering
    [Header("Flicker")]
    [SerializeField] private int flickerMinFrequency;
    [SerializeField] private int flickerMaxFrequency, flickerLength, minX, maxX, minY, maxY; //in 24fps frames

    bool flicker = false;

    public override void TurnOn()
    {
        flicker = true;
        StartCoroutine(Flicker(GameManager.Instance.balls[0].gameObject));
        StartCoroutine(Flicker(GameManager.Instance.balls[0].shadow.gameObject));
        StartCoroutine(Flicker(GameManager.Instance.PaddleControls.gameObject));
        StartCoroutine(FlickerPostProcessing());
    }

    public override void TurnOff()
    {
        flicker = false;
        volume.gameObject.SetActive(true);
    }

    private IEnumerator Flicker(GameObject gameObj)
    {
        SpriteRenderer originalSpriteRenderer = gameObj.GetComponent<SpriteRenderer>();
        GameObject flickerGameObj = new GameObject(gameObj.name + "_flicker", typeof(SpriteRenderer));
        SpriteRenderer flickerSpriteRenderer = flickerGameObj.GetComponent<SpriteRenderer>();
        flickerSpriteRenderer.sortingLayerID = originalSpriteRenderer.sortingLayerID;

        while (flicker)
        {
            flickerSpriteRenderer.sprite = originalSpriteRenderer.sprite;
            originalSpriteRenderer.color = Color.clear;
            flickerGameObj.transform.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            flickerGameObj.SetActive(true);
            yield return new WaitForSeconds(flickerLength / 24f); //time for flicker to be active


            flickerGameObj.SetActive(false);
            originalSpriteRenderer.color = Color.white;
            int frames = Random.Range(flickerMinFrequency, flickerMaxFrequency);
            yield return new WaitForSeconds(frames / 24f); //time to next flicker
        }

        Destroy(flickerGameObj);
    }

    private IEnumerator FlickerPostProcessing()
    {
        while (flicker)
        {
            volume.gameObject.SetActive(true);
            yield return new WaitForSeconds(flickerLength / 24f); //time for flicker to be active

            volume.gameObject.SetActive(false);
            int frames = Random.Range(flickerMinFrequency, flickerMaxFrequency);
            yield return new WaitForSeconds(frames / 24f); //time to next flicker
        }
    }
}

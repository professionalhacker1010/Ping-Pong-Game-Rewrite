using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch_Trail : GameDevGlitch
{
    [Header("Trail")]
    [SerializeField] private int trailTime;
    [SerializeField] private int trailFrameInterval;

    bool trail;

    public override void TurnOn()
    {
        trail = true;
        StartCoroutine(Trail(GameManager.Instance.Pingpong.gameObject));
        StartCoroutine(Trail(GameManager.Instance.Pingpong.shadow.gameObject));
        StartCoroutine(Trail(GameManager.Instance.PaddleControls.gameObject));
        StartCoroutine(Trail(GameManager.Instance.PaddleControls.transform.GetChild(0).gameObject));
    }

    public override void TurnOff()
    {
        trail = false;
    }

    private IEnumerator Trail(GameObject gameObj)
    {
        SpriteRenderer originalSpriteRenderer = gameObj.GetComponent<SpriteRenderer>();
        int counter = 0;

        while (trail)
        {
            StartCoroutine(CreateDestroyTrailObject(gameObj, originalSpriteRenderer, counter++));
            yield return new WaitForSeconds(trailFrameInterval / 24f);
        }
    }

    private IEnumerator CreateDestroyTrailObject(GameObject gameObj, SpriteRenderer originalSpriteRenderer, int num)
    {
        //print("creatdestroytobj");
        GameObject trailObject = new GameObject(gameObj.name + num.ToString(), typeof(SpriteRenderer));
        SpriteRenderer trailObjectSpriteRenderer = trailObject.GetComponent<SpriteRenderer>();

        trailObject.transform.position = gameObj.transform.position;
        trailObjectSpriteRenderer.sprite = originalSpriteRenderer.sprite;
        trailObjectSpriteRenderer.sortingLayerID = originalSpriteRenderer.sortingLayerID;
        yield return new WaitForSeconds(trailTime / 24f);

        Destroy(trailObject);
    }
}

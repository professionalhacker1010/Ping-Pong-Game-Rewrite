using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Birds : ShapeShifterPhase
{
    [Header("Birds")]
    [SerializeField] private List<GameObject> birds;

    List<int> servers = new List<int> { 4, 5, 2, 3 };
    int birdHitter = 0;

    public override IEnumerator ChangeOpponentPosition(float startX, float startY)
    {
        if (shouldHit)
        {
            StartCoroutine(HitBack("hit", birds[birdHitter].GetComponent<Animator>()));
        }
        else
        {
            shouldHit = true;
        }

        yield return null;
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return new WaitForSeconds(serveTime - (oppHitFrame) / 24f);
        
        foreach (var server in servers)
        {
            if (birds[server].activeSelf)
            {
                birds[server].GetComponent<Animator>().SetTrigger("hit");
                break;
            }
        }
    }

    public override Vector3 GetBallPath(int ballId, float X, float Y, bool isServing)
    {
        Vector3 hit = new Vector3(X, Y);

        //check that ball hit a bird
        if (!isServing)
        {
            for (int i = 0; i < weakPointColliders.Count; i++)
            {
                if (Overlaps(i, hit))
                {
                    Debug.Log("Hit a bird");
                    shouldHit = false;
                    StartCoroutine(DeactivateBird(i)); //todo: bird getting hit animation? Or just a sound effect is good enough lol
                    return base.GetBallPath(ballId, X, Y, isServing);
                }
            }
        }

        //otherwise the birds hit the ball back
        List<int> candidates = new List<int>();
        for (int i = 0; i < birds.Count; i++)
        {
            if (birds[i].GetComponent<BoxCollider2D>().OverlapPoint(hit) && birds[i].activeSelf)
            {
                //make the first bird hit it. Otherwise make the other bird hit it
                Debug.Log("Bird hit ball");
                candidates.Add(i);
                
            }
        }
        if (candidates.Count > 0)
        {
            birdHitter = candidates[Random.Range(0, candidates.Count)];
            return base.GetBallPath(ballId, X, Y, isServing);
        }
        

        //otherwise yay u won
        isDefeated = true;
        return missHit;
    }

    private IEnumerator DeactivateBird(int bird)
    {
        yield return new WaitForSeconds(playerBallPath.endFrame / 24f);
        birds[bird].SetActive(false);
    }
}

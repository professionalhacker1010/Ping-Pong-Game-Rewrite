using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickGame_VendingMachine : MonoBehaviour, IHittable
{
    [SerializeField] QuickGame game;
    [SerializeField] int hitsToBreak;
    [SerializeField] SpriteRenderer glassSprite;
    [SerializeField] GameObject successText;
    int hits = 0;

    public void OnHit(float hitX, float hitY)
    {
        hits++;
        if (hits > hitsToBreak) return;
        if (hits == hitsToBreak)
        {
            glassSprite.color = Color.clear;
            FindObjectOfType<CameraShake>().Shake(1);
            successText.SetActive(true);
            StartCoroutine(Util.VoidCallbackTimer(
                2.0f,
                () => StartCoroutine(game.CloseGame(true))
                ));
            return;
        }
        
        FindObjectOfType<CameraShake>().Shake(0);

    }
}

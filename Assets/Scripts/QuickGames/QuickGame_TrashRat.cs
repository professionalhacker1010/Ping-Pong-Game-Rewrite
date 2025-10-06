using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickGame_TrashRat : MonoBehaviour, IHittable
{
    [SerializeField] QuickGame game;
    [SerializeField] int hitsToBreak;
    [SerializeField] Sprite KOsprite;
    [SerializeField] int maxContactColliders;
    [SerializeField] GameObject successText;
    int hits = 0;

    public void OnHit(float hitX, float hitY)
    {
        var col = GetComponent<Collider2D>();
        List<Collider2D> contacts = new List<Collider2D>();
        col.GetContacts(contacts);
        if (contacts.Count >= maxContactColliders) return;

        hits++;
        if (hits > hitsToBreak) return;
        if (hits == hitsToBreak)
        {
            GetComponent<SpriteRenderer>().sprite = KOsprite;
            GetComponent<CameraShake>().Shake(1);
            successText.SetActive(true);
            StartCoroutine(Util.VoidCallbackTimer(
                2.0f,
                () => StartCoroutine(game.CloseGame(true))
                ));
            return;
        }

        GetComponent<CameraShake>().Shake(0);

    }
}

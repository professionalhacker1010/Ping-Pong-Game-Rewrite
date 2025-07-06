using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public enum DamageType { 
        SMALL,
        LARGE,
        BADHIT
    }

    [SerializeField] private RectTransform bar;
    [SerializeField] private bool isPlayerHealth;
    private float points = 0;
    private float smallStarDamage = 0, largeStarDamage = 0, hitOutDamage = 0; //when to trigger stuff based on how filled the bar is
    private float barUnit = 0;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void Initialize(float maxPoints, float smallDamage, float largeDamage, float badHitDamage)
    {
        points = maxPoints;
        smallStarDamage = smallDamage;
        largeStarDamage = largeDamage;
        hitOutDamage = badHitDamage;
        barUnit = bar.sizeDelta.y / maxPoints;
    }

    public void Damage(DamageType strength)
    {
        if (strength == DamageType.SMALL) points -= smallStarDamage;
        else if (strength == DamageType.LARGE) points -= largeStarDamage;
        else if (strength == DamageType.BADHIT) points -= hitOutDamage;
        bar.sizeDelta = new Vector2(bar.sizeDelta.x, barUnit * points);

        if (points <= 0)
        {
            if (isPlayerHealth)
            {
                //trigger lose game stuff... or reset to last checkpoint
                print("player health is ZERO");
                GameManager.Instance.GameOver(false);
            }
            else
            {
                //trigger win game stuff
                print("opponent health is ZERO");
                GameManager.Instance.GameOver(true);
            }
            
        }
    }

    public float Points()
    {
        return points;
    }
}

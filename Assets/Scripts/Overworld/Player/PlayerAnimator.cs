using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator body, arm, swipe;
    [SerializeField] private SpriteRenderer armSpriteRenderer;
    [SerializeField] private CharacterControls playerControls;

    private void Start()
    {
        playerControls.OnFaceRight += FaceRight;
        playerControls.OnFaceLeft += FaceLeft;
        playerControls.OnHit += Hit;
    }

    private void Update()
    {
        UpdaateVelocity(playerControls.Velocity);
    }

    public void UpdaateVelocity(float velocity)
    {
        body.SetFloat("velocity", velocity);
        arm.SetFloat("velocity", velocity);
    }


    public void FaceLeft()
    {
        body.SetBool("faceLeft", true);
        body.SetBool("faceRight", false);
        arm.SetBool("faceLeft", true);
        arm.SetBool("faceRight", false);
        swipe.SetBool("faceLeft", true);
        swipe.SetBool("faceRight", false);
    }

    public void FaceRight()
    {
        body.SetBool("faceLeft", false);
        body.SetBool("faceRight", true);
        arm.SetBool("faceLeft", false);
        arm.SetBool("faceRight", true);
        swipe.SetBool("faceLeft", false);
        swipe.SetBool("faceRight", true);
    }

    private void Hit()
    {
        swipe.SetTrigger("swipe");
        StopAllCoroutines();
        StartCoroutine(HideArms());
    }

    //for hiding arms while swipe animation plays
    private IEnumerator HideArms()
    {
        armSpriteRenderer.color = Color.clear;
        yield return new WaitForSeconds(6 / 24f); //swipe animation lasts 6 frames
        armSpriteRenderer.color = Color.white;
    }

    private void OnDestroy()
    {
        if (OverworldManager.Instance && playerControls)
        {
            playerControls.OnFaceRight -= FaceRight;
            playerControls.OnFaceLeft -= FaceLeft;
            playerControls.OnHit -= Hit;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class IntroCutscene : MonoBehaviour
{
    //for intro cutscene
    [SerializeField] private int panFrames;
    [SerializeField] private float friedItemLocation;
    [SerializeField] private BoxCollider2D doorSlamBoxCollider, introCutsceneTriggerBoxCollider;
    [SerializeField] private CharacterControls playerControls;
    [SerializeField] private CameraFollower cameraFollower;
    [SerializeField] private GameObject door;
    [SerializeField] private string dialogueNode;
    [SerializeField] private Vector3 spacePromptPosition;
    private KeyPressPrompt spacePrompt;

    private void Awake()
    {
        Conditions.Initialize("intro_played", false);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Conditions.Get("intro_played"))
        {
            Destroy(this.gameObject);
            return;
        }

        spacePrompt = KeyPressPromptManager.Instance.GetKeyPressPrompt("space");
        spacePrompt.SetConditions(() => playerControls.OverlapsRightHitBox(doorSlamBoxCollider) || playerControls.OverlapsLeftHitBox(doorSlamBoxCollider), spacePromptPosition);
        StartCoroutine(IntroCutScene());
    }

    private IEnumerator IntroCutScene()
    {
        cameraFollower.LockNormalBehavior();
        door.SetActive(false);
        yield return new WaitUntil(() => playerControls.OverlapsRightHitBox(doorSlamBoxCollider) && KeyCodes.Hit() && !PauseMenu.gameIsPaused);

        spacePrompt.RemoveConditions();
        door.SetActive(true);
        doorSlamBoxCollider.gameObject.SetActive(false);
        StartCoroutine(cameraFollower.PanToPlayer(15));
        cameraFollower.UnlockNormalBehaviour();

        yield return new WaitUntil(() => playerControls.OverlapsHitBox(introCutsceneTriggerBoxCollider));

        playerControls.LockCharacterControls();
        cameraFollower.LockNormalBehavior();
        cameraFollower.StopCurrentPan();

        //pan to fried item
        yield return cameraFollower.PanCamera(friedItemLocation, panFrames);

        yield return new WaitForSeconds(2.0f); //linger on fried item for a while

        //pan back to player
        yield return cameraFollower.PanToPlayer(panFrames);

        yield return new WaitForSeconds(1.0f);

        DialogueManager.Instance.StartDialogue(dialogueNode); //to do: wait for player to make some determined expression

        playerControls.UnlockCharacterControls();
        cameraFollower.UnlockNormalBehaviour();
        cameraFollower.UnlockNormalBehaviour();
        Conditions.Set("intro_played", true);
    }


}

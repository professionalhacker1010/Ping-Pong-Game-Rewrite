using UnityEngine;

public class Rocket : MonoBehaviour, ICanInteract
{
    public int InteractPriority => 0;

    public Vector2 InteractPos => transform.position;

    public bool IsInteractable => isSelectable;

    private bool isSelectable = true;
    private KeyPressPrompt cKeyPrompt;
    [SerializeField] private float cKeyPromptHeight;

    public void OnInteract()
    {
        var om = OverworldManager.Instance;
        if (om.GameScene == "LevelSelect") om.TransitionToOverworld("Moon");
        else om.TransitionToOverworld("LevelSelect");
    }


    public void OnDeselect()
    {
        cKeyPrompt.Hide();
    }

    public void OnSelect()
    {
        cKeyPrompt.Show(new Vector3(transform.position.x, transform.position.y + cKeyPromptHeight));
    }

    // Start is called before the first frame update
    void Start()
    {
        cKeyPrompt = KeyPressPromptManager.Instance.GetKeyPressPrompt("C");
    }
}

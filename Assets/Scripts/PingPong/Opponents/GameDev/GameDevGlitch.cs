using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDevGlitch : MonoBehaviour
{
    [SerializeField] UnityEngine.Rendering.VolumeProfile postProcessEffect;
    protected UnityEngine.Rendering.Volume volume;

    protected virtual void Start()
    {
        volume = FindObjectOfType<UnityEngine.Rendering.Volume>();
    }

    public virtual void TurnOn()
    {
        if (postProcessEffect) volume.profile = postProcessEffect;
    }

    public virtual void TurnOff()
    {
        volume.profile = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static IEnumerator BoolCallbackTimer(float time, System.Action<bool> callback)
    {
        yield return new WaitForSeconds(time);
        callback(true);
    }

    // Execute void callback after a given amount of time.
    public static IEnumerator VoidCallbackTimer(float time, System.Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

    public static IEnumerator VoidCallbackConditional(System.Func<bool> condition, System.Action callback)
    {
        yield return new WaitUntil(condition);
        callback();
    }

    public static IEnumerator VoidCallbackNextFrameConditional(System.Func<bool> condition, System.Action callback)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(condition);
        callback();
    }
}

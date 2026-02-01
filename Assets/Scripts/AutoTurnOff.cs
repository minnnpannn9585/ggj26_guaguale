using System.Collections;
using UnityEngine;

public class AutoTurnOff : MonoBehaviour
{
    [Tooltip("Seconds to wait after the GameObject becomes active before disabling it.")]
    public float delaySeconds = 2f;

    private Coroutine turnOffCoroutine;

    void OnEnable()
    {
        // Start/ restart the auto-off timer whenever this object becomes active.
        StartTurnOffTimer();
    }

    void OnDisable()
    {
        // Ensure any running coroutine is stopped when object is disabled.
        StopTurnOffTimer();
    }

    private void StartTurnOffTimer()
    {
        StopTurnOffTimer();
        turnOffCoroutine = StartCoroutine(TurnOffAfterDelay());
    }

    private void StopTurnOffTimer()
    {
        if (turnOffCoroutine != null)
        {
            StopCoroutine(turnOffCoroutine);
            turnOffCoroutine = null;
        }
    }

    private IEnumerator TurnOffAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        turnOffCoroutine = null;
    }
}

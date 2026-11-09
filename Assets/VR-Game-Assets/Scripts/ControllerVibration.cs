using UnityEngine;
using System.Collections;

public class ControllerVibration : MonoBehaviour
{
    private static ControllerVibration instance;

    public static void Vibrate(OVRInput.Controller controller, float duration = 0.15f, float amplitude = 0.8f, float frequency = 1f)
    {
        if (instance == null)
            instance = new GameObject("ControllerVibration").AddComponent<ControllerVibration>();

        instance.StartCoroutine(instance.Run(controller, duration, amplitude, frequency));
    }

    IEnumerator Run(OVRInput.Controller controller, float duration, float amplitude, float frequency)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSecondsRealtime(duration);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }
}

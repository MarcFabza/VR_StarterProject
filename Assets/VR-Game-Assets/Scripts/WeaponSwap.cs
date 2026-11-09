using UnityEngine;
using System.Collections;

public class WeaponSwap : MonoBehaviour
{
    private static WeaponSwap instance;

    private Sword sword;
    private GunShooter gun;
    private bool swapped = false;
    private Coroutine restoreRoutine;

    public static bool IsSwapped
    {
        get { return instance != null && instance.swapped; }
    }

    public static void Activate(float duration)
    {
        if (instance == null)
            instance = new GameObject("WeaponSwap").AddComponent<WeaponSwap>();

        instance.Begin(duration);
    }

    void Begin(float duration)
    {
        if (!swapped)
        {
            sword = FindFirstObjectByType<Sword>();
            gun = FindFirstObjectByType<GunShooter>();
            Debug.Log("WeaponSwap: sword=" + (sword != null) + " gun=" + (gun != null));
            if (sword == null || gun == null)
                return;

            Debug.Log("WeaponSwap: " + sword.transform.parent.name + " <-> " + gun.transform.parent.name);
            Swap();
            swapped = true;
        }

        else
            Debug.Log("WeaponSwap: ya estaban intercambiadas, se reinicia el tiempo");

        if (restoreRoutine != null)
            StopCoroutine(restoreRoutine);
        restoreRoutine = StartCoroutine(Restore(duration));
    }

    IEnumerator Restore(float duration)
    {
        yield return new WaitForSeconds(duration);
        Swap();
        swapped = false;
        restoreRoutine = null;
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Swap()
    {
        if (sword == null || gun == null)
            return;

        Transform swordParent = sword.transform.parent;
        Transform gunParent = gun.transform.parent;

        sword.transform.SetParent(gunParent, false);
        gun.transform.SetParent(swordParent, false);
        Mirror(sword.transform);
        Mirror(gun.transform);

        OVRInput.Controller swordController = sword.controller;
        sword.controller = gun.controller;
        gun.controller = swordController;
    }

    void Mirror(Transform t)
    {
        Vector3 p = t.localPosition;
        t.localPosition = new Vector3(-p.x, p.y, p.z);

        Quaternion q = t.localRotation;
        t.localRotation = new Quaternion(q.x, -q.y, -q.z, q.w);

        Vector3 s = t.localScale;
        t.localScale = new Vector3(-s.x, s.y, s.z);
    }
}

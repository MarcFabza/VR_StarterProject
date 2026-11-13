using UnityEngine;

public class GunCube : MonoBehaviour
{
    public GameObject mainCube;
    public float extraDestroyDelay = 0.2f;
    public int points = 100;

    private bool exploded = false;
    private Animation shatter;

    void Awake()
    {
        shatter = GetComponentInChildren<Animation>(true);
        if (shatter != null)
            shatter.gameObject.SetActive(false);
    }

    public bool Explode()
    {
        if (exploded)
            return false;
        exploded = true;

        CubePhysics mover = GetComponent<CubePhysics>();
        if (mover != null)
            mover.Stop();

        ScoreManager.AddScore(points);

        if (mainCube != null)
            mainCube.SetActive(false);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        float delay = extraDestroyDelay;
        if (shatter != null)
        {
            shatter.gameObject.SetActive(true);
            if (shatter.clip != null)
            {
                shatter.Play();
                delay += shatter.clip.length;
            }
        }

        CubeSpawner.DestroyCube(gameObject, delay);
        return true;
    }
}

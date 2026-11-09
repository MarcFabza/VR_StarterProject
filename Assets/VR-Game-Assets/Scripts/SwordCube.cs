using UnityEngine;
using EzySlice;

public enum CubeEffect { None, Switch, Slow, Fast }

public class SwordCube : MonoBehaviour
{
    [Header("Efecto especial (lo configura el spawner)")]
    public CubeEffect effect = CubeEffect.None;
    public float effectDuration = 8f;
    public float effectValue = 1f;

    public GameObject slashHitVFX;
    public float vfxLifetime = 2f;
    public AudioClip sliceSound;
    [Range(0f, 1f)] public float sliceVolume = 1f;
    public int points = 100;
    public int wrongWeaponPenalty = 50;

    [Header("Corte (EzySlice)")]
    public Material crossSectionMaterial;
    public float pieceForce = 1.5f;
    public float pieceLifetime = 3f;

    private bool sliced = false;

    public void ConfigureEffect(CubeEffect newEffect, float duration, float value)
    {
        effect = newEffect;
        effectDuration = duration;
        effectValue = value;
    }

    void ApplyEffect()
    {
        Debug.Log("SwordCube cortado, efecto: " + effect);

        switch (effect)
        {
            case CubeEffect.Switch:
                WeaponSwap.Activate(effectDuration);
                break;
            case CubeEffect.Slow:
            case CubeEffect.Fast:
                CubeSpawner.ApplySpeedEffect(effectValue, effectDuration);
                break;
        }
    }

    public void HitByBullet()
    {
        if (sliced)
            return;
        sliced = true;

        if (effect != CubeEffect.None)
        {
            Debug.Log("Cubo especial alcanzado por una bala");
            ApplyEffect();
            CubeSpawner.DestroyCube(gameObject);
            return;
        }

        ScoreManager.AddScore(-wrongWeaponPenalty);
        CubeSpawner.DestroyCube(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Sword sword = other.GetComponentInParent<Sword>();
        if (sliced || sword == null)
            return;
        sliced = true;

        Vector3 cubeCenter = GetComponent<Renderer>().bounds.center;
        Vector3 hitPoint = other.ClosestPoint(cubeCenter);

        if (slashHitVFX != null)
        {
            GameObject vfx = Instantiate(slashHitVFX, hitPoint, other.transform.rotation);
            Destroy(vfx, vfxLifetime);
        }

        if (sliceSound != null)
            AudioSource.PlayClipAtPoint(sliceSound, transform.position, sliceVolume);

        Slice(sword, hitPoint);

        ControllerVibration.Vibrate(sword.controller);
        ApplyEffect();

        ScoreManager.AddScore(points);
        CubeSpawner.DestroyCube(gameObject);
    }

    void Slice(Sword sword, Vector3 hitPoint)
    {
        Vector3 planeNormal = Vector3.Cross(sword.BladeDirection, sword.Velocity);
        if (planeNormal.sqrMagnitude < 0.0001f)
            planeNormal = sword.transform.right;
        planeNormal.Normalize();

        Material sectionMaterial = crossSectionMaterial != null ? crossSectionMaterial : GetComponent<Renderer>().sharedMaterial;

        SlicedHull hull = gameObject.Slice(hitPoint, planeNormal, sectionMaterial);
        if (hull == null)
            return;

        SetupPiece(hull.CreateUpperHull(gameObject, sectionMaterial), planeNormal * pieceForce);
        SetupPiece(hull.CreateLowerHull(gameObject, sectionMaterial), -planeNormal * pieceForce);
    }

    void SetupPiece(GameObject piece, Vector3 velocity)
    {
        if (piece == null)
            return;

        piece.layer = LayerMask.NameToLayer("Hand");

        MeshCollider meshCollider = piece.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        Rigidbody pieceRb = piece.AddComponent<Rigidbody>();
        pieceRb.linearVelocity = velocity;
        pieceRb.angularVelocity = Random.insideUnitSphere * 5f;

        Destroy(piece, pieceLifetime);
    }
}

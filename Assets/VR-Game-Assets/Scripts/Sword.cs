using UnityEngine;

public class Sword : MonoBehaviour
{
    public Collider bladeCollider;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    public Vector3 Velocity { get; private set; }

    public Vector3 BladeDirection
    {
        get { return transform.TransformDirection(bladeAxisLocal).normalized; }
    }

    private Vector3 bladeAxisLocal = Vector3.up;
    private Vector3 lastBladeCenter;

    void Awake()
    {
        if (bladeCollider == null)
            bladeCollider = GetComponentInChildren<Collider>();

        MeshCollider meshCollider = bladeCollider as MeshCollider;
        if (meshCollider != null && meshCollider.sharedMesh != null)
        {
            Vector3 size = meshCollider.sharedMesh.bounds.size;
            Vector3 axis = Vector3.right;
            if (size.y >= size.x && size.y >= size.z)
                axis = Vector3.up;
            else if (size.z >= size.x && size.z >= size.y)
                axis = Vector3.forward;

            bladeAxisLocal = transform.InverseTransformDirection(meshCollider.transform.TransformDirection(axis));
        }

        lastBladeCenter = GetBladeCenter();
    }

    void Update()
    {
        Vector3 center = GetBladeCenter();
        if (Time.deltaTime > 0f)
            Velocity = (center - lastBladeCenter) / Time.deltaTime;
        lastBladeCenter = center;
    }

    Vector3 GetBladeCenter()
    {
        return bladeCollider != null ? bladeCollider.bounds.center : transform.position;
    }
}

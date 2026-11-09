using UnityEngine;

public class CubePhysics : MonoBehaviour
{
    public float speedLevel = 5f;
    public float speedPerLevel = 2f;

    private Vector3 direction = Vector3.forward;
    private Rigidbody body;
    private bool stopped = false;

    public float Speed
    {
        get { return speedLevel * speedPerLevel * CubeSpawner.GameSpeed; }
    }

    public void Init(Vector3 moveDirection, float level, float unitsPerLevel)
    {
        direction = moveDirection.normalized;
        speedLevel = level;
        speedPerLevel = unitsPerLevel;
    }

    public void Stop()
    {
        stopped = true;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (stopped)
            return;

        if (body != null)
            body.linearVelocity = direction * Speed;
        else
            transform.position += direction * Speed * Time.fixedDeltaTime;
    }
}

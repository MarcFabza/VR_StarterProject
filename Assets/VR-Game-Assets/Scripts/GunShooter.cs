using UnityEngine;
using System.Collections;

public class GunShooter : MonoBehaviour
{
    public float fireRate = 0.1f;
    public Transform nozzleTransform;
    public AudioSource sound_shoot;
    public AudioClip shootClip;
    public Animator gunAnimator;

    public OVRInput.Button ShootingButton = OVRInput.Button.PrimaryIndexTrigger;
    public OVRInput.Controller controller = OVRInput.Controller.LTouch;

    public float range = 100f;

    [Header("Bala")]
    public float bulletSpeed = 30f;
    public float bulletSize = 0.05f;
    public Color bulletColor = Color.yellow;

    [Header("Láser para apuntar")]
    public bool showLaser = true;
    public Color laserColor = Color.red;
    public float laserWidth = 0.005f;
    public float laserLength = 6f;

    private float nextFireTime = 0f;
    private LineRenderer laser;

    void Start()
    {
        if (showLaser)
            CreateLaser();
    }

    void Update()
    {
        if (laser != null)
            UpdateLaser();

        if (OVRInput.GetDown(ShootingButton, controller) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void CreateLaser()
    {
        laser = nozzleTransform.gameObject.AddComponent<LineRenderer>();
        laser.positionCount = 2;
        laser.startWidth = laserWidth;
        laser.endWidth = laserWidth;
        laser.useWorldSpace = true;
        laser.material = new Material(Shader.Find("Sprites/Default"));
        laser.startColor = laserColor;
        laser.endColor = laserColor;
        laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laser.receiveShadows = false;
    }

    void UpdateLaser()
    {
        Vector3 start = nozzleTransform.position;
        Vector3 end = start + nozzleTransform.forward * laserLength;

        int mask = ~LayerMask.GetMask("Hand", "HandPlayer");
        RaycastHit hit;
        if (Physics.Raycast(start, nozzleTransform.forward, out hit, laserLength, mask, QueryTriggerInteraction.Ignore))
            end = hit.point;

        Component target;
        float targetDistance;
        if (FindTarget(start, nozzleTransform.forward, laserLength, out target, out targetDistance) && targetDistance < Vector3.Distance(start, end))
            end = start + nozzleTransform.forward * targetDistance;

        laser.SetPosition(0, start);
        laser.SetPosition(1, end);
    }

    void Shoot()
    {
        if (gunAnimator != null)
            gunAnimator.SetTrigger("Fire");

        if (sound_shoot != null && sound_shoot.isActiveAndEnabled && sound_shoot.clip != null)
            sound_shoot.PlayOneShot(sound_shoot.clip);
        else if (shootClip != null)
            AudioSource.PlayClipAtPoint(shootClip, nozzleTransform.position);

        StartCoroutine(MoveBullet(nozzleTransform.position, nozzleTransform.forward));
    }

    IEnumerator MoveBullet(Vector3 position, Vector3 direction)
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(bullet.GetComponent<Collider>());
        bullet.transform.position = position;
        bullet.transform.localScale = Vector3.one * bulletSize;
        bullet.GetComponent<Renderer>().material.color = bulletColor;

        float travelled = 0f;
        while (travelled < range)
        {
            float step = bulletSpeed * Time.deltaTime;

            Component target;
            float targetDistance;
            if (FindTarget(position, direction, step, out target, out targetDistance))
            {
                HitTarget(target);
                break;
            }

            position += direction * step;
            travelled += step;
            bullet.transform.position = position;
            yield return null;
        }

        Destroy(bullet);
    }

    void HitTarget(Component target)
    {
        if (target is GunCube gunCube)
        {
            if (gunCube.Explode())
                ControllerVibration.Vibrate(controller);
        }
        else if (target is SwordCube swordCube)
            swordCube.HitByBullet();
        else if (target is ShootableButton button)
            button.Press();
    }

    bool FindTarget(Vector3 origin, Vector3 direction, float distance, out Component closestTarget, out float closestDistance)
    {
        closestTarget = null;
        closestDistance = Mathf.Infinity;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, ~0, QueryTriggerInteraction.Collide);
        foreach (RaycastHit hit in hits)
        {
            Component target = hit.collider.GetComponentInParent<GunCube>();
            if (target == null)
                target = hit.collider.GetComponentInParent<SwordCube>();
            if (target == null)
                target = hit.collider.GetComponentInParent<ShootableButton>();

            if (target != null && hit.distance < closestDistance)
            {
                closestTarget = target;
                closestDistance = hit.distance;
            }
        }

        return closestTarget != null;
    }
}

using UnityEngine;

public class CubeKiller : MonoBehaviour
{
    public int missPenalty = 50;

    void OnTriggerEnter(Collider other)
    {
        GameObject cube = CubeSpawner.GetSpawnedCube(other.transform);
        if (cube != null)
        {
            SwordCube special = cube.GetComponent<SwordCube>();
            if (special == null || special.effect == CubeEffect.None)
                ScoreManager.AddScore(-missPenalty);
            CubeSpawner.DestroyCube(cube);
        }
    }
}

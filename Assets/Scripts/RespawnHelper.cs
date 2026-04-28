using System.Collections;
using UnityEngine;

public class RespawnHelper : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float respawnDelay = 10f;
    public float spawnRange = 5f;

    private Vector3 spawnPoint;

    void Start()
    {
        spawnPoint = transform.position;
    }

    public void StartRespawn()
    {
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab NO asignado");
            yield break;
        }

        Vector2 random = Random.insideUnitCircle * spawnRange;

        Vector3 spawnPos = new Vector3(
            spawnPoint.x + random.x,
            spawnPoint.y,
            spawnPoint.z + random.y
        );

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public GameObject damagePopupPrefab;
    public int health = 50;

    [Header("Rewards & Loot")]
    public int expReward = 50;
    public int nivelEnemigo = 1;
    public LootTable tablaDeBotin;
    public GameObject lootPrefab;

    [Header("Gold System (Loot Table Style)")]
    public ItemData itemOroReferencia;
    public GameObject goldPrefab;

    [Header("Audio")]
    public AudioClip hitFleshSound;
    private AudioSource audioSource;

    public float gravity = -9.81f;
    private float yVelocity;
    private CharacterController controller;

    public Transform player;
    public float agroRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float attackCooldown = 1.5f;

    private float nextAttackTime;
    private Animator animator;
    private Vector3 horizontalMove;

    private Vector3 startPosition;
    public float patrolRange = 8f;
    public float patrolWaitTime = 4f;

    private Vector3 patrolTarget;
    private float patrolTimer;

    private bool isDead = false;

    public float separationRadius = 1.5f;
    public float separationStrength = 2f;

    [Header("Respawn Settings")]
    public float timeLyingOnGround = 3f;
    public float minRespawnTime = 5f;
    public float maxRespawnTime = 10f;
    public float respawnRadius = 5f;

    public GameObject modelObject;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;
        SetNewPatrolPoint();
        if (modelObject == null && transform.childCount > 0) modelObject = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (isDead) { ApplyDeathGravity(); return; }
        if (player == null) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null && stats.IsDead) { horizontalMove = Vector3.zero; ApplyMovement(); return; }
        if (CanSeePlayer()) HandleAI(); else Patrol();
        ApplyMovement();
    }

    void ApplyDeathGravity()
    {
        if (!IsGrounded()) { yVelocity += gravity * Time.deltaTime; transform.position += new Vector3(0, yVelocity, 0) * Time.deltaTime; }
        else { yVelocity = 0; SnapToGround(); }
    }

    void SnapToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
    }

    bool CanSeePlayer() => Vector3.Distance(transform.position, player.position) <= agroRange;

    void HandleAI()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;
        if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 6f * Time.deltaTime);
        horizontalMove = Vector3.zero;
        if (distance <= agroRange)
        {
            if (distance > attackRange)
            {
                Vector3 separation = GetSeparationForce();
                horizontalMove = (direction.normalized + separation).normalized * moveSpeed;
                if (animator != null) animator.SetFloat("Speed", 1f);
            }
            else { if (animator != null) animator.SetFloat("Speed", 0f); Attack(); }
        }
    }

    void Patrol()
    {
        Vector3 direction = patrolTarget - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;
        if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 4f * Time.deltaTime);
        if (distance < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            horizontalMove = Vector3.zero;
            if (animator != null) animator.SetFloat("Speed", 0f);
            if (patrolTimer >= patrolWaitTime) { SetNewPatrolPoint(); patrolTimer = 0; }
        }
        else
        {
            Vector3 separation = GetSeparationForce();
            horizontalMove = (direction.normalized + separation).normalized * (moveSpeed * 0.6f);
            if (animator != null) animator.SetFloat("Speed", 0.4f);
        }
    }

    void SetNewPatrolPoint()
    {
        Vector2 random = Random.insideUnitCircle * patrolRange;
        patrolTarget = new Vector3(startPosition.x + random.x, transform.position.y, startPosition.z + random.y);
    }

    Vector3 GetSeparationForce()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 force = Vector3.zero;
        foreach (var hit in hits) { if (hit.CompareTag("Enemy") && hit.gameObject != gameObject) { Vector3 dir = transform.position - hit.transform.position; force += dir.normalized / Mathf.Max(dir.magnitude, 0.1f); } }
        return force * separationStrength;
    }

    void ApplyMovement()
    {
        if (IsGrounded() && yVelocity < 0) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;
        Vector3 move = horizontalMove;
        move.y = yVelocity;
        controller.Move(move * Time.deltaTime);
    }

    bool IsGrounded() => Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);

    void Attack()
    {
        if (isDead || player == null) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null && stats.IsDead) return;
        if (Time.time < nextAttackTime) return;
        if (animator != null) animator.SetTrigger("Attack");
        nextAttackTime = Time.time + attackCooldown;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (audioSource != null && hitFleshSound != null) { audioSource.pitch = Random.Range(0.9f, 1.1f); audioSource.PlayOneShot(hitFleshSound); }
        ShowDamage(damage);
        health -= damage;
        if (health <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        GiveExpToPlayer();
        DropLoot();
        StartCoroutine(RespawnRoutine());
    }

    void GiveExpToPlayer()
    {
        if (player == null) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null) stats.AddExp(expReward);
    }

    // --- SECCIÓN DE BOTÍN FIXEADA: ACUMULACIÓN DE ORO ---

    void DropLoot()
    {
        if (tablaDeBotin == null)
        {
            Debug.LogWarning("¡FALTA TABLA DE BOTÍN!");
            return;
        }

        int cantidadIntentos = Random.Range(1, 4);
        int oroTotalAcumulado = 0; // Acumulador para no soltar varias monedas

        for (int i = 0; i < cantidadIntentos; i++)
        {
            ItemData resultado = tablaDeBotin.GetRandomItem(nivelEnemigo);

            if (resultado != null)
            {
                // Si el item es oro, lo sumamos al acumulador en lugar de spawnearlo
                if (resultado == itemOroReferencia || resultado.nombreItem == "Oro" || resultado.nombreItem == "oro")
                {
                    oroTotalAcumulado += CalcularMontoOroIndividual();
                }
                else if (lootPrefab != null)
                {
                    // Los items normales (espadas, etc) se instancian por separado
                    InstanciarLootItem(resultado);
                }
            }
        }

        // Si al final de los intentos juntamos oro, soltamos UNA sola moneda con el total
        if (oroTotalAcumulado > 0)
        {
            GenerarDropDeOroUnico(oroTotalAcumulado);
        }
    }

    int CalcularMontoOroIndividual()
    {
        PlayerStats pStats = (player != null) ? player.GetComponent<PlayerStats>() : null;
        int nivelP = (pStats != null) ? pStats.nivel : 1;
        return Random.Range(nivelEnemigo * 5, nivelEnemigo * 15) + (nivelP * 2);
    }

    void GenerarDropDeOroUnico(int monto)
    {
        if (goldPrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3.up * 1.2f);
        GameObject drop = Instantiate(goldPrefab, spawnPos, Quaternion.identity);

        GoldDrop gd = drop.GetComponent<GoldDrop>();
        if (gd != null) gd.cantidadOro = monto;

        AplicarImpulsoFisico(drop);
    }

    void InstanciarLootItem(ItemData data)
    {
        Vector3 spawnPos = transform.position + (Vector3.up * 1.2f);
        GameObject drop = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        LootItem li = drop.GetComponent<LootItem>();
        if (li != null)
        {
            li.item = data.CreateInstance();
            li.ConfigurarVisuales();
        }
        AplicarImpulsoFisico(drop);
    }

    void AplicarImpulsoFisico(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 fuerza = new Vector3(Random.Range(-2f, 2f), 5f, Random.Range(-2f, 2f));
            rb.AddForce(fuerza, ForceMode.Impulse);
        }
    }

    IEnumerator RespawnRoutine()
    {
        controller.enabled = false;
        if (animator != null) animator.SetTrigger("Die");
        yield return new WaitForSeconds(timeLyingOnGround);
        if (modelObject != null) modelObject.SetActive(false);
        float randomWait = Random.Range(minRespawnTime, maxRespawnTime);
        yield return new WaitForSeconds(randomWait);
        Vector2 randomPos = Random.insideUnitCircle * respawnRadius;
        transform.position = startPosition + new Vector3(randomPos.x, 0, randomPos.y);
        health = 50;
        isDead = false;
        yVelocity = 0;
        horizontalMove = Vector3.zero;
        if (modelObject != null) modelObject.SetActive(true);
        controller.enabled = true;
        if (animator != null) { animator.Rebind(); animator.Update(0f); }
    }

    public float GetHealthPercent() => (float)health / 50f;
    public bool IsAlive() => !isDead && health > 0;

    void ShowDamage(int damage)
    {
        if (damagePopupPrefab == null) return;
        Vector3 pos = transform.position + Vector3.up * 2f;
        GameObject popup = Instantiate(damagePopupPrefab, pos, Quaternion.identity);
        DamagePopup dp = popup.GetComponent<DamagePopup>();
        dp.Setup(damage);
        dp.SetColor(Color.yellow);
    }

    public void DealDamage()
    {
        if (player == null || isDead) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null && !stats.IsDead) stats.TakeDamage(10);
    }
}
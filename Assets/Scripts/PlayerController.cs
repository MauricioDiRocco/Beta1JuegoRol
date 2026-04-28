using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float stopDistance = 0.2f;
    public float attackSpeed = 1f;

    public float gravity = -9.81f;
    public float groundOffset = 0.2f;
    private float yVelocity;

    private Camera cam;
    private Animator animator;
    private CharacterController controller;
    private float nextAttackTime;

    private Vector3 targetPosition;
    private bool hasTarget;
    private bool isAttacking;

    private PlayerStats stats;

    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    public Transform respawnPoint;
    public GameObject deathPanel;

    public float smoothRotationSpeed = 6f;

    public float attackRadius = 2.5f;
    public float attackRange = 1.5f;
    public float attackAngle = 90f;
    public LayerMask enemyLayer;

    // Velocidad base guardada para poder aplicar el bonus de MoveSpeed
    private float baseMoveSpeed;

    void Start()
    {
        attackSpeed = 2.5f;
        baseMoveSpeed = moveSpeed;
        cam = Camera.main;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        HandleClick();
        Move();
        Attack();

        // 🔥 Aplicar bonus de velocidad de movimiento del equipo en tiempo real
        ApplyMoveSpeedBonus();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ajusta moveSpeed según el bonus de equipo
    // ─────────────────────────────────────────────────────────────────────────
    void ApplyMoveSpeedBonus()
    {
        if (stats == null) return;
        float mult = 1f + (stats.moveSpeedBonus / 100f);
        moveSpeed = baseMoveSpeed * mult;
    }

    void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPosition = hit.point;
                hasTarget = true;

                if (hit.collider.CompareTag("Enemy"))
                {
                    if (stats != null) stats.SetTarget(hit.collider.transform);
                }
                else
                {
                    if (stats != null) stats.ClearTarget();
                }
            }
        }
    }

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        return Physics.Raycast(origin, Vector3.down, 0.5f);
    }

    void SnapToGround()
    {
        if (yVelocity > 0) return;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1.2f))
        {
            float dist = transform.position.y - hit.point.y;
            if (dist > 0.05f)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, hit.point.y, 15f * Time.deltaTime);
                transform.position = pos;
            }
        }
    }

    void Move()
    {
        if (isAttacking) { ApplyGravityOnly(); return; }

        Vector3 move = Vector3.zero;

        if (hasTarget)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;

            if (distance > stopDistance)
            {
                move = direction.normalized;
                Quaternion rot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);

                Vector3 localMove = transform.InverseTransformDirection(move);
                animator.SetFloat("MoveX", localMove.x, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveZ", localMove.z, 0.1f, Time.deltaTime);
                animator.SetFloat("Speed", move.magnitude);
            }
            else
            {
                hasTarget = false;
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveZ", 0);
                animator.SetFloat("Speed", 0);
            }
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        ApplyMovement(move);
    }

    void ApplyMovement(Vector3 move)
    {
        Vector3 finalMove = move * moveSpeed;
        if (IsGrounded() && yVelocity < 0) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;
        finalMove.y = yVelocity;
        controller.Move(finalMove * Time.deltaTime);
        SnapToGround();
    }

    void ApplyGravityOnly()
    {
        Vector3 finalMove = Vector3.zero;
        if (IsGrounded() && yVelocity < 0) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;
        finalMove.y = yVelocity;
        controller.Move(finalMove * Time.deltaTime);
        SnapToGround();
    }

    void FaceTarget(Transform target)
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothRotationSpeed * Time.deltaTime);
    }

    void Attack()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 🔥 Velocidad de ataque del equipo aplicada al Animator
        float totalAttackSpeed = attackSpeed;
        if (stats != null && stats.attackSpeed > 0)
            totalAttackSpeed *= (1f + stats.attackSpeed / 100f);

        animator.SetFloat("AttackSpeed", totalAttackSpeed);
        if (Time.time < nextAttackTime) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (stats != null) stats.PlaySwordSound();
            if (stats != null && stats.currentTarget != null) FaceTarget(stats.currentTarget);

            animator.SetTrigger("Attack");
            isAttacking = true;
            nextAttackTime = Time.time + (1f / totalAttackSpeed);
        }
    }

    public void Hit()
    {
        Vector3 center = transform.position + transform.forward * 1f;
        Collider[] hits = Physics.OverlapSphere(center, attackRadius, enemyLayer);

        foreach (Collider col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null) continue;

            // Daño base (ya incluye crítico desde GetPhysicalDamage)
            int damage = (stats != null) ? stats.GetPhysicalDamage() : 10;

            // 🔥 Aplicar penetración de armadura
            if (stats != null && stats.armorPen > 0)
            {
                float reduction = 1f - (stats.armorPen / 100f);
                damage = Mathf.Max(1, Mathf.RoundToInt(damage / reduction));
            }

            enemy.TakeDamage(damage);

            // 🔥 Lifesteal y mana por golpe
            if (stats != null) stats.OnHitEnemy(damage);

            if (stats != null) stats.SetTarget(enemy.transform);
        }
    }

    public void EndAttack() => isAttacking = false;

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;
        hasTarget = false;
        yVelocity = 0;
        nextAttackTime = 0;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Die");
        }

        controller.enabled = false;
        if (deathPanel != null) deathPanel.SetActive(true);
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        transform.position = respawnPoint.position;
        controller.enabled = true;
        hasTarget = false;
        isAttacking = false;
        yVelocity = 0;
        nextAttackTime = 0;

        if (stats != null) stats.RespawnReset();
        if (deathPanel != null) deathPanel.SetActive(false);

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.ResetTrigger("Attack");
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void OnPlayerDeath() => Die();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * 1f;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}
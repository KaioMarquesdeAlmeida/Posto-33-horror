using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("Componentes")]
    public NavMeshAgent ai;
    public Animator aiAnim;
    public Transform player;

    [Header("Audio")]
    public AudioSource audioSource; // Arraste o AudioSource aqui
    public AudioClip spotSound;     // O som do grito/aviso

    [Header("Velocidades")]
    public float walkSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Tempo de Idle")]
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 1.5f;

    [Header("Patrulha Stalker (Perto do Player)")]
    // O bicho vai tentar ir para um lugar entre 10 e 30 metros do player
    public float minDistanceFromPlayer = 15f;
    public float maxDistanceFromPlayer = 100f;

    [Header("Perseguição")]
    public float sightDistance = 15f; 
    public float viewAngle = 90f;
    public float catchDistance = 2f;
    public float minChaseTime = 5f;
    public float maxChaseTime = 10f;
    public Vector3 rayCastOffset = new Vector3(0, 1, 0);

    [Header("Busca")]
    public float maxSearchTime = 3f;

    [Header("Detecção de Terreno")]
    public LayerMask obstacleLayer;

    [Header("Death Scene")]
    public string deathScene = "DeathScene";
    public float jumpscareTime = 2f;

    private bool walking;
    private bool chasing;
    private bool searching;
    private bool isWaiting;
    private Vector3 lastKnownPlayerPosition;

    void Start()
    {
        if (player == null || ai == null)
        {
            Debug.LogError("Faltam componentes no EnemyAI!");
            enabled = false;
            return;
        }

        walking = true;
        chasing = false;
        searching = false;
        isWaiting = false;

        GoToRandomPointNearPlayer();
    }

    void Update()
    {
        if (player == null) return;

        CheckPlayerDetection();

        if (chasing)
            HandleChasing();
        else if (searching)
            HandleSearching();
        else if (walking)
            HandleWalking();
    }

    void CheckPlayerDetection()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = false;
        float proximitySense = 3.0f;

        if (distanceToPlayer <= sightDistance)
        {
            bool inAngle = Vector3.Angle(transform.forward, directionToPlayer) <= viewAngle / 2f;
            bool tooClose = distanceToPlayer <= proximitySense;

            if (inAngle || tooClose)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + rayCastOffset, directionToPlayer, out hit, sightDistance))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        if (!HasObstacleBetween(transform.position, player.position))
                        {
                            canSeePlayer = true;
                        }
                    }
                }
            }
        }

        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;

            // Se ele NÃO estava perseguindo antes, significa que ACABOU de ver
            if (!chasing)
            {
                PlaySpotSound(); // Toca o som
                StartChasing();
            }

            StopCoroutine("chaseRoutine");
            StartCoroutine("chaseRoutine");
        }
        else if (chasing && !searching)
        {
            ai.destination = lastKnownPlayerPosition;
        }
    }

    void PlaySpotSound()
    {
        if (audioSource != null && spotSound != null)
        {
            audioSource.PlayOneShot(spotSound);
        }
    }

    void StartChasing()
    {
        walking = false;
        searching = false;
        isWaiting = false;
        chasing = true;

        ai.isStopped = false;
        ai.speed = chaseSpeed;

        if (aiAnim != null) aiAnim.SetTrigger("sprint");

        StopAllCoroutines();
        StartCoroutine(chaseRoutine());
    }

    void HandleChasing()
    {
        ai.destination = player.position;

        if ((player.position - transform.position).sqrMagnitude <= catchDistance * catchDistance)
        {
            CatchPlayer();
        }
    }

    void StartSearching()
    {
        chasing = false;
        walking = false;
        isWaiting = false;
        searching = true;

        ai.isStopped = false;
        ai.speed = walkSpeed;
        ai.destination = lastKnownPlayerPosition;

        if (aiAnim != null) aiAnim.SetTrigger("walk");

        StopAllCoroutines();
        StartCoroutine(searchRoutine());
    }

    void HandleSearching()
    {
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            if (!isWaiting)
            {
                if (aiAnim != null) aiAnim.SetTrigger("idle");
                isWaiting = true;
            }
        }
    }

    void HandleWalking()
    {
        if (isWaiting) return;

        ai.speed = walkSpeed;

        // Se chegou no destino de patrulha
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            StartIdleState();
        }
    }

    void StartIdleState()
    {
        isWaiting = true;
        ai.isStopped = true;

        if (aiAnim != null) aiAnim.SetTrigger("idle");

        StartCoroutine(stayIdle());
    }

    void GoToRandomPointNearPlayer()
    {
        // Calcula uma direção aleatória
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);

        // Define uma distância aleatória entre o Mínimo e o Máximo configurado
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

        // Define o alvo baseando-se na posição do PLAYER + Direção * Distancia
        Vector3 targetPos = player.position + (randomDirection * distance);

        // Procura um ponto válido no NavMesh perto desse alvo calculado
        Vector3 finalDest = GetValidNavMeshPoint(targetPos, 5f);

        ai.destination = finalDest;
        ai.isStopped = false;

        if (aiAnim != null) aiAnim.SetTrigger("walk");
    }

    IEnumerator stayIdle()
    {
        yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

        isWaiting = false;
        walking = true;
        // Chama a nova função de patrulha perto do player
        GoToRandomPointNearPlayer();
    }

    IEnumerator chaseRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minChaseTime, maxChaseTime));
        StartSearching();
    }

    IEnumerator searchRoutine()
    {
        yield return new WaitForSeconds(maxSearchTime);

        searching = false;
        walking = true;
        isWaiting = false;
        // Volta a rondar o player
        GoToRandomPointNearPlayer();
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
    }

    void CatchPlayer()
    {
        ai.isStopped = true;
        player.gameObject.SetActive(false);
        if (aiAnim != null) aiAnim.SetTrigger("jumpscare");
        StartCoroutine(deathRoutine());
    }

    bool HasObstacleBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        if (Physics.Raycast(start + rayCastOffset, direction, distance, obstacleLayer))
            return true;
        return false;
    }

    // Modifiquei levemente para aceitar um centro específico
    Vector3 GetValidNavMeshPoint(Vector3 center, float range)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(center, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        // Se não achar nada perto do ponto ideal, tenta no centro (que pode ser o player ou parede)
        // ou retorna a posição atual do inimigo para não quebrar
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            // Desenha a área onde o inimigo pode ir (ROSQUINHA)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer); // Área proibida (muito perto)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer); // Área limite (muito longe)

            Vector3 startPos = transform.position + rayCastOffset;
            Vector3 direction = (player.position - startPos).normalized;
            Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
            Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(startPos, leftLimit * sightDistance);
            Gizmos.DrawRay(startPos, rightLimit * sightDistance);
        }
    }
}
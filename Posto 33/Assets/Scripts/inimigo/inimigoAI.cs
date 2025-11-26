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

    [Header("Velocidades")]
    public float walkSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Tempo de Idle")]
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 1.5f;

    [Header("Patrulha Aleatória")]
    public float wanderRadius = 15f;

    [Header("Perseguição")]
    public float sightDistance = 10f;
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
    private bool isWaiting; // Importante para não travar o loop
    private Vector3 lastKnownPlayerPosition;

    void Start()
    {
        // Seguranças
        if (player == null || ai == null)
        {
            Debug.LogError("Faltam componentes no EnemyAI!");
            enabled = false;
            return;
        }

        // Config inicial
        walking = true;
        chasing = false;
        searching = false;
        isWaiting = false;

        // Vai para o primeiro ponto
        GoToRandomPoint();
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

        // CONFIGURAÇÃO DA "AUDIÇÃO" (Distância que ele te percebe de costas)
        float proximitySense = 3.0f;

        // 1. Verifica se está dentro da distância de visão GERAL
        if (distanceToPlayer <= sightDistance)
        {
            // 2. Lógica Combinada: (Está no Ângulo de Visão?) OU (Está Muito Perto?)
            bool inAngle = Vector3.Angle(transform.forward, directionToPlayer) <= viewAngle / 2f;
            bool tooClose = distanceToPlayer <= proximitySense;

            if (inAngle || tooClose)
            {
                // 3. Raio (Raycast) para garantir que não tem parede no meio
                RaycastHit hit;
                if (Physics.Raycast(transform.position + rayCastOffset, directionToPlayer, out hit, sightDistance))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        // 4. Obstáculos
                        if (!HasObstacleBetween(transform.position, player.position))
                        {
                            canSeePlayer = true;
                        }
                    }
                }
            }
        }

        // Lógica de Troca de Estado
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;

            if (!chasing)
            {
                StartChasing();
            }
            // Mantém a perseguição viva
            StopCoroutine("chaseRoutine");
            StartCoroutine("chaseRoutine");
        }
        else if (chasing && !searching)
        {
            // Perdeu de vista! 
            // Em vez de seguir o player magicamente através da parede, 
            // ele vai correr até o último local onde te viu.
            ai.destination = lastKnownPlayerPosition;
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

        // CORREÇÃO: Trigger chamado só UMA vez aqui, não no Update
        if (aiAnim != null) aiAnim.SetTrigger("sprint");

        StopAllCoroutines();
        StartCoroutine(chaseRoutine());
    }

    void HandleChasing()
    {
        ai.destination = player.position;

        // Verifica captura
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
        // Se chegou no local onde o player sumiu
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            // Fica parado olhando em volta (Idle) enquanto o tempo de busca corre
            if (!isWaiting)
            {
                if (aiAnim != null) aiAnim.SetTrigger("idle");
                isWaiting = true; // Só visual, o timer searchRoutine cuida do resto
            }
        }
    }

    void HandleWalking()
    {
       
        if (isWaiting) return;

        ai.speed = walkSpeed;

        // CORREÇÃO DO BUG INFINITO: Verifica se !isWaiting antes de iniciar a espera
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            StartIdleState();
        }
    }

    void StartIdleState()
    {
        isWaiting = true; // Trava
        ai.isStopped = true;

        // CORREÇÃO: Trigger chamado só uma vez
        if (aiAnim != null) aiAnim.SetTrigger("idle");

        StartCoroutine(stayIdle());
    }

    void GoToRandomPoint()
    {
        Vector3 nextDest = GetRandomPoint(transform.position, wanderRadius);
        ai.destination = nextDest;
        ai.isStopped = false;

        // Volta a andar
        if (aiAnim != null) aiAnim.SetTrigger("walk");
    }

    // --- COROUTINES ---

    IEnumerator stayIdle()
    {
        yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

        // Acabou o tempo de espera, volta a patrulhar
        isWaiting = false;
        walking = true;
        GoToRandomPoint();
    }

    IEnumerator chaseRoutine()
    {
        // Persegue "às cegas" por um tempo antes de ir para o modo Busca
        yield return new WaitForSeconds(Random.Range(minChaseTime, maxChaseTime));

        StartSearching();
    }

    IEnumerator searchRoutine()
    {
        // Fica no modo busca (no local onde player sumiu) por um tempo
        yield return new WaitForSeconds(maxSearchTime);

        // Se não achou, volta a patrulhar
        searching = false;
        walking = true;
        isWaiting = false;
        GoToRandomPoint();
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

    // --- UTILITÁRIOS ---

    bool HasObstacleBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        if (Physics.Raycast(start + rayCastOffset, direction, distance, obstacleLayer))
            return true;
        return false;
    }

    Vector3 GetRandomPoint(Vector3 center, float range)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
    }

    // Gizmos (mantive o seu, estava ótimo)
    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);

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
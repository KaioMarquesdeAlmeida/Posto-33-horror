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

    [Header("Destinos")]

    public List<Transform> destinations;

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
    public float viewAngle = 90f; //Campo de visão
    public float catchDistance = 2f;
    public float minChaseTime = 5f;
    public float maxChaseTime = 10f;
    public Vector3 rayCastOffset = new Vector3(0, 1, 0);

    [Header("Busca")]
    public float maxSearchTime = 3f;

    [Header("Detecção de Terreno")]
    public float obstacleCheckDistance = 2f;
    public LayerMask obstacleLayer;
    public float stairCheckDistance = 1.5f;

    [Header("Death Scene")]
    public string deathScene = "DeathScene";
    public float jumpscareTime = 2f;


    private bool walking;
    private bool chasing;
    private bool searching = false;
    private bool isWaiting = false;
    private Vector3 lastKnownPlayerPosition;


    void Start(){
      

        if (player == null)
        {
            Debug.LogError("Player não atribuído no Inspector!", this);
        }
        else if (!player.CompareTag("Player"))
        {
            Debug.LogError("Tag do Player incorreta! O GameObject referenciado (Player) precisa da Tag 'Player'.", player);
        }

        if (ai == null)
        {
            Debug.LogError("NavMeshAgent não encontrado/atribuído!", this);
        }

        // Checagem se o inimigo tem um Collider ativo para o Raycast
        // Note: O Raycast também pode falhar se o Player não tiver um Collider.
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("O Inimigo precisa de um Collider para interação física/jumpscare!", this);
        }

        // Se passou por todas as checagens críticas (sem parar a execução):
        Debug.Log("Configuração Inicial OK. Iniciando Patrulha.");
        
        // IA começa gerando um ponto aleatório para andar
        Vector3 initialDest = GetRandomPoint(transform.position, wanderRadius);
        if (ai != null) // Verifica novamente, caso o erro acima não tenha impedido
        {
            ai.destination = initialDest;
        }
        walking = true;
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

    /*
    void CheckPlayerDetection(){
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        bool playerIsVisible = false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // PASSO 1: Verificar a Distância
        if (distanceToPlayer <= sightDistance)
        {
            // PASSO 2: Verificar o Ângulo (FOV)
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= viewAngle / 2f) // Divide por 2 pois o ângulo é simétrico
            {
                // PASSO 3: Verificar a Linha de Visão (Raycast)
                if (Physics.Raycast(transform.position + rayCastOffset, directionToPlayer, out hit, sightDistance))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        // PASSO 4: Verificar Obstáculos
                        if (!HasObstacleBetween(transform.position, player.position))
                        {
                            playerIsVisible = true;
                        }
                    }
                }
            }
        }

        if (playerIsVisible){
            lastKnownPlayerPosition = player.position;
            if (searching)
            {
                searching = false;
                StopCoroutine("searchRoutine");
            }
            if (!chasing)
            {
                StartChasing();
            }
            else
            {
                StopCoroutine("chaseRoutine");
                StartCoroutine("chaseRoutine");
            }
        }
        else if (chasing)
        {
            StartSearching();
        }
    }
    */
    void CheckPlayerDetection()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        bool playerIsVisible = false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // PASSO 1: Verificar a Distância
        if (distanceToPlayer <= sightDistance)
        {
            // PASSO 2: Verificar o Ângulo (FOV)
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= viewAngle / 2f) // Divide por 2 pois o ângulo é simétrico (ex: 45 graus para cada lado se viewAngle for 90)
            {
                // PASSO 3: Verificar a Linha de Visão (Raycast)
                // O Raycast deve acertar algo dentro do alcance.
                if (Physics.Raycast(transform.position + rayCastOffset, directionToPlayer, out hit, sightDistance))
                {
                    // PASSO 4: Verificar se o que foi acertado é o Player
                    if (hit.collider.CompareTag("Player"))
                    {
                        // PASSO 5: Verificar Obstáculos
                        // Se o HasObstacleBetween retornar false, significa que a visão está livre.
                        if (!HasObstacleBetween(transform.position, player.position))
                        {
                            // Player visível e sem bloqueios!
                            playerIsVisible = true;
                        }
                    }
                }
            }
        }

        // --- Lógica de Transição de Estados ---

        if (playerIsVisible)
        {
            // O jogador foi visto.
            lastKnownPlayerPosition = player.position;

            if (searching)
            {
                // Para a busca se encontrar o player
                searching = false;
                StopCoroutine("searchRoutine");
            }

            if (!chasing)
            {
                StartChasing();
            }
            else
            {
                // Reinicia o cronômetro de perseguição (mantém o chase ativo)
                StopCoroutine("chaseRoutine");
                StartCoroutine("chaseRoutine");
            }
        }
        else if (chasing)
        {
            // Jogador sumiu de vista: Inicia a Busca!
            StartSearching();
        }
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Vector3 startPos = transform.position + rayCastOffset;
            Vector3 direction = (player.position - startPos).normalized;

            // Desenha as laterais do cone de visão (apenas para referência)
            Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
            Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(startPos, leftLimit * sightDistance);
            Gizmos.DrawRay(startPos, rightLimit * sightDistance);

            // Linha do Raycast (Centro do Cone)
            RaycastHit hit;
            if (Physics.Raycast(startPos, direction, out hit, sightDistance)){
                // Cor da linha: Verde = Visto; Vermelho = Bloqueado/Objeto não-player
                if (hit.collider.CompareTag("Player") && !HasObstacleBetween(transform.position, player.position)){
                    Gizmos.color = Color.green; // Visão Clara!
                }
                else
                {
                    Gizmos.color = Color.red; // Bloqueado
                }
                Gizmos.DrawLine(startPos, hit.point);
            }
            else
            {
                // Nada atingido (máximo alcance)
                Gizmos.color = Color.yellow; // Linha amarela para o Raycast que não acertou nada
                Gizmos.DrawRay(startPos, direction * sightDistance);
            }
        }
    }
    bool HasObstacleBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

         // O Raycast verifica a distância até o fim da visão
        if (Physics.Raycast(start + rayCastOffset, direction, out RaycastHit hit, distance, obstacleLayer))
        {
            // Se acertou algo na Obstacle Layer antes do player, significa que há bloqueio.
            return true;
        }

        // Se o Raycast principal já passou por todos os checks (Tags e Distância)
        // e este Raycast de bloqueio falhou, significa que a visão está livre.
        return false;
    }

    // Patrulha Aleatória Contínua 
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

    void HandleWalking()
    {
        // Remove isWaiting e isStopped para fluidez
        ai.speed = walkSpeed;

        if (aiAnim != null) aiAnim.SetTrigger("walk");

        // Se chegou ao destino, escolhe o próximo imediatamente (fluidez)
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            // Pequena pausa (Idle) antes de escolher o próximo ponto aleatório
            StartIdleState();
        }
    }

    IEnumerator stayIdle()
    {
        float waitTime = Random.Range(minIdleTime, maxIdleTime);
        ai.isStopped = true; // Pausa
        yield return new WaitForSeconds(waitTime);
        ai.isStopped = false; // Continua

        // Transição para WALKING (Patrulha Aleatória)
        Vector3 nextDest = GetRandomPoint(transform.position, wanderRadius);

        ai.destination = nextDest;
        isWaiting = false;
        walking = true;
    }

    //---------------------------------------------------------
    // Perseguição (Chase)
    //---------------------------------------------------------

    void StartChasing()
    {
        // Debug.Log("ENTRANDO EM MODO PERSEGUIÇÃO!"); // Ative este log para testar a detecção

        walking = false;
        searching = false;

        if (isWaiting)
        {
            StopCoroutine("stayIdle");
            isWaiting = false;
        }
        ai.isStopped = false;

        chasing = true;

        StartCoroutine(chaseRoutine());
    }

    void HandleChasing()
    {
        // Debug.Log("Perseguindo: " + player.position); // Ative este log para testar se está no estado

        ai.destination = player.position;
        ai.speed = chaseSpeed;

        if (aiAnim != null)
        {
            aiAnim.SetTrigger("sprint");
        }

        float sqrDistance = (player.position - transform.position).sqrMagnitude;
        if (sqrDistance <= catchDistance * catchDistance)
        {
            CatchPlayer();
        }
    }

    IEnumerator chaseRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minChaseTime, maxChaseTime));
        chasing = false;
    }

    //---------------------------------------------------------
    // Busca (Search)
    //---------------------------------------------------------

    void StartSearching()
    {
        walking = false;
        chasing = false;
        isWaiting = false;

        StopCoroutine("chaseRoutine");

        searching = true;

        ai.isStopped = false;
        ai.destination = lastKnownPlayerPosition;
        ai.speed = walkSpeed;

        if (aiAnim != null) aiAnim.SetTrigger("walk");

        StartCoroutine(searchRoutine());
    }

    void HandleSearching()
    {
        ai.speed = walkSpeed;

        // Se chegou ao último local conhecido, para e espera o tempo de busca
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance + 0.1f)
        {
            ai.isStopped = true;
        }
        else
        {
            ai.isStopped = false;
        }
    }

    IEnumerator searchRoutine()
    {
        yield return new WaitForSeconds(maxSearchTime);

        if (!chasing)
        {
            // Falhou em encontrar. Volta a patrulhar aleatoriamente.
            searching = false;
            walking = true;

            isWaiting = false;
            ai.isStopped = false;

            Vector3 nextDest = GetRandomPoint(transform.position, wanderRadius);
            ai.destination = nextDest;
        }
    }

    //---------------------------------------------------------
    // Funções Auxiliares
    //---------------------------------------------------------

    void StartIdleState()
    {
        isWaiting = true;
        walking = false;
        chasing = false;
        searching = false; // Garante que não está buscando durante o idle
        ai.isStopped = true;

        if (aiAnim != null) aiAnim.SetTrigger("idle");

        StartCoroutine(stayIdle());
    }

    void CatchPlayer()
    {
        ai.isStopped = true;
        player.gameObject.SetActive(false);

        if (aiAnim != null) aiAnim.SetTrigger("jumpscare");

        StartCoroutine(deathRoutine());
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
    }
}
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
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [Header("Perseguição")]
    public float sightDistance = 10f;
    public float catchDistance = 2f;
    public float minChaseTime = 5f;
    public float maxChaseTime = 10f;
    public Vector3 rayCastOffset = new Vector3(0, 1, 0);

    [Header("Detecção de Terreno")]
    public float obstacleCheckDistance = 2f;
    public LayerMask obstacleLayer;
    public float stairCheckDistance = 1.5f;

    [Header("Death Scene")]
    public string deathScene = "DeathScene";
    public float jumpscareTime = 2f;

    // Variáveis privadas
    private Transform currentDest;
    private Vector3 dest;
    private int randNum;
    private float idleTime;
    private float chaseTime;
    private bool walking;
    private bool chasing;
    private bool isOnStairs;
    private float lastHeight;

    void Start(){
        // Verificações de segurança
        if (destinations == null || destinations.Count == 0){
            Debug.LogError("EnemyAI: Lista de destinations está vazia!");
            enabled = false;
            return;
        }

        if (ai == null){
            Debug.LogError("EnemyAI: NavMeshAgent não atribuído!");
            enabled = false;
            return;
        }

        if (player == null){
            Debug.LogError("EnemyAI: Player não atribuído!");
            enabled = false;
            return;
        }

        // Inicialização
        walking = true;
        lastHeight = transform.position.y;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
    }

    void Update(){
        if (player == null) return;

        DetectStairs();
        CheckPlayerDetection();

        if (chasing)
            HandleChasing();
        else if (walking)
            HandleWalking();
    }

    // Detecta se está em escada baseado na mudança de altura
    void DetectStairs(){
        float currentHeight = transform.position.y;
        float heightDifference = Mathf.Abs(currentHeight - lastHeight);

        // Se mudou muito de altura em um frame, está em escada
        isOnStairs = heightDifference > 0.1f && ai.velocity.magnitude > 0.1f;

        lastHeight = currentHeight;

        // Ajusta animação para escadas (se tiver)
        if (aiAnim != null && isOnStairs)
        {
            // Você pode adicionar um trigger específico para escadas
            // aiAnim.SetBool("onStairs", true);
        }
    }

    void CheckPlayerDetection(){
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;

        // Raycast para detectar player
        if (Physics.Raycast(transform.position + rayCastOffset, direction, out hit, sightDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Verifica se há parede entre inimigo e player
                if (!HasObstacleBetween(transform.position, player.position))
                {
                    StartChasing();
                }
            }
        }
    }

    // Verifica se há obstáculo/parede entre dois pontos
    bool HasObstacleBetween(Vector3 start, Vector3 end){
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        RaycastHit hit;

        if (Physics.Raycast(start + rayCastOffset, direction, out hit, distance, obstacleLayer))
        {
            // Há uma parede/obstáculo no caminho
            return true;
        }

        return false;
    }

    void StartChasing(){
        walking = false;
        StopCoroutine("stayIdle");
        StopCoroutine("chaseRoutine");
        StartCoroutine("chaseRoutine");
        chasing = true;
    }

    void HandleChasing(){
        // NavMesh automaticamente encontra o caminho, incluindo escadas
        dest = player.position;
        ai.destination = dest;
        ai.speed = chaseSpeed;

        // Animações
        if (aiAnim != null){
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("sprint");
        }

        // Verifica se pegou o player
        float sqrDistance = (player.position - transform.position).sqrMagnitude;
        if (sqrDistance <= catchDistance * catchDistance){
            CatchPlayer();
        }
    }

    void HandleWalking(){
        if (currentDest == null) return;

        dest = currentDest.position;
        ai.destination = dest;
        ai.speed = walkSpeed;

        // Animações
        if (aiAnim != null){
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("walk");
        }

        // Chegou no destino
        if (ai.remainingDistance <= ai.stoppingDistance && !ai.pathPending){
            StartIdleState();
        }
    }

    void CatchPlayer(){
        player.gameObject.SetActive(false);

        if (aiAnim != null){
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("sprint");
            aiAnim.SetTrigger("jumpscare");
        }

        StartCoroutine(deathRoutine());
        chasing = false;
        ai.isStopped = true;
    }

    void StartIdleState(){

        if (aiAnim != null){
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("walk");
            aiAnim.SetTrigger("idle");
        }

        ai.speed = 0;
        StopCoroutine("stayIdle");
        StartCoroutine("stayIdle");
        walking = false;
    }

    IEnumerator stayIdle(){
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);

        walking = true;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
    }

    IEnumerator chaseRoutine(){
        chaseTime = Random.Range(minChaseTime, maxChaseTime);
        yield return new WaitForSeconds(chaseTime);

        walking = true;
        chasing = false;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
    }

    IEnumerator deathRoutine(){
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
    }

    // Visualização no editor
    void OnDrawGizmos(){
        if (player != null){
            // Raycast de visão
            Gizmos.color = chasing ? Color.red : Color.yellow;
            Vector3 direction = (player.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position + rayCastOffset, direction * sightDistance);

            // Distância de captura
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchDistance);
        }

        // Mostra destino atual
        if (currentDest != null){
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentDest.position);
            Gizmos.DrawWireSphere(currentDest.position, 0.5f);
        }

        // Mostra se está em escada
        if (Application.isPlaying && isOnStairs){
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
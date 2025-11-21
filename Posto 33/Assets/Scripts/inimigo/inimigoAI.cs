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
    private int randNum;
    private float chaseTime;
    private bool walking;
    private bool chasing;
    private bool isWaiting = false; 

    void Start()
    {
        if (destinations.Count > 0)
        {
            currentDest = destinations[Random.Range(0, destinations.Count)];
        }
        walking = true;
    }

    void Update()
    {
        if (player == null) return;

        CheckPlayerDetection();

        if (chasing)
            HandleChasing();
        else if (walking)
            HandleWalking();
    }

    void CheckPlayerDetection()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(transform.position + rayCastOffset, direction, out hit, sightDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (!HasObstacleBetween(transform.position, player.position))
                {
                    StartChasing();
                }
            }
        }
    }

    bool HasObstacleBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        // Verifica se bate em algo que NÃO seja o player
        if (Physics.Raycast(start + rayCastOffset, direction, distance, obstacleLayer))
        {
            return true;
        }
        return false;
    }

    void StartChasing()
    {
        walking = false;

        // Cancela idle sem matar todas as coroutines
        if (isWaiting)
        {
            StopCoroutine("stayIdle");
            ai.isStopped = false;
            isWaiting = false;
        }

        chasing = true;

        StartCoroutine(chaseRoutine());
    }


    void HandleChasing()
    {
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

    void HandleWalking()
    {
        if (currentDest == null || isWaiting) return; // Se estiver esperando, não faz nada

        ai.destination = currentDest.position;
        ai.speed = walkSpeed;

        if (aiAnim != null) aiAnim.SetTrigger("walk");

        // Verifica se chegou (CORREÇÃO DO BUG AQUI)
        if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
        {
            StartIdleState();
        }
    }

    void StartIdleState()
    {
        isWaiting = true; // Trava para não chamar essa função de novo
        walking = false;
        ai.isStopped = true; // Para o NavMesh Agent fisicamente

        if (aiAnim != null) aiAnim.SetTrigger("idle");

        StartCoroutine(stayIdle());
    }

    IEnumerator stayIdle()
    {
        float waitTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(waitTime);

        // Volta a andar
        ai.isStopped = false;
        isWaiting = false; // Destrava
        walking = true;

        // Escolhe novo ponto
        if (destinations.Count > 0)
            currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    void CatchPlayer()
    {
        ai.isStopped = true;
        player.gameObject.SetActive(false); // Some com o player

        if (aiAnim != null) aiAnim.SetTrigger("jumpscare");

        StartCoroutine(deathRoutine());
    }

    IEnumerator chaseRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minChaseTime, maxChaseTime));
        chasing = false;
        walking = true;
        if (destinations.Count > 0)
            currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
    }
}
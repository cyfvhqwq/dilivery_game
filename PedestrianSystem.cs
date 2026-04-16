using System.Collections;
using UnityEngine;

namespace CourierCity.City
{
    public class Pedestrian : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 1.2f;
        [SerializeField] private float scatterSpeed = 4f;
        [SerializeField] private float scatterDuration = 2f;
        [SerializeField] private float waypointRadius = 6f;

        private Vector2 wanderTarget;
        private Rigidbody2D rb;
        private bool isScattered;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            PickNewWanderTarget();
        }

        private void OnEnable()
        {
            foreach (var p in FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None))
                p.OnHornUsed += OnHornHeard;
        }

        private void OnDisable()
        {
            foreach (var p in FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None))
                p.OnHornUsed -= OnHornHeard;
        }

        private void FixedUpdate()
        {
            if (isScattered) return;

            Vector2 dir = (wanderTarget - rb.position);
            if (dir.magnitude < 0.3f) PickNewWanderTarget();
            else rb.MovePosition(rb.position + dir.normalized * walkSpeed * Time.fixedDeltaTime);
        }

        private void PickNewWanderTarget()
        {
            wanderTarget = rb.position + Random.insideUnitCircle * waypointRadius;
        }

        private void OnHornHeard()
        {
            var players = FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None);
            float closestDist = float.MaxValue;
            Vector2 closestPos = rb.position;

            foreach (var p in players)
            {
                float d = Vector2.Distance(rb.position, p.transform.position);
                if (d < closestDist) { closestDist = d; closestPos = p.transform.position; }
            }

            if (closestDist < 4f) StartCoroutine(ScatterRoutine(closestPos));
        }

        private IEnumerator ScatterRoutine(Vector2 sourcePos)
        {
            isScattered = true;
            Vector2 fleeDir = ((Vector2)transform.position - sourcePos).normalized;

            float timer = scatterDuration;
            while (timer > 0f)
            {
                rb.MovePosition(rb.position + fleeDir * scatterSpeed * Time.fixedDeltaTime);
                timer -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            isScattered = false;
            PickNewWanderTarget();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!col.gameObject.TryGetComponent<Player.PlayerController>(out _)) return;
            Core.GameManager.Instance?.PenalizeScore(10);
        }
    }

    // ---- Crowd Spawner -----
    public class CrowdSpawner : MonoBehaviour
    {
        [SerializeField] private Pedestrian pedestrianPrefab;
        [SerializeField] private int count = 20;
        [SerializeField] private float spawnRadius = 12f;

        private void Start()
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
                Instantiate(pedestrianPrefab, pos, Quaternion.identity, transform);
            }
        }
    }
}

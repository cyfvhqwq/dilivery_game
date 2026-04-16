using System.Collections;
using UnityEngine;

namespace CourierCity.City
{
    public class TrafficVehicle : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private float speedVariance = 1f;

        [Header("Lane")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private bool looping = true;

        [Header("Horn Response")]
        [SerializeField] private float hornReactionTime = 0.5f;
        [SerializeField] private float brakeForce = 8f;

        private int currentWaypoint;
        private float currentSpeed;
        private bool isBraking;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentSpeed = speed + Random.Range(-speedVariance, speedVariance);
        }

        private void OnEnable()
        {
            // Subscribe to horn event from all players in scene
            var players = FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players) p.OnHornUsed += OnHornHeard;
        }

        private void OnDisable()
        {
            var players = FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players) p.OnHornUsed -= OnHornHeard;
        }

        private void FixedUpdate()
        {
            if (waypoints == null || waypoints.Length == 0) return;
            if (isBraking) return;

            Transform target = waypoints[currentWaypoint];
            Vector2 dir = ((Vector2)target.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * currentSpeed * Time.fixedDeltaTime);

            if (Vector2.Distance(rb.position, target.position) < 0.2f)
                AdvanceWaypoint();
        }

        private void AdvanceWaypoint()
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                if (looping) currentWaypoint = 0;
                else gameObject.SetActive(false);
            }
        }

        private void OnHornHeard()
        {
            float dist = Vector2.Distance(
                transform.position,
                FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None)[0].transform.position);

            if (dist < 5f) StartCoroutine(BrakeRoutine());
        }

        private IEnumerator BrakeRoutine()
        {
            isBraking = true;
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(hornReactionTime);
            isBraking = false;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<Player.PlayerController>(out var player))
            {
                if (player.IsInvincible) return;
                // Knock player back
                Vector2 dir = (col.transform.position - transform.position).normalized;
                col.rigidbody.AddForce(dir * 8f, ForceMode2D.Impulse);
                Core.GameManager.Instance?.PenalizeScore(50);
            }
        }
    }

    // ---- Traffic Spawner ----
    public class TrafficSpawner : MonoBehaviour
    {
        [SerializeField] private TrafficVehicle[] vehiclePrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int maxVehicles = 12;
        [SerializeField] private float spawnInterval = 3f;

        private int activeCount;

        private void Start() => InvokeRepeating(nameof(TrySpawn), 1f, spawnInterval);

        private void TrySpawn()
        {
            if (activeCount >= maxVehicles) return;
            if (vehiclePrefabs.Length == 0 || spawnPoints.Length == 0) return;

            var prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var vehicle = Instantiate(prefab, point.position, point.rotation);
            vehicle.gameObject.AddComponent<VehicleLifetime>().Init(this);
            activeCount++;
        }

        public void OnVehicleDestroyed() => activeCount--;
    }

    public class VehicleLifetime : MonoBehaviour
    {
        private TrafficSpawner spawner;
        public void Init(TrafficSpawner s) => spawner = s;
        private void OnDestroy() => spawner?.OnVehicleDestroyed();
    }
}

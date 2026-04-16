using UnityEngine;
using System.Collections;

namespace CourierCity.Core
{
    public class Package : MonoBehaviour
    {
        [Header("Package Info")]
        public string PackageID;
        public DeliveryZone TargetZone;
        public int ScoreValue = 100;

        [Header("Runaway")]
        [SerializeField] private float runawaySpeed = 4f;
        [SerializeField] private float runawayDuration = 15f;

        private PlayerController carrier;
        private bool isRunaway;
        private Rigidbody2D rb;
        private SpriteRenderer sr;

        public System.Action<Package> OnCaught;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void OnPickedUp(PlayerController player)
        {
            carrier = player;
            rb.simulated = false;
            isRunaway = false;
            StopAllCoroutines();
            AttachToPlayer();
        }

        public void OnDropped(Vector2 position)
        {
            carrier = null;
            rb.simulated = true;
            transform.position = position;
        }

        public void StartRunaway()
        {
            if (carrier == null) return;
            carrier.DropPackage(this);
            isRunaway = true;
            StartCoroutine(RunawayRoutine());
        }

        private IEnumerator RunawayRoutine()
        {
            float timer = runawayDuration;
            sr.color = Color.yellow; // visual cue

            while (isRunaway && timer > 0f)
            {
                var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
                Vector2 fleeDir = Vector2.zero;

                foreach (var p in players)
                {
                    Vector2 dir = (transform.position - p.transform.position).normalized;
                    fleeDir += dir;
                }

                fleeDir.Normalize();
                rb.linearVelocity = fleeDir * runawaySpeed;

                rb.linearVelocity += Random.insideUnitCircle * 1.5f;

                timer -= Time.deltaTime;
                yield return null;
            }

            isRunaway = false;
            rb.linearVelocity = Vector2.zero;
            sr.color = Color.white;
        }

        private void AttachToPlayer()
        {

            transform.SetParent(carrier.transform);
            transform.localPosition = new Vector3(0.3f, 0.6f, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isRunaway) return;
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                isRunaway = false;
                StopAllCoroutines();
                player.TryPickup(this);
                OnCaught?.Invoke(this);
            }
        }
    }
}

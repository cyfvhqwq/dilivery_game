using System.Collections;
using UnityEngine;

namespace CourierCity.Powerups
{
    // ── Base class ──────────────────────────────────────────────────────────────
    public abstract class PowerupEffect : MonoBehaviour
    {
        [SerializeField] protected float duration = 6f;
        public abstract void Apply(Player.PlayerController player);
        public abstract void Remove(Player.PlayerController player);
    }

    // ── Lucky Box ───────────────────────────────────────────────────────────────
    public class LuckyBox : MonoBehaviour
    {
        [SerializeField] private PowerupEffect[] possibleEffects;
        [SerializeField] private GameObject collectFX;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Player.PlayerController>(out var player)) return;
            if (possibleEffects == null || possibleEffects.Length == 0) return;

            var effect = possibleEffects[Random.Range(0, possibleEffects.Length)];
            var instance = Instantiate(effect);
            player.StartCoroutine(ApplyTimedEffect(instance, player));

            if (collectFX) Instantiate(collectFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private IEnumerator ApplyTimedEffect(PowerupEffect effect, Player.PlayerController player)
        {
            effect.Apply(player);
            yield return new WaitForSeconds(effect.duration);
            effect.Remove(player);
            Destroy(effect.gameObject);
        }
    }

    // ── Speed Boots ─────────────────────────────────────────────────────────────
    public class SpeedBootsEffect : PowerupEffect
    {
        [SerializeField] private float speedBonus = 2f;
        public override void Apply(Player.PlayerController p) => p.SpeedMultiplier += speedBonus;
        public override void Remove(Player.PlayerController p) => p.SpeedMultiplier -= speedBonus;
    }

    // ── Invincibility ───────────────────────────────────────────────────────────
    public class InvincibilityEffect : PowerupEffect
    {
        public override void Apply(Player.PlayerController p) => p.IsInvincible = true;
        public override void Remove(Player.PlayerController p) => p.IsInvincible = false;
    }

    // ── Dog Bone (distract aggressive animals) ──────────────────────────────────
    public class DogBoneEffect : PowerupEffect
    {
        public override void Apply(Player.PlayerController p)
        {
            // Find all aggressive animals and distract them
            foreach (var animal in FindObjectsByType<AggressiveAnimal>(FindObjectsSortMode.None))
                animal.Distract(duration);
        }
        public override void Remove(Player.PlayerController p) { /* Animals return on their own */ }
    }

    // ── Aggressive Animal (for DogBone to distract) ─────────────────────────────
    public class AggressiveAnimal : MonoBehaviour
    {
        [SerializeField] private float chaseSpeed = 3.5f;
        private Player.PlayerController target;
        private bool isDistracted;
        private Rigidbody2D rb;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Start()
        {
            var players = FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None);
            if (players.Length > 0) target = players[0];
        }

        private void FixedUpdate()
        {
            if (isDistracted || target == null) return;
            Vector2 dir = ((Vector2)target.transform.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);
        }

        public void Distract(float seconds) => StartCoroutine(DistractRoutine(seconds));

        private IEnumerator DistractRoutine(float seconds)
        {
            isDistracted = true;
            yield return new WaitForSeconds(seconds);
            isDistracted = false;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!col.gameObject.TryGetComponent<Player.PlayerController>(out var p)) return;
            if (p.IsInvincible) return;
            p.SpeedMultiplier = Mathf.Max(0.1f, p.SpeedMultiplier - 1f);
            Core.GameManager.Instance?.PenalizeScore(25);
        }
    }

    // ── Ice Slide ───────────────────────────────────────────────────────────────
    public class IceSlideEffect : PowerupEffect
    {
        [SerializeField] private GameObject icePatch; // visual overlay

        private GameObject spawnedPatch;

        public override void Apply(Player.PlayerController p)
        {
            p.IsSlipping = true;
            if (icePatch)
                spawnedPatch = Instantiate(icePatch, p.transform.position, Quaternion.identity, p.transform);

            // Also affect nearby vehicles / pedestrians via area trigger — simplified here
        }

        public override void Remove(Player.PlayerController p)
        {
            p.IsSlipping = false;
            if (spawnedPatch) Destroy(spawnedPatch);
        }
    }

    // ── Runaway Package ─────────────────────────────────────────────────────────
    public class RunawayPackageEffect : PowerupEffect
    {
        public override void Apply(Player.PlayerController p)
        {
            if (p.CarriedPackages.Count == 0) return;
            // Pick a random carried package and make it flee
            var pkg = p.CarriedPackages[Random.Range(0, p.CarriedPackages.Count)];
            pkg.StartRunaway();
        }

        public override void Remove(Player.PlayerController p) { /* Package catches itself on contact */ }
    }

    // ── Fog ─────────────────────────────────────────────────────────────────────
    public class FogEffect : PowerupEffect
    {
        [SerializeField] private GameObject fogOverlayPrefab; // Dark sprite covering camera
        [SerializeField] private float visibilityRadius = 2f;

        private GameObject fogInstance;

        public override void Apply(Player.PlayerController p)
        {
            if (fogOverlayPrefab)
            {
                fogInstance = Instantiate(fogOverlayPrefab, p.transform);
                // The overlay should be a large black sprite with a circular cutout shader
                // (use a custom shader or URP 2D Light for the reveal area)
            }
        }

        public override void Remove(Player.PlayerController p)
        {
            if (fogInstance) Destroy(fogInstance);
        }
    }
}

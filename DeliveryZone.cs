using UnityEngine;

namespace CourierCity.Player
{
    public class DeliveryZone : MonoBehaviour
    {
        [SerializeField] private string zoneID;
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private Color activeColor = new Color(0f, 1f, 0f, 0.4f);

        public string ZoneID => zoneID;
        public bool HasPendingDelivery { get; private set; }
        private Package expectedPackage;

        public System.Action<DeliveryZone, Package> OnDeliveryCompleted;

        public void AssignPackage(Package package)
        {
            expectedPackage = package;
            HasPendingDelivery = true;
            if (highlight) highlight.color = activeColor;
        }

        public void ClearAssignment()
        {
            expectedPackage = null;
            HasPendingDelivery = false;
            if (highlight) highlight.color = Color.clear;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!HasPendingDelivery) return;
            if (!other.TryGetComponent<PlayerController>(out var player)) return;

            foreach (var pkg in player.CarriedPackages)
            {
                if (pkg == expectedPackage || expectedPackage == null)
                {
                    player.DeliverPackage(pkg);
                    OnDeliveryCompleted?.Invoke(this, pkg);
                    ClearAssignment();
                    return;
                }
            }
        }
    }

    public class PickupZone : MonoBehaviour
    {
        [SerializeField] private Package packagePrefab;
        [SerializeField] private DeliveryZone targetZone;
        [SerializeField] private float respawnTime = 8f;

        private Package currentPackage;
        private float respawnTimer;
        public System.Action<Package> OnPackageSpawned;

        private void Start() => SpawnPackage();

        private void Update()
        {
            if (currentPackage != null) return;
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f) SpawnPackage();
        }

        private void SpawnPackage()
        {
            if (packagePrefab == null) return;
            currentPackage = Instantiate(packagePrefab, transform.position, Quaternion.identity);
            currentPackage.TargetZone = targetZone;
            currentPackage.PackageID = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            targetZone.AssignPackage(currentPackage);
            OnPackageSpawned?.Invoke(currentPackage);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (currentPackage == null) return;
            if (!other.TryGetComponent<PlayerController>(out var player)) return;
            if (player.TryPickup(currentPackage))
            {
                currentPackage = null;
                respawnTimer = respawnTime;
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CourierCity.Player
{
    [RequireComponent(typeof(LineRenderer))]
    public class AutoNavigator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Line Settings")]
        [SerializeField] private float updateInterval = 0.25f;
        [SerializeField] private int lineSegments = 32;
        [SerializeField] private Color lineColor = new Color(0.2f, 0.9f, 0.4f, 0.8f);

        private LineRenderer lr;
        private Transform currentTarget;
        private Coroutine updateRoutine;

        private void Awake()
        {
            lr = GetComponent<LineRenderer>();
            lr.startWidth = 0.08f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lineColor;
            lr.endColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            lr.positionCount = 0;
        }

        private void OnEnable()
        {
            player.OnPackagePickedUp += OnPackagePickedUp;
            player.OnPackageDelivered += OnPackageDelivered;
        }

        private void OnDisable()
        {
            player.OnPackagePickedUp -= OnPackagePickedUp;
            player.OnPackageDelivered -= OnPackageDelivered;
        }

        private void OnPackagePickedUp(Package pkg)
        {
            if (pkg.TargetZone == null) return;
            SetTarget(pkg.TargetZone.transform);
        }

        private void OnPackageDelivered(Package pkg)
        {
            // Point to next carried package's destination, or clear
            if (player.CarriedPackages.Count > 0 && player.CarriedPackages[0].TargetZone != null)
                SetTarget(player.CarriedPackages[0].TargetZone.transform);
            else
                ClearTarget();
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;
            if (updateRoutine != null) StopCoroutine(updateRoutine);
            updateRoutine = StartCoroutine(UpdateLine());
        }

        public void ClearTarget()
        {
            currentTarget = null;
            if (updateRoutine != null) StopCoroutine(updateRoutine);
            lr.positionCount = 0;
        }

        private IEnumerator UpdateLine()
        {
            while (currentTarget != null)
            {
                DrawLine();
                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void DrawLine()
        {
            if (currentTarget == null) return;

            Vector3 start = player.transform.position;
            Vector3 end = currentTarget.position;

            // Simple straight line — swap with A* result if pathfinding added
            lr.positionCount = lineSegments;
            for (int i = 0; i < lineSegments; i++)
            {
                float t = i / (float)(lineSegments - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                // Slight wave for visual style
                pos.y += Mathf.Sin(t * Mathf.PI) * 0.3f;
                lr.SetPosition(i, pos);
            }
        }
    }
}

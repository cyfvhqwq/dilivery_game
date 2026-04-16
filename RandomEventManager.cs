using System.Collections;
using UnityEngine;

namespace CourierCity.City
{
    public enum CityEventType { HeavyRain, RoadAccident, CrowdSurge }

    [System.Serializable]
    public class CityEvent
    {
        public CityEventType Type;
        public float Duration;
        public float Weight = 1f; 
    }

    public class RandomEventManager : MonoBehaviour
    {
        [SerializeField] private CityEvent[] possibleEvents;
        [SerializeField] private float minTimeBetweenEvents = 15f;
        [SerializeField] private float maxTimeBetweenEvents = 40f;

        [Header("FX References")]
        [SerializeField] private GameObject rainParticlesPrefab;
        [SerializeField] private GameObject accidentBlockPrefab;
        [SerializeField] private GameObject crowdPrefab;

        private bool eventActive;

        private void Start() => StartCoroutine(EventLoop());

        private IEnumerator EventLoop()
        {
            while (true)
            {
                float wait = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
                yield return new WaitForSeconds(wait);
                if (!eventActive) yield return StartCoroutine(RunRandomEvent());
            }
        }

        private IEnumerator RunRandomEvent()
        {
            var ev = PickEvent();
            if (ev == null) yield break;

            eventActive = true;
            Debug.Log($"[CityEvent] Starting: {ev.Type}");

            switch (ev.Type)
            {
                case CityEventType.HeavyRain:
                    yield return StartCoroutine(HeavyRain(ev.Duration));
                    break;
                case CityEventType.RoadAccident:
                    yield return StartCoroutine(RoadAccident(ev.Duration));
                    break;
                case CityEventType.CrowdSurge:
                    yield return StartCoroutine(CrowdSurge(ev.Duration));
                    break;
            }

            eventActive = false;
        }

        private IEnumerator HeavyRain(float duration)
        {
            var rain = rainParticlesPrefab ? Instantiate(rainParticlesPrefab) : null;
            foreach (var p in FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None))
                p.SpeedMultiplier = 0.6f;

            yield return new WaitForSeconds(duration);

            foreach (var p in FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None))
                p.SpeedMultiplier = 1f;
            if (rain) Destroy(rain);
        }

        private IEnumerator RoadAccident(float duration)
        {
            Vector3 pos = GetRandomRoadPosition();
            var block = accidentBlockPrefab ? Instantiate(accidentBlockPrefab, pos, Quaternion.identity) : null;
            yield return new WaitForSeconds(duration);
            if (block) Destroy(block);
        }

        private IEnumerator CrowdSurge(float duration)
        {
            Vector3 pos = GetRandomRoadPosition();
            var crowd = crowdPrefab ? Instantiate(crowdPrefab, pos, Quaternion.identity) : null;
            yield return new WaitForSeconds(duration);
            if (crowd) Destroy(crowd);
        }

        private CityEvent PickEvent()
        {
            if (possibleEvents == null || possibleEvents.Length == 0) return null;
            float totalWeight = 0f;
            foreach (var e in possibleEvents) totalWeight += e.Weight;
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var e in possibleEvents)
            {
                cumulative += e.Weight;
                if (roll <= cumulative) return e;
            }
            return possibleEvents[0];
        }

        private Vector3 GetRandomRoadPosition()
        {

            return new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PackageManager;

namespace CourierCity.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 180f; 
        [SerializeField] private int penaltyPerSecondLate = 2;

        [Header("Events")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<float> OnTimeChanged;
        public UnityEvent OnGameOver;

        public int Score { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsPlaying { get; private set; }

        private int deliveryCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => StartGame();

        private void Update()
        {
            if (!IsPlaying) return;
            TimeRemaining -= Time.deltaTime;
            OnTimeChanged?.Invoke(TimeRemaining);
            if (TimeRemaining <= 0f) EndGame();
        }

        public void StartGame()
        {
            Score = 0;
            TimeRemaining = gameDuration;
            deliveryCount = 0;
            IsPlaying = true;
        }

        public void RegisterDelivery(Package package)
        {
            if (!IsPlaying) return;
            deliveryCount++;
            AddScore(package.ScoreValue);
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void PenalizeScore(int amount)
        {
            Score = Mathf.Max(0, Score - amount);
            OnScoreChanged?.Invoke(Score);
        }

        private void EndGame()
        {
            IsPlaying = false;
            TimeRemaining = 0f;
            OnGameOver?.Invoke();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CourierCity.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Score & Timer")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Image timerBar;

        [Header("Package Slots")]
        [SerializeField] private Transform packageSlotParent;
        [SerializeField] private GameObject packageSlotPrefab;
        [SerializeField] private int maxSlots = 3;

        [Header("Minimap")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private Camera minimapCamera;

        [Header("Event Banner")]
        [SerializeField] private TMP_Text eventBannerText;
        [SerializeField] private float bannerDuration = 3f;

        [Header("References")]
        [SerializeField] private Player.PlayerController player;
        [SerializeField] private Core.GameManager gameManager;

        private List<GameObject> packageSlots = new();
        private float totalGameTime;
        private float bannerTimer;

        private void Awake()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (packageSlotPrefab == null) break;
                var slot = Instantiate(packageSlotPrefab, packageSlotParent);
                slot.SetActive(false);
                packageSlots.Add(slot);
            }
        }

        private void OnEnable()
        {
            if (gameManager)
            {
                gameManager.OnScoreChanged.AddListener(UpdateScore);
                gameManager.OnTimeChanged.AddListener(UpdateTimer);
            }
            if (player)
            {
                player.OnPackagePickedUp += RefreshPackageSlots;
                player.OnPackageDelivered += RefreshPackageSlots;
            }
            totalGameTime = gameManager ? gameManager.TimeRemaining : 180f;
        }

        private void OnDisable()
        {
            if (gameManager)
            {
                gameManager.OnScoreChanged.RemoveListener(UpdateScore);
                gameManager.OnTimeChanged.RemoveListener(UpdateTimer);
            }
            if (player)
            {
                player.OnPackagePickedUp -= RefreshPackageSlots;
                player.OnPackageDelivered -= RefreshPackageSlots;
            }
        }

        private void Update()
        {
            if (bannerTimer > 0f)
            {
                bannerTimer -= Time.deltaTime;
                if (bannerTimer <= 0f && eventBannerText)
                    eventBannerText.gameObject.SetActive(false);
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText) scoreText.text = score.ToString("N0");
        }

        private void UpdateTimer(float time)
        {
            if (timerText)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
                timerText.color = time < 30f ? Color.red : Color.white;
            }
            if (timerBar)
                timerBar.fillAmount = Mathf.Clamp01(time / totalGameTime);
        }

        private void RefreshPackageSlots(Player.Package _)
        {
            for (int i = 0; i < packageSlots.Count; i++)
            {
                bool occupied = i < player.CarriedPackages.Count;
                packageSlots[i].SetActive(occupied);

                if (occupied)
                {
                    var label = packageSlots[i].GetComponentInChildren<TMP_Text>();
                    if (label) label.text = $"#{player.CarriedPackages[i].PackageID[..4]}";
                }
            }
        }

        public void ShowEventBanner(string message)
        {
            if (!eventBannerText) return;
            eventBannerText.text = message;
            eventBannerText.gameObject.SetActive(true);
            bannerTimer = bannerDuration;
        }
    }
}

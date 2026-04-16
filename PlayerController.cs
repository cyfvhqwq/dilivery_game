using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CourierCity.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float hornCooldown = 2f;

        [Header("Package Carrying")]
        [SerializeField] private int maxCarryCount = 3;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Animator animator;
        private float hornTimer;

        public List<Package> CarriedPackages { get; private set; } = new();
        public bool IsSlipping { get; set; }
        public bool IsInvincible { get; set; }
        public float SpeedMultiplier { get; set; } = 1f;

        // Events
        public System.Action<Package> OnPackagePickedUp;
        public System.Action<Package> OnPackageDelivered;
        public System.Action OnHornUsed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            hornTimer -= Time.deltaTime;
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            float speed = moveSpeed * SpeedMultiplier;

            if (IsSlipping)
            {
                rb.linearDamping = 0.5f;
            }
            else
            {
                rb.linearDamping = 5f;
                rb.linearVelocity = moveInput * speed;
            }
        }


        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnHorn(InputValue value)
        {
            if (!value.isPressed || hornTimer > 0f) return;
            hornTimer = hornCooldown;
            OnHornUsed?.Invoke();
        }

        public bool TryPickup(Package package)
        {
            if (CarriedPackages.Count >= maxCarryCount) return false;
            CarriedPackages.Add(package);
            package.OnPickedUp(this);
            OnPackagePickedUp?.Invoke(package);
            return true;
        }

        public void DeliverPackage(Package package)
        {
            if (!CarriedPackages.Contains(package)) return;
            CarriedPackages.Remove(package);
            OnPackageDelivered?.Invoke(package);
        }

        public void DropPackage(Package package)
        {
            if (!CarriedPackages.Contains(package)) return;
            CarriedPackages.Remove(package);
            package.OnDropped(transform.position);
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetBool("Carrying", CarriedPackages.Count > 0);
        }
    }
}

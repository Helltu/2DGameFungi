using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2D.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Movement")] public float moveSpeed;

        private float _moveDirectionX;
        private Vector2 _watchDirection;

        [Header("Jump")] public float jumpForce;
        public float jumpAdjustForce;
        public float jumpAllowAdjustTime;
        public float coyoteTime;
        public bool hasDoubleJump = true;

        private bool _isJumping;
        private float _jumpStartTime;
        private bool _isGrounded;
        private float _lastTimeGrounded;

        [Header("Dash")] public float dashForce;
        [Tooltip("Dash duration in seconds")] public float dashDuration;
        public float dashTimeout;

        private bool _isDashing;
        private float _dashStartTime;

        [Header("Land")] public float landForce;

        private bool _isLanding;

        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // Don't allow moving while during dash
            if (!_isDashing)
            {
                _rigidbody2D.velocity = new Vector2(_moveDirectionX * moveSpeed, _rigidbody2D.velocity.y);
            }

            // Allow to jump higher on button hold
            if (_isJumping && Time.fixedTime - _jumpStartTime <= jumpAllowAdjustTime)
            {
                _rigidbody2D.AddForce(Vector2.up * jumpAdjustForce, ForceMode2D.Impulse);
            }
        }

        // All input events are connected via PlayerInput
        public void OnMove(InputAction.CallbackContext callbackContext)
        {
            _moveDirectionX = callbackContext.ReadValue<Vector2>().x;

            if (callbackContext.performed && _moveDirectionX != 0)
            {
                _watchDirection = _moveDirectionX > 0 ? Vector2.right : Vector2.left;
            }
        }

        public void OnLand(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                var isForcingLand = callbackContext.ReadValue<Vector2>().y < 0;

                if (!isForcingLand || _isLanding || _isGrounded)
                    return;

                _isLanding = true;
                _rigidbody2D.AddForce(Vector2.down * landForce, ForceMode2D.Impulse);
            }
        }

        public void OnJump(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.started)
            {
                var isCoyoteTimeJump = Time.time - _lastTimeGrounded <= coyoteTime;
                // Initial jump boost
                if (isCoyoteTimeJump || _isGrounded || hasDoubleJump)
                {
                    _isJumping = true;

                    // To reset speed for jump to work correctly
                    _rigidbody2D.velocity = Vector2.zero;

                    _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    _jumpStartTime = Time.time;

                    // Consume double jump if not jump from ground
                    if (!_isGrounded && !isCoyoteTimeJump && hasDoubleJump)
                        hasDoubleJump = false;
                }
            }
            else if (callbackContext.canceled)
            {
                _isJumping = false;
                _jumpStartTime = 0f;
            }
        }

        public void OnDash(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.started && !_isDashing && Time.time - _dashStartTime >= dashTimeout)
            {
                StartCoroutine(DashCoroutine());
            }
        }

        private IEnumerator DashCoroutine()
        {
            _isDashing = true;
            _rigidbody2D.gravityScale = 0f;

            // To reset speed for dash to work correctly
            _rigidbody2D.velocity = Vector2.zero;

            _rigidbody2D.AddForce(_watchDirection * dashForce, ForceMode2D.Impulse);
            _dashStartTime = Time.time;

            yield return new WaitForSeconds(dashDuration);

            _isDashing = false;
            _rigidbody2D.gravityScale = 1f;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
            {
                _isGrounded = true;
                _isLanding = false;
                hasDoubleJump = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
            {
                _isGrounded = false;
                _lastTimeGrounded = Time.time;
            }
        }
    }
}
using System;
using System.Collections;
using Game2D.Scripts.Collectables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2D.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMovementController : MonoBehaviour, IDoubleJumpResetable
    {
        [Header("Movement")] public float moveSpeed;

        private float _moveDirectionX;
        private Vector2 _faceDirection;

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
        private bool _isFacingRight = true;

        [Header("Wall slide")] public bool enableWallGrip;
        public float wallSlideDelay;
        public float slideSpeed;

        private GameObject _wallSlidingFrom;
        private bool _isOnWall;
        private bool _wallIsOnRight;
        private float _yLevelToMaintain;
        private Coroutine _slidingCoroutine;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // Don't allow moving while during dash
            if (!_isDashing)
                _rigidbody2D.velocity = new Vector2(_moveDirectionX * moveSpeed, _rigidbody2D.velocity.y);

            // Allow to jump higher on button hold
            if (_isJumping && Time.fixedTime - _jumpStartTime <= jumpAllowAdjustTime)
                _rigidbody2D.AddForce(Vector2.up * jumpAdjustForce, ForceMode2D.Impulse);

            // Handle wall sliding
            if (enableWallGrip && !_isGrounded && _wallSlidingFrom != null)
            {
                switch (_moveDirectionX)
                {
                    // Pressing wall on right
                    case > 0 when _wallIsOnRight:
                    // Pressing wall on left
                    case < 0 when !_wallIsOnRight:
                        if (!_isOnWall)
                            OnWallSlidingEnter(_wallSlidingFrom);
                        break;
                    default:
                        if (_isOnWall)
                            OnWallSlidingExit();
                        break;
                }
            }

            // Handle rotation on face direction change
            if (_faceDirection == Vector2.left && _isFacingRight)
            {
                _isFacingRight = false;
                Rotate();
            }
            else if (_faceDirection == Vector2.right && !_isFacingRight)
            {
                _isFacingRight = true;
                Rotate();
            }
        }

        private void Rotate()
        {
            transform.localScale =
                new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        // All input events are connected via PlayerInput
        public void OnMove(InputAction.CallbackContext callbackContext)
        {
            _moveDirectionX = Mathf.Round(callbackContext.ReadValue<Vector2>().x);

            // Change watch direction when moving
            if (callbackContext.performed && _moveDirectionX != 0)
            {
                _faceDirection = _moveDirectionX > 0 ? Vector2.right : Vector2.left;
            }
        }

        // Pressing 's' on keyboard launches player downwards
        public void OnLand(InputAction.CallbackContext callbackContext)
        {
            // To allow landing when moving
            if (!callbackContext.performed)
                return;

            var isForcingLand = callbackContext.ReadValue<Vector2>().y < 0;

            if (!isForcingLand || _isLanding || _isOnWall || _isGrounded)
                return;

            _isLanding = true;
            _rigidbody2D.AddForce(Vector2.down * landForce, ForceMode2D.Impulse);
        }

        public void OnJump(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.started)
            {
                var isCoyoteTimeJump = Time.time - _lastTimeGrounded <= coyoteTime;

                // Not jumping
                if (!isCoyoteTimeJump && !_isGrounded && !hasDoubleJump || _isOnWall)
                    return;

                Debug.Log("Jump");
                _isJumping = true;
                _isLanding = false;

                // To reset speed for jump to work correctly
                _rigidbody2D.velocity = Vector2.zero;

                _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _jumpStartTime = Time.time;

                // Consume double jump if not jump from ground
                if (!_isGrounded && !isCoyoteTimeJump && hasDoubleJump)
                    hasDoubleJump = false;
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
                // Don't allow dash while on wall
                if (_isOnWall)
                    return;

                StartCoroutine(DashCoroutine());
            }
        }

        private IEnumerator DashCoroutine()
        {
            Debug.Log("Dash");
            _isDashing = true;
            _rigidbody2D.gravityScale = 0f;

            // To reset speed for dash to work correctly
            _rigidbody2D.velocity = Vector2.zero;

            _rigidbody2D.AddForce(_faceDirection * dashForce, ForceMode2D.Impulse);
            _dashStartTime = Time.time;

            yield return new WaitForSeconds(dashDuration);

            _isDashing = false;
            _rigidbody2D.gravityScale = 1f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var collisionPoint = collision.collider.ClosestPoint(transform.position);

            // If collision point is not underneath -> colliding with wall (+0.2f is threshold)
            if (collisionPoint.y > (transform.position.y - transform.localScale.y / 2f) + 0.2f)
            {
                _wallIsOnRight = collisionPoint.x > transform.position.x;
                _wallSlidingFrom = collision.gameObject;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.Equals(_wallSlidingFrom))
                _wallSlidingFrom = null;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
                return;

            OnGrounding(collision.gameObject);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
                return;

            OnDegrounding();
        }

        private void OnGrounding(GameObject gameObjectGroundingOn)
        {
            _isGrounded = true;
            _isLanding = false;
            hasDoubleJump = true;

            // To make player move with object he is grounded on
            gameObject.transform.SetParent(gameObjectGroundingOn.transform);
        }

        private void OnDegrounding()
        {
            _isGrounded = false;
            _lastTimeGrounded = Time.time;

            gameObject.transform.SetParent(null);
        }

        private void OnWallSlidingEnter(GameObject gameObjectSlidingOn)
        {
            Debug.Log("Wall sliding enter");

            _isOnWall = true;
            _isLanding = false;
            hasDoubleJump = true;

            _rigidbody2D.gravityScale = 0;
            _yLevelToMaintain = transform.position.y;

            // To make player move with object he is grounded on
            gameObject.transform.SetParent(gameObjectSlidingOn.transform);

            _slidingCoroutine = StartCoroutine(SlidingCoroutine());
        }

        private IEnumerator SlidingCoroutine()
        {
            var slidingStartTime = Time.time;
            while (!_isGrounded)
            {
                Debug.Log("Wall sliding");

                _rigidbody2D.MovePosition(new Vector2(transform.position.x, _yLevelToMaintain));

                if (Time.time - slidingStartTime >= wallSlideDelay)
                    _yLevelToMaintain -= slideSpeed;

                yield return new WaitForFixedUpdate();
            }

            OnWallSlidingExit();
        }

        private void OnWallSlidingExit()
        {
            Debug.Log("Wall sliding exit");

            _isOnWall = false;
            _lastTimeGrounded = Time.time;

            _rigidbody2D.gravityScale = 1;

            gameObject.transform.SetParent(null);

            if (_slidingCoroutine != null)
                StopCoroutine(_slidingCoroutine);
        }

        public void ResetDoubleJump()
        {
            hasDoubleJump = true;
        }
    }
}
using System;
using System.Collections;
using DG.Tweening;
using Game2D.Scripts.Player;
using UnityEngine;

namespace Game2D.Scripts.Collectables
{
    public class DoubleJumpResetter : MonoBehaviour
    {
        [Header("Animation properties")]
        public float appearTime = 0.1f;
        public float resetTime = 5f;
        public float idleAnimationDuration;
        [Tooltip("Between 0 and 1")] public float idleAnimationSizeChange;

        private Collider2D _collider2D;
        private SpriteRenderer _spriteRenderer;
        
        private Coroutine _idleAnimationCoroutine;
        private Coroutine _resetCoroutine;
        
        private Vector3 _initialScale;

        private void Awake()
        {
            _initialScale = transform.localScale;
            _collider2D = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _idleAnimationCoroutine = StartCoroutine(IdleAnimationCoroutine());
        }

        private IEnumerator IdleAnimationCoroutine()
        {
            while (true)
            {
                transform.DOScale(_initialScale * idleAnimationSizeChange, idleAnimationDuration);
                yield return new WaitForSeconds(idleAnimationDuration);
                transform.DOScale(_initialScale, idleAnimationDuration);
                yield return new WaitForSeconds(idleAnimationDuration);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.TryGetComponent<IDoubleJumpResetable>(out var doubleJumpResetable))
                return;

            doubleJumpResetable.ResetDoubleJump();
            
            if(_resetCoroutine == null)
                _resetCoroutine = StartCoroutine(ResetCoroutine());
        }

        private IEnumerator ResetCoroutine()
        {
            if (_idleAnimationCoroutine != null)
            {
                StopCoroutine(_idleAnimationCoroutine);
                transform.DOKill();
                _idleAnimationCoroutine = null;
            }
            
            transform.DOScale(new Vector3(0, 0, 0), appearTime);
            yield return new WaitForSeconds(appearTime);

            _spriteRenderer.enabled = false;
            _collider2D.enabled = false;

            yield return new WaitForSeconds(resetTime);
            _spriteRenderer.enabled = true;

            transform.DOScale(_initialScale, appearTime);
            yield return new WaitForSeconds(appearTime);
            _collider2D.enabled = true;

            _idleAnimationCoroutine = StartCoroutine(IdleAnimationCoroutine());
            _resetCoroutine = null;
        }
    }
}
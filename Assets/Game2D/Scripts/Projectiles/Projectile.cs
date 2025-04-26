using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game2D.Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public float initialForce;
        public float liveTime;
        private float _bornTime;

        private Rigidbody2D _rigidbody2D;
        
        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _bornTime = Time.time;
        }

        private void OnDisable()
        {
            _bornTime = 0f;
        }

        private void FixedUpdate()
        {
            if (Time.time - _bornTime >= liveTime)
            {
                ProjectilePool.Singleton.DeallocateProjectile(this);
            }
        }

        public void Launch(Vector2 startPosition, Vector2 direction)
        {
            transform.position = startPosition;
            _rigidbody2D.AddForce(direction * initialForce, ForceMode2D.Impulse);
        }

        private void OnCollisionEnter2D(Collision2D collision2D)
        {
            ProjectilePool.Singleton.DeallocateProjectile(this);
        }
    }
}
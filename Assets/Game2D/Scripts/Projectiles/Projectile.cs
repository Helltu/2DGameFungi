using System.Collections;
using UnityEngine;

namespace Game2D.Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour, IReturnableToPool
    {
        public float initialForce;
        public float liveTime;

        private ProjectilePool _projectilePool;
        private Rigidbody2D _rigidbody2D;
        private Coroutine _lifeCoroutine;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _lifeCoroutine = StartCoroutine(LifeCoroutine());
        }

        private void OnDisable()
        {
            StopCoroutine(_lifeCoroutine);
        }

        private IEnumerator LifeCoroutine()
        {
            yield return new WaitForSeconds(liveTime);

            if (isActiveAndEnabled)
                ReturnToPool();
        }

        public void Initialize(ProjectilePool projectilePool)
        {
            _projectilePool = projectilePool;
        }

        public void Launch(Vector2 startPosition, Vector2 direction)
        {
            transform.position = startPosition;
            _rigidbody2D.AddForce(direction * initialForce, ForceMode2D.Impulse);
        }

        public void ReturnToPool()
        {
            _projectilePool.DeallocateProjectile(this);
        }
    }
}
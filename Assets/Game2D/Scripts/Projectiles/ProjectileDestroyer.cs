using UnityEngine;

namespace Game2D.Scripts.Projectiles
{
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileDestroyer : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision2D)
        {
            if (collision2D.gameObject.TryGetComponent<IReturnableToPool>(out var projectile))
                projectile.ReturnToPool();
        }
    }
}
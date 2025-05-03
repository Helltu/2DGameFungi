using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game2D.Scripts.Projectiles
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField]
        private Projectile projectilePrefab;
        [SerializeField]
        private int maxProjectiles;
        [SerializeField]
        private bool isExpandable;

        private Queue<Projectile> _projectilePool;

        private void Start()
        {
            _projectilePool = new Queue<Projectile>();

            for (var i = 0; i < maxProjectiles; i++)
            {
                AddProjectileToPool();
            }
        }

        private void AddProjectileToPool()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("projectilePrefab is not set");
                return;
            }
            
            var projectileInstance = Instantiate(projectilePrefab, transform);
            _projectilePool.Enqueue(projectileInstance);
            projectileInstance.gameObject.SetActive(false);
        }

        public Projectile AllocateProjectile()
        {
            if (_projectilePool.Count == 0)
            {
                Debug.LogWarning("No projectiles in pool available");
                if (isExpandable)
                {
                    Debug.LogWarning("Adding projectile to expandable pool");
                    AddProjectileToPool();
                }
                else
                {
                    return null;
                }
            }
            
            var projectile = _projectilePool.Dequeue();
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public void DeallocateProjectile(Projectile projectile)
        {
            _projectilePool.Enqueue(projectile);
            projectile.transform.position = Vector3.zero;
            projectile.gameObject.SetActive(false);
        }
    }
}
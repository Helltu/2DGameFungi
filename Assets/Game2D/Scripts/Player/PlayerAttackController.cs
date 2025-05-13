using System;
using Game2D.Scripts.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2D.Scripts.Player
{
    public class PlayerAttackController : MonoBehaviour
    {
        [Header("Projectiles properties")] public float projectileSpawnDistance;
        public float projectileInitialForce;
        public float projectileLiveTime;

        private Camera _mainCamera;
        [SerializeField] private ProjectilePool projectilePool;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        // All input events are connected via PlayerInput
        public void OnFire(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                var projectile = projectilePool.AllocateProjectile();

                if (projectile == null)
                    return;

                projectile.Initialize(projectilePool);

                projectile.initialForce = projectileInitialForce;
                projectile.liveTime = projectileLiveTime;

                var mousePosition = Mouse.current.position.ReadValue();

                if (_mainCamera == null)
                {
                    Debug.LogWarning("MainCamera is null");
                    return;
                }

                var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
                mouseWorldPosition.z = 0;
                var launchDirection = (mouseWorldPosition - transform.position).normalized;
                var bulletSpawnPosition = transform.position + launchDirection * projectileSpawnDistance;

                projectile.Launch(bulletSpawnPosition, launchDirection);
            }
        }
    }
}
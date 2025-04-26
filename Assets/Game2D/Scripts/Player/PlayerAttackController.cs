using Game2D.Scripts.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game2D.Scripts.Player
{
    public class PlayerAttackController : MonoBehaviour
    {
        [Header("Projectiles")]
        public float projectileInitialForce;
        public float projectileLiveTime;
        
        // All input events are connected via PlayerInput
        public void OnFire(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                var projectile = ProjectilePool.Singleton.AllocateProjectile();
                projectile.initialForce = projectileInitialForce;
                projectile.liveTime = projectileLiveTime;
            
                var mousePosition = Mouse.current.position.ReadValue();
            
                projectile.Launch(mousePosition, Vector2.up);
            }
        }
    }
}
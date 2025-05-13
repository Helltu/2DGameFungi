using System;
using UnityEngine;

namespace Game2D.Scripts.Player
{
    public class PlayerCameraFollower : MonoBehaviour
    {
        [Header("Camera properties")] public float cameraSpeed;

        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            var targetCameraPosition = Vector3.Lerp(_mainCamera.transform.position, transform.position,
                Time.fixedDeltaTime * cameraSpeed);
            targetCameraPosition.z = _mainCamera.transform.position.z;

            _mainCamera.transform.position = targetCameraPosition;
        }
    }
}
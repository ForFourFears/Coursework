using System;
using UnityEngine;

namespace Coursework.UI 
{
    public class ParallaxLayer : MonoBehaviour, ISceneInitializable
    {
        
        [SerializeField, Range(0, 1f)] private float _parallaxCoefficient;
        [SerializeField] Transform _camera;

        private float startBackgroundPosX;
        private float startCameraPosX;

        private bool isInitialized;

        public void Initialize()
        {

            _camera = _camera != null ? _camera : Camera.main.transform;

            startBackgroundPosX = transform.position.x;
            startCameraPosX = _camera.transform.position.x;

            isInitialized = true;
        }

        private void LateUpdate()
        {
            if (!isInitialized) return;
             //5. Считаем, на сколько камера ушла от своей стартовой позиции:
                float cameraMovementX = _camera.transform.position.x - startCameraPosX;

            // 6. Считаем новую позицию для фона:
            float targetX = startBackgroundPosX + (cameraMovementX * _parallaxCoefficient);

            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        }
    }
}
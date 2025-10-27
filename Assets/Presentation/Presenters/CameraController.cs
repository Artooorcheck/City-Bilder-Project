using CityBuilder.Presentation.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CityBuilder.Presentation.Presenters
{
    public sealed class CameraController : MonoBehaviour
    {
        private Camera _camera;
        private GridView _gridView;
        private Vector2 _lastMousePosition;

        public void Initialize(GridView gridView)
        {
            _gridView = gridView;
            _camera = Camera.main;
            if (_camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            _camera.transform.position = new Vector3(0f, 25f, -15f);
            _camera.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        }

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null)
            {
                return;
            }

            var moveInput = Vector2.zero;
            if (keyboard.wKey.isPressed) moveInput.y += 1f;
            if (keyboard.sKey.isPressed) moveInput.y -= 1f;
            if (keyboard.dKey.isPressed) moveInput.x += 1f;
            if (keyboard.aKey.isPressed) moveInput.x -= 1f;

            var forward = new Vector3(_camera.transform.forward.x, 0f, _camera.transform.forward.z).normalized;
            var right = new Vector3(_camera.transform.right.x, 0f, _camera.transform.right.z).normalized;
            var moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            var moveSpeed = 20f;
            _camera.transform.position += moveDirection * moveSpeed * Time.deltaTime;

            if (mouse.rightButton.isPressed)
            {
                var delta = mouse.delta.ReadValue();
                var panSpeed = 0.1f;
                _camera.transform.position -= right * delta.x * panSpeed;
                _camera.transform.position -= forward * delta.y * panSpeed;
            }

            var scrollValue = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollValue) > Mathf.Epsilon)
            {
                var zoomSpeed = 0.5f;
                _camera.transform.position += _camera.transform.forward * scrollValue * zoomSpeed;
                var position = _camera.transform.position;
                position.y = Mathf.Clamp(position.y, 10f, 80f);
                _camera.transform.position = position;
            }

            _lastMousePosition = mouse.position.ReadValue();
        }
    }
}

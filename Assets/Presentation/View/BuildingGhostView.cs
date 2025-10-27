using UnityEngine;

namespace CityBuilder.Presentation.View
{
    public sealed class BuildingGhostView : MonoBehaviour
    {
        private MeshRenderer? _renderer;
        private Material? _material;

        private void Awake()
        {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primitive.transform.SetParent(transform, false);
            primitive.transform.localPosition = Vector3.zero;
            primitive.transform.localScale = Vector3.one;
            Destroy(primitive.GetComponent<Collider>());
            _renderer = primitive.GetComponent<MeshRenderer>();
            var meshFilter = primitive.GetComponent<MeshFilter>();
            _material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _material.color = new Color(0f, 1f, 0f, 0.35f);
            _renderer.material = _material;
            primitive.layer = gameObject.layer;
            gameObject.SetActive(false);
        }

        public void Show(Vector3 position, bool canPlace, int rotation)
        {
            if (_renderer == null || _material == null)
            {
                return;
            }

            gameObject.SetActive(true);
            transform.position = position + Vector3.up * 0.51f;
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            _material.color = canPlace ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

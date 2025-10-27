using System;
using UnityEngine;

namespace CityBuilder.Presentation.View
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class BuildingView : MonoBehaviour
    {
        private MeshRenderer _renderer;
        private Color _baseColor;

        public Guid BuildingId { get; private set; }

        public string BuildingTypeId { get; private set; } = string.Empty;

        public int Level { get; private set; }

        public void Initialize(Guid id, string typeId, int level, Color color)
        {
            BuildingId = id;
            BuildingTypeId = typeId;
            _renderer = GetComponent<MeshRenderer>();
            _baseColor = color;
            _renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _renderer.material.color = color;
            SetLevel(level);
        }

        public void SetLevel(int level)
        {
            Level = level;
            var scale = transform.localScale;
            scale.y = 1f + (level - 1) * 0.3f;
            transform.localScale = scale;
        }

        public void SetSelected(bool selected)
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.material.color = selected ? Color.yellow : _baseColor;
        }

        public void ResetColor()
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.material.color = _baseColor;
        }
    }
}

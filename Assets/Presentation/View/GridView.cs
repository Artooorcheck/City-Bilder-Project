using System.Collections.Generic;
using CityBuilder.Domain;
using UnityEngine;

namespace CityBuilder.Presentation.View
{
    public sealed class GridView : MonoBehaviour
    {
        private int _width;
        private int _height;
        private GameObject _highlight;
        private MeshRenderer _highlightRenderer;
        private Material _gridMaterial;
        private Material _highlightMaterial;

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            CreateGround();
            CreateGridLines();
            CreateHighlight();
        }

        public Vector3 GridToWorld(GridPosition position)
        {
            var origin = GetOrigin();
            return new Vector3(origin.x + position.X + 0.5f, 0f, origin.z + position.Y + 0.5f);
        }

        public bool TryGetGridPosition(Vector3 worldPosition, out GridPosition position)
        {
            var origin = GetOrigin();
            var localX = worldPosition.x - origin.x;
            var localY = worldPosition.z - origin.z;
            var gridX = Mathf.FloorToInt(localX);
            var gridY = Mathf.FloorToInt(localY);

            position = new GridPosition(gridX, gridY);
            return gridX >= 0 && gridX < _width && gridY >= 0 && gridY < _height;
        }

        public void ShowHighlight(GridPosition position, bool isValid)
        {
            if (_highlight == null || _highlightRenderer == null)
            {
                return;
            }

            _highlight.SetActive(true);
            _highlight.transform.position = GridToWorld(position) + Vector3.up * 0.01f;
            var color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            _highlightRenderer.material.color = color;
        }

        public void HideHighlight()
        {
            if (_highlight != null)
            {
                _highlight.SetActive(false);
            }
        }

        private (float x, float z) GetOrigin() => (-_width / 2f, -_height / 2f);

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GridGround";
            ground.transform.SetParent(transform, false);
            ground.transform.localScale = new Vector3(_width / 10f, 1f, _height / 10f);
            if (ground.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            }
        }

        private void CreateGridLines()
        {
            var meshObject = new GameObject("GridLines");
            meshObject.transform.SetParent(transform, false);
            meshObject.transform.localPosition = Vector3.zero;
            var meshFilter = meshObject.AddComponent<MeshFilter>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();
            _gridMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
            {
                color = new Color(1f, 1f, 1f, 0.1f)
            };
            meshRenderer.sharedMaterial = _gridMaterial;

            var vertices = new List<Vector3>();
            var indices = new List<int>();
            var origin = GetOrigin();
            for (var x = 0; x <= _width; x++)
            {
                vertices.Add(new Vector3(origin.x + x, 0.01f, origin.z));
                vertices.Add(new Vector3(origin.x + x, 0.01f, origin.z + _height));
                indices.Add(vertices.Count - 2);
                indices.Add(vertices.Count - 1);
            }

            for (var y = 0; y <= _height; y++)
            {
                vertices.Add(new Vector3(origin.x, 0.01f, origin.z + y));
                vertices.Add(new Vector3(origin.x + _width, 0.01f, origin.z + y));
                indices.Add(vertices.Count - 2);
                indices.Add(vertices.Count - 1);
            }

            var mesh = new Mesh
            {
                name = "GridLinesMesh"
            };
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void CreateHighlight()
        {
            _highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _highlight.name = "CellHighlight";
            _highlight.transform.SetParent(transform, false);
            _highlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _highlight.SetActive(false);
            if (_highlight.TryGetComponent<MeshRenderer>(out var renderer))
            {
                _highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                renderer.material = _highlightMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                _highlightRenderer = renderer;
            }

            if (_highlight.TryGetComponent<Collider>(out var collider))
            {
                Destroy(collider);
            }
        }
    }
}

/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirViewSpaceIndicator : MonoBehaviour {
    private const float MaxDistance = 1.5f;
    private const float OriginIndicatorLineWidth = 0.025f;

    private AirViewCamera _camera;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Color[] _colors;
    private int[] _gridIndices;
    private int[] _originIndicatorIndices;

    private int gridSize => 7;
    private int gridVerticeCount => gridSize * 4;
    private int verticeCount => gridVerticeCount + 8;

    [SerializeField] private Color _colorGrid = new Color(0.357f, 0.357f, 0.357f);
    [SerializeField] private Color _colorForward = new Color(0.255f, 0.412f, 0.882f);
    [SerializeField] private Color _colorRightward = new Color(0.698f, 0.133f, 0.133f);

    private void Awake() {
        _camera = FindObjectOfType<AirViewCamera>();
        _mesh = new Mesh();
    }

    private void Update() {
        ensureComponentIntegrity();
        updateMesh();
    }

    public void Enable(bool enable) {
        gameObject.SetActive(enable);
    }

    private void ensureComponentIntegrity() {
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (meshFilter.sharedMesh != _mesh) {
            meshFilter.sharedMesh = _mesh;
        }
        var renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer == null) {
            renderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (renderer.sharedMaterials == null ||
            renderer.sharedMaterials.Length < 2 ||
            renderer.sharedMaterials[0] == null) {
            var material = new Material(Shader.Find("onAirXR/Unlit world space"));
            material.hideFlags = HideFlags.HideAndDontSave;

            renderer.sharedMaterials = new Material[] { material, material };
        }
    }

    private void updateMesh() {
        _mesh.Clear();
        _mesh.subMeshCount = 2;

        if (_vertices == null || _vertices.Length != verticeCount) {
            _vertices = new Vector3[verticeCount];
            _colors = new Color[verticeCount];
        }
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;

        var offset = updateGridLines(_colorGrid);
        updateOriginIndicator(offset, _colorForward, _colorRightward);
    }

    private int updateGridLines(Color color) {
        if (_gridIndices == null || _gridIndices.Length != gridVerticeCount) {
            _gridIndices = new int[gridVerticeCount];
        }

        var origin = worldPositionOfTrackingSpace(Vector3.zero);
        var front = worldPositionOfTrackingSpace(Vector3.forward);
        var right = worldPositionOfTrackingSpace(Vector3.right);

        var rightward = (right - origin).normalized;
        var forward = (front - origin).normalized;

        for (int i = 0; i < gridSize; i++) {
            var first = i * 4;
            var offset = i * (MaxDistance * 2 / (gridSize - 1));

            _vertices[first] = origin + forward * -MaxDistance + rightward * (offset - MaxDistance);
            _vertices[first + 1] = origin + forward * MaxDistance + rightward * (offset - MaxDistance);
            _vertices[first + 2] = origin + forward * (offset - MaxDistance) + rightward * -MaxDistance;
            _vertices[first + 3] = origin + forward * (offset - MaxDistance) + rightward * MaxDistance;

            _colors[first] = _colors[first + 1] = _colors[first + 2] = _colors[first + 3] = color;

            _gridIndices[first] = first;
            _gridIndices[first + 1] = first + 1;
            _gridIndices[first + 2] = first + 2;
            _gridIndices[first + 3] = first + 3;
        }

        _mesh.SetIndices(_gridIndices, MeshTopology.Lines, 0);
        return gridVerticeCount;
    }

    private void updateOriginIndicator(int offset, Color colorForward, Color colorRightward) {
        if (_originIndicatorIndices == null) {
            _originIndicatorIndices = new int[12];
        }

        var origin = worldPositionOfTrackingSpace(Vector3.zero);
        var front = worldPositionOfTrackingSpace(Vector3.forward);
        var right = worldPositionOfTrackingSpace(Vector3.right / 2);

        var rightward = (right - origin).normalized;
        var forward = (front - origin).normalized;

        _vertices[offset] = front - rightward * OriginIndicatorLineWidth / 2;
        _vertices[offset + 1] = front + rightward * OriginIndicatorLineWidth / 2;
        _vertices[offset + 2] = origin - (forward + rightward) * OriginIndicatorLineWidth / 2;
        _vertices[offset + 3] = origin + (forward + rightward) * OriginIndicatorLineWidth / 2;
        _vertices[offset + 4] = origin - (forward + rightward) * OriginIndicatorLineWidth / 2;
        _vertices[offset + 5] = origin + (forward + rightward) * OriginIndicatorLineWidth / 2;
        _vertices[offset + 6] = right - forward * OriginIndicatorLineWidth / 2;
        _vertices[offset + 7] = right + forward * OriginIndicatorLineWidth / 2;

        _colors[offset] = _colors[offset + 1] = _colors[offset + 2] = _colors[offset + 3] = colorForward;
        _colors[offset + 4] = _colors[offset + 5] = _colors[offset + 6] = _colors[offset + 7] = colorRightward;

        _originIndicatorIndices[0] = offset;
        _originIndicatorIndices[1] = offset + 1;
        _originIndicatorIndices[2] = offset + 2;
        _originIndicatorIndices[3] = offset + 2;
        _originIndicatorIndices[4] = offset + 1;
        _originIndicatorIndices[5] = offset + 3;
        _originIndicatorIndices[6] = offset + 4;
        _originIndicatorIndices[7] = offset + 5;
        _originIndicatorIndices[8] = offset + 6;
        _originIndicatorIndices[9] = offset + 6;
        _originIndicatorIndices[10] = offset + 5;
        _originIndicatorIndices[11] = offset + 7;

        _mesh.SetIndices(_originIndicatorIndices, MeshTopology.Triangles, 1);
    }

    private Vector3 worldPositionOfTrackingSpace(Vector3 position) {
        if (Application.isEditor) { return position; }

        return _camera.trackingSpace.trackingSpaceToWorldMatrix.MultiplyPoint(position);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
NOTE: Make sure you add "Particles/Alpha Blended Premultiply" to the list of
Always Included Shaders under Edit -> Project Settings -> Graphics !!
*/

// The intended purpose of MeshVisualizer is to visualize the mesh of a detected
// surface from XRSurfaceController. It depends on having Version 8.0 or higher.
public class MeshVisualizer : MonoBehaviour {

  private MeshRenderer meshRenderer;
  private MeshFilter meshFilter;

  public Color meshColor = new Color(0.541f, 0.0f, 0.796f, 0.5f);
  public float lineWidth = 0.007f;
  public float pointWidth = 0.015f;

  private GameObject visualization;
  private bool hasSurface = false;

  private class LineWithRenderer {
    public readonly GameObject line;
    public readonly LineRenderer renderer;

    public LineWithRenderer(Transform parentTransform, Color meshColor, float lineWidth) {
      line = new GameObject();
      line.name = "Line";
      line.transform.SetParent(parentTransform, false);
      renderer = line.AddComponent<LineRenderer>();
      renderer.useWorldSpace = false;
      renderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
      renderer.startColor = meshColor;
      renderer.endColor = meshColor;
      renderer.startWidth = lineWidth;
      renderer.endWidth = lineWidth;
    }

    public void RenderLine(Vector3 v0, Vector3 v1) {
      renderer.SetPosition(0, v0);
      renderer.SetPosition(1, v1);
      renderer.enabled = true;
    }
  }

  private class PointWithRenderer {
    public readonly GameObject point;
    public readonly MeshRenderer renderer;
    public PointWithRenderer(Transform parentTransform, Color meshColor, float pointWidth) {
      point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      point.name = "Point";
      point.transform.SetParent(parentTransform, false);
      point.transform.localScale = new Vector3(pointWidth, pointWidth, pointWidth);
      renderer = point.GetComponent<MeshRenderer>();
      renderer.material.color = meshColor;
    }
    public void RenderPoint(Vector3 v0) {
      point.transform.localPosition = v0;
      renderer.enabled = true;
    }
  }

  private List<LineWithRenderer> meshLines;
  private List<PointWithRenderer> meshPoints;

  void RenderMesh(Mesh mesh) {

    if (visualization == null) {
      visualization = new GameObject();
      visualization.name = "MeshVisualization";
      visualization.transform.SetParent(transform, false);
    }
    if (meshRenderer != null) {
      //meshRenderer.enabled = false;
    }
    // Add each edge to a set so that we can deduplicate edges and only render each edge once.
    HashSet<KeyValuePair<int, int>> lines = new HashSet<KeyValuePair<int, int>>();
    for (int t = 0; t < mesh.triangles.Length; t += 3) {
      List<int> vs = new List<int>();
      vs.Add(mesh.triangles[t]);
      vs.Add(mesh.triangles[t + 1]);
      vs.Add(mesh.triangles[t + 2]);
      // Sort so the lower vertex index comes first in the pair
      vs.Sort();
      lines.Add(new KeyValuePair<int, int>(vs[0], vs[1]));
      lines.Add(new KeyValuePair<int, int>(vs[0], vs[2]));
      lines.Add(new KeyValuePair<int, int>(vs[1], vs[2]));
    }

    int l = 0;
    foreach (KeyValuePair<int, int> line in lines) {
      if (meshLines.Count < l + 1) {
        meshLines.Add(new LineWithRenderer(visualization.transform, meshColor, lineWidth));
      }
      meshLines[l].RenderLine(
        mesh.vertices[line.Key],
        mesh.vertices[line.Value]);

      l++;
    }

    for (; l < meshLines.Count; ++l) {
      meshLines[l].renderer.enabled = false;
    }

    int v = 0;
    for (; v < mesh.vertices.Length; ++v) {
      if (meshPoints.Count < v + 1) {
        meshPoints.Add(new PointWithRenderer(visualization.transform, meshColor, pointWidth));
      }
      meshPoints[v].RenderPoint(mesh.vertices[v]);
    }

    for (; v < meshPoints.Count; ++v) {
      meshPoints[v].renderer.enabled = false;
    }
  }

  void Start() {
    meshLines = new List<LineWithRenderer>();
    meshPoints = new List<PointWithRenderer>();

    meshFilter = gameObject.GetComponent<MeshFilter>();
    if (meshFilter == null) {
      meshFilter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
    }
    meshRenderer = gameObject.GetComponent<MeshRenderer>();

    GetComponent<XRSurfaceController>().onSurfaceReady.AddListener(delegate {hasSurface = true;});
  }

  void Update () {
    if (!hasSurface || meshFilter.mesh == null || meshFilter.mesh.vertices.Length == 0) {
      return;
    }
    RenderMesh(meshFilter.mesh);
  }
}

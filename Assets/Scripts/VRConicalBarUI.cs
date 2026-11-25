using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRConicalBarUI : MonoBehaviour
{
    [Header("Connections")]
    public CSVBarManager barManager;
    public Transform userStartReference;      // Optional: drag XR Origin or leave empty for Camera.main
    public RectTransform worldSpaceCanvas;    // World Space Canvas (Scale: 0.001, 0.001, 0.001)
    public RectTransform barUIPrefab;         // Simple UI Image prefab (Center anchor preset)

    [Header("Cone Shape Around User (Meters & Degrees)")]
    public float bottomRadius = 1.2f;
    public float heightOffset = -0.45f;
    public float coneSlopeLength = 0.45f;
    [Range(0f, 80f)]
    public float coneTiltAngle = 40f;

    [Header("2D Bar Appearance (Meters & Scale)")]
    public float barWidthMeters = 0.03f;
    public float maxBarHeightMeters = 0.35f;  // Height of a 100% bar
    public float maxPercentage = 100f;        // Set to 100 if CSV uses 0-100, or 1 if CSV uses 0.0-1.0
    public Color defaultBarColor = new Color(0.2f, 0.7f, 1f, 0.9f);

    [Header("Optional Conical Backdrop")]
    public bool generateConeBackdrop = true;
    public Material backdropMaterial;

    private class ConicalBarEntry
    {
        public BarData bar;
        public RectTransform rect;
        public Image image;
        public float lastValue = -1f;
    }

    private readonly List<ConicalBarEntry> barEntries = new List<ConicalBarEntry>();
    private readonly Dictionary<string, Image> uiBarsByName = new Dictionary<string, Image>();
    private float canvasScale = 0.001f;

    void Start()
    {
        Invoke(nameof(BuildConicalUI), 1.5f);
    }

    public void BuildConicalUI()
    {
        if (barManager == null || barManager.allBars == null || barManager.allBars.Count == 0)
        {
            Debug.LogWarning("[CONICAL UI] No bars found in CSVBarManager yet. Retrying...");
            Invoke(nameof(BuildConicalUI), 1.0f);
            return;
        }

        Transform originRef = userStartReference != null ? userStartReference : Camera.main.transform;
        Vector3 coneCenter = new Vector3(
            originRef.position.x,
            originRef.position.y + heightOffset,
            originRef.position.z
        );

        if (generateConeBackdrop)
        {
            CreateConeBackdropMesh(coneCenter);
        }

        canvasScale = worldSpaceCanvas.lossyScale.x;
        if (canvasScale <= 0f) canvasScale = 0.001f;

        foreach (BarData bar in barManager.allBars)
        {
            if (bar == null) continue;

            Vector3 toBar = bar.transform.position - coneCenter;
            toBar.y = 0f;
            if (toBar.sqrMagnitude < 0.0001f) continue;

            Vector3 azimuthDir = toBar.normalized;

            RectTransform barRect = Instantiate(barUIPrefab, worldSpaceCanvas);
            barRect.gameObject.name = "UI_Bar_" + bar.gameObject.name;
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.position = coneCenter + azimuthDir * (bottomRadius - 0.005f);

            Quaternion faceInward = Quaternion.LookRotation(azimuthDir, Vector3.up);
            barRect.rotation = faceInward * Quaternion.Euler(coneTiltAngle, 0f, 0f);

            Image img = barRect.GetComponent<Image>();
            if (img != null)
            {
                img.color = defaultBarColor;
                uiBarsByName[bar.gameObject.name.ToLower()] = img;
            }

            ConicalBarEntry entry = new ConicalBarEntry
            {
                bar = bar,
                rect = barRect,
                image = img
            };

            barEntries.Add(entry);
            ApplyBarHeight(entry, GetBarPercentage(bar));
        }
    }

    void Update()
    {
        // Automatically syncs 2D bars whenever any 3D bar's value changes
        for (int i = 0; i < barEntries.Count; i++)
        {
            ConicalBarEntry entry = barEntries[i];
            if (entry.bar == null || entry.rect == null) continue;

            float currentVal = GetBarPercentage(entry.bar);
            if (!Mathf.Approximately(currentVal, entry.lastValue))
            {
                ApplyBarHeight(entry, currentVal);
            }
        }
    }

    private float GetBarPercentage(BarData bar)
    {
        // REPLACE 'currentValue' with the exact float variable name in your BarData.cs
        return bar.currentValue;
    }

    private void ApplyBarHeight(ConicalBarEntry entry, float rawValue)
    {
        entry.lastValue = rawValue;
        float normalizedHeight = Mathf.Clamp01(rawValue / maxPercentage);
        float heightMeters = normalizedHeight * maxBarHeightMeters;

        entry.rect.sizeDelta = new Vector2(barWidthMeters / canvasScale, heightMeters / canvasScale);
    }

    public void HighlightBar(string barName, Color highlightColor)
    {
        if (string.IsNullOrEmpty(barName)) return;
        if (uiBarsByName.TryGetValue(barName.ToLower(), out Image img))
        {
            img.color = highlightColor;
        }
    }

    public void ResetAllBarColors()
    {
        foreach (var img in uiBarsByName.Values)
        {
            if (img != null) img.color = defaultBarColor;
        }
    }

    private void CreateConeBackdropMesh(Vector3 center)
    {
        GameObject coneObj = new GameObject("Conical_UI_Backdrop");
        coneObj.transform.position = center;

        MeshFilter mf = coneObj.AddComponent<MeshFilter>();
        MeshRenderer mr = coneObj.AddComponent<MeshRenderer>();
        if (backdropMaterial != null) mr.material = backdropMaterial;

        int segments = 72;
        float tiltRad = coneTiltAngle * Mathf.Deg2Rad;
        float topRadius = bottomRadius + Mathf.Sin(tiltRad) * coneSlopeLength;
        float topY = Mathf.Cos(tiltRad) * coneSlopeLength;

        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            vertices[i * 2] = new Vector3(sin * bottomRadius, 0f, cos * bottomRadius);
            vertices[i * 2 + 1] = new Vector3(sin * topRadius, topY, cos * topRadius);

            if (i < segments)
            {
                int v = i * 2;
                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 3;

                triangles[t + 3] = v;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }
}
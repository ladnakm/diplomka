using UnityEngine;

public class BarData : MonoBehaviour
{
    [Header("Bar Attributes")]
    public string categoryName;
    
    [Header("Raw Data")]
    public float dataset1Value;
    public float dataset2Value;

    [Header("Visual Settings")]
    public float minVisualHeight = 20f; 
    public float maxVisualHeight = 150f;

    [Header("Color Scale")]
    public Color minColor = new Color(0.65f, 0.83f, 1.0f);
    public Color maxColor = new Color(0.0f, 0.0f, 0.45f);

    [Header("Connected Buildings")]
    public Renderer[] buildingRenderers;

    public void SetHeightRelative(int datasetIndex, float currentDataMin, float currentDataMax, bool isPercentage)
    {
        float rawValue = 0f;

        if (datasetIndex == 1) rawValue = dataset1Value;
        else if (datasetIndex == 2) rawValue = dataset2Value;

        float normalizedValue = 0f;
        float finalHeight = 0f;

        if (isPercentage)
        {
            // ABSOLUTE PROPORTIONAL SCALING (Perfect for guessing)
            // Assumes CSV values are written from 0 to 100 (e.g. 45.5 for 45.5%)
            normalizedValue = rawValue / 100f; 
            finalHeight = normalizedValue * maxVisualHeight; // 0% literally equals 0 height
        }
        else
        {
            // RELATIVE SCALING (Standard for population/density)
            float range = currentDataMax - currentDataMin;
            normalizedValue = (range == 0) ? 0 : (rawValue - currentDataMin) / range;
            finalHeight = minVisualHeight + (normalizedValue * (maxVisualHeight - minVisualHeight));
        }

        // Apply height
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, finalHeight);

        // Apply color
        Color finalColor = Color.Lerp(minColor, maxColor, normalizedValue);
        GetComponent<MeshRenderer>().material.color = finalColor;

        // Paint buildings
        if (buildingRenderers != null)
        {
            foreach (Renderer bldg in buildingRenderers)
            {
                if (bldg != null)
                {
                    foreach (Material mat in bldg.materials) mat.color = finalColor;
                }
            }
        }
    }
}
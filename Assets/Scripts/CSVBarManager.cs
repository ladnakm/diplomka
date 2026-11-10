using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class CSVBarManager : MonoBehaviour
{
    [Header("CSV Data File")]
    public TextAsset csvFile;

    [Header("Parent Asset")]
    public GameObject grafy;

    [Header("Dataset Rules (Remember: Column 1 is 0)")]
    public int dataset1Column = 9;  // 10th attribute in CSV
    public int dataset2Column = 10; // 11th attribute in CSV
    public bool isDataset1Percentage = false;
    public bool isDataset2Percentage = true; 

    [Header("UI Legend")]
    public RawImage legendColorBar;
    public TextMeshProUGUI minValueText;
    public TextMeshProUGUI maxValueText;
    public Color minColor = new Color(0.65f, 0.3f, 1.0f); 
    public Color maxColor = new Color(0.0f, 0.0f, 0.45f); 

    [Header("Bar References")]
    public List<BarData> allBars = new List<BarData>();

    void Start()
    {
        if (legendColorBar != null)
        {
            Texture2D gradTex = new Texture2D(1, 2);
            gradTex.wrapMode = TextureWrapMode.Clamp;
            gradTex.SetPixel(0, 0, minColor);
            gradTex.SetPixel(0, 1, maxColor);
            gradTex.Apply();
            legendColorBar.texture = gradTex;
        }

        GameObject liveGrafy = GameObject.Find("grafy");
        GameObject liveBuildings = GameObject.Find("City_Buildings"); 
        
        if (liveGrafy != null)
        {
            foreach (Transform child in liveGrafy.transform)
            {
                BarData bd = child.GetComponent<BarData>();
                if (bd != null)
                {
                    allBars.Add(bd);
                    string searchName = child.name.Trim();
                    if (liveBuildings != null)
                    {
                        foreach (Transform bChild in liveBuildings.transform)
                        {
                            if (bChild.name.Trim().Equals(searchName, StringComparison.OrdinalIgnoreCase))
                            {
                                bd.buildingRenderers = bChild.GetComponentsInChildren<Renderer>();
                                break;
                            }
                        }
                    }
                }
            }
        }
        LoadDataset(1);
    }

    public void LoadDataset(int datasetIndex)
    {
        if (csvFile == null) return;
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        float minVal = float.MaxValue;
        float maxVal = float.MinValue;

        bool isPercentage = (datasetIndex == 1) ? isDataset1Percentage : isDataset2Percentage;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(';');
            
            // Safety check: ensure the line is long enough to contain your target columns
            int highestColumnNeeded = Mathf.Max(dataset1Column, dataset2Column);
            if (values.Length <= highestColumnNeeded) continue; 

            string searchName = values[0].Trim().Replace("\uFEFF", "");
            
            // Read the exact columns you specified in the Inspector
            if (!float.TryParse(values[dataset1Column].Trim().Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float val1)) continue;
            if (!float.TryParse(values[dataset2Column].Trim().Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float val2)) continue;

            BarData bar = allBars.Find(b => b.gameObject.name.Trim().Equals(searchName, StringComparison.OrdinalIgnoreCase));

            if (bar != null)
            {
                bar.dataset1Value = val1;
                bar.dataset2Value = val2;

                float activeVal = (datasetIndex == 1) ? val1 : val2;
                if (activeVal < minVal) minVal = activeVal;
                if (activeVal > maxVal) maxVal = activeVal;
            }
        }

        if (isPercentage)
        {
            minVal = 0f;
            maxVal = 100f;
        }

        if (minValueText != null) minValueText.text = isPercentage ? "0 %" : minVal.ToString("N0"); 
        if (maxValueText != null) maxValueText.text = isPercentage ? "100 %" : maxVal.ToString("N0");

        foreach (BarData bar in allBars)
        {
            bar.SetHeightRelative(datasetIndex, minVal, maxVal, isPercentage);
        }
    }
}
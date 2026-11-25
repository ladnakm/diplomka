using UnityEngine;

public class VRStudySectionManager : MonoBehaviour
{
    [Header("System Connections")]
    public CSVBarManager barManager;
    public VRBarScaleMode scaleModeScript;
    public VRValueGuessTask guessTaskScript;

    [Header("Section 1 Building Color")]
    public Color neutralGreyColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Status (Read-Only)")]
    public int currentSection = 1; // 1, 2, or 3

    void Start()
    {
        // Wait 2 seconds for CSVBarManager to finish loading bars
        Invoke(nameof(StartFirstSection), 2.0f);
    }

    private void StartFirstSection()
    {
        StartSection(1);
    }

    public void StartSection(int sectionNumber)
    {
        currentSection = sectionNumber;
        Debug.Log($"========== STARTING SECTION {currentSection} ==========");

        // 1. Configure Building Colors (Grey in Section 1, Data-colored in Sections 2 & 3)
        bool useDataColors = (currentSection >= 2);
        if (barManager != null && barManager.allBars != null)
        {
            foreach (BarData bar in barManager.allBars)
            {
                if (bar != null)
                {
                    bar.SetBuildingColorMode(useDataColors, neutralGreyColor);
                }
            }
        }

        // 2. Configure Secondary Button Scaling (Enabled ONLY in Section 3)
        if (scaleModeScript != null)
        {
            scaleModeScript.allowScaling = (currentSection == 3);
        }

        // 3. Start Task 1 (Value Guessing), which chains into Task 2 -> Task 3
        if (guessTaskScript != null)
        {
            guessTaskScript.StartGuessTasks();
        }
    }

    // Called automatically by VRDistanceTask when Task 3 finishes
    public void OnSectionComplete()
    {
        Debug.Log($"========== COMPLETED SECTION {currentSection} ==========");

        if (currentSection < 3)
        {
            StartSection(currentSection + 1);
        }
        else
        {
            Debug.Log("========== ALL 3 SECTIONS COMPLETE! STUDY FINISHED ==========");
            if (scaleModeScript != null) scaleModeScript.allowScaling = false;
        }
    }
}
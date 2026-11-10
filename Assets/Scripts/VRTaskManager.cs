using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[System.Serializable]
public class ResearchTask
{
    public string barNameA; 
    public string barNameB; 
}

public class VRTaskManager : MonoBehaviour
{
    [Header("Connections")]
    public CSVBarManager manager;
    public GameObject arrowPrefab;
    public GameObject compareTaskUI; 
    public VRDistanceTask distanceTaskScript;

    [Header("VR Input - Confirm (X / A)")]
    public InputActionReference leftConfirmAction; 
    public InputActionReference rightConfirmAction; 

    [Header("Colors")]
    public string hexColorA = "#f1a340"; 
    public string hexColorB = "#998ec3"; 

    [Header("Task List")]
    public float arrowHeightOffset = 160f;
    public List<ResearchTask> tasks = new List<ResearchTask>();
    
    private int currentTaskIndex = 0;
    private GameObject arrowA;
    private GameObject arrowB;
    private string pendingSelection = ""; // Remembers the choice before confirming

    void OnEnable()
    {
        if (leftConfirmAction != null && leftConfirmAction.action != null) leftConfirmAction.action.Enable();
        if (rightConfirmAction != null && rightConfirmAction.action != null) rightConfirmAction.action.Enable();
    }

    public void StartComparisonTasks()
    {
        arrowA = Instantiate(arrowPrefab);
        arrowB = Instantiate(arrowPrefab);

        if (ColorUtility.TryParseHtmlString(hexColorA, out Color colorA)) arrowA.GetComponentInChildren<Renderer>().material.color = colorA;
        if (ColorUtility.TryParseHtmlString(hexColorB, out Color colorB)) arrowB.GetComponentInChildren<Renderer>().material.color = colorB;

        currentTaskIndex = 0;
        ShowCurrentTask();
    }

    public void ShowCurrentTask()
    {
        pendingSelection = ""; // Reset selection for the new task

        if (currentTaskIndex >= tasks.Count)
        {
            if (arrowA != null) arrowA.SetActive(false);
            if (arrowB != null) arrowB.SetActive(false);
            if (compareTaskUI != null) compareTaskUI.SetActive(false);
            if (distanceTaskScript != null) distanceTaskScript.StartDistanceTasks();
            return;
        }

        if (compareTaskUI != null) compareTaskUI.SetActive(true);

        ResearchTask currentTask = tasks[currentTaskIndex];
        BarData barA = manager.allBars.Find(b => b.gameObject.name.Equals(currentTask.barNameA, System.StringComparison.OrdinalIgnoreCase));
        BarData barB = manager.allBars.Find(b => b.gameObject.name.Equals(currentTask.barNameB, System.StringComparison.OrdinalIgnoreCase));

        if (barA != null && barB != null)
        {
            arrowA.transform.position = barA.transform.position + (Vector3.up * arrowHeightOffset);
            arrowB.transform.position = barB.transform.position + (Vector3.up * arrowHeightOffset);
        }
    }

    void Update()
    {
        if (compareTaskUI != null && compareTaskUI.activeInHierarchy)
        {
            // 1. Arrow Keys to Select
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame) SelectOrange();
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) SelectPurple();
            }

            // 2. Listen for Confirmation (Enter or A/X)
            bool confirmPressed = false;
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) confirmPressed = true;
            if (leftConfirmAction != null && leftConfirmAction.action != null && leftConfirmAction.action.WasPressedThisFrame()) confirmPressed = true;
            if (rightConfirmAction != null && rightConfirmAction.action != null && rightConfirmAction.action.WasPressedThisFrame()) confirmPressed = true;

            // 3. If confirm is pressed AND a button was selected, proceed
            if (confirmPressed && !string.IsNullOrEmpty(pendingSelection))
            {
                LogAndNext(pendingSelection);
            }
        }
    }

    public void SelectOrange()
    {
        pendingSelection = "Orange";
        Debug.Log("[RESEARCH] Orange selected. Press A/X or Enter to confirm.");
    }

    public void SelectPurple()
    {
        pendingSelection = "Purple";
        Debug.Log("[RESEARCH] Purple selected. Press A/X or Enter to confirm.");
    }

    private void LogAndNext(string selectedColor)
    {
        if (currentTaskIndex >= tasks.Count) return;
        Debug.Log($"[RESULT - VALUE] Pair: {tasks[currentTaskIndex].barNameA} vs {tasks[currentTaskIndex].barNameB} | User chose: {selectedColor}");
        currentTaskIndex++;
        ShowCurrentTask();
    }
}
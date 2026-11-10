using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class VRDistanceTask : MonoBehaviour
{
    [Header("Connections")]
    public CSVBarManager manager;
    public GameObject arrowPrefab;
    public GameObject compareTaskUI; 

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
    private BarData currentBarA;
    private BarData currentBarB;
    private string pendingSelection = ""; 

    void OnEnable()
    {
        if (leftConfirmAction != null && leftConfirmAction.action != null) leftConfirmAction.action.Enable();
        if (rightConfirmAction != null && rightConfirmAction.action != null) rightConfirmAction.action.Enable();
    }

    public void StartDistanceTasks()
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
        pendingSelection = "";

        if (currentTaskIndex >= tasks.Count)
        {
            Debug.Log("[RESEARCH] Distance tasks complete!");
            if (arrowA != null) arrowA.SetActive(false);
            if (arrowB != null) arrowB.SetActive(false);
            if (compareTaskUI != null) compareTaskUI.SetActive(false);
            return;
        }

        if (compareTaskUI != null) compareTaskUI.SetActive(true);

        ResearchTask currentTask = tasks[currentTaskIndex];
        currentBarA = manager.allBars.Find(b => b.gameObject.name.Equals(currentTask.barNameA, System.StringComparison.OrdinalIgnoreCase));
        currentBarB = manager.allBars.Find(b => b.gameObject.name.Equals(currentTask.barNameB, System.StringComparison.OrdinalIgnoreCase));

        if (currentBarA != null && currentBarB != null)
        {
            arrowA.transform.position = currentBarA.transform.position + (Vector3.up * arrowHeightOffset);
            arrowB.transform.position = currentBarB.transform.position + (Vector3.up * arrowHeightOffset);
        }
    }

    void Update()
    {
        if (compareTaskUI != null && compareTaskUI.activeInHierarchy)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame) SelectOrange();
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) SelectPurple();
            }

            bool confirmPressed = false;
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) confirmPressed = true;
            if (leftConfirmAction != null && leftConfirmAction.action != null && leftConfirmAction.action.WasPressedThisFrame()) confirmPressed = true;
            if (rightConfirmAction != null && rightConfirmAction.action != null && rightConfirmAction.action.WasPressedThisFrame()) confirmPressed = true;

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
        
        Vector3 userPos = Camera.main.transform.position;
        Vector3 posA = currentBarA.transform.position;
        Vector3 posB = currentBarB.transform.position;

        userPos.y = 0; posA.y = 0; posB.y = 0;

        float distA = Vector3.Distance(userPos, posA);
        float distB = Vector3.Distance(userPos, posB);
        
        string closerColor = (distA < distB) ? "Orange" : "Purple";
        string grade = (selectedColor == closerColor) ? "CORRECT" : "INCORRECT";

        Debug.Log($"[RESULT - DISTANCE] Pair: {currentBarA.gameObject.name} vs {currentBarB.gameObject.name} | User chose: {selectedColor} | Grade: {grade} | Actual Distances -> Orange: {distA:F1}m, Purple: {distB:F1}m");
        
        currentTaskIndex++;
        ShowCurrentTask();
    }
}
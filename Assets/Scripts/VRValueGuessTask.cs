using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class VRValueGuessTask : MonoBehaviour
{
    [Header("System Connections")]
    public CSVBarManager manager;
    public GameObject arrowPrefab;
    public VRTaskManager comparisonTaskScript;
    
    [Header("UI Elements")]
    public TextMeshProUGUI guessValueText;
    public GameObject guessTaskUI; 

    [Header("VR Input - Movement")]
    public InputActionReference leftJoystickInput; 
    public InputActionReference rightJoystickInput; 
    public float changeSpeed = 30f; 

    [Header("VR Input - Confirm (X / A)")]
    public InputActionReference leftConfirmAction; 
    public InputActionReference rightConfirmAction; 

    [Header("Task List")]
    public float arrowHeightOffset = 160f;
    public List<string> districtsToGuess = new List<string>();
    
    private int currentTaskIndex = 0;
    private float currentGuess = 50f; 
    private GameObject activeArrow;

    void OnEnable()
    {
        if (leftJoystickInput != null && leftJoystickInput.action != null) leftJoystickInput.action.Enable();
        if (rightJoystickInput != null && rightJoystickInput.action != null) rightJoystickInput.action.Enable();
        if (leftConfirmAction != null && leftConfirmAction.action != null) leftConfirmAction.action.Enable();
        if (rightConfirmAction != null && rightConfirmAction.action != null) rightConfirmAction.action.Enable();
    }

    void Start()
    {
        activeArrow = Instantiate(arrowPrefab);
        if (ColorUtility.TryParseHtmlString("#e6c229", out Color guessColor))
            activeArrow.GetComponentInChildren<Renderer>().material.color = guessColor;

        Invoke("ShowCurrentTask", 2.0f);
    }

    void Update()
    {
        if (currentTaskIndex >= districtsToGuess.Count) return;

        Vector2 stickValue = Vector2.zero;
        if (leftJoystickInput != null && leftJoystickInput.action != null) stickValue += leftJoystickInput.action.ReadValue<Vector2>();
        if (rightJoystickInput != null && rightJoystickInput.action != null) stickValue += rightJoystickInput.action.ReadValue<Vector2>();

        if (Mathf.Abs(stickValue.y) > 0.1f)
        {
            currentGuess += stickValue.y * changeSpeed * Time.deltaTime;
            currentGuess = Mathf.Clamp(currentGuess, 0f, 100f); 
            UpdateUI();
        }

        // Check for Confirmation (Keyboard Enter OR VR X/A Buttons)
        bool confirmPressed = false;
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) confirmPressed = true;
        if (leftConfirmAction != null && leftConfirmAction.action != null && leftConfirmAction.action.WasPressedThisFrame()) confirmPressed = true;
        if (rightConfirmAction != null && rightConfirmAction.action != null && rightConfirmAction.action.WasPressedThisFrame()) confirmPressed = true;

        if (confirmPressed) ConfirmGuess();
    }

    public void ShowCurrentTask()
    {
        if (currentTaskIndex >= districtsToGuess.Count)
        {
            activeArrow.SetActive(false);
            if (guessTaskUI != null) guessTaskUI.SetActive(false);
            if (comparisonTaskScript != null) comparisonTaskScript.StartComparisonTasks();
            return;
        }

        currentGuess = 50f; 
        UpdateUI();
        if (guessTaskUI != null) guessTaskUI.SetActive(true);

        string targetName = districtsToGuess[currentTaskIndex];
        BarData targetBar = manager.allBars.Find(b => b.gameObject.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase));

        if (targetBar != null)
        {
            activeArrow.SetActive(true);
            activeArrow.transform.position = targetBar.transform.position + (Vector3.up * arrowHeightOffset);
        }
    }

    private void UpdateUI()
    {
        if (guessValueText != null) guessValueText.text = Mathf.RoundToInt(currentGuess).ToString() + " %";
    }

    // Kept public so your Canvas button can still click it!
    public void ConfirmGuess()
    {
        if (currentTaskIndex >= districtsToGuess.Count) return;
        Debug.Log($"[RESULT] Bar: {districtsToGuess[currentTaskIndex]} | User Guessed: {Mathf.RoundToInt(currentGuess)} %");
        currentTaskIndex++;
        ShowCurrentTask();
    }
}
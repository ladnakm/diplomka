using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRBarScaleMode : MonoBehaviour
{
    public bool allowScaling = false;

    // Other scripts check this to pause their own joystick controls while scaling
    public static bool isScalingModeActive = false;
    public static float currentScaleMultiplier = 1.0f; 

    [Header("Connections")]
    public CSVBarManager barManager;

    [Header("VR Input - Toggle Scale Mode (Y / B)")]
    public InputActionReference leftSecondaryButton;  // Y button
    public InputActionReference rightSecondaryButton; // B button

    [Header("VR Input - Scale Height (Joysticks)")]
    public InputActionReference leftJoystickInput;
    public InputActionReference rightJoystickInput;

    [Header("Scaling Settings")]
    public float scaleSpeed = 1.0f;
    public float minMultiplier = 0.25f; // 25% of original height
    public float maxMultiplier = 3.0f;  // 300% of original height

    [Header("Green Halo Effect")]
    public Color haloColor = new Color(0f, 1f, 0.2f, 1f);
    public float haloIntensity = 1.5f;

    [Header("Max Value Indicator Bars")]
    public Transform maxIndicatorParent; // Drag the parent object holding all semitransparent bars
    public bool scaleIndicatorsOnZ = true; // Uncheck if your indicator bars scale on Y instead of Z

    private Dictionary<Transform, float> baseIndicatorHeights = new Dictionary<Transform, float>();

    private float currentMultiplier = 1.0f;
    private Dictionary<BarData, float> baseHeightsZ = new Dictionary<BarData, float>();

    void OnEnable()
    {
        if (leftSecondaryButton != null && leftSecondaryButton.action != null) leftSecondaryButton.action.Enable();
        if (rightSecondaryButton != null && rightSecondaryButton.action != null) rightSecondaryButton.action.Enable();
        if (leftJoystickInput != null && leftJoystickInput.action != null) leftJoystickInput.action.Enable();
        if (rightJoystickInput != null && rightJoystickInput.action != null) rightJoystickInput.action.Enable();
    }

    void Update()
    {
        if (!allowScaling) return;
        // 1. Check for Toggle (VR B/Y buttons OR Keyboard 'B' key)
        bool togglePressed = false;
        if (leftSecondaryButton != null && leftSecondaryButton.action != null && leftSecondaryButton.action.WasPressedThisFrame()) togglePressed = true;
        if (rightSecondaryButton != null && rightSecondaryButton.action != null && rightSecondaryButton.action.WasPressedThisFrame()) togglePressed = true;
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) togglePressed = true;

        if (togglePressed)
        {
            ToggleScaleMode();
        }

        // 2. If Scale Mode is active, read joystick (or Up/Down arrows) to resize bars
        if (!isScalingModeActive || barManager == null) return;

        float verticalInput = 0f;
        if (leftJoystickInput != null && leftJoystickInput.action != null)
            verticalInput += leftJoystickInput.action.ReadValue<Vector2>().y;
        if (rightJoystickInput != null && rightJoystickInput.action != null)
            verticalInput += rightJoystickInput.action.ReadValue<Vector2>().y;

        // PC Testing Fallback
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed) verticalInput += 1f;
            if (Keyboard.current.downArrowKey.isPressed) verticalInput -= 1f;
        }

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            currentMultiplier += verticalInput * scaleSpeed * Time.deltaTime;
            currentMultiplier = Mathf.Clamp(currentMultiplier, minMultiplier, maxMultiplier);
            currentScaleMultiplier = currentMultiplier;
            ApplyScaleToAllBars();
        }
    }

    private void ToggleScaleMode()
    {
        if (barManager == null || barManager.allBars == null) return;

        isScalingModeActive = !isScalingModeActive;

        if (isScalingModeActive)
        {
            // Snapshot original heights of data bars
            foreach (BarData bar in barManager.allBars)
            {
                if (bar != null && !baseHeightsZ.ContainsKey(bar))
                {
                    baseHeightsZ[bar] = bar.transform.localScale.z;
                }
            }

            // Snapshot original heights of semitransparent indicator bars
            if (maxIndicatorParent != null && baseIndicatorHeights.Count == 0)
            {
                foreach (Transform indicator in maxIndicatorParent)
                {
                    baseIndicatorHeights[indicator] = scaleIndicatorsOnZ
                        ? indicator.localScale.z
                        : indicator.localScale.y;
                }
            }

            SetGreenHalo(true);
            Debug.Log("[SCALE MODE] ON - Push joystick up/down to scale bars.");
        }
        else
        {
            SetGreenHalo(false);
            Debug.Log($"[SCALE MODE] OFF - Saved bar height multiplier: {currentMultiplier:F2}x");
        }
    }

    private void ApplyScaleToAllBars()
    {
        // Scale main data bars
        foreach (BarData bar in barManager.allBars)
        {
            if (bar == null || !baseHeightsZ.ContainsKey(bar)) continue;

            Vector3 scale = bar.transform.localScale;
            scale.z = baseHeightsZ[bar] * currentMultiplier;
            bar.transform.localScale = scale;
        }

        // Scale semitransparent max-value indicator bars
        foreach (var pair in baseIndicatorHeights)
        {
            Transform indicator = pair.Key;
            if (indicator == null) continue;

            Vector3 scale = indicator.localScale;
            if (scaleIndicatorsOnZ)
                scale.z = pair.Value * currentMultiplier;
            else
                scale.y = pair.Value * currentMultiplier;

            indicator.localScale = scale;
        }
    }

    private void SetGreenHalo(bool enable)
    {
        foreach (BarData bar in barManager.allBars)
        {
            if (bar == null) continue;

            MeshRenderer rend = bar.GetComponent<MeshRenderer>();
            if (rend == null) continue;

            Material mat = rend.material;
            if (enable)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", haloColor * haloIntensity);
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
            }
        }
    }
}
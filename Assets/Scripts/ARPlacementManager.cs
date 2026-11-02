using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] GameObject labPrefab;
    [SerializeField] GameObject scanHintUI;   // "Move your phone slowly..."
    
    [SerializeField] LessonManager lessonManager;

    GameObject spawnedLab;
    static readonly List<ARRaycastHit> hits = new();

    void Update()
    {
        if (spawnedLab != null) return;

        Vector2 screenPos = Vector2.zero;
        bool pressed = false;

        // 1. Check for real mobile touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue())) return;
            
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            pressed = true;
        }
        // 2. Fallback to Mouse click (for Unity Editor / XR Simulation testing)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            
            screenPos = Mouse.current.position.ReadValue();
            pressed = true;
        }

        if (!pressed) return;

        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)) return;

        Pose pose = hits[0].pose;

        // Face the lab toward the user (flat, ignoring camera tilt)
        Vector3 toCam = Camera.main.transform.position - pose.position;
        toCam.y = 0;
        Quaternion rot = Quaternion.LookRotation(toCam.sqrMagnitude > 0.001f ? toCam : Vector3.forward);

        spawnedLab = Instantiate(labPrefab, pose.position, rot);

        // Stop showing planes once placed
        foreach (var plane in planeManager.trackables) plane.gameObject.SetActive(false);
        planeManager.enabled = false;

        scanHintUI.SetActive(false);
        lessonManager.Begin(spawnedLab);
    }
}
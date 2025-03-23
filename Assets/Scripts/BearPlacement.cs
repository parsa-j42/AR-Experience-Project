using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using Unity.XR.CoreUtils;  // Add this for XROrigin

public class BearPlacement : MonoBehaviour
{
    [SerializeField] private GameObject bearPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    
    private GameObject spawnedBear;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    
    private bool isPlacingMode = true;
    
    void Start()
    {
        // Make sure we have a raycast manager
        if (raycastManager == null)
        {
            var xrOrigin = FindFirstObjectByType<XROrigin>();  // Updated from FindObjectOfType
            if (xrOrigin != null)
            {
                raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
            }
        }
        
        // Add a text instruction at the top of the screen
        CreateInstructionText("Tap on the floor to place the bear");
    }
    
    void Update()
    {
        if (raycastManager == null)
        {
            UpdateInstructionText("ERROR: Raycast Manager not found!");
            return;
        }
        
        // Check if we have any planes detected
        var planeManager = FindFirstObjectByType<ARPlaneManager>();
        string debugInfo = "";
        
        if (planeManager != null)
        {
            debugInfo = $"Planes detected: {planeManager.trackables.count}";
            
            if (planeManager.trackables.count == 0)
            {
                UpdateInstructionText("Move your device around to detect the floor. " + debugInfo);
                return;
            }
        }
        
        if (!isPlacingMode)
            return;
            
        // Show basic instructions if we have planes
        UpdateInstructionText("Tap on the floor to place the bear. " + debugInfo);
        
        // Handle user input for placement
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Raycast from the touch position
            Vector2 touchPosition = Input.GetTouch(0).position;
            
            UpdateInstructionText("Touch detected at: " + touchPosition + ". " + debugInfo);
            
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                // Get the hit position and place the bear
                Pose hitPose = hits[0].pose;
                
                UpdateInstructionText("Hit detected! Placing bear at: " + hitPose.position);
                
                // Create or move the bear
                if (spawnedBear == null)
                {
                    // Instantiate the bear
                    spawnedBear = Instantiate(bearPrefab, hitPose.position, Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0));
                    
                    // Update instructions
                    UpdateInstructionText("Bear placed! You can move around to see it from different angles.");
                    
                    // Done placing
                    isPlacingMode = false;
                }
            }
            else
            {
                UpdateInstructionText("Touch detected but no plane hit. Try tapping on the detected floor plane. " + debugInfo);
            }
        }
    }
    
    // Create a simple UI text element for instructions
    private void CreateInstructionText(string message)
    {
        GameObject canvasObj = new GameObject("InstructionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject textObj = new GameObject("InstructionText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        // Add background
        Image background = textObj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.5f);
        
        // Set position and size
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 60);
        rect.anchoredPosition = Vector2.zero;
        
        // Add a button for manual placement
        GameObject buttonObj = new GameObject("PlaceButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.green;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "PLACE BEAR HERE";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0);
        buttonRect.anchorMax = new Vector2(0.5f, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = new Vector2(0, 100);
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        
        button.onClick.AddListener(PlaceBearAtCamera);
    }
    
    // Place the bear in front of the camera on the ground
    private void PlaceBearAtCamera()
    {
        if (spawnedBear != null)
            return;
            
        Camera camera = Camera.main;
        if (camera == null)
            return;
            
        // Create a ray going down from the camera
        Ray ray = new Ray(camera.transform.position, Vector3.down);
        
        if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            spawnedBear = Instantiate(bearPrefab, hitPose.position, 
                Quaternion.Euler(0, camera.transform.eulerAngles.y, 0));
            
            UpdateInstructionText("Bear placed using manual placement button!");
            isPlacingMode = false;
        }
        else
        {
            // If no ground detected, place it 1.5 meters in front of camera at -1.5m height
            Vector3 placementPos = camera.transform.position + camera.transform.forward * 1.5f;
            placementPos.y = camera.transform.position.y - 1.5f;
            
            spawnedBear = Instantiate(bearPrefab, placementPos, 
                Quaternion.Euler(0, camera.transform.eulerAngles.y, 0));
            
            UpdateInstructionText("Bear placed at estimated position (no ground detected)");
            isPlacingMode = false;
        }
    }
    
    // Update the instruction text
    private void UpdateInstructionText(string message)
    {
        var textObj = GameObject.Find("InstructionText");
        if (textObj != null)
        {
            var text = textObj.GetComponent<Text>();
            if (text != null)
            {
                text.text = message;
            }
        }
    }
}
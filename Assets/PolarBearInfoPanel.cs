using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PolarBearInfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject bearModel;
    [SerializeField] private Vector3 panelOffset = new Vector3(0, 0.5f, 0); // Offset from bear's head
    [SerializeField] private float panelScale = 0.01f; // Controls the size of the info panel
    
    private Camera mainCamera;
    private Canvas infoCanvas;
    private Text infoText;
    private int currentFactIndex = 0;
    
    // List of polar bear facts
    private string[] polarBearFacts = new string[]
    {
        "Polar bears are the largest land carnivores currently in existence, rivaled only by the Kodiak brown bears.",
        
        "Polar bears have black skin under their transparent fur, which appears white or yellowish. The transparent fur helps trap heat from the sun.",
        
        "A polar bear can swim constantly for days at a time, reaching speeds up to 6 mph. Their Latin name means 'maritime bear'.",
        
        "Polar bears primarily eat seals. They often wait at seal breathing holes in the ice, sometimes waiting as long as 14 hours for a seal to appear.",
        
        "Polar bears have an extraordinary sense of smell and can detect seals through 3 feet of ice and from up to a mile away.",
        
        "Female polar bears typically give birth to twins in winter dens dug into snowbanks. The cubs stay with their mother for 2-3 years learning to hunt.",
        
        "Polar bears are classified as marine mammals because they spend most of their lives on the sea ice of the Arctic Ocean.",
        
        "Scientists estimate there are about 22,000-31,000 polar bears left in the world, and they are listed as a vulnerable species.",
        
        "Polar bears can reach speeds of up to 25 mph (40 km/h) on land and 6 mph (10 km/h) in water.",
        
        "A polar bear's liver contains toxic levels of vitamin A. It's so toxic that humans and other animals can die if they eat it."
    };
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (bearModel == null)
        {
            // Try to find the bear in the scene
            bearModel = GameObject.FindWithTag("Player"); // You may need to tag your bear model
            
            // If still not found, try to find by name or another method
            if (bearModel == null)
            {
                bearModel = GameObject.Find("Bear1"); // Or whatever your bear model is named
            }
            
            // Last resort - use this gameObject
            if (bearModel == null)
            {
                bearModel = this.gameObject;
                Debug.LogWarning("Bear model not assigned. Using this GameObject instead.");
            }
        }
        
        CreateInfoPanel();
    }
    
    void Update()
    {
        if (bearModel != null && infoCanvas != null)
        {
            // Position the info panel near the bear's head
            UpdatePanelPosition();
            
            // Always face the panel toward the camera
            infoCanvas.transform.rotation = Quaternion.LookRotation(
                infoCanvas.transform.position - mainCamera.transform.position);
        }
    }
    
    private void CreateInfoPanel()
    {
        // Create a new GameObject for the Canvas
        GameObject canvasObj = new GameObject("InfoCanvas");
        infoCanvas = canvasObj.AddComponent<Canvas>();
        infoCanvas.renderMode = RenderMode.WorldSpace;
        
        // Add necessary components
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create panel background
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(infoCanvas.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Set panel size and position
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(300, 200);
        panelRect.localPosition = Vector3.zero;
        
        // Add text to the panel
        GameObject textObj = new GameObject("FactText");
        textObj.transform.SetParent(panelObj.transform, false);
        infoText = textObj.AddComponent<Text>();
        infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        infoText.fontSize = 18;
        infoText.color = Color.white;
        infoText.alignment = TextAnchor.MiddleCenter;
        infoText.text = polarBearFacts[currentFactIndex];
        
        // Set text position and size
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -40);
        
        // Add a title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "Polar Bear Facts";
        
        // Set title position
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(10, -40);
        titleRect.offsetMax = new Vector2(-10, -5);
        
        // Create next button
        GameObject nextButtonObj = new GameObject("NextButton");
        nextButtonObj.transform.SetParent(panelObj.transform, false);
        Image nextButtonImage = nextButtonObj.AddComponent<Image>();
        nextButtonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        Button nextButton = nextButtonObj.AddComponent<Button>();
        nextButton.targetGraphic = nextButtonImage;
        nextButton.onClick.AddListener(ShowNextFact);
        
        // Add text to button
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(nextButtonObj.transform, false);
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.text = "Next Fact";
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Set button position and size
        RectTransform nextButtonRect = nextButtonObj.GetComponent<RectTransform>();
        nextButtonRect.anchorMin = new Vector2(1, 0);
        nextButtonRect.anchorMax = new Vector2(1, 0);
        nextButtonRect.pivot = new Vector2(1, 0);
        nextButtonRect.sizeDelta = new Vector2(100, 40);
        nextButtonRect.anchoredPosition = new Vector2(-10, 10);
        
        // Make the button text fill the button
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        
        // Make the canvas face the camera initially
        UpdatePanelPosition();
        
        // Add a BoxCollider to the panel for touch interaction
        BoxCollider panelCollider = panelObj.AddComponent<BoxCollider>();
        panelCollider.size = new Vector3(300, 200, 1);
        panelCollider.isTrigger = true;
        
        // Add a script to handle touches directly on the panel
        panelObj.AddComponent<PanelTouchHandler>().SetInfoPanel(this);
        
        // Set initial scale for the world space canvas using the configurable panel scale
        infoCanvas.transform.localScale = new Vector3(panelScale, panelScale, panelScale);
    }
    
    private void UpdatePanelPosition()
    {
        if (bearModel == null || infoCanvas == null) return;
        
        // Get the approximate head position (top of the model + offset)
        Renderer renderer = bearModel.GetComponentInChildren<Renderer>();
        Vector3 headPosition;
        
        if (renderer != null)
        {
            // Use the bounds of the renderer to find the top
            Bounds bounds = renderer.bounds;
            headPosition = new Vector3(
                bounds.center.x, 
                bounds.max.y,  // Top of the model
                bounds.center.z
            );
        }
        else
        {
            // Fallback - just use the object's position
            headPosition = bearModel.transform.position;
            headPosition.y += 1.0f; // Estimate head is 1 unit above origin
        }
        
        // Apply the user-defined offset
        headPosition += panelOffset;
        
        // Position the info panel at this position
        infoCanvas.transform.position = headPosition;
    }
    
    public void ShowNextFact()
    {
        currentFactIndex = (currentFactIndex + 1) % polarBearFacts.Length;
        infoText.text = polarBearFacts[currentFactIndex];
    }
}

// Helper class to handle touch input directly on the panel
public class PanelTouchHandler : MonoBehaviour
{
    private PolarBearInfoPanel infoPanel;
    
    public void SetInfoPanel(PolarBearInfoPanel panel)
    {
        infoPanel = panel;
    }
    
    void Update()
    {
        if (infoPanel == null) return;
        
        // Check for touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Cast a ray from the touch position
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.transform.gameObject == this.gameObject)
            {
                infoPanel.ShowNextFact();
            }
        }
        
        // For testing in the editor with mouse
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.transform.gameObject == this.gameObject)
            {
                infoPanel.ShowNextFact();
            }
        }
        #endif
    }
}
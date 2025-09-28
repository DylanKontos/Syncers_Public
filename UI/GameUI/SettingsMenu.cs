using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour // Will automatically enable mobile controls canvas if on IOS or Android (mobile)
{
    Canvas canvas;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button lowGraphicsButton;

    [SerializeField]
    private Button debuggerButton;

    [SerializeField]
    private GameObject gameObjectSensitivitySlider;

    [SerializeField]
    private Slider sensitivitySlider;

    [SerializeField]
    private Button mobileControlsButton;

    [SerializeField]
    private GameObject nameSetterCanvas;

    [SerializeField]
    private GameObject mobileCanvas;

    public Material asteroidSkybox;
    public Material monumentSkybox;

    private bool lowGraphicsEnabled = false;
    private Material originalSkybox;
    GameManager gameManager;
    GameObject environment;
    GameObject environmentNormalGraphics;
    GameObject environmentLowGraphics;
    ParticleSystem[] particleSystems;
    private GameObject ingameDebugConsole;
    private GameObject graphy;

    private float sensitivity = 50f;
    private float mouseX;
    private float mouseY;

    PlayerObjectCameraController playerObjectCameraController;


    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvas.enabled = true;
        Player player = Player.Instance;
        particleSystems = FindObjectsOfType<ParticleSystem>();
        gameManager = FindObjectOfType<GameManager>();

        environment = GameObject.Find("Environment");
        environmentNormalGraphics = environment.transform.GetChild(0).gameObject;
        environmentLowGraphics = environment.transform.GetChild(1).gameObject;

        originalSkybox = RenderSettings.skybox;


        // SENSITIVITY // TODO: X + Y affect Camera (cinnemachine)
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 50);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Buttons
        mobileControlsButton.onClick.AddListener(() =>
        {
            ToggleMobileControlsCanvas();
        });

        lowGraphicsButton.onClick.AddListener(() =>
        {
            ToggleLowGraphics();
        });

        settingsButton.onClick.AddListener(() =>
        {
            SettingsMenuEnable();
        });

        debuggerButton.onClick.AddListener(() =>
        {
            ToggleDebugger();
        });

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////// Enable mobile controls by default if running on a mobile device //////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #if UNITY_IOS || UNITY_ANDROID
        ToggleMobileControlsCanvas();
        #endif
    }

    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
    }

    private void OnSensitivityChanged(float value)
    {
        
        playerObjectCameraController = FindAnyObjectByType<PlayerObjectCameraController>();
        sensitivity = value;

        playerObjectCameraController.ChangeSensitivity(sensitivity);
        // Debug.Log(value);
    }

    private void ToggleMobileControlsCanvas()
    {
        if (mobileCanvas != null)
        {
            Debug.Log("ToggleMobileControlsCanvas called.", this);
            Debug.Log("ToggleMobileControlsCanvas called. Stack Trace: " + System.Environment.StackTrace);
            SetChildrenActive(mobileCanvas);
        }
    }

    private void ToggleLowGraphics()
    {
        lowGraphicsEnabled = !lowGraphicsEnabled;

        if (lowGraphicsEnabled)
        {
            EnableLowGraphics();
        }
        else
        {
            RestoreGraphics();
        }
    }

    private void EnableLowGraphics()
    {
        particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(false);
        }

        // Disable the Environment GameObject

        if (environmentNormalGraphics != null)
        {
            environmentNormalGraphics.SetActive(false);
            environmentLowGraphics.SetActive(true);
        }

        // Remove the skybox
        RenderSettings.skybox = null;
    }

    private void RestoreGraphics()
    {
        particleSystems = FindObjectsOfType<ParticleSystem>(true); // true overload paramater finds inactive game objects
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(true);
        }

        if (environmentNormalGraphics != null)
        {
            environmentNormalGraphics.SetActive(true);
            environmentLowGraphics.SetActive(false);
        }

        // Restore the skybox

        if (gameManager._map == "Asteroid")
        {
            RenderSettings.skybox = asteroidSkybox;
        }

        if (gameManager._map == "Monument")
        {
            RenderSettings.skybox = monumentSkybox;
        }
    }

    private void SettingsMenuEnable()
    {
        ingameDebugConsole = GameObject.Find("IngameDebugConsole");
        graphy = GameObject.Find("[Graphy]");

        debuggerButton.gameObject.SetActive(!debuggerButton.gameObject.activeSelf);
        gameObjectSensitivitySlider.gameObject.SetActive(!gameObjectSensitivitySlider.gameObject.activeSelf);

        SetChildrenActiveNameSetterCanvas(nameSetterCanvas);
    }

    private void SetChildrenActiveNameSetterCanvas(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }

    private void ToggleDebugger()
    {

			if (ingameDebugConsole != null)
			{
				SetChildrenActive(ingameDebugConsole);
			}

			if (graphy != null)
			{
				SetChildrenActive(graphy);
			}
    }

    private void SetChildrenActive(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
}

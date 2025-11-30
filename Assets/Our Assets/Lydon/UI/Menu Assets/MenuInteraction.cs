using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuInteraction : MonoBehaviour
{ public Camera menuCamera;

    private PlayerControls _playerControls; // This is the actual input mappings
    private PlayerControls.MenuActionsActions _menuActions;
    
    private Renderer myRenderer;
    private bool isHovering = false;
    
    private Color hoverColor = Color.white;
    private Color normalColor = Color.black;

    void Awake()
    {
        _playerControls = new PlayerControls();
        _menuActions = _playerControls.MenuActions;
        _menuActions.MenuInteract.performed += ctx => PlayerInteract();
    }
    
    void Start()
    {
        if (menuCamera == null)
        {
            menuCamera = Camera.main;
        }
        
        myRenderer = GetComponent<MeshRenderer>();
        if (myRenderer == null)
        {
             myRenderer = GetComponentInChildren<MeshRenderer>();
        }
    }

    void Update()
    {
        HandleHover();
    }

    void PlayerInteract()
    {
        print("Interacting!");
        if (isHovering)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void HandleHover()
    {
        if (Mouse.current == null || myRenderer == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = menuCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            //print(hitInfo.collider.gameObject.name);
            if (hitInfo.collider.gameObject == gameObject)
            {
                if (!isHovering)
                {
                    StartHover();
                }
                return;
            }
        }

        if (isHovering)
        {
            StopHover();
        }
    }

    private void StartHover()
    {
        isHovering = true;
        Material[] materials = myRenderer.materials;
        
        if (materials.Length > 1)
        {
            if (materials[1].HasProperty("_BaseColor"))
            {
                 materials[1].SetColor("_BaseColor", hoverColor);
            }
            else
            {
                 materials[1].color = hoverColor;
            }
            
            myRenderer.materials = materials;
        }
    }

    private void StopHover()
    {
        isHovering = false;
        Material[] materials = myRenderer.materials;
        
        if (materials.Length > 1)
        {
            if (materials[1].HasProperty("_BaseColor"))
            {
                 materials[1].SetColor("_BaseColor", normalColor);
            }
            else
            {
                 materials[1].color = normalColor;
            }

            myRenderer.materials = materials;
        }
    }
    void OnEnable()
    {
        _playerControls.MenuActions.Enable();
    }
    void OnDisable()
    {
        _playerControls.MenuActions.Disable();
    }
}
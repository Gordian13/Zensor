/*using UnityEngine;

/// <summary>
///  Attach this script to a Fotowand GameObject.
///  Handles proximity detection, outline highlighting, and triggering the UI panel.
///  
/// Setup:
///     1. Add a Collider to this GameObject and set it to "Is Trigger"
///     2. Assign a FotowandData asset in the Inspector
///     3. Make sure the Player GameObject is tagged "Player"
///     4. Assign the FotowandUI reference in the Inspector
/// </summary>

public class FotowandInteractable : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The ScriptableObject containing this wall's photo and info")]
    public FotowandData data;

    [Header("References")]
    [Tooltip("Reference to the shared FotowandUI panel in the scene")]
    public FotowandUI ui;

    [Header("Interaction Settings")]
    [Tooltip("Key the player presses to open this Fotowand")]
    public KeyCode interactKey = KeyCode.E;
    public float interactDistance = 3f;
    
   
    [Header("Emission color shown when the player is in range")]
    public Color outlineColor = Color.yellow;

    // -- internal state --
    private bool _playerInRange = false;
    private Renderer[] _renderers;
    private Transform _player;

    // -- Unity lifecycle -- 
    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        SetOutline(false);
        _player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        
        if (_player == null) return;

        float distance = Vector3.Distance(
            GetComponent<Collider>().ClosestPoint(_player.position),
            _player.position
        );
        
        bool inRange = distance <= interactDistance;
       

        // Hint & Outline use interactDistance
        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            SetOutline(true);
            ui.ShowHint(interactKey);
        }
        else if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            SetOutline(false);
            ui.HideHint();
            
        }

        //
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ui.Open(data);
        }

       
    }

    //// -- Trigger detection --
    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("Trigger hit by: " + other.gameObject.name + " | Tag: " + other.tag);

    //    if (!other.CompareTag("Player")) return;

    //    _playerInRange = true;
    //    SetOutline(true);
    //    ui.ShowHint(interactKey);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("Player")) return;
    //    Debug.Log("Player EXIT trigger: " + gameObject.name);

    //    StopAllCoroutines();
    //    StartCoroutine(DelayedExit());
    //}

    //private System.Collections.IEnumerator DelayedExit()
    //{
    //    yield return new WaitForSeconds(0.2f);
    //    _playerInRange = false;
    //    SetOutline(false);
    //    ui.HideHint();
    //}

    // -- Outline helpers --
    private void SetOutline(bool enabled)
    {
        foreach (var r in _renderers)
        {
            if (enabled)
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", outlineColor * 0.4f);
            }
            else
            {
                r.material.DisableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", Color.black);
            }
        }

    }

    
}
*/

using UnityEngine;

/// <summary>
/// Attach this script to a Fotowand GameObject.
/// Handles UI display when triggered by the scene's interaction system.
/// 
/// Note: Click detection and highlight are handled by Nils & Jess's scripts.
/// This script only manages the UI panel and data assignment.
/// </summary>
public class FotowandInteractable : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The ScriptableObject containing this wall's photo and info")]
    public FotowandData data;

    [Header("References")]
    [Tooltip("Reference to the shared FotowandUI panel in the scene")]
    public FotowandUI ui;

    // ?? Public API (called by the scene's interaction system) ????????????

    /// <summary>
    /// Call this method to open the UI panel for this Fotowand.
    /// Should be called by the scene's existing interaction scripts.
    /// </summary>
    public void Interact()
    {
        if (ui != null && data != null)
            ui.Open(data);
    }

    /// <summary>
    /// Call this to close the UI panel.
    /// </summary>
    public void StopInteract()
    {
        if (ui != null)
            ui.Close();
    }
}
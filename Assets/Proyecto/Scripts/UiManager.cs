// Archivo: UIManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI; // IMPORTANT: Add this for Button

public class UiManager : MonoBehaviour
{
    #region Singleton
    public static UiManager Instance { get; private set; }
    #endregion

    #region Referencias a Paneles UI Principales
    [Header("Paneles Principales de UI")]
    [SerializeField] private GameObject panelArquitectura;
    [SerializeField] private GameObject panelMonolito;
    [Tooltip("Panel genérico para mostrar información del edificio seleccionado.")]
    [SerializeField] private GameObject panelInformacionEdificio;
    [SerializeField] private GameObject panelControlCrisol;
    #endregion

    #region Referencias a Elementos UI Comunes

    // Renamed from textoRecursoFe for consistency with the new Faith UI
    [Header("Displays de Recursos Globales")]
    [Tooltip("Texto para mostrar el progreso actual de Fe (ej. 50/100).")]
    [SerializeField] private TextMeshProUGUI currentFaithProgressText; // Assign your existing Fe TextMeshProUGUI here
    [SerializeField] private TextMeshProUGUI textoRecursoFragmentos;

    // NEW: UI Elements for Faith Limit Upgrade
    [Header("UI de Fe (Límite y Mejora)")]
    [Tooltip("Texto para mostrar el límite máximo de Fe. Se actualiza con eventos de ResourceManager.")]
    [SerializeField] private TextMeshProUGUI maxFaithLimitText;
    [Tooltip("Texto para mostrar el costo de la mejora del límite de Fe.")]
    [SerializeField] private TextMeshProUGUI upgradeFaithCostText;
    [Tooltip("El botón para activar la mejora del límite de Fe.")]
    [SerializeField] private Button upgradeFaithLimitButton;

    [Header("Configuración de Recursos (para UI)")]
    // Consistent naming: feRecurSO
    [SerializeField] private RecurSO feRecurSO; // Assign your existing Fe RecurSO here
    [SerializeField] private RecurSO fragmentoSO; // Assign your existing Fragmento RecurSO here

    [Header("Configuración de Mejora de Límite de Fe")]
    [Tooltip("El RecurSO que define el tipo de recurso necesario para la mejora del límite de Fe (ej. Piedra, Madera).")]
    [SerializeField] private RecurSO faithUpgradeCostResource;
    [Tooltip("La cantidad de recurso necesaria para la mejora del límite de Fe.")]
    [SerializeField] private int faithUpgradeCostAmount = 50; // Example: 50 of Stone/Wood
    [Tooltip("La cantidad en la que se incrementa el límite de Fe al mejorar.")]
    [SerializeField] private float faithLimitIncrementAmount = 200f; // Example: +200 to Faith limit
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Your existing panel visibility controls
        ControlarVisibilidadPanel(panelArquitectura, false);
        ControlarVisibilidadPanel(panelMonolito, false);
        ControlarVisibilidadPanel(panelInformacionEdificio, false);
        ControlarVisibilidadPanel(panelControlCrisol, false);

        // Validations for new fields
        if (feRecurSO == null) Debug.LogError("UIManager: feRecurSO no asignado. No se puede manejar la UI de Fe.");
        if (faithUpgradeCostResource == null) Debug.LogError("UIManager: faithUpgradeCostResource (costo de mejora) no asignado.");

        // Subscribe to ResourceManager events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.OnRecursoActualizado += ActualizarDisplayRecursoEspecifico;
            ResourceManager.OnFaithLimitChanged += UpdateMaxFaithLimitUI; // NEW subscription
        }
        else
        {
            Debug.LogError("UIManager: ResourceManager.Instance no encontrado. La UI no se actualizará correctamente.");
        }

        // Configure the upgrade button
        if (upgradeFaithLimitButton != null)
        {
            upgradeFaithLimitButton.onClick.RemoveAllListeners();
            upgradeFaithLimitButton.onClick.AddListener(TryIncrementMaxFaithLimit);
        }

        // Initialize all UI displays
        InicializarDisplaysDeRecursos();
        UpdateUpgradeFaithCostUI(); // Display initial upgrade cost
        CheckFaithUpgradeButtonState(); // Check button state on start
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.OnRecursoActualizado -= ActualizarDisplayRecursoEspecifico;
            ResourceManager.OnFaithLimitChanged -= UpdateMaxFaithLimitUI;
        }
    }

    #region Panel Management
    private void ControlarVisibilidadPanel(GameObject panel, bool mostrar)
    {
        if (panel != null)
        {
            panel.SetActive(mostrar);
        }
    }

    public void MostrarPanelArquitectura(bool mostrar)
    {
        ControlarVisibilidadPanel(panelArquitectura, mostrar);
        if (mostrar && GameManager.Instance != null) GameManager.Instance.DeseleccionarEstructuraActual();
    }

    public void MostrarPanelContextualMonolito(bool mostrar)
    {
        ControlarVisibilidadPanel(panelMonolito, mostrar);
    }

    public void MostrarPanelInformacionEdificio(bool mostrar)
    {
        ControlarVisibilidadPanel(panelInformacionEdificio, mostrar);
    }
    
    public void MostrarPanelControlCrisol(bool mostrar)
    {
        ControlarVisibilidadPanel(panelControlCrisol, mostrar);
    }
    #endregion

    #region UI Data Updates

    private void InicializarDisplaysDeRecursos()
    {
        if (ResourceManager.Instance == null) return;

        // Initialize Faith display (now using currentFaithProgressText)
        if (feRecurSO != null) 
            ActualizarDisplayRecursoEspecifico(feRecurSO.Nombre, 
                                                ResourceManager.Instance.GetCantidad(feRecurSO.Nombre), 
                                                ResourceManager.Instance.GetMaximo(feRecurSO.Nombre));
        
        // Initialize Fragment display
        if (fragmentoSO != null) 
            ActualizarDisplayRecursoEspecifico(fragmentoSO.Nombre, 
                                                ResourceManager.Instance.GetCantidad(fragmentoSO.Nombre), 
                                                ResourceManager.Instance.GetMaximo(fragmentoSO.Nombre)); // Maximo for fragments might be 0 or irrelevant
        
        // Ensure max faith limit text is also initialized
        if (feRecurSO != null && maxFaithLimitText != null) {
            maxFaithLimitText.text = $"Límite Máx. Fe: {ResourceManager.Instance.GetMaximo(feRecurSO.Nombre):F0}";
        }
    }
    
    // This method now handles updates for all relevant resources, including the upgrade button state
    private void ActualizarDisplayRecursoEspecifico(string nombreRecurso, float cantidad, float maximo)
    {
        // Update Faith progress display
        if (feRecurSO != null && nombreRecurso == feRecurSO.Nombre && currentFaithProgressText != null)
        {
             currentFaithProgressText.text = $"Fe: {cantidad.ToString("F0")}/{maximo.ToString("F0")}";
        }
        // Update Fragment display
        else if (fragmentoSO != null && nombreRecurso == fragmentoSO.Nombre && textoRecursoFragmentos != null)
        {
            textoRecursoFragmentos.text = $"Fragmentos: {cantidad.ToString("F0")}";
        }

        // Always check the upgrade button state if the cost resource changes
        if (faithUpgradeCostResource != null && nombreRecurso == faithUpgradeCostResource.Nombre)
        {
            CheckFaithUpgradeButtonState();
        }
    }

    /// <summary>
    /// Intenta incrementar el límite máximo de Fe de la civilización.
    /// Verifica los requisitos de recursos antes de realizar la mejora.
    /// Este método se llama desde el evento onClick de un botón de UI.
    /// </summary>
    public void TryIncrementMaxFaithLimit()
    {
        if (ResourceManager.Instance == null || feRecurSO == null || faithUpgradeCostResource == null)
        {
            Debug.LogError("UIManager: Dependencias no asignadas para incrementar el límite de Fe.");
            return;
        }

        // 1. Verificar si el jugador tiene suficientes recursos
        if (ResourceManager.Instance.TieneSuficiente(faithUpgradeCostResource.Nombre, faithUpgradeCostAmount))
        {
            // 2. Gastar los recursos
            ResourceManager.Instance.Gastar(faithUpgradeCostResource.Nombre, faithUpgradeCostAmount);
            Debug.Log($"UIManager: Gastados {faithUpgradeCostAmount} de {faithUpgradeCostResource.Nombre} para mejorar el límite de Fe.");

            // 3. Incrementar el límite máximo de Fe en el ResourceManager
            // El ResourceManager se encargará de disparar los eventos OnRecursoActualizado y OnFaithLimitChanged
            ResourceManager.Instance.IncrementarLimiteMaximoRecurso(feRecurSO.Nombre, faithLimitIncrementAmount);
            
            // Re-check button state and update cost display immediately after upgrade
            CheckFaithUpgradeButtonState();
            UpdateUpgradeFaithCostUI(); // If cost increases per upgrade, this is essential.
        }
        else
        {
            Debug.LogWarning($"UIManager: No hay suficientes {faithUpgradeCostResource.Nombre} para mejorar el límite de Fe. " +
                             $"Se necesitan {faithUpgradeCostAmount}, tienes {ResourceManager.Instance.GetCantidad(faithUpgradeCostResource.Nombre):F0}.");
            // Here you could show a temporary message in the UI to the player.
        }
    }

    /// <summary>
    /// Actualiza el texto que muestra el límite máximo de Fe.
    /// Llamado cuando el límite de Fe cambia (a través del evento OnFaithLimitChanged).
    /// </summary>
    private void UpdateMaxFaithLimitUI(float newLimit)
    {
        if (maxFaithLimitText != null)
        {
            maxFaithLimitText.text = $"Límite Máx. Fe: {newLimit:F0}";
        }
        // Also update the progress to reflect the new limit in the Fe: X/Y format.
        // This ensures visual consistency.
        if (ResourceManager.Instance != null && feRecurSO != null)
        {
            ActualizarDisplayRecursoEspecifico(feRecurSO.Nombre, 
                                            ResourceManager.Instance.GetCantidad(feRecurSO.Nombre), 
                                            newLimit);
        }
        CheckFaithUpgradeButtonState(); // Re-check button state after limit changes (might unlock new upgrades)
    }

    /// <summary>
    /// Actualiza el texto que muestra el costo de la mejora del límite de Fe.
    /// </summary>
    private void UpdateUpgradeFaithCostUI()
    {
        if (upgradeFaithCostText != null && faithUpgradeCostResource != null)
        {
            upgradeFaithCostText.text = $"Costo Mejora: {faithUpgradeCostAmount} {faithUpgradeCostResource.Nombre}";
        }
        else if (upgradeFaithCostText != null)
        {
            upgradeFaithCostText.text = "Costo: N/A";
        }
    }

    /// <summary>
    /// Verifica si el jugador tiene los recursos necesarios y actualiza la interactividad del botón de mejora.
    /// </summary>
    private void CheckFaithUpgradeButtonState()
    {
        if (upgradeFaithLimitButton != null && faithUpgradeCostResource != null && ResourceManager.Instance != null)
        {
            bool canAfford = ResourceManager.Instance.TieneSuficiente(faithUpgradeCostResource.Nombre, faithUpgradeCostAmount);
            upgradeFaithLimitButton.interactable = canAfford; // Controls if the button can be clicked
        }
        else if (upgradeFaithLimitButton != null)
        {
            upgradeFaithLimitButton.interactable = false; // Disable if something is not configured
        }
    }
    #endregion
}
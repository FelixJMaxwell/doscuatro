// Archivo: UIManager.cs (VERSION REFACTORIZADA)
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text; // Necesario para StringBuilder
// Asegúrate de tener la referencia a tu nueva clase de configuración:
// using ConfiguracionEntradaArquitecturaUI; // No es necesario si está en el mismo namespace
// O simplemente asegúrate de que esté en el mismo namespace raíz

public class UIManager : MonoBehaviour
{
    #region Singleton Instance
    public static UIManager Instance { get; private set; }
    #endregion

    #region Panel References
    // =================================================================================================================
    // REFERENCIAS A PANELES PRINCIPALES Y MODALES
    // =================================================================================================================
    [Header("Paneles Principales de UI")]
    [Tooltip("Panel UI para el menú de construcción de arquitecturas.")]
    [SerializeField] private GameObject panelArquitectura;
    public GameObject PanelArquitectura => panelArquitectura; // Propiedad pública para acceso externo

    [Tooltip("Panel UI para interacciones con el Monolito seleccionado.")]
    [SerializeField] private GameObject panelMonolito;
    public GameObject PanelMonolito => panelMonolito;

    [Tooltip("Panel genérico para mostrar información del edificio seleccionado.")]
    [SerializeField] private GameObject panelInformacionEdificio;
    public GameObject PanelInformacionEdificio => panelInformacionEdificio; // Aunque no se usa ahora, bueno tenerlo

    [Tooltip("Panel UI que contiene los controles específicos para el Crisol de Almas seleccionado.")]
    [SerializeField] private GameObject panelControlCrisol;
    public GameObject PanelControlCrisol => panelControlCrisol;

    [Tooltip("Panel UI para interacciones con personajes (NPCs) seleccionados.")]
    [SerializeField] private GameObject panelNPC;
    public GameObject PanelNPC => panelNPC;
    #endregion

    #region UI Element References (Specific)
    // =================================================================================================================
    // REFERENCIAS A ELEMENTOS UI ESPECÍFICOS (Mover aquí del GameManager)
    // =================================================================================================================
    [Header("UI Específica de Crisol de Almas")]
    [Tooltip("Botón para generar aldeanos desde el Crisol.")]
    [SerializeField] private Button generarAldeanoBtn; // Ahora privado y serializado
    [Tooltip("Toggle para decidir si generar un aldeano legendario.")]
    [SerializeField] private Toggle toggleLegendario; // Ahora privado y serializado
    [Tooltip("Botón para actualizar/mejorar el Crisol de Almas.")]
    [SerializeField] private Button actualizarCrisolBtn; // Ahora privado y serializado
    private Building_Personajes _crisolSeleccionadoActual = null; // Interno para UIManager
    #endregion

    #region Architecture UI Configuration
    // =================================================================================================================
    // CONFIGURACIÓN UI DE ARQUITECTURA (Mover aquí del GameManager)
    // =================================================================================================================
    [Header("Configuración UI de Arquitectura")]
    [Tooltip("Lista de entradas para la UI de arquitectura. Cada entrada vincula un GameObject de UI con su EdificioDataSO.")]
    public List<ConfiguracionEntradaArquitecturaUI> listaConfiguracionArquitecturaUI;
    public GameObject PanelCheats;
    #endregion

    #region Unity Lifecycle Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ConfigurarElementosUIConstruccion(); // UIManager ahora es quien configura su propia UI
        // Opcional: Suscribirse a eventos de ResourceManager para actualizar la interactividad de los botones de construcción
        // if (ResourceManager.Instance != null)
        // {
        //     ResourceManager.Instance.OnRecursosCambiados += UpdateAllBuildingButtonStates;
        // }
    }

    // Asegúrate de desuscribirte si te suscribes a eventos
    // private void OnDestroy()
    // {
    //     if (ResourceManager.Instance != null)
    //     {
    //         ResourceManager.Instance.OnRecursosCambiados -= UpdateAllBuildingButtonStates;
    //     }
    // }
    #endregion

    #region UI Management (Core Methods)
    // =================================================================================================================
    // MÉTODOS DE GESTIÓN DE UI
    // =================================================================================================================

    /// <summary>
    /// Activa un panel UI dado.
    /// </summary>
    public void AbrirPanel(GameObject panel)
    {
        if (panel != null && !panel.activeInHierarchy)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// Desactiva un panel UI dado.
    /// </summary>
    public void CerrarPanel(GameObject panel)
    {
        if (panel != null && panel.activeInHierarchy)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Alterna la visibilidad de un panel UI.
    /// </summary>
    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeInHierarchy);
        }
    }

    public void TogglePanelCheats()
    {
        TogglePanel(PanelCheats);
    }

    /// <summary>
    /// Actualiza el texto de un elemento UI TextMeshProUGUI.
    /// </summary>
    public void ActualizarTextoUI(TextMeshProUGUI elementoUI, string nuevoTexto)
    {
        if (elementoUI != null)
        {
            elementoUI.text = nuevoTexto;
        }
    }
    #endregion

    #region Architecture UI Configuration Logic (Moved from GameManager)
    // =================================================================================================================
    // LÓGICA DE CONFIGURACIÓN AUTOMÁTICA DE UI DE ARQUITECTURA
    // =================================================================================================================
    private void ConfigurarElementosUIConstruccion()
    {
        if (listaConfiguracionArquitecturaUI == null || listaConfiguracionArquitecturaUI.Count == 0)
        {
            Debug.LogWarning("UIManager: 'listaConfiguracionArquitecturaUI' está vacía. No se configurarán elementos de UI de construcción.");
            return;
        }

        for (int i = 0; i < listaConfiguracionArquitecturaUI.Count; i++)
        {
            ConfiguracionEntradaArquitecturaUI entrada = listaConfiguracionArquitecturaUI[i];

            if (entrada.elementoRaizUI == null || entrada.datosDelEdificioSO == null || entrada.datosDelEdificioSO.prefabDelEdificio == null)
            {
                Debug.LogWarning($"UIManager: Entrada de arquitectura UI en el índice {i} (Nombre Editor: '{entrada.nombreEditor ?? "N/A"}') está incompleta (elementoRaizUI, datosDelEdificioSO o su prefab es nulo). Saltando.");
                if (entrada.elementoRaizUI != null) entrada.elementoRaizUI.SetActive(false); // Ocultar si está mal configurado
                continue;
            }

            Transform panelTransform = entrada.elementoRaizUI.transform;
            EdificioDataSO edificioData = entrada.datosDelEdificioSO;

            // Asignar según el orden de hijos especificado:
            // Hijo 0: SpriteEdificio (Image)
            // Hijo 1: NombreEdificio (TextMeshProUGUI)
            // Hijo 2: DescripcionEdificio (TextMeshProUGUI)
            // Hijo 3: Button (Button)
            // Hijo 4: CostoEdificio (TextMeshProUGUI)

            // Sprite Edificio
            if (panelTransform.childCount > 0)
            {
                Image spriteEdificio = panelTransform.GetChild(0).GetComponent<Image>();
                if (spriteEdificio != null)
                {
                    if (edificioData.icono != null)
                    {
                        spriteEdificio.sprite = edificioData.icono;
                        spriteEdificio.enabled = true;
                    }
                    else spriteEdificio.enabled = false;
                }
                else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': Hijo 0 no tiene Image.");
            }
            else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': No tiene suficientes hijos (necesita al menos 1 para Sprite).");

            // Nombre Edificio
            if (panelTransform.childCount > 1)
            {
                TextMeshProUGUI textoNombre = panelTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (textoNombre != null) textoNombre.text = edificioData.nombreMostrado;
                else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': Hijo 1 no tiene TextMeshProUGUI.");
            }
            else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': No tiene suficientes hijos para Nombre.");

            // Descripción Edificio
            if (panelTransform.childCount > 2)
            {
                TextMeshProUGUI textoDesc = panelTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
                if (textoDesc != null) textoDesc.text = edificioData.descripcion;
                else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': Hijo 2 no tiene TextMeshProUGUI.");
            }
            else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': No tiene suficientes hijos para Descripción.");

            // Botón de Construcción
            if (panelTransform.childCount > 3)
            {
                Button boton = panelTransform.GetChild(3).GetComponent<Button>();
                if (boton != null)
                {
                    boton.onClick.RemoveAllListeners();
                    EdificioDataSO dataParaEsteBoton = edificioData; // Capturar 'edificioData' para la clausura de la lambda
                    boton.onClick.AddListener(() => OnBuildButtonClicked(dataParaEsteBoton)); // Llama a un método específico de UIManager
                    // La interactividad se gestionará en un método separado para refrescar al cambiar recursos
                    UpdateBuildingButtonState(boton, dataParaEsteBoton);
                }
                else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': Hijo 3 no tiene Button.");
            }
            else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': No tiene suficientes hijos para Botón.");

            // Costo Edificio
            if (panelTransform.childCount > 4)
            {
                TextMeshProUGUI textoCosto = panelTransform.GetChild(4).GetComponent<TextMeshProUGUI>();
                if (textoCosto != null)
                {
                    BaseBuilding edificioBase = edificioData.prefabDelEdificio.GetComponent<BaseBuilding>();
                    if (edificioBase != null && edificioBase.constructionCosts != null)
                    {
                        textoCosto.text = FormatearCostosSimple(edificioBase.constructionCosts);
                    }
                    else textoCosto.text = "N/A";
                }
                else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': Hijo 4 no tiene TextMeshProUGUI.");
            }
            else Debug.LogWarning($"Entrada UI '{entrada.nombreEditor}': No tiene suficientes hijos para Costo.");

            // Activar el panel UI de la entrada por si estaba desactivado por defecto
            entrada.elementoRaizUI.SetActive(true);
        }
    }

    /// <summary>
    /// Método llamado cuando un botón de construcción es presionado.
    /// Delega la acción de iniciar la construcción al BuildingManager.
    /// </summary>
    public void OnBuildButtonClicked(EdificioDataSO edificioData)
    {
        if (edificioData == null || edificioData.prefabDelEdificio == null)
        {
            Debug.LogError("UIManager: Datos de edificio o prefab nulo al intentar construir.");
            return;
        }

        BaseBuilding buildingPrefabComponent = edificioData.prefabDelEdificio.GetComponent<BaseBuilding>();
        if (buildingPrefabComponent == null)
        {
            Debug.LogError($"UIManager: El prefab '{edificioData.prefabDelEdificio.name}' no tiene un componente BaseBuilding. No se puede construir.");
            return;
        }

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.StartPlacementMode(buildingPrefabComponent);
            CerrarPanel(PanelArquitectura); // Cierra el panel de arquitectura al iniciar la construcción
        }
        else
        {
            Debug.LogError("UIManager: BuildingManager no encontrado para iniciar la construcción.");
        }
    }

    /// <summary>
    /// Actualiza el estado de interactividad de un botón de construcción basado en los costos y recursos.
    /// </summary>
    private void UpdateBuildingButtonState(Button button, EdificioDataSO buildingData)
    {
        if (button == null || buildingData == null || buildingData.prefabDelEdificio == null || ResourceManager.Instance == null)
        {
            button.interactable = false;
            return;
        }

        BaseBuilding buildingPrefabScript = buildingData.prefabDelEdificio.GetComponent<BaseBuilding>();
        if (buildingPrefabScript == null || buildingPrefabScript.constructionCosts == null || buildingPrefabScript.constructionCosts.Count == 0)
        {
            button.interactable = true; // Si no tiene costos, es construible
            return;
        }

        bool canAfford = true;
        foreach (var cost in buildingPrefabScript.constructionCosts)
        {
            if (cost.resourceSO != null && !ResourceManager.Instance.TieneSuficiente(cost.resourceSO.Nombre, cost.amount))
            {
                canAfford = false;
                break;
            }
        }
        button.interactable = canAfford;
    }

    /// <summary>
    /// Método para actualizar el estado de todos los botones de construcción.
    /// Debería ser llamado cuando los recursos del jugador cambian.
    /// </summary>
    public void UpdateAllBuildingButtonStates()
    {
        foreach (ConfiguracionEntradaArquitecturaUI entrada in listaConfiguracionArquitecturaUI)
        {
            if (entrada.elementoRaizUI != null && entrada.datosDelEdificioSO != null)
            {
                Button boton = entrada.elementoRaizUI.transform.GetChild(3).GetComponent<Button>();
                if (boton != null)
                {
                    UpdateBuildingButtonState(boton, entrada.datosDelEdificioSO);
                }
            }
        }
    }

    // Método de utilidad para formatear costos (mantenido aquí porque es específico de la UI de costos)
    private string FormatearCostosSimple(List<ConstructionCostEntry> costos)
    {
        if (costos == null || costos.Count == 0) return "Gratis";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < costos.Count; i++)
        {
            ConstructionCostEntry costo = costos[i];
            if (costo.resourceSO != null)
            {
                sb.Append($"{costo.resourceSO.Nombre}: {costo.amount.ToString("F0")}");
                if (i < costos.Count - 1) sb.Append(" | ");
            }
        }
        return sb.ToString();
    }
    #endregion

    #region Specific UI Management (Moved from GameManager)
    // =================================================================================================================
    // LÓGICA DE GESTIÓN DE UI ESPECÍFICA (CRISOL, MONOLITO, NPC)
    // =================================================================================================================

    /// <summary>
    /// Configura y abre la UI para un Crisol de Almas seleccionado.
    /// </summary>
    /// <param name="crisol">La instancia de Building_Personajes del crisol seleccionado.</param>
    public void ConfigurarUICrisol(Building_Personajes crisol)
    {
        if (crisol == null)
        {
            LimpiarUICrisol();
            return;
        }
        _crisolSeleccionadoActual = crisol; // Guardar referencia al crisol actual

        if (generarAldeanoBtn != null) // Usar las referencias privadas (generarAldeanoBtn)
        {
            generarAldeanoBtn.onClick.RemoveAllListeners();
            generarAldeanoBtn.onClick.AddListener(() => _crisolSeleccionadoActual.IntentarGenerarPersonajeDesdeUI());
            generarAldeanoBtn.interactable = true; // O basarlo en condiciones del crisol
        }

        if (toggleLegendario != null)
        {
            toggleLegendario.onValueChanged.RemoveAllListeners();
            // Necesitarías una forma de que Building_Personajes exponga su estado para inicializar el toggle
            // Por ejemplo, con una propiedad pública get: public bool IntentaGenerarLegendario => _intentaGenerarLegendarioToggleState;
            // toggleLegendario.isOn = _crisolSeleccionadoActual.IntentaGenerarLegendario; // Descomentar si tienes la propiedad
            toggleLegendario.onValueChanged.AddListener((value) =>
            {
                if (_crisolSeleccionadoActual != null)
                    _crisolSeleccionadoActual.SetIntentaGenerarLegendarioToggleState(value);
            });
        }

        if (actualizarCrisolBtn != null)
        {
            actualizarCrisolBtn.onClick.RemoveAllListeners();
            actualizarCrisolBtn.onClick.AddListener(() => _crisolSeleccionadoActual.IntentarActualizarEdificio());
            actualizarCrisolBtn.interactable = true; // O basarlo en condiciones
        }

        if (PanelControlCrisol != null) // Usar la propiedad pública (PanelControlCrisol)
        {
            AbrirPanel(PanelControlCrisol);
        }
        // Aquí también podrías llamar a un método para actualizar cualquier texto en PanelControlCrisol
        // que muestre información del _crisolSeleccionadoActual (ej. su nivel, producción, etc.)
        // UpdateTextosPanelCrisol(_crisolSeleccionadoActual);
    }

    /// <summary>
    /// Limpia y cierra la UI para el Crisol de Almas.
    /// </summary>
    public void LimpiarUICrisol()
    {
        if (generarAldeanoBtn != null) generarAldeanoBtn.onClick.RemoveAllListeners();
        if (toggleLegendario != null) toggleLegendario.onValueChanged.RemoveAllListeners();
        if (actualizarCrisolBtn != null) actualizarCrisolBtn.onClick.RemoveAllListeners();

        // Opcional: Desactivar botones si no hay crisol seleccionado
        if (generarAldeanoBtn != null) generarAldeanoBtn.interactable = false;
        if (toggleLegendario != null) toggleLegendario.interactable = false;
        if (actualizarCrisolBtn != null) actualizarCrisolBtn.interactable = false;

        if (PanelControlCrisol != null)
        {
            CerrarPanel(PanelControlCrisol);
        }
        _crisolSeleccionadoActual = null;
    }

    // Aquí podrías añadir ConfigurarUIMonolito, ConfigurarUINPC, etc., si son complejos
    // public void ConfigurarUIMonolito(MonolitoBehaviour monolito) { /* ... */ }
    // public void ConfigurarUINPC(PersonajeBehaviour npc) { /* ... */ }

    #endregion
}
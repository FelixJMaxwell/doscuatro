// Archivo: GameManager.cs (VERSION REFACTORIZADA)
using System.Collections.Generic;
using UnityEngine;

// Ya no necesitarás:
// using System.Text;
// using TMPro;
// using UnityEngine.UI;
// [System.Serializable] public class ConfiguracionEntradaArquitecturaUI (¡Ahora está en su propio archivo!)

public class GameManager : MonoBehaviour
{
    #region Singleton Instance
    // =================================================================================================================
    // SINGLETON
    // =================================================================================================================
    public static GameManager Instance { get; private set; }
    #endregion

    #region Public Game State & Core References
    // =================================================================================================================
    // ESTADO GLOBAL DEL JUEGO Y REFERENCIAS PRINCIPALES
    // =================================================================================================================
    [Header("Estado Global y Referencias Principales")]
    [Tooltip("Puntos generales del jugador o alguna métrica global.")]
    public float PuntosGlobales;
    [Tooltip("Referencia al objeto Monolito principal en la escena.")]
    public MonolitoBehaviour MonolitoPrincipal;
    #endregion

    #region Building Placement & Selection State (Partially Managed by BuildingManager now)
    // =================================================================================================================
    // ESTADO DE COLOCACIÓN Y SELECCIÓN DE EDIFICIOS
    // (Menos estado directo de colocación aquí, más delegado a BuildingManager)
    // =================================================================================================================
    [Header("Sistema de Construcción y Selección")]
    // 'EstructuraEnModoColocacion' ya no debería ser directamente gestionada por GameManager.
    // Esa lógica vive en BuildingManager.
    // public GameObject EstructuraEnModoColocacion; // Eliminar o dejar como referencia interna para otros fines

    [Tooltip("Referencia al GameObject de la estructura actualmente seleccionada por el jugador para interacción.")]
    public GameObject EstructuraSeleccionadaParaInteraccion;
    // 'PrefabEstructuraAGenerica' ya no es necesaria aquí; la UI se encargará de pasar el prefab correcto.
    // public GameObject PrefabEstructuraAGenerica; // Eliminar

    [Tooltip("Transform padre bajo el cual se organizarán todas las estructuras instanciadas en el juego.")]
    public Transform ContenedorDeEstructuras;
    [Tooltip("Lista de todos los scripts BaseBuilding de las estructuras activas en el juego.")]
    public List<BaseBuilding> TodasLasEstructurasActivas = new List<BaseBuilding>();
    #endregion

    #region Unity Lifecycle Methods
    // =================================================================================================================
    // MÉTODOS DEL CICLO DE VIDA DE UNITY (AWAKE, UPDATE)
    // =================================================================================================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Considera si GameManager debe persistir
        }
        else
        {
            Debug.LogWarning("GameManager: Ya existe una instancia. Destruyendo este duplicado.");
            Destroy(gameObject);
        }

        if (TodasLasEstructurasActivas == null)
        {
            TodasLasEstructurasActivas = new List<BaseBuilding>();
        }
    }

    private void Start()
    {
        // GameManager ya NO configura la UI de construcción. Eso lo hará UIManager.
        // ConfigurarElementosUIConstruccion(); // ¡ELIMINAR ESTA LÍNEA!
    }

    private void Update()
    {
        ManejarInputGlobal();
    }
    #endregion

    #region Global Input & Selection Management
    // =================================================================================================================
    // GESTIÓN DE INPUT GLOBAL Y SELECCIÓN
    // (Ahora delega a UIManager y BuildingManager para la lógica específica)
    // =================================================================================================================
    private void ManejarInputGlobal()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (BuildingManager.Instance != null && BuildingManager.Instance.IsInPlacementMode)
            {
                BuildingManager.Instance.CancelPlacementMode();
                Debug.Log("GameManager: Construcción cancelada vía Escape (BuildingManager).");
            }
            else if (EstructuraSeleccionadaParaInteraccion != null)
            {
                DeseleccionarEstructuraActual();
            }
            // Importante: UIManager ahora es el responsable de saber qué paneles están abiertos/cerrados.
            // Si quieres que Escape cierre el panel de arquitectura, UIManager debe tener el control.
            // Aquí, GameManager solo le dice a UIManager que intente cerrar un panel general de UI si está abierto.
            else if (UIManager.Instance != null) // Reemplaza la comprobación específica del PanelArquitectura aquí
            {
                // UIManager.Instance.CerrarCualquierPanelActivo(); // Podrías tener un método genérico así
                // O si quieres que solo cierre el de arquitectura:
                if (UIManager.Instance.PanelArquitectura != null && UIManager.Instance.PanelArquitectura.activeInHierarchy)
                {
                    UIManager.Instance.CerrarPanel(UIManager.Instance.PanelArquitectura);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            // Pide al UIManager que se encargue de abrir/cerrar el panel de cheats
            if (UIManager.Instance != null && UIManager.Instance.PanelCheats != null)
            {
                UIManager.Instance.TogglePanelCheats();
                Debug.Log("GameManager: Presionado 'C', UIManager toggling Cheats Panel.");
            }
        }
    }

    public void SeleccionarEstructura(GameObject estructuraObj)
    {
        if (EstructuraSeleccionadaParaInteraccion != null)
        {
            // Antes de deseleccionar la vieja, dile a UIManager que limpie su UI asociada
            if (UIManager.Instance != null)
            {
                UIManager.Instance.LimpiarUICrisol(); // Asumiendo que esta lógica se mueve a UIManager
            }
        }

        DeseleccionarEstructuraActual();

        EstructuraSeleccionadaParaInteraccion = estructuraObj;

        if (EstructuraSeleccionadaParaInteraccion == null) return;

        // Delegar la configuración de UI a UIManager
        Building_Personajes crisolSeleccionado = EstructuraSeleccionadaParaInteraccion.GetComponent<Building_Personajes>();
        if (crisolSeleccionado != null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ConfigurarUICrisol(crisolSeleccionado);
            }
        }
        else if (EstructuraSeleccionadaParaInteraccion.GetComponent<MonolitoBehaviour>() != null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AbrirPanel(UIManager.Instance.PanelMonolito);
                // Configurar UI del Monolito aquí si fuera necesario
                // UIManager.Instance.ConfigurarUIMonolito(EstructuraSeleccionadaParaInteraccion.GetComponent<MonolitoBehaviour>());
            }
        }
        else if (EstructuraSeleccionadaParaInteraccion.GetComponent<PersonajeBehaviour>() != null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AbrirPanel(UIManager.Instance.PanelNPC);
                // Configurar UI del NPC aquí si fuera necesario
                // UIManager.Instance.ConfigurarUINPC(EstructuraSeleccionadaParaInteraccion.GetComponent<PersonajeBehaviour>());
            }
        }
        // ... añadir más tipos de edificios y su manejo de UI
    }

    public void DeseleccionarEstructuraActual()
    {
        if (EstructuraSeleccionadaParaInteraccion == null) return;

        // Delegar la limpieza de UI a UIManager
        if (UIManager.Instance != null)
        {
            Building_Personajes crisolSeleccionado = EstructuraSeleccionadaParaInteraccion.GetComponent<Building_Personajes>();
            if (crisolSeleccionado != null)
            {
                UIManager.Instance.LimpiarUICrisol();
            }
            else if (EstructuraSeleccionadaParaInteraccion.GetComponent<MonolitoBehaviour>() != null)
            {
                UIManager.Instance.CerrarPanel(UIManager.Instance.PanelMonolito);
            }
            else if (EstructuraSeleccionadaParaInteraccion.GetComponent<PersonajeBehaviour>() != null)
            {
                UIManager.Instance.CerrarPanel(UIManager.Instance.PanelNPC);
            }
            // Añadir más condiciones para cerrar otros paneles específicos de estructuras seleccionadas
        }

        EstructuraSeleccionadaParaInteraccion = null;
    }

    // La lógica de Crisol de Almas (ConfigurarUICrisol, LimpiarUICrisol) se mueve al UIManager
    // private void ConfigurarUICrisol(Building_Personajes crisol) { ... } // ELIMINAR
    // private void LimpiarUICrisol() { ... } // ELIMINAR
    #endregion

    #region Configuración Automática de UI de Construcción (¡ESTA SECCIÓN SE MUEVE A UIManager.cs!)
    // private void ConfigurarElementosUIConstruccion() { ... } // ELIMINAR
    // private string FormatearCostosSimple(List<ConstructionCostEntry> costos) { ... } // ELIMINAR

    // private void IniciarConstruccionConDataSO(EdificioDataSO edificioData) { ... } // ELIMINAR
    #endregion

    #region UI Management (Basic) (¡ESTA SECCIÓN SE MUEVE A UIManager.cs!)
    // public void ActualizarTextoUI(TextMeshProUGUI elementoUI, string nuevoTexto) { ... } // ELIMINAR
    // private void AbrirPanel(GameObject panel) { ... } // ELIMINAR
    // private void CerrarPanel(GameObject panel) { ... } // ELIMINAR

    // Esto es un método público de tu GameManager que un botón puede llamar.
    // Lo ideal es que el botón llame directamente a UIManager.Instance.TogglePanel(UIManager.Instance.PanelArquitectura)
    // Pero si necesitas que GameManager haga pre-lógica, se mantiene, pero la acción final es de UIManager.
    public void TogglePanelArquitectura()
    {
        if (UIManager.Instance != null)
        {
            DeseleccionarEstructuraActual(); // GameManager deselecciona una estructura del mundo
            if (BuildingManager.Instance != null && BuildingManager.Instance.IsInPlacementMode)
            {
                BuildingManager.Instance.CancelPlacementMode(); // GameManager le pide a BuildingManager que cancele
            }
            UIManager.Instance.TogglePanel(UIManager.Instance.PanelArquitectura); // UIManager abre/cierra su panel
        }
    }
    #endregion

    #region Building List Management (Callbacks from BaseBuilding or BuildingManager)
    // =================================================================================================================
    // GESTIÓN DE LA LISTA DE EDIFICIOS ACTIVOS
    // (Esto permanece en GameManager, ya que es una preocupación global del estado del juego)
    // =================================================================================================================
    public void RegistrarEdificioConstruido(BaseBuilding edificio)
    {
        if (edificio != null && !TodasLasEstructurasActivas.Contains(edificio))
        {
            TodasLasEstructurasActivas.Add(edificio);
            // Debug.Log($"GameManager: Edificio '{edificio.buildingName}' registrado. Total: {TodasLasEstructurasActivas.Count}");
        }
    }

    public void DesregistrarEdificioDestruido(BaseBuilding edificio)
    {
        if (edificio != null && TodasLasEstructurasActivas.Contains(edificio))
        {
            TodasLasEstructurasActivas.Remove(edificio);
            // Debug.Log($"GameManager: Edificio '{edificio.buildingName}' desregistrado. Total: {TodasLasEstructurasActivas.Count}");
        }
    }
    #endregion
}
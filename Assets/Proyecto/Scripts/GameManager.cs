// Archivo: GameManager.cs
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Necesario para Button y Toggle (aunque se acceden por GameObject en tu código, el tipo es de UI)

// Esta clase se puede definir dentro de GameManager.cs o en su propio archivo .cs
// Si está dentro de GameManager.cs, ponla FUERA de la clase GameManager pero en el mismo archivo,
// o como una clase pública anidada si prefieres (public class GameManager { [System.Serializable] public class NombreClase ... })
// Por simplicidad, la definimos aquí como si estuviera lista para ser usada por GameManager.

[System.Serializable] // Necesario para que aparezca en el Inspector como una lista de objetos.
public class ConfiguracionEntradaArquitecturaUI
{
    [Tooltip("Nombre descriptivo para esta entrada en el Inspector (ej. 'Entrada UI Casa').")]
    public string nombreEditor; // Para tu organización en el Inspector.

    [Tooltip("El GameObject raíz de la 'tarjeta' o 'entrada' de UI para este edificio. Sus hijos deben seguir un orden específico.")]
    public GameObject elementoRaizUI;

    [Tooltip("El ScriptableObject que contiene todos los datos (nombre, descripción, icono, prefab del edificio) para esta entrada.")]
    public EdificioDataSO datosDelEdificioSO;
}

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
    [Tooltip("Puntos generales del jugador o alguna métrica global (actualmente no usada).")]
    public float PuntosGlobales; // Renombrado de Puntos
    [Tooltip("Referencia al objeto Monolito principal en la escena.")]
    public MonolitoBehaviour MonolitoPrincipal; // Cambiado a tipo específico para acceso directo.
    #endregion

    #region Building Placement & Selection State
    // =================================================================================================================
    // ESTADO DE COLOCACIÓN Y SELECCIÓN DE EDIFICIOS
    // =================================================================================================================
    [Header("Sistema de Construcción y Selección")]
    [Tooltip("Referencia al GameObject de la estructura que el jugador está actualmente colocando (modo fantasma).")]
    public GameObject EstructuraEnModoColocacion; // Renombrado de EstructuraConstruyendo
    [Tooltip("Referencia al GameObject de la estructura actualmente seleccionada por el jugador para interacción.")]
    public GameObject EstructuraSeleccionadaParaInteraccion; // Renombrado de EstructuraSeleccionada
    [Tooltip("Prefab de la estructura que se instanciará al iniciar un proceso de construcción (ej. desde un botón genérico 'ComprarDado').")]
    public GameObject PrefabEstructuraAGenerica; // Renombrado de EstructurAConstruir

    [Tooltip("Transform padre bajo el cual se organizarán todas las estructuras instanciadas en el juego.")]
    public Transform ContenedorDeEstructuras; // Renombrado de EstructurasHolder
    [Tooltip("Lista de todos los scripts BaseBuilding de las estructuras activas en el juego.")]
    public List<BaseBuilding> TodasLasEstructurasActivas = new List<BaseBuilding>();
    #endregion

    #region UI Panel & Element References
    // =================================================================================================================
    // REFERENCIAS A ELEMENTOS Y PANELES DE LA UI
    // (Idealmente, gran parte de esto estaría en un UIManager dedicado)
    // =================================================================================================================
    [Header("Paneles UI Principales")]
    [Tooltip("Panel UI para interacciones con NPCs seleccionados.")]
    public GameObject PanelNPC; // Renombrado
    [Tooltip("Panel UI para interacciones con el Monolito seleccionado.")]
    public GameObject PanelMonolito; // Renombrado
    [Tooltip("Panel UI para el menú de construcción de arquitecturas.")]
    public GameObject PanelArquitectura;

    [Header("UI Específica (Temporal)")]
    [Tooltip("Lista de GameObjects que contienen elementos de texto UI generales. Usar con precaución, preferir referencias directas.")]
    public List<GameObject> TextosUIGenerales; // Renombrado de TextosUI

    [Header("UI para Crisol de Almas (Ejemplo de Interacción Directa)")]
    [Tooltip("Referencia al GameObject del Crisol de Almas en la escena o un prefab a instanciar.")]
    public GameObject CrisolDeAlmasGOPrefab; // Renombrado para indicar que puede ser un prefab
    [Tooltip("Botón para generar aldeanos desde el Crisol.")]
    public Button GenerarAldeanoBtn;
    [Tooltip("Toggle para decidir si generar un aldeano legendario.")]
    public Toggle ToggleLegendario;
    [Tooltip("Botón para actualizar/mejorar el Crisol de Almas.")]
    public Button ActualizarCrisolBtn;
    // GameObject del panel que contiene estos elementos, si es un panel específico
    [Tooltip("Panel UI que contiene los controles para el Crisol de Almas seleccionado.")]
    public GameObject panelControlCrisol;
    private Building_Personajes _crisolSeleccionadoActual = null;
    #endregion

    #region Arquitectura y Configuración Automática de UI
    // =================================================================================================================
    // CONFIGURACIÓN AUTOMÁTICA DE LA UI DE ARQUITECTURA
    // =================================================================================================================
    [Header("Configuración UI de Arquitectura")]
    [Tooltip("Lista de entradas para la UI de arquitectura. Cada entrada vincula un GameObject de UI con su EdificioDataSO.")]
    public List<ConfiguracionEntradaArquitecturaUI> listaConfiguracionArquitecturaUI; // CAMBIADO a la nueva clase/struct
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
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("GameManager: Ya existe una instancia. Destruyendo este duplicado.");
            Destroy(gameObject);
        }

        // Asegurar que la lista esté inicializada
        if (TodasLasEstructurasActivas == null)
        {
            TodasLasEstructurasActivas = new List<BaseBuilding>();
        }
    }

    private void Start() // Modificamos Start para añadir la configuración de botones
    {
        // ... (código existente en Start si lo hubiera, como _mainCamera = Camera.main;) ...
        ConfigurarElementosUIConstruccion();
    }

    private void Update()
    {
        ManejarInputGlobal(); // Deselección con Escape, etc.
        // La lógica de mover el fantasma de construcción y la interacción con la estructura seleccionada
        // se ha movido/comentado para ser manejada por sistemas más dedicados o directamente
        // por los scripts de los objetos (ej. BuildingManager para colocación, MonolitoBehaviour para su input).
        // UpdateActualizarLogicaSeleccionInteraccion(); // Método que contenía la lógica de F y G
    }
    #endregion

    #region Global Input & Selection Management
    // =================================================================================================================
    // GESTIÓN DE INPUT GLOBAL Y SELECCIÓN
    // =================================================================================================================
    private void ManejarInputGlobal()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (BuildingManager.Instance != null && BuildingManager.Instance.IsInPlacementMode) // Chequea si BuildingManager está colocando
            {
                BuildingManager.Instance.CancelPlacementMode();
                Debug.Log("GameManager: Construcción cancelada vía Escape (BuildingManager).");
            }
            else if (EstructuraSeleccionadaParaInteraccion != null)
            {
                DeseleccionarEstructuraActual();
            }
            else if (PanelArquitectura != null && PanelArquitectura.activeInHierarchy)
            {
                CerrarPanel(PanelArquitectura);
            }
            // Considerar cerrar otros paneles modales aquí también.
        }
        // La selección por clic se manejaría ahora en un sistema de Input/Selección dedicado
        // o en el Update de BuildingManager para la colocación.
    }

    public void SeleccionarEstructura(GameObject estructuraObj)
    {
        // Primero, limpiar la UI del objeto anteriormente seleccionado
        if (EstructuraSeleccionadaParaInteraccion != null)
        {
            if (EstructuraSeleccionadaParaInteraccion.GetComponent<Building_Personajes>() != null)
            {
                LimpiarUICrisol(); // Desconecta listeners y oculta panel del Crisol
            }
            // ... (lógica para otros tipos de paneles como Monolito, NPC) ...
        }

        DeseleccionarEstructuraActual(); // Esto ya oculta otros paneles genéricos y pone la variable a null

        EstructuraSeleccionadaParaInteraccion = estructuraObj;

        if (EstructuraSeleccionadaParaInteraccion == null) return;

        // Debug.Log($"'{EstructuraSeleccionadaParaInteraccion.name}' seleccionada por GameManager.");

        Building_Personajes crisolSeleccionado = EstructuraSeleccionadaParaInteraccion.GetComponent<Building_Personajes>();
        if (crisolSeleccionado != null)
        {
            ConfigurarUICrisol(crisolSeleccionado); // Configura y muestra la UI del Crisol
        }
        else if (EstructuraSeleccionadaParaInteraccion.GetComponent<MonolitoBehaviour>() != null && PanelMonolito != null)
        {
            AbrirPanel(PanelMonolito);
            // Configurar UI del Monolito aquí...
        }
        else if (EstructuraSeleccionadaParaInteraccion.GetComponent<PersonajeBehaviour>() != null && PanelNPC != null)
        {
            AbrirPanel(PanelNPC);
            // Configurar UI del NPC aquí...
        }
        // ... más lógica para otros tipos de selección ...
    }

    public void DeseleccionarEstructuraActual() // Este método se llama ANTES de una nueva selección o con Escape
    {
        if (EstructuraSeleccionadaParaInteraccion == null) return;

        if (EstructuraSeleccionadaParaInteraccion.GetComponent<Building_Personajes>() != null)
        {
            LimpiarUICrisol();
        }
        else if (EstructuraSeleccionadaParaInteraccion.GetComponent<MonolitoBehaviour>() != null && PanelMonolito != null)
        {
            CerrarPanel(PanelMonolito);
        }
        else if (EstructuraSeleccionadaParaInteraccion.GetComponent<PersonajeBehaviour>() != null && PanelNPC != null)
        {
            CerrarPanel(PanelNPC);
        }
        // ... más lógica para otros paneles ...

        EstructuraSeleccionadaParaInteraccion = null;
    }
    
    private void ConfigurarUICrisol(Building_Personajes crisol)
    {
        if (crisol == null)
        {
            LimpiarUICrisol();
            return;
        }
        _crisolSeleccionadoActual = crisol; // Guardar referencia al crisol actual

        if (GenerarAldeanoBtn != null)
        {
            GenerarAldeanoBtn.onClick.RemoveAllListeners();
            GenerarAldeanoBtn.onClick.AddListener(() => _crisolSeleccionadoActual.IntentarGenerarPersonajeDesdeUI());
            // La interactividad del botón se manejará en un UpdateUI específico si es necesario, o siempre activo.
            GenerarAldeanoBtn.interactable = true; // O basarlo en condiciones del crisol
        }

        if (ToggleLegendario != null)
        {
            ToggleLegendario.onValueChanged.RemoveAllListeners();
            // Establecer el estado inicial del toggle basado en el estado interno del crisol
            // Necesitarías una forma de que Building_Personajes exponga su '_intentaGenerarLegendarioToggleState'
            // Por ejemplo, con una propiedad pública get: public bool IntentaGenerarLegendario => _intentaGenerarLegendarioToggleState;
            // ToggleLegendario.isOn = _crisolSeleccionadoActual.IntentaGenerarLegendario;
            ToggleLegendario.onValueChanged.AddListener((value) => {
                if (_crisolSeleccionadoActual != null) // Chequeo extra
                    _crisolSeleccionadoActual.SetIntentaGenerarLegendarioToggleState(value);
            });
            // La interactividad del toggle se maneja en Building_Personajes.AplicarConfiguracionDeNivelActual
            // Pero GameManager también podría forzarla si el crisol no puede generar legendarios aún.
            // ToggleLegendario.interactable = _crisolSeleccionadoActual._puedeGenerarLegendariosActual; (necesitaría acceso)
        }

        if (ActualizarCrisolBtn != null)
        {
            ActualizarCrisolBtn.onClick.RemoveAllListeners();
            ActualizarCrisolBtn.onClick.AddListener(() => _crisolSeleccionadoActual.IntentarActualizarEdificio());
            // La interactividad se podría basar en si el crisol puede mejorarse (recursos, nivel max)
            ActualizarCrisolBtn.interactable = true; // O basarlo en condiciones
        }

        if (panelControlCrisol != null)
        {
            AbrirPanel(panelControlCrisol);
        }
        // Aquí también podrías llamar a un método para actualizar cualquier texto en panelControlCrisol
        // que muestre información del _crisolSeleccionadoActual (ej. su nivel, producción, etc.)
        // UpdateTextosPanelCrisol(_crisolSeleccionadoActual);
    }

    private void LimpiarUICrisol()
    {
        if (GenerarAldeanoBtn != null) GenerarAldeanoBtn.onClick.RemoveAllListeners();
        if (ToggleLegendario != null) ToggleLegendario.onValueChanged.RemoveAllListeners();
        if (ActualizarCrisolBtn != null) ActualizarCrisolBtn.onClick.RemoveAllListeners();

        // Opcional: Desactivar botones si no hay crisol seleccionado
        if (GenerarAldeanoBtn != null) GenerarAldeanoBtn.interactable = false;
        if (ToggleLegendario != null) ToggleLegendario.interactable = false;
        if (ActualizarCrisolBtn != null) ActualizarCrisolBtn.interactable = false;


        if (panelControlCrisol != null)
        {
            CerrarPanel(panelControlCrisol);
        }
        _crisolSeleccionadoActual = null;
    }
    #endregion

    #region Configuración Automática de UI de Construcción (Modificado)
    private void ConfigurarElementosUIConstruccion()
    {
        if (listaConfiguracionArquitecturaUI == null || listaConfiguracionArquitecturaUI.Count == 0)
        {
            Debug.LogWarning("GameManager: 'listaConfiguracionArquitecturaUI' está vacía. No se configurarán elementos de UI de construcción.");
            return;
        }

        for (int i = 0; i < listaConfiguracionArquitecturaUI.Count; i++)
        {
            ConfiguracionEntradaArquitecturaUI entrada = listaConfiguracionArquitecturaUI[i];

            if (entrada.elementoRaizUI == null || entrada.datosDelEdificioSO == null || entrada.datosDelEdificioSO.prefabDelEdificio == null)
            {
                Debug.LogWarning($"GameManager: Entrada de arquitectura UI en el índice {i} (Nombre Editor: '{entrada.nombreEditor ?? "N/A"}') está incompleta (elementoRaizUI, datosDelEdificioSO o su prefab es nulo). Saltando.");
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
                    // Capturar 'edificioData' localmente para la clausura de la lambda
                    EdificioDataSO dataParaEsteBoton = edificioData;
                    boton.onClick.AddListener(() => IniciarConstruccionConDataSO(dataParaEsteBoton));
                    boton.interactable = true;
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

    private string FormatearCostosSimple(List<ConstructionCostEntry> costos)
    {
        if (costos == null || costos.Count == 0) return "Gratis";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < costos.Count; i++) {
            ConstructionCostEntry costo = costos[i];
            if (costo.resourceSO != null) {
                sb.Append($"{costo.resourceSO.Nombre}: {costo.amount.ToString("F0")}");
                if (i < costos.Count - 1) sb.Append(" | "); // Separador más corto
            }
        }
        return sb.ToString();
    }

    private void IniciarConstruccionConDataSO(EdificioDataSO edificioData)
    {
        if (edificioData == null || edificioData.prefabDelEdificio == null) return;
        if (BuildingManager.Instance != null) {
            BuildingManager.Instance.StartPlacementMode(edificioData.prefabDelEdificio);
            if (PanelArquitectura != null && PanelArquitectura.activeInHierarchy) {
                CerrarPanel(PanelArquitectura);
            }
        }
    }
    #endregion

    #region UI Management (Basic)
    // =================================================================================================================
    // GESTIÓN BÁSICA DE UI (MOVER A UIMANAGER)
    // =================================================================================================================
    public void ActualizarTextoUI(TextMeshProUGUI elementoUI, string nuevoTexto) // Renombrado y modificado
    {
        if (elementoUI != null)
        {
            elementoUI.text = nuevoTexto;
        }
        // else Debug.LogError("GameManager: ElementoUI (TextMeshProUGUI) es nulo en ActualizarTextoUI.");
        // La línea que reseteaba 'TodasLasEstructurasActivas' ha sido eliminada.
    }

    private void AbrirPanel(GameObject panel) { if (panel != null && !panel.activeInHierarchy) panel.SetActive(true); }
    private void CerrarPanel(GameObject panel) { if (panel != null && panel.activeInHierarchy) panel.SetActive(false); }

    // Ejemplo de método para un botón de la UI principal (como el de Arquitectura)
    public void TogglePanelArquitectura()
    {
        if (PanelArquitectura != null)
        {
            bool estabaActivo = PanelArquitectura.activeInHierarchy;
            DeseleccionarEstructuraActual(); // Buena práctica deseleccionar al abrir un menú principal
            if (BuildingManager.Instance != null && BuildingManager.Instance.IsInPlacementMode) BuildingManager.Instance.CancelPlacementMode();
            PanelArquitectura.SetActive(!estabaActivo);
        }
    }
    // Crear métodos similares para otros botones de paneles principales (Administración, Investigación, etc.)
    // y asignarlos a los onClick de los botones en el Inspector.
    #endregion

    #region Building Interaction & Deprecated/Placeholder Logic
    // =================================================================================================================
    // INTERACCIÓN CON EDIFICIOS Y LÓGICA ANTIGUA/PLACEHOLDER
    // (Gran parte de esto debería moverse a los scripts de los edificios o a sistemas dedicados)
    // =================================================================================================================

    // El método 'ComprarDado' ahora debería ser reemplazado por la interacción con BuildingManager
    // Ejemplo: Un botón en PanelArquitectura llama a BuildingManager.Instance.StartPlacementMode(prefabDelCrisol);
    public void Obsoleto_ComprarDado()
    {
        if (PrefabEstructuraAGenerica == null) { /* ... */ return; }
        if (BuildingManager.Instance != null)
        {
            // Aquí necesitarías lógica para que el BuildingManager sepa qué prefab usar,
            // o este método debería pasar el prefab directamente.
            // BuildingManager.Instance.StartPlacementMode(PrefabEstructuraAGenerica.GetComponent<BaseBuilding>());
            Debug.LogWarning("ComprarDado es obsoleto, usar BuildingManager.StartPlacementMode con un prefab específico.");
        }
    }

    // Este método es un ejemplo de cómo podrías interactuar con el Crisol de Almas
    // pero idealmente, la UI del Crisol llamaría directamente a los métodos de su script Building_Personajes.
    public void AccionComprarEdificioCrisol() // Renombrado de ComprarEdificio
    {
        if (CrisolDeAlmasGOPrefab == null)
        {
            Debug.LogError("GameManager: CrisolDeAlmasGOPrefab no asignado.");
            return;
        }

        // Esto instancia un nuevo Crisol. ¿Es esa la intención o interactuar con uno ya existente?
        // Si es para construirlo, debería usar el BuildingManager.
        // BuildingManager.Instance.StartPlacementMode(CrisolDeAlmasGOPrefab.GetComponent<BaseBuilding>());
        // La siguiente lógica parece ser para conectar una UI a un Crisol YA EXISTENTE o recién instanciado.

        // Ejemplo si quieres instanciar y conectar una UI específica (esto es un poco forzado aquí):
        // GameObject crisolInstanciado = Instantiate(CrisolDeAlmasGOPrefab, new Vector3(-5, 0, 0), Quaternion.identity);
        // Building_Personajes scriptCrisol = crisolInstanciado.GetComponent<Building_Personajes>();
        // if (scriptCrisol != null)
        // {
        //     scriptCrisol.ActivarEdificio();
        //     // Conectar botones (esto es MUY propenso a errores y acoplamiento fuerte, evitar si es posible)
        //     // Es mejor que el panel UI del Crisol tenga su propio script que obtenga la referencia
        //     // al Building_Personajes cuando se selecciona/abre el panel.
        //     if (GenerarAldeanoBtn != null) GenerarAldeanoBtn.onClick.AddListener(() => scriptCrisol.GenerarPersonaje(ToggleLegendario != null ? ToggleLegendario.isOn : false));
        //     if (ToggleLegendario != null) ToggleLegendario.onValueChanged.AddListener(scriptCrisol.SetIntentaGenerarLegendarioToggleState);
        //     if (ActualizarCrisolBtn != null) ActualizarCrisolBtn.onClick.AddListener(scriptCrisol.IntentarActualizarEdificio);
        //     Debug.Log("Crisol de Almas instanciado y UI (potencialmente) conectada.");
        // }
        Debug.LogWarning("GameManager.AccionComprarEdificioCrisol: Lógica de ejemplo, revisar e integrar con BuildingManager para construcción.");
    }
    #endregion

    #region Building List Management (Callbacks from BaseBuilding or BuildingManager)
    // =================================================================================================================
    // GESTIÓN DE LA LISTA DE EDIFICIOS ACTIVOS
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
// Archivo: MonolitoBehaviour.cs
using UnityEngine;
using System.Collections; // Para Coroutines
using System.Collections.Generic; // Para List
using TMPro;                // Para TextMeshProUGUI
using UnityEngine.EventSystems; // Para EventSystem.current

public class MonolitoBehaviour : MonoBehaviour
{
    #region Serialized Fields - Configuración del Inspector
    // =================================================================================================================
    // CAMPOS CONFIGURABLES DESDE EL INSPECTOR DE UNITY
    // =================================================================================================================

    [Header("Generación de Fragmentos")]
    [Tooltip("Costo base en Fe para el primer fragmento extraído.")]
    public float costoBaseFragmentoFe = 10f;
    [Tooltip("Factor por el cual se multiplica el costo de Fe para cada fragmento subsiguiente (ej. 1.2 = 20% más caro).")]
    public float incrementoCostoFragmento = 1.2f;

    [Header("Eventos Aleatorios (Singularidad)")]
    [Range(0f, 1f)]
    [Tooltip("Probabilidad (0-1) de que ocurra un evento especial o 'singularidad' (lógica no implementada).")]
    public float porcentajeSingularidad = 0.1f;

    [Header("Sistema de Niveles del Monolito")]
    [Tooltip("Referencia al ScriptableObject (RecurSO) que define la 'Fe'. Esencial para la lógica de niveles y costos.")]
    [SerializeField] private RecurSO feDataSO;
    [Tooltip("Referencia al ScriptableObject (RecurSO) que define los 'Fragmentos'.")]
    [SerializeField] private RecurSO fragmentoSO; // Nombre en mayúscula para consistencia con FeDataSO
    [Tooltip("Nivel actual de poder o influencia del Monolito. Se usa para calcular costos de mejora.")]
    public int nivelActualMonolito = 1; // Cambiado a int para niveles discretos

    [Header("Generación de Fe por Rezo de NPCs")]
    [Tooltip("Cuántas unidades de Fe se añaden al ResourceManager cuando el progreso de rezo de los NPCs llega al 100%.")]
    public float unidadesFePorCicloCompletoDeRezo = 1.0f;

    [Header("Visuales de Fe (Pilares)")]
    [Tooltip("Prefab del objeto Pilar que se instancia para representar la Fe visualmente.")]
    public GameObject pilarPrefab;
    [Tooltip("Radio mínimo desde el centro del Monolito donde pueden aparecer los pilares.")]
    public float radioMinimoAparicionPilar = 2f;
    [Tooltip("Radio máximo desde el centro del Monolito donde pueden aparecer los pilares.")]
    public float radioMaximoAparicionPilar = 5f;
    [Tooltip("Transform padre bajo el cual se instanciarán los pilares de Fe.")]
    public Transform contenedorPilaresFe; // Renombrado de FaithHolder

    [Header("Control de Partículas y Efectos Visuales")]
    [Tooltip("Referencia al script MonolitoParticles que controla los efectos visuales de partículas.")]
    public MonolitoParticles controladorDeParticulas;
    [Tooltip("Transform al que las partículas convergerán durante la extracción de fragmentos. Puede ser el propio Monolito.")]
    public Transform puntoConvergenciaParticulasMonolito;
    [Tooltip("Prefab del GameObject que se instanciará después de la convergencia de partículas durante la extracción de fragmento.")]
    public GameObject objetoAInstanciarPostConvergencia; // Renombrado de objetoAInstanciarPrefab
    [Tooltip("Transform que define la posición y rotación para instanciar el objeto post-convergencia. Si es nulo, se usará la posición del Monolito.")]
    public Transform puntoDeInstanciacionObjetoPostConvergencia; // Renombrado

    [Header("UI y Referencias Externas")]
    [Tooltip("Referencia al TextMeshProUGUI que muestra el costo actual para extraer un fragmento.")]
    public TextMeshProUGUI textoCostoFragmento; // Renombrado de CostoFragmentoBtnText
    [Tooltip("El GameObject (Panel UI, Texto, etc.) que se mostrará cuando este Monolito esté seleccionado.")]
    public GameObject panelOpcionInteraccion;
    [Tooltip("El GameObject (ej. un TextMeshProUGUI) que muestra el prompt 'Presiona F para Rezar' cuando el Monolito está seleccionado.")]
    public GameObject textoPromptRezar;
    [Tooltip("Radio para el Gizmo de depuración que se dibuja en el editor cuando el Monolito está seleccionado.")]
    public float radioGizmo = 5f;

    // 'EstructurasConectadas', 'Ticker' y 'TickerLimit' para lógica futura
    [Header("Interacción con Otras Estructuras (Futuro)")]
    public List<GameObject> estructurasConectadas;
    public float tickerActual = 0f; // Renombrado de Ticker
    public float limiteTicker = 5f; // Renombrado de TickerLimit
    #endregion

    #region Propiedades Públicas (Calculadas)
    // =================================================================================================================
    // PROPIEDADES PÚBLICAS CALCULADAS (SOLO LECTURA DESDE FUERA)
    // =================================================================================================================
    [Tooltip("Costo actual en Fe para extraer el siguiente fragmento.")]
    public float CostoActualFragmento { get; private set; }

    [Tooltip("Progreso actual (0-1) para generar la siguiente unidad de Fe a través del rezo de NPCs.")]
    public float ProgresoFePorRezoActual => _progresoFePorRezoActual;

    public float CostoParaSiguienteNivelMonolito // Renombrado de FeNecesariaParaSiguienteNivelMonolito
    {
        get
        {
            if (feDataSO != null && feDataSO.requiereNivel) // Usa la config de niveles del SO de Fe
            {
                return feDataSO.CalcularCostoSubirNivel(nivelActualMonolito);
            }
            return float.MaxValue; // No se puede subir de nivel si no está configurado
        }
    }
    #endregion

    #region Estado Interno Privado
    // =================================================================================================================
    // VARIABLES DE ESTADO INTERNO DEL MONOLITO
    // =================================================================================================================
    private int _contadorFragmentosExtraidos = 0; // Renombrado
    private bool _estaExtrayendoFragmento = false;
    [SerializeField] // Para verlo en Inspector (debug)
    private float _progresoFePorRezoActual = 0f; // Acumulador para la Fe por rezo

    private GameManager _gameManagerInstance;
    #endregion

    #region Unity Lifecycle Methods
    // =================================================================================================================
    // MÉTODOS DEL CICLO DE VIDA DE UNITY (AWAKE, START, UPDATE, ONDRAWGIZMOS)
    // =================================================================================================================
    private void Awake()
    {
        _gameManagerInstance = GameManager.Instance;
        if (_gameManagerInstance == null)
        {
            Debug.LogError($"MonolitoBehaviour ({gameObject.name}): GameManager.Instance no encontrado.");
        }
    }

    private void Start()
    {
        ValidarReferenciasDelInspector();
        if (panelOpcionInteraccion != null) panelOpcionInteraccion.SetActive(false);
        if (textoPromptRezar != null) // NUEVA LÓGICA EN START
        {
            textoPromptRezar.SetActive(false); // Asegurarse que el prompt esté oculto al inicio
        }
        ActualizarUICostoFragmento();
    }

    private void Update()
    {
        ManejarSeleccionEInteraccion();
        // Futuro: Lógica del Ticker para EstructurasConectadas
        // if (estructurasConectadas.Count > 0) {
        //     tickerActual += Time.deltaTime;
        //     if (tickerActual >= limiteTicker) {
        //         tickerActual = 0f;
        //         // Realizar acción con estructuras conectadas
        //     }
        // }
    }

    private void OnMouseDown() // Para la selección
    {
        if (_gameManagerInstance != null && EventSystem.current.IsPointerOverGameObject() == false)
        {
            _gameManagerInstance.SeleccionarEstructura(this.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.3f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radioGizmo);
        if (pilarPrefab != null) // Solo dibujar radios de pilares si hay prefab
        {
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, radioMinimoAparicionPilar);
            Gizmos.DrawWireSphere(transform.position, radioMaximoAparicionPilar);
        }
    }
    #endregion

    #region Métodos Públicos de Interacción y Gestión
    // =================================================================================================================
    // MÉTODOS PÚBLICOS PARA INTERACTUAR CON EL MONOLITO Y GESTIONAR SU ESTADO
    // =================================================================================================================

    /// <summary>
    /// Inicia el proceso ceremonial de extracción de un fragmento, si se cumplen los requisitos.
    /// Este método es llamado por la UI o una acción del jugador.
    /// </summary>
    public void IniciarProcesoExtraccionFragmento()
    {
        if (_estaExtrayendoFragmento) {
            Debug.Log("Monolito: Ya se está extrayendo un fragmento.");
            return;
        }
        if (!ValidarRecursosParaExtraccion()) return;

        StartCoroutine(ProcesoExtraerFragmentoCoroutine());
    }

    /// <summary>
    /// Intenta subir el nivel del Monolito, consumiendo los recursos necesarios.
    /// </summary>
    public void IntentarSubirNivelMonolito()
    {
        if (feDataSO == null || !feDataSO.requiereNivel || ResourceManager.Instance == null) {
            Debug.LogError("Monolito: Configuración de niveles o ResourceManager faltante.");
            return;
        }

        string recursoDeCosto = string.IsNullOrEmpty(feDataSO.recursoRequeridoParaNivel) ?
                                feDataSO.Nombre : feDataSO.recursoRequeridoParaNivel;
        float costoSubida = CostoParaSiguienteNivelMonolito;

        if (costoSubida == float.MaxValue) {
            Debug.Log("Monolito: Ya está al máximo nivel o no se puede subir más.");
            return;
        }

        if (ResourceManager.Instance.TieneSuficiente(recursoDeCosto, costoSubida))
        {
            ResourceManager.Instance.Gastar(recursoDeCosto, costoSubida);
            nivelActualMonolito++; // Incrementa el nivel
            Debug.Log($"Monolito: ¡Nivel subido a {nivelActualMonolito}! Costo: {costoSubida} de {recursoDeCosto}.");
            // Disparar evento OnMonolitoLevelUp(nivelActualMonolito);
            // Actualizar cualquier efecto que dependa del nivel
        }
        else {
            Debug.Log($"Monolito: Recursos insuficientes para subir nivel. Necesitas {costoSubida} de '{recursoDeCosto}'.");
        }
    }

    /// <summary>
    /// Llamado por un PersonajeBehaviour para contribuir al progreso de Fe del Monolito.
    /// </summary>
    public void RecibirContribucionDeRezo(float cantidadProgreso)
    {
        if (cantidadProgreso <= 0) return;
        _progresoFePorRezoActual += cantidadProgreso;

        while (_progresoFePorRezoActual >= 1.0f)
        {
            _progresoFePorRezoActual -= 1.0f;
            AñadirFeAlSistema(unidadesFePorCicloCompletoDeRezo); // Llama al método que añade Fe y genera pilares
            Debug.Log($"Monolito: ¡Unidad de Fe ({unidadesFePorCicloCompletoDeRezo}) generada por rezo! Progreso restante: {_progresoFePorRezoActual * 100:F1}%.");
        }
    }

    /// <summary>
    /// Acción principal que el jugador puede activar en el Monolito (ej. al presionar 'F').
    /// </summary>
    public void AccionPrincipalDelJugadorEnMonolito()
    {
        AñadirFeAlSistema(1.0f); // Ejemplo: el jugador genera 1 de Fe y sus pilares
        // Debug.Log("Monolito: Acción del jugador concedió 1 de Fe.");
    }
    #endregion

    #region Lógica Interna y Corutinas
    // =================================================================================================================
    // MÉTODOS PRIVADOS, CORUTINAS Y LÓGICA INTERNA
    // =================================================================================================================

    private void ValidarReferenciasDelInspector()
    {
        if (feDataSO == null) Debug.LogError($"Monolito ({gameObject.name}): FeDataSO no asignado.");
        if (fragmentoSO == null) Debug.LogError($"Monolito ({gameObject.name}): FragmentoSO no asignado.");
        if (pilarPrefab == null) Debug.LogError($"Monolito ({gameObject.name}): PilarPrefab no asignado.");
        if (contenedorPilaresFe == null) Debug.LogError($"Monolito ({gameObject.name}): ContenedorPilaresFe no asignado.");
        if (textoCostoFragmento == null) Debug.LogError($"Monolito ({gameObject.name}): TextoCostoFragmento no asignado.");
        if (controladorDeParticulas == null) Debug.LogWarning($"Monolito ({gameObject.name}): ControladorDeParticulas no asignado. Las animaciones de partículas no funcionarán.");
        if (puntoConvergenciaParticulasMonolito == null && controladorDeParticulas != null) Debug.LogWarning($"Monolito ({gameObject.name}): PuntoConvergenciaParticulasMonolito no asignado.");
        if (objetoAInstanciarPostConvergencia == null) Debug.LogWarning($"Monolito ({gameObject.name}): 'objetoAInstanciarPostConvergencia' no asignado.");
        if (panelOpcionInteraccion == null) Debug.LogWarning($"Monolito ({gameObject.name}): 'panelOpcionInteraccion' no asignado.");
        if (textoPromptRezar == null) Debug.LogWarning($"Monolito ({gameObject.name}): 'textoPromptRezar' no asignado.");
        if (textoCostoFragmento == null) Debug.LogWarning($"Monolito ({gameObject.name}): 'textoCostoFragmento' no asignado.");
    }

    // Modificamos ManejarSeleccionEInteraccion para incluir el nuevo texto
    private void ManejarSeleccionEInteraccion()
    {
        if (_gameManagerInstance == null) return; // No hacer nada si no hay GameManager

        bool estaSeleccionado = (_gameManagerInstance.EstructuraSeleccionadaParaInteraccion == this.gameObject);

        // Manejar visibilidad del panel de interacción general
        if (panelOpcionInteraccion != null)
        {
            if (estaSeleccionado && !panelOpcionInteraccion.activeInHierarchy)
            {
                panelOpcionInteraccion.SetActive(true);
            }
            else if (!estaSeleccionado && panelOpcionInteraccion.activeInHierarchy)
            {
                panelOpcionInteraccion.SetActive(false);
            }
        }

        // Manejar visibilidad del prompt "Presiona F para Rezar"
        if (textoPromptRezar != null) // NUEVA LÓGICA
        {
            if (estaSeleccionado && !textoPromptRezar.activeInHierarchy)
            {
                textoPromptRezar.SetActive(true);
            }
            else if (!estaSeleccionado && textoPromptRezar.activeInHierarchy)
            {
                textoPromptRezar.SetActive(false);
            }
        }

        // Manejar input si está seleccionado
        if (estaSeleccionado)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // AccionPrincipalDelJugadorEnMonolito actualmente añade Fe y genera pilares,
                // lo cual es una buena acción para "rezar".
                AccionPrincipalDelJugadorEnMonolito();
            }
            // Aquí podrías añadir otros inputs para otras acciones del Monolito
            // if (Input.GetKeyDown(KeyCode.U)) { IntentarSubirNivelMonolito(); }
            // if (Input.GetKeyDown(KeyCode.E)) { IniciarProcesoExtraccionFragmento(); } // Si 'E' es para extraer
        }
    }

    private bool ValidarRecursosParaExtraccion()
    {
        if (ResourceManager.Instance == null || feDataSO == null)
        {
            Debug.LogError("Monolito: ResourceManager o FeDataSO no asignados. No se puede verificar costo de extracción.");
            return false;
        }
        if (!ResourceManager.Instance.TieneSuficiente(feDataSO.Nombre, CostoActualFragmento))
        {
            // Debug.Log($"Monolito: Fe insuficiente para extraer fragmento. Necesitas: {CostoActualFragmento:F0}, Tienes: {ResourceManager.Instance.GetCantidad(feDataSO.Nombre):F0}");
            return false;
        }
        return true;
    }

    private IEnumerator ProcesoExtraerFragmentoCoroutine()
    {
        _estaExtrayendoFragmento = true;
        // Debug.Log("Monolito: Iniciando proceso de extracción de fragmento...");

        // 1. GASTAR FE
        ResourceManager.Instance.Gastar(feDataSO.Nombre, CostoActualFragmento);
        // Debug.Log($"Monolito: {CostoActualFragmento:F0} de Fe gastada para fragmento No. {_contadorFragmentosExtraidos + 1}.");

        // 2. ANIMACIÓN DE CONVERGENCIA DE PARTÍCULAS
        if (controladorDeParticulas != null && puntoConvergenciaParticulasMonolito != null) {
            // Debug.Log("Monolito: Iniciando convergencia de partículas...");
            Coroutine convergeCoroutine = controladorDeParticulas.CongelarYConvergerParticulas(puntoConvergenciaParticulasMonolito);
            if (convergeCoroutine != null) yield return convergeCoroutine;
            // Debug.Log("Monolito: Convergencia completada.");
        } else {
            // yield return new WaitForSeconds(1.5f); // Espera simbólica si no hay partículas
        }

        // 3. INSTANCIAR OBJETO POST-CONVERGENCIA
        if (objetoAInstanciarPostConvergencia != null) {
            Transform spawnPoint = (puntoDeInstanciacionObjetoPostConvergencia != null) ? puntoDeInstanciacionObjetoPostConvergencia : this.transform;
            Instantiate(objetoAInstanciarPostConvergencia, spawnPoint.position, spawnPoint.rotation);
            // Debug.Log($"Monolito: Objeto '{objetoAInstanciarPostConvergencia.name}' instanciado.");
        }

        // 4. AÑADIR FRAGMENTO
        if (ResourceManager.Instance != null && fragmentoSO != null) {
            ResourceManager.Instance.Añadir(fragmentoSO.Nombre, 1);
            _contadorFragmentosExtraidos++;
            // Debug.Log($"Monolito: ¡Fragmento No. {_contadorFragmentosExtraidos} obtenido! Total: {ResourceManager.Instance.GetCantidad(fragmentoSO.Nombre)}");
        } else {
            Debug.LogError("Monolito: ResourceManager o FragmentoSO no asignado. No se pudo añadir fragmento.");
        }

        // 5. ACTUALIZAR UI DEL COSTO
        ActualizarUICostoFragmento();

        // 6. ANIMACIÓN DE RETORNO DE PARTÍCULAS
        if (controladorDeParticulas != null) {
            // Debug.Log("Monolito: Iniciando retorno de partículas...");
            Coroutine returnCoroutine = controladorDeParticulas.DevolverParticulasASuOrigen();
            if (returnCoroutine != null) yield return returnCoroutine;
            // Debug.Log("Monolito: Retorno de partículas completado.");
        }
        // else { yield return new WaitForSeconds(0.5f); }

        // Debug.Log("Monolito: Proceso de extracción de fragmento finalizado.");
        _estaExtrayendoFragmento = false;
    }

    private void ActualizarUICostoFragmento()
    {
        CostoActualFragmento = costoBaseFragmentoFe * Mathf.Pow(incrementoCostoFragmento, _contadorFragmentosExtraidos);
        if (textoCostoFragmento != null) {
            textoCostoFragmento.text = $"Extraer Fragmento ({CostoActualFragmento.ToString("F0")} {feDataSO?.Nombre ?? "Fe"})";
        }
    }

    // Este método centraliza la adición de Fe (por rezo, acción del jugador, etc.) y la generación de pilares.
    private void AñadirFeAlSistema(float cantidad) // Era público, pero parece más un helper interno ahora. Hacerlo público si es necesario.
    {
        if (feDataSO == null || ResourceManager.Instance == null || cantidad <= 0) return;
        ResourceManager.Instance.Añadir(feDataSO.Nombre, cantidad);
        int pilaresAGenerar = Mathf.FloorToInt(cantidad) * 3;
        for(int i=0; i < pilaresAGenerar; i++) GenerarPilar();
    }

    public void GenerarPilar() // Hecho público por si algo externo necesita forzarlo, aunque usualmente es interno.
    {
        if (pilarPrefab == null || contenedorPilaresFe == null) {
            // Debug.LogError("Monolito: PilarPrefab o ContenedorPilaresFe no asignado.");
            return;
        }
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distancia = Random.Range(radioMinimoAparicionPilar, radioMaximoAparicionPilar);
        float x = transform.position.x + Mathf.Cos(angulo) * distancia;
        float z = transform.position.z + Mathf.Sin(angulo) * distancia;
        float randomYOffset = Random.Range(-0.3f, 0.5f);
        float initialYPos = transform.position.y -1.5f; // Posición inicial de emergencia más abajo

        Vector3 posicionFinalPilar = new Vector3(x, transform.position.y + randomYOffset, z);
        Vector3 posicionInicialPilar = new Vector3(x, initialYPos, z);

        GameObject pilarObj = Instantiate(pilarPrefab, posicionInicialPilar, Quaternion.identity, contenedorPilaresFe);
        pilarObj.name = "PilarFe_" + contenedorPilaresFe.childCount;
        PilarBehaviour pilarScript = pilarObj.GetComponent<PilarBehaviour>();
        if (pilarScript != null) {
            pilarScript.PosicionObjetivoAlSubir = posicionFinalPilar;
            pilarScript.EstablecerEstadoBajada(false);
        } else Debug.LogWarning("Prefab de Pilar no tiene script PilarBehaviour.");
    }
    #endregion
}
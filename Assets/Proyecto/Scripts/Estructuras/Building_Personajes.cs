using UnityEngine;
using System.Collections.Generic; // Necesario para List
using UnityEngine.UI;             // Necesario para Button y Toggle

// Define la configuración para cada nivel del edificio generador de personajes.
[System.Serializable] // Para que se muestre en el Inspector dentro de la lista.
public struct ConfiguracionNivelEdificioPersonaje
{
    [Header("Parámetros de Generación EN este Nivel")]
    [Tooltip("Tiempo en segundos entre cada intento de generación de personaje.")]
    public float tiempoEntreGeneraciones;
    [Tooltip("Máximo número de personajes que este edificio puede haber generado (o mantener) mientras está en este nivel.")]
    public int maxPersonajesPermitidos; // Límite de generación para este nivel.
    [Tooltip("Costo en Fe para generar un NPC normal en este nivel.")]
    public float costoFeGeneracionNormal;
    [Tooltip("Costo en Fragmentos para generar un NPC legendario en este nivel (si aplica).")]
    public float costoFragmentoGeneracionLegendario;

    [Header("Costos para MEJORAR AL SIGUIENTE Nivel (desde este nivel)")]
    [Tooltip("Costo en Fe para MEJORAR el edificio DESDE este nivel AL SIGUIENTE. Si es el último nivel, puede ser 0 o no usarse.")]
    public float costoMejoraFeAlSiguienteNivel;
    [Tooltip("Costo en Fragmentos para MEJORAR el edificio DESDE este nivel AL SIGUIENTE.")]
    public float costoMejoraFragmentoAlSiguienteNivel;
    [Tooltip("Herramientas/recursos adicionales necesarios para MEJORAR DESDE este nivel AL SIGUIENTE.")]
    public Building_Personajes.ObjetoProfesion[] costoMejoraHerramientasAlSiguienteNivel; // Usamos la struct ObjetoProfesion

    [Header("Desbloqueos AL LLEGAR a este Nivel")]
    [Tooltip("¿Se desbloquea/requiere el uso de objetos de profesión al llegar a este nivel?")]
    public bool requiereObjetosProfesionAlLlegar;
    [Tooltip("¿Se desbloquea la generación de legendarios al llegar a este nivel?")]
    public bool desbloqueaLegendariosAlLlegar;
}

public class Building_Personajes : BaseBuilding
{
    [Header("Configuración General de Generación")]
    [Tooltip("Define el tipo de NPC normal que este edificio genera.")]
    [SerializeField] private NPCDataSO tipoPersonajeAGenerar;
    [Tooltip("Punto específico (Transform) donde se instanciarán los NPCs generados.")]
    [SerializeField] private Transform puntoDeGeneracion;
    [Tooltip("ScriptableObject que define el recurso 'Fe'.")]
    [SerializeField] private RecurSO feDataSO;
    [Tooltip("ScriptableObject que define el recurso 'Fragmento'.")]
    [SerializeField] private RecurSO fragmentoDataSO;
    [Tooltip("Define el tipo de NPC legendario que este edificio puede generar.")]
    [SerializeField] private NPCDataSO tipoPersonajeLegendario;
    [Tooltip("Lista de objetos/recursos y sus cantidades necesarias para generar NPCs con profesión (si la funcionalidad está activa para el nivel).")]
    [SerializeField] private List<ObjetoProfesion> objetosProfesionParaGeneracion; // Requisitos para *generar* si 'requiereObjetosProfesion' está activo.


    [Header("Configuración de Niveles del Edificio (Usando ScriptableObjects)")]
    [Tooltip("Arrastra aquí los ScriptableObjects que definen cada nivel de este edificio, en orden.")]
    [SerializeField] private List<NivelEdificioPersonajeSO> progresionDeNiveles;

    // Estado actual del edificio
    private int nivelActualEdificio = 1; // Nivel actual, empieza en 1.
    private int personajesGeneradosCount = 0; // Contador de personajes generados por este edificio.
    private float tiempoTranscurridoDesdeUltimaGeneracion;
    private bool _puedeGenerarLegendariosActual = false; // Estado actual basado en el nivel.
    private bool _requiereObjetosProfesionActual = false; // Estado actual basado en el nivel.
    private bool _intentaGenerarLegendarioToggleState = false; // Estado del toggle de la UI.

    // --- Propiedades calculadas basadas en el nivel actual y el SO correspondiente ---
    private NivelEdificioPersonajeSO ConfigSOActual
    {
        get
        {
            if (progresionDeNiveles == null || progresionDeNiveles.Count == 0 ||
                nivelActualEdificio < 1 || nivelActualEdificio > progresionDeNiveles.Count)
            {
                Debug.LogError($"Intento de acceder a configuración de nivel inválida. Nivel: {nivelActualEdificio}, Total SOs: {progresionDeNiveles?.Count ?? 0}");
                return null; // O un SO por defecto si tienes uno.
            }
            return progresionDeNiveles[nivelActualEdificio - 1]; // Índice 0 para Nivel 1.
        }
    }


    // Evento que se dispara cuando un nuevo personaje es generado.
    public delegate void OnPersonajeGeneradoAction(GameObject nuevoPersonaje, Building_Personajes generador);
    public static event OnPersonajeGeneradoAction PersonajeGeneradoGlobal;
    public event OnPersonajeGeneradoAction OnEsteEdificioGeneroPersonaje;

    // Struct para definir requisitos de objetos/recursos.
    [System.Serializable]
    public struct ObjetoProfesion
    {
        [Tooltip("Nombre del recurso/objeto requerido (debe coincidir con un nombre en ResourceManager).")]
        public string nombreObjeto; // Sugerencia: Usar RecurSO aquí también para evitar errores de tipeo.
        [Tooltip("Cantidad necesaria de este objeto/recurso.")]
        public int cantidadNecesaria;
    }

    [Header("Referencias de la UI (Asignar en Inspector)")]
    public Button generarPersonajeButton;
    public Toggle generarLegendarioToggle;
    public Button mejorarEdificioButton; // Botón para mejorar el edificio.

    // Ahora estas propiedades leen del ConfigSOActual
    private float TiempoEntreGeneracionesActual => ConfigSOActual?.tiempoEntreGeneraciones ?? float.MaxValue;
    private int MaxPersonajesPermitidosActual => ConfigSOActual?.maxPersonajesPermitidos ?? 0;
    private float CostoFeGeneracionNormalActual => ConfigSOActual?.costoFeGeneracionNormal ?? float.MaxValue;
    private float CostoFragmentoGeneracionLegendarioActual => ConfigSOActual?.costoFragmentoGeneracionLegendario ?? float.MaxValue;


    protected override void Start()
    {
        base.Start();

        // Validaciones de configuración esencial.
        if (puntoDeGeneracion == null) Debug.LogError($"Edificio '{buildingName}': PuntoDeGeneracion no asignado.");
        if (feDataSO == null) Debug.LogError($"Edificio '{buildingName}': FeDataSO no asignado.");
        if (tipoPersonajeAGenerar == null) Debug.LogError($"Edificio '{buildingName}': TipoPersonajeAGenerar (normal) no asignado.");
        if (fragmentoDataSO == null) Debug.LogWarning($"Edificio '{buildingName}': FragmentoDataSO no asignado (necesario para legendarios y algunas mejoras).");
        if (tipoPersonajeLegendario == null) Debug.LogWarning($"Edificio '{buildingName}': TipoPersonajeLegendario no asignado.");

        if (progresionDeNiveles == null || progresionDeNiveles.Count == 0)
        {
            Debug.LogError($"Edificio '{buildingName}': ¡No hay ScriptableObjects de configuración de nivel asignados! El edificio no funcionará.");
            isActive = false;
            return;
        }

        AplicarConfiguracionDeNivelActual(); // Aplica los parámetros del nivel 1 y desbloqueos.

        // Configuración de la UI.
        if (generarPersonajeButton != null)
        {
            generarPersonajeButton.onClick.AddListener(IntentarGenerarPersonajeDesdeUI);
        } else Debug.LogError($"Edificio '{buildingName}': generarPersonajeButton no asignado.");

        if (generarLegendarioToggle != null)
        {
            generarLegendarioToggle.onValueChanged.AddListener(SetIntentaGenerarLegendarioToggleState);
            generarLegendarioToggle.isOn = _intentaGenerarLegendarioToggleState;
            generarLegendarioToggle.interactable = _puedeGenerarLegendariosActual; // Basado en el desbloqueo del nivel.
        } else Debug.LogError($"Edificio '{buildingName}': generarLegendarioToggle no asignado.");

        if (mejorarEdificioButton != null) {
            mejorarEdificioButton.onClick.AddListener(IntentarActualizarEdificio);
        } else Debug.LogWarning($"Edificio '{buildingName}': mejorarEdificioButton no asignado (mejora manual no disponible).");


        tiempoTranscurridoDesdeUltimaGeneracion = TiempoEntreGeneracionesActual; // Para permitir generación inmediata si es posible.
        UpdateUIInteractables();
    }

    protected override void Update()
    {
        base.Update();
        if (!isActive || progresionDeNiveles == null || progresionDeNiveles.Count == 0) return;

        tiempoTranscurridoDesdeUltimaGeneracion += Time.deltaTime;
        UpdateGenerarButtonInteractable();
    }

    private void IntentarGenerarPersonajeDesdeUI()
    {
        GenerarPersonaje(_intentaGenerarLegendarioToggleState);
    }

    public void GenerarPersonaje(bool quiereLegendario)
    {
        if (!isActive) return;
        // ... (Validaciones de cooldown, límite de personajes)
        if (tiempoTranscurridoDesdeUltimaGeneracion < TiempoEntreGeneracionesActual) { /* ... log y return ... */ return; }
        if (personajesGeneradosCount >= MaxPersonajesPermitidosActual) { /* ... log y return ... */ return; }

        if (_requiereObjetosProfesionActual && !CumplirRequisitosObjetosProfesionParaGeneracion())
        {
            Debug.Log($"Edificio '{buildingName}': No se cumplen los requisitos de objetos de profesión para generar.");
            return;
        }

        NPCDataSO soParaGenerar = null;
        RecurSO recursoDeCostoSO = null;
        float costoDelRecurso = 0f;
        string descripcionTipoPersonaje = "";
        bool costosConfigurados = false;

        // Intento Legendario
        if (quiereLegendario && _puedeGenerarLegendariosActual && tipoPersonajeLegendario != null && fragmentoDataSO != null)
        {
            if (ResourceManager.Instance.TieneSuficiente(fragmentoDataSO.Nombre, CostoFragmentoGeneracionLegendarioActual))
            {
                soParaGenerar = tipoPersonajeLegendario;
                recursoDeCostoSO = fragmentoDataSO;
                costoDelRecurso = CostoFragmentoGeneracionLegendarioActual;
                descripcionTipoPersonaje = "legendario";
                costosConfigurados = true;
            } else {
                Debug.Log($"No hay suficientes '{fragmentoDataSO.Nombre}' para legendario. Intentando normal...");
            }
        }

        // Intento Normal (o fallback)
        if (!costosConfigurados)
        {
            if (tipoPersonajeAGenerar != null && feDataSO != null)
            {
                if (ResourceManager.Instance.TieneSuficiente(feDataSO.Nombre, CostoFeGeneracionNormalActual))
                {
                    soParaGenerar = tipoPersonajeAGenerar;
                    recursoDeCostoSO = feDataSO;
                    costoDelRecurso = CostoFeGeneracionNormalActual;
                    descripcionTipoPersonaje = "normal";
                    costosConfigurados = true;
                } else {
                    Debug.Log($"No hay suficientes '{feDataSO.Nombre}' para normal.");
                    return;
                }
            } else {
                Debug.LogError($"Edificio '{buildingName}': Configuración para normal (tipo o FeSO) es nula.");
                return;
            }
        }

        if (!costosConfigurados || soParaGenerar == null || recursoDeCostoSO == null) {
            Debug.LogError($"Edificio '{buildingName}': Error crítico, no se pudo determinar personaje/costo. Abortando.");
            return;
        }

        // Consumir recursos
        if (_requiereObjetosProfesionActual) {
            foreach (ObjetoProfesion req in objetosProfesionParaGeneracion) {
                ResourceManager.Instance.Gastar(req.nombreObjeto, req.cantidadNecesaria);
            }
        }
        ResourceManager.Instance.Gastar(recursoDeCostoSO.Nombre, costoDelRecurso);
        Debug.Log($"Gastados {costoDelRecurso} '{recursoDeCostoSO.Nombre}' para generar {descripcionTipoPersonaje}.");

        // Instanciar
        GameObject nuevoPersonajeObj = Instantiate(soParaGenerar.prefabModelo, puntoDeGeneracion.position, puntoDeGeneracion.rotation);
        personajesGeneradosCount++;
        tiempoTranscurridoDesdeUltimaGeneracion = 0f;

        PersonajeBehaviour pb = nuevoPersonajeObj.GetComponent<PersonajeBehaviour>();
        if (pb != null) {
            pb.Inicializar(soParaGenerar);
            Debug.Log($"Generado '{pb.nombre}' ({descripcionTipoPersonaje}). Total: {personajesGeneradosCount}");
        } else {
            Debug.LogError($"Prefab '{soParaGenerar.prefabModelo.name}' no tiene PersonajeBehaviour.");
        }

        PersonajeGeneradoGlobal?.Invoke(nuevoPersonajeObj, this);
        OnEsteEdificioGeneroPersonaje?.Invoke(nuevoPersonajeObj, this);
        UpdateUIInteractables();
    }

    private bool CumplirRequisitosObjetosProfesionParaGeneracion()
    {
        if (!_requiereObjetosProfesionActual || objetosProfesionParaGeneracion == null || objetosProfesionParaGeneracion.Count == 0) return true;
        if (ResourceManager.Instance == null) return false;
        foreach (ObjetoProfesion req in objetosProfesionParaGeneracion) {
            if (!ResourceManager.Instance.TieneSuficiente(req.nombreObjeto, req.cantidadNecesaria)) return false;
        }
        return true;
    }

    // Modificar la firma para aceptar List<ObjetoProfesion>
    private bool CumplirRequisitosHerramientasParaMejora(List<ObjetoProfesion> herramientasNecesarias)
    {
        // Ahora 'herramientasNecesarias' es una lista.
        // La lógica interna para iterar puede seguir igual usando .Count en lugar de .Length.
        if (herramientasNecesarias == null || herramientasNecesarias.Count == 0)
        {
            return true; // No se necesitan herramientas.
        }
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager.Instance no encontrado. No se pueden verificar requisitos de herramientas.");
            return false;
        }

        foreach (ObjetoProfesion requisito in herramientasNecesarias) // Funciona igual para List
        {
            if (requisito.cantidadNecesaria > 0 && !string.IsNullOrEmpty(requisito.nombreObjeto))
            {
                if (!ResourceManager.Instance.TieneSuficiente(requisito.nombreObjeto, requisito.cantidadNecesaria))
                {
                    return false; // Falta alguna herramienta/recurso.
                }
            }
        }
        return true; // Se cumplen todos los requisitos.
    }

    public void IntentarActualizarEdificio()
    {
        if (progresionDeNiveles == null || progresionDeNiveles.Count == 0) return;
        int nivelMaximoReal = progresionDeNiveles.Count;
        if (nivelActualEdificio >= nivelMaximoReal)
        {
            Debug.Log($"Edificio '{buildingName}' ya está al nivel máximo ({nivelMaximoReal}).");
            return;
        }

        // Los costos para mejorar AL SIGUIENTE nivel están en la configuración (ScriptableObject) DEL NIVEL ACTUAL.
        NivelEdificioPersonajeSO configActualParaMejoraSO = ConfigSOActual; // <--- CORREGIDO

        // Luego, asegúrate de que estás accediendo a los campos correctos de este SO:
        if (configActualParaMejoraSO == null)
        {
            Debug.LogError("No se pudo obtener la configuración del nivel actual para la mejora.");
            return;
        }

        float costoFeMejora = configActualParaMejoraSO.costoMejoraFeAlSiguienteNivel;
        float costoFragmentoMejora = configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel;
        // Asumiendo que 'costoMejoraHerramientasAlSiguienteNivel' es una List<ObjetoProfesion> en NivelEdificioPersonajeSO
        List<Building_Personajes.ObjetoProfesion> herramientasNecesariasMejora = configActualParaMejoraSO.costoMejoraHerramientasAlSiguienteNivel;

        if (feDataSO == null) { Debug.LogError("FeDataSO no asignado, no se puede mejorar."); return; }
        // El fragmentoDataSO podría ser opcional si el costo es 0.

        bool tieneFe = ResourceManager.Instance.TieneSuficiente(feDataSO.Nombre, configActualParaMejoraSO.costoMejoraFeAlSiguienteNivel);
        bool tieneFragmentos = (configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel <= 0) ||
                               (fragmentoDataSO != null && ResourceManager.Instance.TieneSuficiente(fragmentoDataSO.Nombre, configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel));
        bool cumpleHerramientas = CumplirRequisitosHerramientasParaMejora(configActualParaMejoraSO.costoMejoraHerramientasAlSiguienteNivel);

        if (tieneFe && tieneFragmentos && cumpleHerramientas)
        {
            ResourceManager.Instance.Gastar(feDataSO.Nombre, configActualParaMejoraSO.costoMejoraFeAlSiguienteNivel);
            if (configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel > 0 && fragmentoDataSO != null)
            {
                ResourceManager.Instance.Gastar(fragmentoDataSO.Nombre, configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel);
            }
            foreach (ObjetoProfesion herramienta in configActualParaMejoraSO.costoMejoraHerramientasAlSiguienteNivel)
            {
                if (herramienta.cantidadNecesaria > 0 && !string.IsNullOrEmpty(herramienta.nombreObjeto))
                    ResourceManager.Instance.Gastar(herramienta.nombreObjeto, herramienta.cantidadNecesaria);
            }

            nivelActualEdificio++;
            AplicarConfiguracionDeNivelActual();
            Debug.Log($"¡Edificio '{buildingName}' actualizado a Nivel {nivelActualEdificio}!");
            UpdateUIInteractables();
        }
        else
        {
            Debug.Log($"Recursos insuficientes para mejorar '{buildingName}' a Nivel {nivelActualEdificio + 1}.");
        }
    }

    private void AplicarConfiguracionDeNivelActual()
    {
        if (progresionDeNiveles == null || nivelActualEdificio < 1 || nivelActualEdificio > progresionDeNiveles.Count) {
            Debug.LogError($"Nivel ({nivelActualEdificio}) fuera de rango o config no definida para '{buildingName}'.");
            isActive = false;
            return;
        }

        // Aplicar desbloqueos del nivel actual
        if (ConfigSOActual.requiereObjetosProfesionAlLlegar && !_requiereObjetosProfesionActual) {
            _requiereObjetosProfesionActual = true;
            Debug.Log($"Edificio '{buildingName}' (Nivel {nivelActualEdificio}): Ahora requiere objetos de profesión para generar.");
        }
        if (ConfigSOActual.desbloqueaLegendariosAlLlegar && !_puedeGenerarLegendariosActual) {
            _puedeGenerarLegendariosActual = true;
            if (generarLegendarioToggle != null) generarLegendarioToggle.interactable = true;
            Debug.Log($"Edificio '{buildingName}' (Nivel {nivelActualEdificio}): ¡Generación de legendarios DESBLOQUEADA!");
        }

        // Resetea el contador de personajes si el límite es POR NIVEL y no acumulativo.
        // personajesGeneradosCount = 0; // Descomentar si el límite es por nivel.
        tiempoTranscurridoDesdeUltimaGeneracion = TiempoEntreGeneracionesActual; // Permitir generar de nuevo.

        Debug.Log($"Edificio '{buildingName}' configurado para Nivel {nivelActualEdificio}. " +
                  $"Tiempo Gen: {TiempoEntreGeneracionesActual}s, " +
                  $"Max NPCs: {MaxPersonajesPermitidosActual}, " +
                  $"Costo Fe Normal: {CostoFeGeneracionNormalActual}, " +
                  $"Costo Frag Legend: {CostoFragmentoGeneracionLegendarioActual}");

        UpdateUIInteractables(); // Actualizar botones después de aplicar el nivel.
    }

    private bool CumplirRequisitosHerramientasParaMejora(ObjetoProfesion[] herramientasNecesarias)
    {
        if (herramientasNecesarias == null || herramientasNecesarias.Length == 0) return true;
        if (ResourceManager.Instance == null) return false;
        foreach (ObjetoProfesion req in herramientasNecesarias) {
            if (req.cantidadNecesaria > 0 && !string.IsNullOrEmpty(req.nombreObjeto)) {
                if (!ResourceManager.Instance.TieneSuficiente(req.nombreObjeto, req.cantidadNecesaria)) return false;
            }
        }
        return true;
    }

    private void UpdateUIInteractables()
    {
        UpdateGenerarButtonInteractable();
        UpdateMejorarButtonInteractable();
    }

    private void UpdateGenerarButtonInteractable()
    {
        if (generarPersonajeButton != null && isActive && progresionDeNiveles != null && progresionDeNiveles.Count >0) {
            bool enCooldown = tiempoTranscurridoDesdeUltimaGeneracion < TiempoEntreGeneracionesActual;
            bool limiteAlcanzado = personajesGeneradosCount >= MaxPersonajesPermitidosActual;
            bool recursosSuficientes = (ResourceManager.Instance != null && feDataSO != null && ResourceManager.Instance.TieneSuficiente(feDataSO.Nombre, CostoFeGeneracionNormalActual));
            if (_intentaGenerarLegendarioToggleState && _puedeGenerarLegendariosActual && fragmentoDataSO != null) { // Si intenta legendario, chequear esos recursos
                recursosSuficientes = (ResourceManager.Instance != null && ResourceManager.Instance.TieneSuficiente(fragmentoDataSO.Nombre, CostoFragmentoGeneracionLegendarioActual));
            }
            generarPersonajeButton.interactable = !enCooldown && !limiteAlcanzado && recursosSuficientes && (_requiereObjetosProfesionActual ? CumplirRequisitosObjetosProfesionParaGeneracion() : true) ;
        } else if (generarPersonajeButton != null) {
            generarPersonajeButton.interactable = false;
        }
    }

    private void UpdateMejorarButtonInteractable(){
        if(mejorarEdificioButton != null && isActive && progresionDeNiveles != null && progresionDeNiveles.Count >0){
            bool esNivelMaximo = nivelActualEdificio >= progresionDeNiveles.Count;
            mejorarEdificioButton.interactable = !esNivelMaximo;
            // Podrías añadir aquí una comprobación de si se tienen los recursos para mejorar,
            // para que el botón se active/desactive dinámicamente.
            if(!esNivelMaximo){
                NivelEdificioPersonajeSO configActualParaMejoraSO = ConfigSOActual; // <--- CORREGIDO

                if (configActualParaMejoraSO == null) { // Añadir un chequeo de nulidad por si ConfigSOActual falla
                    if (mejorarEdificioButton != null) mejorarEdificioButton.interactable = false;
                    return;
                }

                // Ahora usas configActualParaMejoraSO para acceder a los costos
                bool tieneFe = ResourceManager.Instance.TieneSuficiente(feDataSO.Nombre, configActualParaMejoraSO.costoMejoraFeAlSiguienteNivel);
                bool tieneFragmentos = (configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel <= 0) ||
                                    (fragmentoDataSO != null && ResourceManager.Instance.TieneSuficiente(fragmentoDataSO.Nombre, configActualParaMejoraSO.costoMejoraFragmentoAlSiguienteNivel));
                // Asumiendo que costoMejoraHerramientasAlSiguienteNivel es una List y el método CumplirRequisitos... espera List o la conviertes
                bool cumpleHerramientas = CumplirRequisitosHerramientasParaMejora(configActualParaMejoraSO.costoMejoraHerramientasAlSiguienteNivel);
                mejorarEdificioButton.interactable = tieneFe && tieneFragmentos && cumpleHerramientas;
            }

        } else if (mejorarEdificioButton != null) {
            mejorarEdificioButton.interactable = false;
        }
    }

    public void SetIntentaGenerarLegendarioToggleState(bool value)
    {
        _intentaGenerarLegendarioToggleState = value;
        UpdateGenerarButtonInteractable(); // Actualizar el botón cuando cambia el toggle.
    }

    void OnMouseOver()
    {
        if (GameManager.Instance == null) return;
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.EstructuraSeleccionada == null) {
            GameManager.Instance.EstructuraSeleccionada = this.gameObject;
            Debug.Log($"Edificio '{buildingName}' seleccionado.");
            // UIManager.Instance.AbrirPanelGeneradorPersonajes(this);
        }
    }
}
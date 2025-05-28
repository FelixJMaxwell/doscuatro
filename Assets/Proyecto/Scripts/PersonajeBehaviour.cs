using UnityEngine;
using System.Collections.Generic;
using System; // Necesario para List y Dictionary

// Estados posibles para el NPC
public enum EstadoNPC
{
    Ocioso,
    MoviendoseAlTrabajo,
    Trabajando,
    MoviendoseACasa,
    EnCasa,
    MoviendoseAlMonolito, // NUEVO ESTADO
    RezandoAlMonolito    // NUEVO ESTADO
}

// Representa el comportamiento y estado de un NPC (Personaje No Jugador) en el juego.
public class PersonajeBehaviour : MonoBehaviour
{
    // --- Propiedades de Identidad (privadas con getter público) ---
    [Header("Identidad del Personaje")] // Aunque sean private set, un Header ayuda en el Inspector si se muestran con [SerializeField]
    [SerializeField] // Para ver en Inspector (debug) aunque el set sea privado.
    private string _nombre; // Convención: _prefijo para campos privados.
    public string nombre { get { return _nombre; } private set { _nombre = value; } }

    [SerializeField]
    private int _edad;
    public int edad { get { return _edad; } private set { _edad = value; } }

    [SerializeField]
    private string _genero;
    public string genero { get { return _genero; } private set { _genero = value; } }

    [SerializeField]
    private bool _esLegendario;
    public bool esLegendario { get { return _esLegendario; } private set { _esLegendario = value; } }

    // --- Propiedades de Estado ---
    [Header("Estado del Personaje")]
    [SerializeField]
    private float _salud;
    public float salud { get { return _salud; } private set { _salud = value; } }

    [SerializeField]
    private string _ropa;
    public string ropa { get { return _ropa; } private set { _ropa = value; } }

    [SerializeField]
    private string _herramienta;
    public string herramienta { get { return _herramienta; } private set { _herramienta = value; } }

    [SerializeField]
    [Range(0, 100)] // Asumiendo que felicidad va de 0 a 100.
    private int _felicidad;
    public int felicidad { get { return _felicidad; } private set { _felicidad = value; } }

    // --- Ocupación ---
    [Header("Ocupación del Personaje")]
    [SerializeField]
    private string _trabajo;
    public string trabajo { get { return _trabajo; } private set { _trabajo = value; } }

    [SerializeField]
    private string _educacion;
    public string educacion { get { return _educacion; } private set { _educacion = value; } }

    // --- Vivienda ---
    [Header("Vivienda")]
    [SerializeField] // Para ver en Inspector (debug)
    private Building_Casa _casaAsignada;
    public Building_Casa casaAsignada { get { return _casaAsignada; } private set { _casaAsignada = value; } }

    // --- Inventario ---
    // Si el inventario va a contener items complejos, considera una clase/struct Item en lugar de GameObject.
    // O si son prefabs de items, GameObject está bien.
    [Header("Inventario")]
    public List<GameObject> inventario = new List<GameObject>(); // Podría ser privado con métodos Add/Remove.

    // --- Relaciones ---
    // Diccionario para almacenar el nivel de relación con otros PersonajeBehaviour.
    // No es serializable por defecto si PersonajeBehaviour es un MonoBehaviour, lo que significa
    // que no se verá en el Inspector y no se guardará fácilmente con la serialización de Unity.
    // Para debugging, podrías necesitar una representación visual custom o una lista de structs serializables.
    private Dictionary<PersonajeBehaviour, int> relaciones = new Dictionary<PersonajeBehaviour, int>();

    // Evento que se puede disparar cuando el personaje muere. Otros sistemas pueden suscribirse.
    public static event System.Action<PersonajeBehaviour> OnPersonajeMuerto;

    [Header("Identidad del Personaje")]
    // Podríamos añadir una estadística de "Fe Individual" para el NPC
    [SerializeField][Range(0, 100)] private int _feIndividual = 50;
    public int FeIndividual => _feIndividual;


    [Header("Ocupación del Personaje")]
    [SerializeField] private string _trabajoDefinido;
    public string TrabajoDefinido => _trabajoDefinido;

    private Dictionary<PersonajeBehaviour, int> _relaciones = new Dictionary<PersonajeBehaviour, int>();


    // --- Máquina de Estados (FSM) ---
    [Header("Estado Actual (FSM)")]
    [SerializeField]
    private EstadoNPC _estadoActualNPC = EstadoNPC.Ocioso;
    public EstadoNPC EstadoActualNPC => _estadoActualNPC;

    private BaseBuilding _edificioDeTrabajoActual = null;
    private MonolitoBehaviour _monolitoObjetivoRezo = null; // NUEVA VARIABLE para el objetivo del rezo
    private float _timerEstadoActual = 0f;
    private float _duracionRezo = 10f; // Ejemplo: cuánto tiempo reza el NPC

    // --- Eventos ---
    public static event Action<PersonajeBehaviour> OnPersonajeMuertoGlobal;
    public event Action<PersonajeBehaviour> OnEstePersonajeMuerto;
    public event Action<int> OnFelicidadCambiada;
    public event Action<float> OnSaludCambiada;
    public event Action<int> OnFeIndividualCambiada; // NUEVO EVENTO
    public event Action<EstadoNPC, EstadoNPC> OnEstadoNPCCambiado;

    [Header("Configuración de Rezo del NPC")]
    [Tooltip("Cuánto progreso (0.0 a 1.0) contribuye este NPC al Monolito por cada pulso de rezo.")]
    public float progresoPorPulsoDeRezo = 0.05f; // Ej: 5% por pulso. (20 pulsos para 1 unidad de Fe)
    [Tooltip("Intervalo en segundos entre cada pulso de contribución de rezo.")]
    public float intervaloPulsoRezo = 1.0f; // Contribuye cada segundo mientras reza.

    private float _timerSiguientePulsoRezo = 0f;

    private void Start()
    {
        CambiarEstado(EstadoNPC.Ocioso);
    }

    private void Update()
    {
        ActualizarFSM();
    }

    private void ActualizarFSM()
    {
        _timerEstadoActual += Time.deltaTime;

        switch (_estadoActualNPC)
        {
            case EstadoNPC.Ocioso:
                ActualizarEstadoOcioso();
                break;
            case EstadoNPC.MoviendoseAlTrabajo:
                ActualizarEstadoMoviendoseAlTrabajo();
                break;
            case EstadoNPC.Trabajando:
                ActualizarEstadoTrabajando();
                break;
            case EstadoNPC.MoviendoseACasa:
                ActualizarEstadoMoviendoseACasa();
                break;
            case EstadoNPC.EnCasa:
                ActualizarEstadoEnCasa();
                break;
            case EstadoNPC.MoviendoseAlMonolito: // NUEVO CASE
                ActualizarEstadoMoviendoseAlMonolito();
                break;
            case EstadoNPC.RezandoAlMonolito:    // NUEVO CASE
                ActualizarEstadoRezandoAlMonolito();
                break;
        }
    }

    private void CambiarEstado(EstadoNPC nuevoEstado)
    {
        if (_estadoActualNPC == nuevoEstado) return;
        EstadoNPC estadoAnterior = _estadoActualNPC;
        _estadoActualNPC = nuevoEstado;
        _timerEstadoActual = 0f;
        OnEstadoNPCCambiado?.Invoke(estadoAnterior, _estadoActualNPC);
        // Debug.Log($"'{Nombre}' cambió de {estadoAnterior} a {nuevoEstado}");
    }

    // Inicializa al personaje con datos de un ScriptableObject.
    public void Inicializar(NPCDataSO data)
    {
        if (data == null)
        {
            Debug.LogError($"Intento de inicializar PersonajeBehaviour ({gameObject.name}) con NPCDataSO nulo.");
            // Considera destruir el personaje o asignarle valores por defecto si esto ocurre.
            // Destroy(gameObject);
            return;
        }

        _nombre = data.npcNombre;
        gameObject.name = "NPC_" + _nombre + "_" + GetInstanceID();
        _edad = data.edad;
        _genero = data.genero;
        _esLegendario = data.esLegendario;
        _salud = data.saludBase;
        _ropa = "Ropa Básica de Aldeano";
        _herramienta = "Ninguna";
        _felicidad = UnityEngine.Random.Range(40, 61);
        _feIndividual = UnityEngine.Random.Range(30, 71); // Fe inicial
        _trabajoDefinido = data.trabajo;
        _educacion = data.educacion;

        OnSaludCambiada?.Invoke(_salud);
        OnFelicidadCambiada?.Invoke(_felicidad);
        OnFeIndividualCambiada?.Invoke(_feIndividual);

        Debug.Log($"Personaje '{nombre}' inicializado. Edad: {edad}, Género: {genero}, Legendario: {esLegendario}, Trabajo: {trabajo}");
    }

    // --- Lógica de cada Estado ---
    private void ActualizarEstadoOcioso()
    {
        // Podría decidir ir a rezar si su fe es baja o si no hay trabajo
        // if (_timerEstadoActual > 5f) { // Cada 5 segundos, tomar una decisión
        //     if (FeIndividual < 40 && GameManager.Instance != null && GameManager.Instance.MonolitoPrincipal != null) {
        //         OrdenarRezarAlMonolito(GameManager.Instance.MonolitoPrincipal);
        //     }
        //     _timerEstadoActual = 0f;
        // }
    }

    private void ActualizarEstadoMoviendoseAlTrabajo()
    {
        // Debug.Log($"'{Nombre}' llegó conceptualmente a '{_edificioDeTrabajoActual?.buildingName ?? "su trabajo"}'.");
        CambiarEstado(EstadoNPC.Trabajando);
    }

    private void ActualizarEstadoTrabajando()
    {
        if (_edificioDeTrabajoActual == null)
        {
            CambiarEstado(EstadoNPC.Ocioso);
            return;
        }
        // if (_timerEstadoActual > 3.0f) {
        //     // Debug.Log($"'{Nombre}' completó ciclo de trabajo en '{_edificioDeTrabajoActual.buildingName}'.");
        //     _timerEstadoActual = 0f;
        // }
    }
    private void ActualizarEstadoMoviendoseACasa()
    {
        if (_casaAsignada == null) { CambiarEstado(EstadoNPC.Ocioso); return; }
        // Debug.Log($"'{Nombre}' llegó conceptualmente a su casa '{_casaAsignada.buildingName}'.");
        CambiarEstado(EstadoNPC.EnCasa);
    }

    private void ActualizarEstadoEnCasa()
    {
        // if (_timerEstadoActual > 10f) {
        //     ModificarFelicidad(2);
        //     _timerEstadoActual = 0f;
        // }
    }

    private void ActualizarEstadoMoviendoseAlMonolito() // NUEVO MÉTODO
    {
        if (_monolitoObjetivoRezo == null)
        {
            Debug.LogWarning($"'{nombre}' intentando moverse al Monolito, pero no hay objetivo. Volviendo a Ocioso.");
            CambiarEstado(EstadoNPC.Ocioso);
            return;
        }
        // En esta versión básica, el movimiento es instantáneo.
        // Debug.Log($"'{Nombre}' llegó conceptualmente al Monolito '{_monolitoObjetivoRezo.name}'.");
        CambiarEstado(EstadoNPC.RezandoAlMonolito);
        // Futuro: Lógica de movimiento hacia _monolitoObjetivoRezo.transform.position
    }

    private void ActualizarEstadoRezandoAlMonolito()
    {
        if (_monolitoObjetivoRezo == null)
        {
            Debug.LogWarning($"'{nombre}' está en estado RezandoAlMonolito pero no hay Monolito objetivo. Volviendo a Ocioso.");
            CambiarEstado(EstadoNPC.Ocioso);
            return;
        }

        // El NPC permanece en este estado durante _duracionRezo
        // Debug.Log($"'{Nombre}' está rezando al Monolito '{_monolitoObjetivoRezo.name}'... (Tiempo Total: {_timerEstadoActual:F1}/{_duracionRezo:F1})");

        // Lógica de pulso de rezo
        _timerSiguientePulsoRezo += Time.deltaTime;
        if (_timerSiguientePulsoRezo >= intervaloPulsoRezo)
        {
            _monolitoObjetivoRezo.RecibirContribucionDeRezo(progresoPorPulsoDeRezo);
            // Debug.Log($"'{Nombre}' envió pulso de rezo (+{progresoPorPulsoDeRezo*100}%) al Monolito.");
            _timerSiguientePulsoRezo = 0f; // Resetear timer para el siguiente pulso
        }

        if (_timerEstadoActual >= _duracionRezo)
        {
            // El NPC ya no contribuye con Fe Individual directamente, sino al progreso del Monolito.
            // Podrías darle una pequeña recompensa de felicidad o mantener/aumentar su Fe Individual aquí si aún la tiene.
            ModificarFelicidad(5); // Ejemplo: Gana algo de felicidad por completar el rezo.
            // Debug.Log($"'{Nombre}' terminó su sesión de rezo de {_duracionRezo}s.");

            if (casaAsignada != null) CambiarEstado(EstadoNPC.MoviendoseACasa);
            else CambiarEstado(EstadoNPC.Ocioso);
        }
    }

    // --- Métodos para Controlar la FSM desde Fuera ---
    public void AsignarTrabajo(BaseBuilding puestoDeTrabajo)
    {
        // ... (código existente) ...
        if (puestoDeTrabajo == null) return;
        // Quitar de trabajo/rezo anterior
        if (_edificioDeTrabajoActual != null && _edificioDeTrabajoActual is ITrabajable) { ((ITrabajable)_edificioDeTrabajoActual).QuitarTrabajador(this); }
        _monolitoObjetivoRezo = null; // Dejar de tener el monolito como objetivo si estaba rezando/yendo a rezar

        _edificioDeTrabajoActual = puestoDeTrabajo;
        _trabajoDefinido = puestoDeTrabajo.buildingName;
        if (puestoDeTrabajo is ITrabajable it) it.AnadirTrabajador(this);
        CambiarEstado(EstadoNPC.MoviendoseAlTrabajo);
    }

    public void QuitarDelTrabajoActual()
    {
        // ... (código existente) ...
        if (_edificioDeTrabajoActual != null && _edificioDeTrabajoActual is ITrabajable it) {
            it.QuitarTrabajador(this);
        }
        _edificioDeTrabajoActual = null;
        if (_casaAsignada != null) CambiarEstado(EstadoNPC.MoviendoseACasa);
        else CambiarEstado(EstadoNPC.Ocioso);
    }

    public void OrdenarRezarAlMonolito(MonolitoBehaviour monolitoARezar) // NUEVO MÉTODO
    {
        if (monolitoARezar == null)
        {
            Debug.LogWarning($"'{nombre}': Se intentó ordenar rezar a un Monolito nulo.");
            return;
        }
        // Quitar de trabajo anterior si lo tuviera
        if (_edificioDeTrabajoActual != null && _edificioDeTrabajoActual is ITrabajable it) {
            it.QuitarTrabajador(this);
            _edificioDeTrabajoActual = null;
        }

        _monolitoObjetivoRezo = monolitoARezar;
        // Debug.Log($"'{Nombre}' ha recibido la orden de ir a rezar al Monolito '{_monolitoObjetivoRezo.name}'.");
        CambiarEstado(EstadoNPC.MoviendoseAlMonolito);
    }

    // --- Modificadores de Estadísticas ---
    public void ModificarFeIndividual(int cantidad) // NUEVO MÉTODO
    {
        int fePrevia = _feIndividual;
        _feIndividual = Mathf.Clamp(_feIndividual + cantidad, 0, 100); // Asumiendo máximo de 100
        if (fePrevia != _feIndividual)
        {
            OnFeIndividualCambiada?.Invoke(_feIndividual);
            // Debug.Log($"Fe Individual de '{Nombre}' cambió a {_feIndividual}.");
        }
    }

    // Interfaz hipotética para edificios que aceptan trabajadores (si no la tienes ya)
    public interface ITrabajable {
        bool AnadirTrabajador(PersonajeBehaviour trabajador);
        void QuitarTrabajador(PersonajeBehaviour trabajador);
        // int GetMaxTrabajadores();
        // int GetTrabajadoresActuales();
    }

    // Aplica daño al personaje y maneja la muerte si la salud llega a cero.
    public void RecibirDaño(float daño)
    {
        if (daño <= 0) return; // No procesar daño no positivo.

        salud -= daño;
        Debug.Log($"Personaje '{nombre}' recibió {daño} de daño. Salud restante: {salud}");

        if (salud <= 0)
        {
            salud = 0; // Asegura que la salud no sea negativa.
            Morir();
        }
    }

    // Maneja la muerte del personaje.
    private void Morir()
    {
        Debug.Log($"Personaje '{nombre}' ha muerto.");

        // Notificar a la casa si estaba asignado.
        if (casaAsignada != null)
        {
            // Building_Casa se encargará de quitarlo de su lista de habitantes.
            casaAsignada.RemoverPersonaPorComportamiento(this);
            // No es necesario hacer `casaAsignada = null;` aquí porque el objeto se va a destruir.
        }

        // Limpiar relaciones: notificar a otros NPCs que este personaje ha muerto.
        // Iterar sobre una copia de las claves para poder modificar el diccionario 'relaciones' de otros.
        List<PersonajeBehaviour> npcsRelacionados = new List<PersonajeBehaviour>(relaciones.Keys);
        foreach (var otroNPC in npcsRelacionados)
        {
            if (otroNPC != null) // Verifica que el NPC relacionado no haya sido destruido también.
            {
                // El otro NPC elimina a este de sus relaciones.
                otroNPC.EliminarRelacionConPersonajeMuerto(this);
            }
        }
        relaciones.Clear(); // Limpia las relaciones de este NPC.

        // Disparar evento de muerte para que otros sistemas reaccionen (ej. GameManager, generador de tumbas, etc.).
        OnPersonajeMuerto?.Invoke(this);

        // Finalmente, destruir el GameObject del personaje.
        Destroy(gameObject);
    }

    // Método para que otros NPCs actualicen sus relaciones cuando este muere.
    public void EliminarRelacionConPersonajeMuerto(PersonajeBehaviour personajeMuerto)
    {
        if (personajeMuerto != null && relaciones.ContainsKey(personajeMuerto))
        {
            relaciones.Remove(personajeMuerto);
            // Debug.Log($"Relación con el personaje muerto '{personajeMuerto.nombre}' eliminada de '{nombre}'.");
        }
    }


    public void CambiarRopa(string nuevaRopa)
    {
        if (!string.IsNullOrEmpty(nuevaRopa))
        {
            ropa = nuevaRopa;
            // Debug.Log($"Personaje '{nombre}' cambió su ropa a: {ropa}");
            // Aquí podrías añadir lógica para cambiar visualmente el modelo del personaje si es necesario.
        }
    }

    public void CambiarHerramienta(string nuevaHerramienta)
    {
        if (!string.IsNullOrEmpty(nuevaHerramienta))
        {
            herramienta = nuevaHerramienta;
            // Debug.Log($"Personaje '{nombre}' cambió su herramienta a: {herramienta}");
            // Lógica para efectos de la herramienta (ej. mejora en trabajos).
        }
    }

    // Modifica la felicidad del personaje, asegurando que se mantenga en un rango (0-100).
    public void ModificarFelicidad(int cantidad)
    {
        int felicidadPrevia = felicidad;
        felicidad = Mathf.Clamp(felicidad + cantidad, 0, 100);
        // Debug.Log($"Felicidad de '{nombre}' cambió de {felicidadPrevia} a {felicidad} (Modificación: {cantidad}).");
        // La felicidad podría influir en la productividad, riesgo de abandono, etc.
    }

    // Asigna una casa al personaje. Llamado por Building_Casa.
    public void AsignarCasa(Building_Casa casa)
    {
        // Si ya tenía una casa diferente, primero lo quitamos de la anterior.
        // Building_Casa.AsignarHabitante se encarga de la lógica de añadir,
        // y si esta es una reasignación, el PersonajeBehaviour ya no debería estar en la lista de la casa vieja
        // si la casa vieja llamó a QuitarHabitante correctamente.
        // Sin embargo, por seguridad, nos aseguramos que la referencia interna se actualice.
        if (_casaAsignada != null && _casaAsignada != casa)
        {
            Debug.LogWarning($"Personaje '{nombre}' está siendo reasignado de la casa '{_casaAsignada.buildingName ?? "DESCONOCIDA"}' a '{casa?.buildingName ?? "NUEVA DESCONOCIDA"}'. Asegúrate que la casa anterior lo haya quitado.");
            // _casaAsignada.QuitarHabitante(this); // Esto podría ser problemático si 'casa' es la misma que '_casaAsignada' y causa un bucle.
            // La lógica de quitar de la casa vieja debería estar en el sistema que inicia la reasignación,
            // o Building_Casa.AsignarHabitante podría manejarlo.
        }
        _casaAsignada = casa; // Asigna la nueva casa.
        // Debug.Log($"Personaje '{nombre}' ahora tiene asignada la casa: '{_casaAsignada?.buildingName ?? "Ninguna"}'.");
    }

    // Quita la asignación de la casa actual del personaje.
    public void DesasignarCasa()
    {
        // if (_casaAsignada != null)
        // {
        //     // Debug.Log($"Personaje '{nombre}' desasignado de la casa: '{_casaAsignada.buildingName}'.");
        // }
        _casaAsignada = null;
    }

    // --- Gestión de Relaciones ---
    // Establece una relación inicial con otro NPC.
    public void ConocerNuevoNPC(PersonajeBehaviour otroNPC, int relacionInicial = 0)
    {
        if (otroNPC == null || otroNPC == this || relaciones.ContainsKey(otroNPC))
        {
            // No relacionarse consigo mismo, con nulos, o si ya existe relación.
            return;
        }
        relaciones.Add(otroNPC, Mathf.Clamp(relacionInicial, -100, 100)); // Añade y clampa la relación.
        // Para asegurar bidireccionalidad sin bucles, el otro NPC también debe registrarlo,
        // pero solo si no lo ha hecho ya.
        if (!otroNPC.relaciones.ContainsKey(this))
        {
            // El otro NPC registra la relación desde su perspectiva.
            // Aquí 'this' se pasa como el conocido.
            otroNPC.RegistrarConocimientoDe(this, relacionInicial);
        }
        // Debug.Log($"'{nombre}' ahora conoce a '{otroNPC.nombre}' con una relación de: {relaciones[otroNPC]}.");
    }

    // Método interno para ser llamado por otro NPC para establecer la relación bidireccional.
    protected void RegistrarConocimientoDe(PersonajeBehaviour npcQueMeConocio, int relacionInicial)
    {
        if (npcQueMeConocio == null || npcQueMeConocio == this || relaciones.ContainsKey(npcQueMeConocio))
        {
            return;
        }
        relaciones.Add(npcQueMeConocio, Mathf.Clamp(relacionInicial, -100, 100));
        // Debug.Log($"'{nombre}' fue conocido por '{npcQueMeConocio.nombre}' con una relación de: {relaciones[npcQueMeConocio]}.");
    }


    // Modifica el nivel de relación con otro NPC.
    public void ModificarRelacion(PersonajeBehaviour otroNPC, int cantidad)
    {
        if (otroNPC != null && relaciones.ContainsKey(otroNPC))
        {
            int relacionPrevia = relaciones[otroNPC];
            relaciones[otroNPC] = Mathf.Clamp(relaciones[otroNPC] + cantidad, -100, 100); // Clampa entre -100 y 100.
            // Debug.Log($"Relación de '{nombre}' con '{otroNPC.nombre}' cambió de {relacionPrevia} a {relaciones[otroNPC]}.");
        }
    }

    // Obtiene el nivel de relación con otro NPC.
    public int ObtenerRelacion(PersonajeBehaviour otroNPC)
    {
        if (otroNPC != null && relaciones.TryGetValue(otroNPC, out int nivelRelacion))
        {
            return nivelRelacion;
        }
        return 0; // Valor por defecto si no hay relación o el NPC es nulo.
    }

    // Elimina la relación con otro NPC (ej. si uno se va, o por eventos).
    // No implica la muerte del otro NPC, solo que ya no hay relación registrada.
    public void OlvidarNPC(PersonajeBehaviour otroNPC) // Renombrado para más claridad que 'EliminarRelacion'
    {
        if (otroNPC != null && relaciones.Remove(otroNPC)) // .Remove devuelve true si se quitó.
        {
            // Debug.Log($"'{nombre}' ha olvidado o terminado su relación con '{otroNPC.nombre}'.");
            // Considerar si el otro NPC también debe olvidar a este.
            // otroNPC.OlvidarNPC(this); // ¡Cuidado con bucles infinitos aquí! Necesita una condición de parada.
        }
    }
}
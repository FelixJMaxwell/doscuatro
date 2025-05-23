using UnityEngine;
using System.Collections.Generic; // Necesario para List y Dictionary

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


    private void Start()
    {
        // La inicialización principal ahora se hace en Inicializar(NPCDataSO).
        // 'felicidad' se establece aquí como un valor por defecto si no viniera de NPCDataSO.
        // Si NPCDataSO puede definir felicidad inicial, mover esta línea a Inicializar.
        // Si no, aquí está bien como valor por defecto post-instanciación.
        _felicidad = 75; // Ejemplo de felicidad inicial por defecto.
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

        // Información general
        nombre = data.npcNombre; // Asigna el nombre del SO. Podrías añadir un número o hacerlo único si es necesario.
                                // gameObject.name = nombre; // Opcional: sincronizar nombre del GameObject.
        edad = data.edad;         // Si quieres variabilidad, data.edad podría ser un rango.
        genero = data.genero;
        esLegendario = data.esLegendario;

        // Estado
        salud = data.saludBase;
        ropa = "Ropa Básica de Aldeano"; // Valor inicial por defecto. Podría venir de NPCDataSO.
        herramienta = "Ninguna";       // Valor inicial por defecto.
        felicidad = 50; // Valor inicial de felicidad al ser creado, podría venir de NPCDataSO.

        // Ocupación
        trabajo = data.trabajo;       // Podría ser "Ninguno" o un trabajo base.
        educacion = data.educacion;   // Podría ser "Básica" o "Ninguna".

        Debug.Log($"Personaje '{nombre}' inicializado. Edad: {edad}, Género: {genero}, Legendario: {esLegendario}, Trabajo: {trabajo}");
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
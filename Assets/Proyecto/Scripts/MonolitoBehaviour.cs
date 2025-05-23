using UnityEngine;
using System.Collections.Generic; // Necesario para List<GameObject>
using TMPro; // Necesario para TextMeshProUGUI

public class MonolitoBehaviour : MonoBehaviour // Considerar si debería heredar de BaseBuilding si comparte lógica.
{
    [Header("Generación de Fragmentos")]
    [Tooltip("Costo base en Fe para el primer fragmento extraído.")]
    public float CostoFragmentoBase = 10f;
    [Tooltip("Factor por el cual se multiplica el costo de Fe para cada fragmento subsiguiente (ej. 1.2 = 20% más caro).")]
    public float IncrementoCostoFragmento = 1.2f;
    // Propiedad pública para saber el costo actual, calculada internamente.
    public float CostoFragmentoActual { get; private set; } // Renombrado para claridad
    private int contadorFragmentosGenerados = 0; // Contador de cuántos fragmentos se han extraído.

    // 'PorcentajeSingularidad' no se usa. ¿Para futuros eventos aleatorios?
    [Header("Eventos Aleatorios")]
    [Range(0f, 1f)]
    [Tooltip("Probabilidad (0-1) de que ocurra un evento especial o 'singularidad' (lógica no implementada).")]
    public float PorcentajeSingularidad = 0.1f;

    [Header("Sistema de Fe y Niveles del Monolito")]
    [Tooltip("Referencia al ScriptableObject (RecurSO) que define la 'Fe'.")]
    [SerializeField] private RecurSO FeDataSO; // Esencial para saber el nombre del recurso "Fe".
    // 'FeActual' aquí parece redundante si ResourceManager ya gestiona la cantidad de "Fe".
    // El Monolito no debería tener su propia copia de 'FeActual'. Debería consultar ResourceManager.
    // public float FeActual { get; private set; } // ELIMINAR - Usar ResourceManager.Instance.GetCantidad(FeDataSO.Nombre)

    [Tooltip("Nivel actual de poder o influencia del Monolito.")]
    public float NivelDeFeDelMonolito { get; private set; } = 1f; // Nivel inicial. Renombrado para claridad.
    // Estas propiedades también son redundantes o deberían obtenerse del ResourceManager usando FeDataSO.
    // public float FeMaxima => FeDataSO != null ? FeDataSO.Maximo : 100f; // ELIMINAR
    // public float FeBase => FeDataSO != null ? FeDataSO.ValorBase : 10f; // ELIMINAR

    // Calcula la Fe necesaria para subir al siguiente nivel del Monolito.
    public float FeNecesariaParaSiguienteNivelMonolito
    {
        get
        {
            if (FeDataSO != null && FeDataSO.requiereNivel) // Usa la configuración de niveles de FeDataSO.
            {
                // Aquí, 'NivelDeFeDelMonolito' es el nivel actual.
                // 'CalcularCostoSubirNivel' espera el nivel actual para dar el costo al *siguiente*.
                return FeDataSO.CalcularCostoSubirNivel(NivelDeFeDelMonolito);
            }
            return float.MaxValue; // Si no hay sistema de niveles o no hay FeDataSO, es imposible subir.
        }
    }

    [Tooltip("Referencia al ScriptableObject (RecurSO) que define los 'Fragmentos'.")]
    [SerializeField] private RecurSO FragmentoSO; // Necesario para añadir fragmentos.

    [Header("Visuales de Fe (Pilares)")]
    [Tooltip("Prefab del objeto Pilar que se instancia para representar la Fe visualmente.")]
    public GameObject PilarPrefab; // Renombrado de 'Pilar' a 'PilarPrefab' para claridad.
    [Tooltip("Radio mínimo desde el centro del Monolito donde pueden aparecer los pilares.")]
    public float RadioMinimoAparicionPilar = 2f;
    [Tooltip("Radio máximo desde el centro del Monolito donde pueden aparecer los pilares.")]
    public float RadioMaximoAparicionPilar = 5f;
    [Tooltip("Transform padre bajo el cual se instanciarán los pilares de Fe.")]
    public Transform FaithHolder; // Correcto para organizar los pilares.

    [Header("Conexiones y Ticker (Lógica Incompleta)")]
    // 'EstructurasConectadas' no se usa actualmente en la lógica de 'Update'.
    [Tooltip("Lista de estructuras que están 'conectadas' al Monolito (lógica futura).")]
    public List<GameObject> EstructurasConectadas;
    // 'Ticker' y 'TickerLimit' no tienen una función clara en el Update actual.
    // Si es para generación pasiva de Fe, esa lógica falta.
    public float Ticker = 0f;
    public float TickerLimit = 5f; // ¿Cada 5 segundos ocurre algo?

    [Header("UI y Referencias")]
    [Tooltip("Referencia al TextMeshProUGUI que muestra el costo para extraer un fragmento.")]
    public TextMeshProUGUI CostoFragmentoBtnText; // Renombrado para claridad (es el texto del botón).
    // 'gameManager' se busca por nombre, es mejor asignarlo en el Inspector o usar un Singleton.
    private GameManager gameManager; // Cambiado a private, asignar en Start/Awake.
    [Tooltip("Radio para el Gizmo de depuración que se dibuja en el editor.")]
    public float GizmoRadio = 5f;

    private void Awake() // Awake se llama antes que Start. Bueno para inicializar referencias.
    {
        gameManager = FindFirstObjectByType<GameManager>(); // Más robusto que GameObject.Find.
        if (gameManager == null)
        {
            Debug.LogError("MonolitoBehaviour: GameManager no encontrado en la escena.");
        }
    }

    private void Start()
    {
        // Validar asignaciones cruciales.
        if (FeDataSO == null) Debug.LogError($"MonolitoBehaviour ({gameObject.name}): FeDataSO no asignado.");
        if (FragmentoSO == null) Debug.LogError($"MonolitoBehaviour ({gameObject.name}): FragmentoSO no asignado.");
        if (PilarPrefab == null) Debug.LogError($"MonolitoBehaviour ({gameObject.name}): PilarPrefab no asignado.");
        if (FaithHolder == null) Debug.LogError($"MonolitoBehaviour ({gameObject.name}): FaithHolder no asignado.");
        if (CostoFragmentoBtnText == null) Debug.LogError($"MonolitoBehaviour ({gameObject.name}): CostoFragmentoBtnText no asignado.");


        // Añadir el Monolito a la lista de estructuras del GameManager si es necesario.
        // Esta lógica de que el Monolito se añada a sí mismo a una lista en GameManager es un poco extraña.
        // GameManager debería descubrirlo o tener una referencia directa.
        // if (gameManager != null && gameManager.Estructuras != null && !gameManager.Estructuras.Contains(transform))
        // {
        //     gameManager.Estructuras.Add(transform);
        // }

        ActualizarCostoFragmentoUI(); // Calcula y muestra el costo inicial del fragmento.
    }

    private void Update()
    {
        // La lógica del Ticker no hace nada actualmente.
        // if (EstructurasConectadas.Count <= 0) return; // Esto detendría cualquier lógica de Ticker si no hay conexiones.
        // Ticker += Time.deltaTime;
        // if (Ticker >= TickerLimit)
        // {
        //     Ticker = 0;
        //     // Lógica futura: generar Fe pasivamente, activar efectos en estructuras conectadas, etc.
        // }
    }

    // Genera un pilar visual de Fe en una posición aleatoria alrededor del Monolito.
    public void GenerarPilar()
    {
        if (PilarPrefab == null || FaithHolder == null)
        {
            Debug.LogError("PilarPrefab o FaithHolder no asignados. No se puede generar pilar.");
            return;
        }

        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad; // Convertir a radianes.
        float distancia = Random.Range(RadioMinimoAparicionPilar, RadioMaximoAparicionPilar);

        // Calcula la posición en el plano XZ.
        float x = transform.position.x + Mathf.Cos(angulo) * distancia;
        float z = transform.position.z + Mathf.Sin(angulo) * distancia;
        // Altura Y aleatoria para un efecto visual.
        float randomYOffset = Random.Range(-0.3f, 0.5f); // Posición final del pilar.
        float initialYPos = -0.5f; // Posición inicial desde donde emerge.

        Vector3 posicionFinalPilar = new Vector3(x, transform.position.y + randomYOffset, z); // Relativa al monolito o absoluta?
                                                                                           // Asumo que Y es relativa a la base del monolito.
        Vector3 posicionInicialPilar = new Vector3(x, transform.position.y + initialYPos, z);


        GameObject tempPilarObj = Instantiate(PilarPrefab, posicionInicialPilar, Quaternion.identity, FaithHolder);
        tempPilarObj.name = "Pilar_Fe_" + FaithHolder.childCount;

        PilarBehaviour pilarBehaviour = tempPilarObj.GetComponent<PilarBehaviour>();
        if (pilarBehaviour != null)
        {
            pilarBehaviour.PosicionObjetivoAlSubir = posicionFinalPilar; // Asigna la posición objetivo al script del pilar.
            pilarBehaviour.Bajar = false; // Asegura que el pilar suba al ser generado.
        }
        else
        {
            Debug.LogWarning("El Prefab del Pilar no tiene el script PilarBehaviour.");
        }
    }

    // Intenta subir el nivel de Fe del Monolito.
    public void IntentarSubirNivelFeMonolito() // Renombrado para claridad.
    {
        if (FeDataSO == null || ResourceManager.Instance == null)
        {
            Debug.LogError("FeDataSO no asignado o ResourceManager.Instance no encontrado. No se puede subir nivel.");
            return;
        }
        if (!FeDataSO.requiereNivel)
        {
            Debug.Log("El recurso Fe (según FeDataSO) no está configurado para tener niveles.");
            return;
        }
        if (string.IsNullOrEmpty(FeDataSO.recursoRequeridoParaNivel))
        {
            // Esto significa que para subir el nivel de "Fe" del Monolito, se necesita "Fe" misma.
            // Lo cual es una mecánica válida (invertir Fe para hacer la Fe más potente/mayor capacidad).
            // Si se necesitara OTRO recurso, 'recursoRequeridoParaNivel' lo indicaría.
            Debug.LogWarning("FeDataSO.recursoRequeridoParaNivel no está especificado. Se asumirá que se necesita 'Fe' para subir nivel de 'Fe'.");
            // O si esto es un error: return;
        }

        string recursoDeCosto = string.IsNullOrEmpty(FeDataSO.recursoRequeridoParaNivel) ? FeDataSO.Nombre : FeDataSO.recursoRequeridoParaNivel;
        float costoSubida = FeNecesariaParaSiguienteNivelMonolito;

        if (ResourceManager.Instance.TieneSuficiente(recursoDeCosto, costoSubida))
        {
            ResourceManager.Instance.Gastar(recursoDeCosto, costoSubida);
            NivelDeFeDelMonolito++;
            Debug.Log($"¡Nivel de Fe del Monolito subido a {NivelDeFeDelMonolito}! Costo: {costoSubida} de {recursoDeCosto}.");
            // Considera eventos aquí para que otros sistemas reaccionen al cambio de nivel.
            // Ejemplo: MonolithLevelledUp?.Invoke(NivelDeFeDelMonolito);
            // También, si el nivel afecta el máximo de Fe, ese máximo debería actualizarse en ResourceManager
            // o la forma en que se obtiene el máximo de Fe debería considerar el NivelDeFeDelMonolito.
        }
        else
        {
            Debug.Log($"No hay suficiente '{recursoDeCosto}' para subir el nivel de Fe del Monolito. Necesitas: {costoSubida}, Tienes: {ResourceManager.Instance.GetCantidad(recursoDeCosto)}");
        }
    }

    // Actualiza el costo del fragmento y la UI correspondiente.
    private void ActualizarCostoFragmentoUI()
    {
        // Costo = CostoBase * (IncrementoPorNivel ^ ContadorFragmentos)
        CostoFragmentoActual = CostoFragmentoBase * Mathf.Pow(IncrementoCostoFragmento, contadorFragmentosGenerados);
        if (CostoFragmentoBtnText != null)
        {
            CostoFragmentoBtnText.text = $"Extraer Fragmento ({CostoFragmentoActual.ToString("F0")} Fe)"; // Asume que cuesta Fe.
        }
    }

    // Evento de Unity llamado cuando el ratón está sobre el Collider de este GameObject.
    private void OnMouseOver()
    {
        // Si se hace clic izquierdo, no hay otra estructura seleccionada, y gameManager existe.
        if (Input.GetMouseButtonDown(0) && gameManager != null && gameManager.EstructuraSeleccionada == null)
        {
            gameManager.EstructuraSeleccionada = this.gameObject; // Selecciona este Monolito.
            // La lógica de abrir el panel del Monolito ya está en GameManager.Update().
            // Idealmente, GameManager notificaría al Monolito "has sido seleccionado"
            // y el Monolito se encargaría de su UI o de pedir a un UIManager que la muestre.
            // Ejemplo: gameManager.SeleccionarObjeto(this.gameObject);
        }
    }

    // Dibuja Gizmos en el editor cuando el Monolito está seleccionado. Útil para debugging visual.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.3f, 1f, 0.5f); // Un color púrpura translúcido.
        Gizmos.DrawWireSphere(transform.position, GizmoRadio); // Dibuja el radio de influencia o interés.
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, RadioMinimoAparicionPilar);
        Gizmos.DrawWireSphere(transform.position, RadioMaximoAparicionPilar);
    }

    // Método para añadir Fe "manualmente" (ej. por una acción del jugador directa sobre el Monolito).
    public void AñadirFeManualmente(float cantidad)
    {
        if (FeDataSO == null || ResourceManager.Instance == null)
        {
            Debug.LogError("FeDataSO no asignado o ResourceManager no encontrado. No se puede añadir Fe.");
            return;
        }
        ResourceManager.Instance.Añadir(FeDataSO.Nombre, cantidad);
        // La UI de Fe se actualiza a través del callback en ResourceManager.
        // Debug.Log($"{cantidad} de Fe añadida manualmente al sistema a través del Monolito.");
    }

    // Intenta extraer un fragmento del Monolito, consumiendo Fe.
    public void ExtraerFragmentoDelMonolito() // Renombrado para claridad.
    {
        if (ResourceManager.Instance == null || FragmentoSO == null || FeDataSO == null)
        {
            Debug.LogError("ResourceManager, FragmentoSO, o FeDataSO no asignados. No se puede extraer fragmento.");
            return;
        }

        // Verifica si hay suficiente Fe para pagar el costo actual del fragmento.
        if (ResourceManager.Instance.TieneSuficiente(FeDataSO.Nombre, CostoFragmentoActual))
        {
            ResourceManager.Instance.Gastar(FeDataSO.Nombre, CostoFragmentoActual); // Gasta la Fe.
            ResourceManager.Instance.Añadir(FragmentoSO.Nombre, 1); // Añade 1 Fragmento.
            contadorFragmentosGenerados++; // Incrementa el contador de fragmentos.
            ActualizarCostoFragmentoUI(); // Recalcula y actualiza el costo para el siguiente.

            Debug.Log($"¡Fragmento extraído! Se gastaron {CostoFragmentoActual.ToString("F0")} de Fe. Nuevo costo de fragmento: {CostoFragmentoActual.ToString("F0")} Fe.");
        }
        else
        {
            Debug.Log($"No hay suficiente Fe para extraer un fragmento. Necesitas: {CostoFragmentoActual.ToString("F0")}, Tienes: {ResourceManager.Instance.GetCantidad(FeDataSO.Nombre).ToString("F0")}");
        }
    }
}
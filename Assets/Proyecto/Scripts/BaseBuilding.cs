using UnityEngine;

// Clase base abstracta para todos los edificios en el juego.
// Define propiedades y funcionalidades comunes.
public abstract class BaseBuilding : MonoBehaviour
{
    [Header("Información General del Edificio")]
    [Tooltip("Nombre visible del edificio.")]
    public string buildingName;
    [Tooltip("Icono para mostrar en la UI, si aplica.")]
    public Sprite buildingIcon; // Bien para la UI.
    [Tooltip("Costo en algún recurso principal para construir este edificio.")]
    public int constructionCost; // ¿Qué recurso usa? Podría ser una lista de (RecurSO, cantidad).
    [Tooltip("Tipo de estructura, usando el enum definido en BuildingType.")]
    public BuildingType.Estructura TipoEstructura; // Buena forma de categorizar.

    [Header("Producción de Recursos")]
    [Tooltip("Marcar si este edificio produce recursos pasivamente.")]
    public bool producesResources; // Controla la lógica de producción.
    [Tooltip("El ScriptableObject del recurso que este edificio produce (si 'producesResources' es true).")]
    [SerializeField] protected RecurSO RecursoProducir; // El SO del recurso.
    [Tooltip("Cantidad de 'RecursoProducir' generada por segundo (si 'producesResources' es true).")]
    [SerializeField] protected float TasaDeProduccion = 1f; // Unidades por segundo.

    // Estado interno del edificio.
    protected bool isActive = false; // Indica si el edificio está funcionando (ej. después de construido o activado).
    private float productionTimer = 0f; // Temporizador para la lógica de producción.

    // Llamado al inicio. Las clases hijas pueden sobreescribirlo para su propia inicialización.
    protected virtual void Start()
    {
        // Podría ser buena idea que si 'buildingName' está vacío, se use el nombre del GameObject.
        // if (string.IsNullOrEmpty(buildingName)) buildingName = gameObject.name;
        InitializeBuilding();
    }

    // Llamado cada frame. Maneja la producción de recursos si el edificio está activo.
    protected virtual void Update()
    {
        // Solo produce si está activo, configurado para producir, tiene un recurso definido,
        // y el ResourceManager existe.
        if (isActive && producesResources && RecursoProducir != null && ResourceManager.Instance != null)
        {
            productionTimer += Time.deltaTime;
            // En lugar de producir basado en un ratio inverso, es más común acumular y producir unidades enteras.
            // float recursosAGenerarEsteFrame = TasaDeProduccion * Time.deltaTime;
            // ResourceManager.Instance.Añadir(RecursoProducir.Nombre, recursosAGenerarEsteFrame);
            // productionTimer no sería necesario con este enfoque.

            // Tu enfoque actual produce 'TasaDeProduccion' unidades de golpe cada '1f / TasaDeProduccion' segundos.
            // Esto puede ser deseable para algunos tipos de producción (lotes).
            // Si TasaDeProduccion es muy alta, 1f / TasaDeProduccion será muy pequeño, produciendo muy frecuentemente.
            // Si TasaDeProduccion es muy baja (ej. 0.1), 1f / TasaDeProduccion será grande (10s).
            float intervaloDeProduccion = 1f / TasaDeProduccion;
            if (TasaDeProduccion <= 0) intervaloDeProduccion = float.MaxValue; // Evitar división por cero y producción infinita.

            if (productionTimer >= intervaloDeProduccion)
            {
                // Produce un "lote" de recursos. La cantidad debería ser 'TasaDeProduccion * intervaloDeProduccion', que es 1.
                // Si quieres que 'TasaDeProduccion' sea "unidades por lote", entonces la cantidad a producir es 'TasaDeProduccion'.
                // Si 'TasaDeProduccion' es "unidades por segundo", la cantidad a producir cada 'intervaloDeProduccion'
                // sería 'TasaDeProduccion * intervaloDeProduccion'.
                // Lo que parece que quieres es producir 1 unidad del recurso cada 'intervaloDeProduccion' segundos,
                // y 'TasaDeProduccion' define cuántos de esos eventos de producción ocurren por segundo.
                // Si TasaDeProduccion = 2 (produce 2 veces por segundo), el intervalo es 0.5s.
                // En ProduceResources(), estás añadiendo 'TasaDeProduccion * Time.deltaTime', lo cual es incorrecto
                // para una producción por lotes. Debería ser una cantidad fija por lote.
                ProduceResources(1); // Ejemplo: producir 1 unidad del recurso.
                productionTimer -= intervaloDeProduccion; // Restar el intervalo en lugar de resetear a 0 para mayor precisión.
            }
        }
    }

    // Método virtual para que las clases hijas puedan añadir su lógica de inicialización.
    protected virtual void InitializeBuilding()
    {
        // Debug.Log($"Edificio '{buildingName}' inicializado.");
        // Por defecto, los edificios podrían no estar activos hasta que se completan/activan.
        // isActive = false; // O manejar esto en la lógica de construcción.
    }

    // Método virtual para la lógica de producción.
    // 'cantidadPorLote' es la cantidad a producir en este evento de producción.
    protected virtual void ProduceResources(float cantidadPorLote) // Modificado para aceptar cantidad
    {
        if (RecursoProducir != null && ResourceManager.Instance != null)
        {
            // Anteriormente: ResourceManager.Instance.Añadir(RecursoProducir.Nombre, TasaDeProduccion * Time.deltaTime);
            // Esto es incorrecto si llamas a ProduceResources() en intervalos fijos.
            // Debería añadir la cantidad que representa un "lote" de producción.
            ResourceManager.Instance.Añadir(RecursoProducir.Nombre, cantidadPorLote);
            // Debug.Log($"{buildingName} produjo {cantidadPorLote} de {RecursoProducir.Nombre}");
        }
    }

    // Activa el edificio (ej. comienza la producción).
    public virtual void ActivateBuilding()
    {
        isActive = true;
        productionTimer = 0f; // Reiniciar el timer al activar.
        // Debug.Log($"Edificio '{buildingName}' activado.");
    }

    // Desactiva el edificio (ej. detiene la producción).
    public virtual void DeactivateBuilding()
    {
        isActive = false;
        // Debug.Log($"Edificio '{buildingName}' desactivado.");
    }

    // Podrías añadir métodos comunes como:
    // public virtual string GetBuildingInfo() { return $"Nombre: {buildingName}, Tipo: {TipoEstructura}"; }
    // public virtual void UpgradeBuilding() { /* Lógica de mejora base */ }
}
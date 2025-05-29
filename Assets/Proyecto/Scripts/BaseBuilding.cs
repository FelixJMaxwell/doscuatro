// Archivo: BaseBuilding.cs
using UnityEngine;
using System.Collections.Generic; // Necesario para List<CostoRecurso>

/// <summary>
/// Define el costo de un recurso específico para la construcción.
/// </summary>
[System.Serializable]
public struct ConstructionCostEntry // Renombrado de CostoRecurso para claridad en inglés
{
    [Tooltip("El ScriptableObject del recurso requerido.")]
    public RecurSO resourceSO; // El SO del recurso
    [Tooltip("La cantidad necesaria de este recurso.")]
    public float amount;       // La cantidad
}

public abstract class BaseBuilding : MonoBehaviour
{
    #region Core Properties & Configuration
    // =================================================================================================================
    // PROPIEDADES PRINCIPALES Y CONFIGURACIÓN DEL EDIFICIO
    // =================================================================================================================
    [Header("Información General del Edificio")]
    [Tooltip("Nombre visible del edificio. Si está vacío, se usará el nombre del GameObject.")]
    public string buildingName;
    [Tooltip("Icono para mostrar en la UI, si aplica.")]
    public Sprite buildingIcon;
    [Tooltip("Tipo de estructura, usando el enum definido en BuildingType.")]
    public BuildingType.Estructura buildingType; // Renombrado de TipoEstructura

    [Header("Costos de Construcción")]
    [Tooltip("Lista de recursos y sus cantidades necesarias para construir este edificio.")]
    public List<ConstructionCostEntry> constructionCosts; // Antes 'constructionCost' (int) y luego 'costosConstruccion'

    [Header("Producción de Recursos (Opcional)")]
    [Tooltip("Marcar si este edificio produce recursos pasivamente.")]
    public bool producesResources;
    [Tooltip("El ScriptableObject del recurso que este edificio produce.")]
    [SerializeField] protected RecurSO resourceToProduceSO; // Renombrado de RecursoProducir
    [Tooltip("Cantidad del recurso producido por cada lote/evento de producción.")]
    [SerializeField] protected float unitsPerProductionBatch = 1f; // Antes unidadesPorLoteDeProduccion
    [Tooltip("Tiempo en segundos entre cada lote/evento de producción.")]
    [SerializeField] protected float intervalBetweenProduction = 5f; // Antes intervaloEntreLotesProduccion
    #endregion

    #region Internal State
    // =================================================================================================================
    // ESTADO INTERNO DEL EDIFICIO
    // =================================================================================================================
    [SerializeField] // Para verlo en el Inspector para debug
    protected bool isActive = false; // Indica si el edificio está funcionando.
    private float _productionTimer = 0f; // Renombrado de productionTimer
    #endregion

    #region Unity Lifecycle Methods
    // =================================================================================================================
    // MÉTODOS DEL CICLO DE VIDA DE UNITY
    // =================================================================================================================
    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(buildingName))
        {
            buildingName = gameObject.name;
        }
        // La activación (isActive = true) generalmente se maneja después de que
        // el proceso de construcción finaliza, no directamente en Start,
        // a menos que el edificio se coloque ya construido.
        // InitializePostConstruction(); // Renombrado de InitializeBuilding
    }

    protected virtual void Update()
    {
        if (isActive && producesResources && resourceToProduceSO != null && unitsPerProductionBatch > 0 && intervalBetweenProduction > 0)
        {
            _productionTimer += Time.deltaTime;
            if (_productionTimer >= intervalBetweenProduction)
            {
                ExecuteProduction(); // Renombrado de ProduceResources
                _productionTimer -= intervalBetweenProduction; // Mantener la cadencia
            }
        }
    }
    #endregion

    #region Core Building Logic
    // =================================================================================================================
    // LÓGICA PRINCIPAL DEL EDIFICIO (ACTIVACIÓN, PRODUCCIÓN, INICIALIZACIÓN)
    // =================================================================================================================

    /// <summary>
    /// Llamado por el BuildingManager después de que el edificio ha sido colocado y sus costos pagados.
    /// Aquí es donde el edificio debería volverse completamente funcional.
    /// </summary>
    public virtual void InitializePostConstruction() // Renombrado de InitializeBuilding
    {
        // Debug.Log($"Edificio '{buildingName}' (Tipo: {buildingType}) inicializado post-construcción.");
        // Podría activarse aquí directamente o esperar una orden explícita si hay etapas de construcción.
        // ActivateBuilding();
    }

    /// <summary>
    /// Ejecuta un ciclo de producción de recursos.
    /// </summary>
    protected virtual void ExecuteProduction() // Renombrado de ProduceResources
    {
        if (ResourceManager.Instance != null && resourceToProduceSO != null)
        {
            ResourceManager.Instance.Añadir(resourceToProduceSO.Nombre, unitsPerProductionBatch);
            // Debug.Log($"{buildingName} produjo {unitsPerProductionBatch} de {resourceToProduceSO.Nombre}");
        }
    }

    /// <summary>
    /// Activa el edificio, permitiendo que comience sus funciones (ej. producción).
    /// </summary>
    public virtual void ActivateBuilding()
    {
        if (isActive) return;
        isActive = true;
        _productionTimer = 0f; // Reiniciar el timer al activar.
        // Debug.Log($"Edificio '{buildingName}' activado.");
    }

    /// <summary>
    /// Desactiva el edificio, deteniendo sus funciones.
    /// </summary>
    public virtual void DeactivateBuilding()
    {
        if (!isActive) return;
        isActive = false;
        // Debug.Log($"Edificio '{buildingName}' desactivado.");
    }
    #endregion

    #region Construction Cost Management
    // =================================================================================================================
    // GESTIÓN DE COSTOS DE CONSTRUCCIÓN (PARA BUILDINGMANAGER)
    // =================================================================================================================

    /// <summary>
    /// Verifica si se tienen los recursos necesarios para construir este edificio.
    /// </summary>
    /// <returns>True si se puede construir, false en caso contrario.</returns>
    public bool CanBeBuilt() // Antes PuedeSerConstruido
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError($"ResourceManager no disponible para verificar costos de '{buildingName}'.");
            return false;
        }
        if (constructionCosts == null || constructionCosts.Count == 0)
        {
            // Debug.Log($"'{buildingName}' no tiene costos de construcción definidos, se asume construible.");
            return true; // Si no hay costos definidos, se puede construir "gratis".
        }

        foreach (ConstructionCostEntry costEntry in constructionCosts) // Usando la struct renombrada
        {
            if (costEntry.resourceSO == null || costEntry.amount <= 0)
            {
                // Debug.LogWarning($"Costo mal configurado para '{buildingName}': recurso nulo o cantidad cero/negativa.");
                continue; // Ignorar entradas de costo mal configuradas
            }
            if (!ResourceManager.Instance.TieneSuficiente(costEntry.resourceSO.Nombre, costEntry.amount))
            {
                // Debug.Log($"Faltan recursos para '{buildingName}'. Necesita {costEntry.amount} de '{costEntry.resourceSO.Nombre}', tiene {ResourceManager.Instance.GetCantidad(costEntry.resourceSO.Nombre)}.");
                return false; // Falta al menos un recurso.
            }
        }
        return true; // Se tienen todos los recursos necesarios.
    }

    /// <summary>
    /// Gasta los recursos necesarios para la construcción de este edificio.
    /// Se debe llamar DESPUÉS de verificar con CanBeBuilt().
    /// </summary>
    public void SpendConstructionResources() // Antes GastarRecursosDeConstruccion
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError($"ResourceManager no disponible para gastar recursos de '{buildingName}'.");
            return;
        }
        if (constructionCosts == null || constructionCosts.Count == 0)
        {
            // Debug.Log($"'{buildingName}' no tiene costos de construcción, no se gasta nada.");
            return; // No hay costos que gastar.
        }

        // Debug.Log($"Gastando recursos para '{buildingName}':");
        foreach (ConstructionCostEntry costEntry in constructionCosts) // Usando la struct renombrada
        {
            if (costEntry.resourceSO != null && costEntry.amount > 0)
            {
                // Debug.Log($"- Gastando {costEntry.amount} de '{costEntry.resourceSO.Nombre}'");
                ResourceManager.Instance.Gastar(costEntry.resourceSO.Nombre, costEntry.amount);
            }
        }
    }
    #endregion

    #region Optional Building Info & Upgrades
    // =================================================================================================================
    // INFORMACIÓN ADICIONAL Y FUTURAS MEJORAS (PLACEHOLDERS)
    // =================================================================================================================
    // public virtual string GetBuildingInfo()
    // {
    //     return $"Nombre: {buildingName}, Tipo: {buildingType}\nActivo: {isActive}";
    // }

    // public virtual void UpgradeBuilding()
    // {
    //     // Lógica base para mejorar un edificio, si aplica de forma general.
    //     // Podría ser más específico en clases derivadas.
    //     Debug.Log($"Intentando mejorar {buildingName}...");
    // }
    #endregion
}
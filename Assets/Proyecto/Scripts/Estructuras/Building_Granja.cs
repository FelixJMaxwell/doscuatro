// Archivo: Building_Granja.cs
using UnityEngine;
using UnityEngine.EventSystems; // Para EventSystem.current.IsPointerOverGameObject()

public class Building_Granja : BaseBuilding
{
    #region Configuration
    // =================================================================================================================
    // CONFIGURACIÓN ESPECÍFICA DE LA GRANJA (ASIGNAR EN INSPECTOR O EN START)
    // =================================================================================================================
    [Header("Configuración Específica de Granja")]
    [Tooltip("El ScriptableObject del recurso que esta granja produce (ej. Comida).")]
    [SerializeField] private RecurSO recursoAlimentoSO; // Ejemplo, asigna el SO de "Comida"

    [Tooltip("Cuántas unidades de alimento produce la granja por cada lote/evento.")]
    [SerializeField] private float unidadesAlimentoPorLote = 5f;

    [Tooltip("Tiempo en segundos entre cada lote de producción de alimento.")]
    [SerializeField] private float intervaloProduccionAlimento = 10f;

    // Ejemplo de necesidad de trabajadores (conceptual)
    // [SerializeField] private int trabajadoresNecesariosParaMaxEficiencia = 3;
    // private List<PersonajeBehaviour> _trabajadoresActuales = new List<PersonajeBehaviour>();
    #endregion

    #region Unity Lifecycle Methods
    // =================================================================================================================
    // MÉTODOS DEL CICLO DE VIDA DE UNITY
    // =================================================================================================================
    protected override void Start()
    {
        base.Start(); // Llama al Start de la clase base (importante para 'buildingName' por defecto)

        // --- Configuración específica de la Granja ---
        if (string.IsNullOrEmpty(buildingName)) // Si BaseBuilding.Start() no asignó uno
        {
            buildingName = "Granja de Cultivos";
        }
        buildingType = BuildingType.Estructura.Granja; // Asigna el tipo de estructura correcto

        // Configurar la producción de recursos
        producesResources = true; // Indicar que este edificio produce recursos

        if (recursoAlimentoSO != null)
        {
            resourceToProduceSO = recursoAlimentoSO; // Asigna el SO del recurso a producir (de BaseBuilding)
            unitsPerProductionBatch = unidadesAlimentoPorLote; // Asigna las unidades por lote (de BaseBuilding)
            intervalBetweenProduction = intervaloProduccionAlimento; // Asigna el intervalo (de BaseBuilding)
        }
        else
        {
            Debug.LogError($"'{buildingName}': No se ha asignado 'recursoAlimentoSO'. La granja no producirá.");
            producesResources = false; // Desactivar producción si no está bien configurada
        }

        // La activación (isActive = true) se maneja ahora a través de BuildingManager
        // al finalizar la construcción, llamando a buildingScript.ActivateBuilding().
    }
    #endregion

    #region Building Logic Overrides
    // =================================================================================================================
    // SOBREESCRITURA DE LÓGICA DEL EDIFICIO (DE BASEBUILDING)
    // =================================================================================================================

    /// <summary>
    /// Se llama cuando el edificio se activa (ej. después de ser construido).
    /// Aquí puedes añadir lógica específica de la granja que ocurra al activarse.
    /// </summary>
    public override void ActivateBuilding()
    {
        // Primero, verificar si la configuración de producción es válida antes de activar la lógica base
        if (resourceToProduceSO == null || unitsPerProductionBatch <= 0 || intervalBetweenProduction <= 0)
        {
            Debug.LogWarning($"'{buildingName}': No se puede activar la producción. Parámetros de producción no configurados correctamente.");
            // No llamar a base.ActivateBuilding() si no puede producir, o manejar 'isActive' de forma diferente.
            // Opcionalmente, permitir que se active pero no produzca.
            // Por ahora, si está mal configurada, no se activará la producción de BaseBuilding.
            // Pero el edificio sí puede estar "activo" para otras interacciones.
            this.isActive = true; // Marcar como activo para selección, etc.
                                  // pero la lógica de Update() en BaseBuilding no producirá.
            Debug.Log($"Edificio '{buildingName}' activado, pero la producción de recursos puede no funcionar debido a configuración incompleta.");
            return;
        }

        base.ActivateBuilding(); // Llama a la lógica base (pone isActive = true, resetea timer de producción).
        // Debug.Log($"La granja '{buildingName}' ha comenzado su producción de '{resourceToProduceSO.Nombre}'.");
        // Aquí podrías iniciar animaciones, efectos visuales de granja activa, etc.
    }

    /// <summary>
    /// Se llama cuando el edificio debe producir recursos.
    /// Aquí puedes añadir lógica específica de la granja (ej. consumir agua, depender de trabajadores).
    /// </summary>
    protected override void ExecuteProduction()
    {
        // Ejemplo de condición adicional: ¿La granja necesita agua para producir comida?
        // string aguaResourceName = "Agua"; // Asumir que tienes un recurso "Agua"
        // float aguaNecesariaPorLote = 1.0f;
        // if (ResourceManager.Instance != null && ResourceManager.Instance.TieneSuficiente(aguaResourceName, aguaNecesariaPorLote))
        // {
        //     ResourceManager.Instance.Gastar(aguaResourceName, aguaNecesariaPorLote);
        //     base.ExecuteProduction(); // Llama a la lógica de producción de BaseBuilding (que añade 'resourceToProduceSO')
        //     Debug.Log($"'{buildingName}' produjo alimento consumiendo {aguaNecesariaPorLote} de {aguaResourceName}.");
        // }
        // else
        // {
        //    Debug.LogWarning($"'{buildingName}': No hay suficiente '{aguaResourceName}' para producir. Producción detenida este ciclo.");
        //    // Aquí podrías querer que el _productionTimer de BaseBuilding no se resetee completamente,
        //    // o que la granja entre en un estado de "esperando recursos" y muestre un ícono.
        //    // Por ahora, si no se llama a base.ExecuteProduction(), simplemente no se produce nada en este ciclo.
        // }

        // Si no hay condiciones especiales, simplemente llama a la producción base:
        base.ExecuteProduction();
    }
    #endregion

    #region Player Interaction
    // =================================================================================================================
    // INTERACCIÓN DEL JUGADOR CON ESTE EDIFICIO
    // =================================================================================================================

    /// <summary>
    /// Se llama cuando el jugador hace clic sobre este edificio (si tiene un Collider).
    /// </summary>
    private void OnMouseDown()
    {
        // Evitar selección si el clic fue sobre un elemento de la UI.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Si el GameManager existe y no está en modo de colocar este mismo edificio.
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.EstructuraEnModoColocacion == this.gameObject)
            {
                // El jugador está intentando colocar este edificio, no seleccionarlo para interacción.
                return;
            }

            // Llama al método de selección del GameManager.
            GameManager.Instance.SeleccionarEstructura(this.gameObject);
            // Debug.Log($"'{buildingName}' (Granja) clickeado y enviado a GameManager para selección.");

            // Aquí podrías, por ejemplo, decirle a un UIManager que abra el panel específico de la Granja:
            // UIManager.Instance?.AbrirPanelInfoGranja(this);
        }
    }
    #endregion

    #region Farm-Specific Logic (Ejemplos para futuro)
    // =================================================================================================================
    // LÓGICA ESPECÍFICA DE LA GRANJA (PARA FUTURAS AMPLIACIONES)
    // =================================================================================================================
    /*
    public bool AsignarTrabajador(PersonajeBehaviour trabajador)
    {
        if (_trabajadoresActuales.Count < trabajadoresNecesariosParaMaxEficiencia)
        {
            _trabajadoresActuales.Add(trabajador);
            ActualizarEficienciaProduccion();
            return true;
        }
        return false;
    }

    public void QuitarTrabajador(PersonajeBehaviour trabajador)
    {
        if (_trabajadoresActuales.Remove(trabajador))
        {
            ActualizarEficienciaProduccion();
        }
    }

    private void ActualizarEficienciaProduccion()
    {
        // Lógica para cambiar 'unitsPerProductionBatch' o 'intervalBetweenProduction'
        // basado en el número de '_trabajadoresActuales'.
        // Ejemplo:
        // float eficienciaBase = 5f; // Unidades si está llena
        // unitsPerProductionBatch = eficienciaBase * ((float)_trabajadoresActuales.Count / trabajadoresNecesariosParaMaxEficiencia);
    }
    */
    #endregion
}
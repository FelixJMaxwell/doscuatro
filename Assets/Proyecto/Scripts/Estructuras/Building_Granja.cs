// Archivo: Building_Granja.cs
using UnityEngine;
using UnityEngine.EventSystems; // Para EventSystem.current.IsPointerOverGameObject()
using System.Collections.Generic; // Necesario para List

public class Building_Granja : BaseBuilding, ITrabajable
{
    #region Configuration
    // =================================================================================================================
    // CONFIGURACIÓN ESPECÍFICA DE LA GRANJA (ASIGNAR EN INSPECTOR O EN START)
    // =================================================================================================================
    [Header("Configuración Específica de Granja")]
    [Tooltip("El ScriptableObject del recurso que esta granja produce (ej. Comida).")]
    [SerializeField] private RecurSO recursoAlimentoSO; // Ejemplo, asigna el SO de "Comida"

    [Tooltip("Cuántas unidades de alimento produce la granja por cada lote/evento.")]
    [SerializeField] private float unidadesAlimentoPorLoteBase = 5f; // Renombrado a 'Base'
    [Tooltip("Tiempo en segundos entre cada lote de producción de alimento.")]
    [SerializeField] private float intervaloProduccionAlimentoBase = 10f; // Renombrado a 'Base'

    [Header("Gestión de Trabajadores")]
    [Tooltip("El número máximo de personajes que pueden trabajar en esta granja.")]
    public int maxTrabajadores = 3; // Límite de trabajadores
    private List<PersonajeBehaviour> _trabajadoresActuales = new List<PersonajeBehaviour>();

    // --- NUEVAS PROPIEDADES PARA EFECTO DE TRABAJADORES ---
    [Tooltip("Modificador a la producción por cada trabajador adicional después del primero.")]
    [SerializeField] private float bonusProduccionPorTrabajador = 0.5f; // Ej: 0.5 unidades extra por trabajador
    [Tooltip("Modificador al intervalo de producción por cada trabajador (negativo para hacerlo más rápido).")]
    [SerializeField] private float reduccionIntervaloPorTrabajador = 0.5f; // Ej: 0.5 segundos menos por trabajador
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

        // Configurar la producción de recursos (valores base)
        producesResources = true; // Indicar que este edificio produce recursos

        if (recursoAlimentoSO != null)
        {
            resourceToProduceSO = recursoAlimentoSO; // Asigna el SO del recurso a producir (de BaseBuilding)
            unitsPerProductionBatch = unidadesAlimentoPorLoteBase; // Asigna las unidades por lote (de BaseBuilding)
            intervalBetweenProduction = intervaloProduccionAlimentoBase; // Asigna el intervalo (de BaseBuilding)
        }
        else
        {
            Debug.LogError($"'{buildingName}': No se ha asignado 'recursoAlimentoSO'. La granja no producirá.");
            producesResources = false; // Desactivar producción si no está bien configurada
        }

        // Llamamos a esto en Start para que se calculen los valores iniciales
        // (incluso si no hay trabajadores, aseguramos que 'unitsPerProductionBatch' y 'intervalBetweenProduction'
        // tengan los valores base del Inspector).
        ActualizarProduccionBasadaEnTrabajadores();

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
            this.isActive = true; // Marcar como activo para selección, etc.
            Debug.Log($"Edificio '{buildingName}' activado, pero la producción de recursos puede no funcionar debido a configuración incompleta.");
            return;
        }

        base.ActivateBuilding(); // Llama a la lógica base (pone isActive = true, resetea timer de producción).
        Debug.Log($"La granja '{buildingName}' ha comenzado su producción de '{resourceToProduceSO.Nombre}'.");
        // Aquí podrías iniciar animaciones, efectos visuales de granja activa, etc.
    }

    /// <summary>
    /// Se llama cuando el edificio debe producir recursos.
    /// Aquí puedes añadir lógica específica de la granja (ej. consumir agua, depender de trabajadores).
    /// </summary>
    protected override void ExecuteProduction()
    {
        // Condición de producción: ¿Hay al menos 1 trabajador?
        // Puedes ajustar esto si quieres que produzca algo incluso sin trabajadores, pero con menos eficiencia.
        if (_trabajadoresActuales.Count == 0)
        {
            // Opcional: Producir una cantidad muy pequeña o nada si no hay trabajadores.
            // Debug.Log($"'{buildingName}': No hay trabajadores asignados. Producción mínima o nula.");
            // Si no queremos que produzca nada, simplemente retornamos.
            return;
        }

        // Llama a la lógica de producción de BaseBuilding
        base.ExecuteProduction();
        Debug.Log($"'{buildingName}' produjo {unitsPerProductionBatch} de {resourceToProduceSO.Nombre} con {_trabajadoresActuales.Count} trabajadores.");
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

    #region ITrabajable Implementation
    // =================================================================================================================
    // IMPLEMENTACIÓN DE LA INTERFAZ ITrabajable
    // =================================================================================================================

    /// <summary>
    /// Añade un personaje a la lista de trabajadores de esta granja.
    /// </summary>
    /// <param name="trabajador">El PersonajeBehaviour que se va a añadir.</param>
    /// <returns>True si el trabajador fue añadido, false si ya estaba o si la granja está llena.</returns>
    public bool AnadirTrabajador(PersonajeBehaviour trabajador)
    {
        if (_trabajadoresActuales.Count >= maxTrabajadores)
        {
            Debug.LogWarning($"'{trabajador.nombre}' no puede ser añadido a '{buildingName}'. Máximo de trabajadores ({maxTrabajadores}) alcanzado.");
            return false;
        }
        if (!_trabajadoresActuales.Contains(trabajador))
        {
            _trabajadoresActuales.Add(trabajador);
            Debug.Log($"'{trabajador.nombre}' ha sido añadido como trabajador a '{buildingName}'. Trabajadores: {_trabajadoresActuales.Count}/{maxTrabajadores}");
            ActualizarProduccionBasadaEnTrabajadores(); // Recalcula la producción al añadir
            return true;
        }
        return false;
    }

    /// <summary>
    /// Quita un personaje de la lista de trabajadores de esta granja.
    /// </summary>
    /// <param name="trabajador">El PersonajeBehaviour que se va a quitar.</param>
    public void QuitarTrabajador(PersonajeBehaviour trabajador)
    {
        if (_trabajadoresActuales.Remove(trabajador))
        {
            Debug.Log($"'{trabajador.nombre}' ha sido quitado como trabajador de '{buildingName}'. Trabajadores: {_trabajadoresActuales.Count}/{maxTrabajadores}");
            ActualizarProduccionBasadaEnTrabajadores(); // Recalcula la producción al quitar
        }
    }

    /// <summary>
    /// Método para que los NPCs "contribuyan" a la granja.
    /// Este método puede ser llamado por PersonajeBehaviour cuando está en estado "Trabajando".
    /// Aquí simplemente confirmamos que el NPC está trabajando y la lógica de producción está en ExecuteProduction.
    /// </summary>
    /// <param name="trabajador">El personaje que está contribuyendo.</param>
    public void ContribuirProduccion(PersonajeBehaviour trabajador)
    {
        if (!_trabajadoresActuales.Contains(trabajador))
        {
            Debug.LogWarning($"'{trabajador.nombre}' intentó contribuir a '{buildingName}' pero no es un trabajador asignado.");
            return;
        }
        // No necesitamos hacer la llamada a ResourceManager.Instance.AddResource aquí
        // porque la lógica de producción ya está en ExecuteProduction() de BaseBuilding,
        // que se llama cada 'intervalBetweenProduction'.
        // Aquí podríamos, por ejemplo, mejorar la "habilidad" del trabajador o su felicidad.
        // trabajador.ModificarHabilidadTrabajo(0.1f); // Ejemplo
    }
    #endregion

    #region Farm-Specific Production Logic
    // =================================================================================================================
    // LÓGICA DE PRODUCCIÓN ESPECÍFICA DE LA GRANJA (AFECTADA POR TRABAJADORES)
    // =================================================================================================================

    /// <summary>
    /// Actualiza los valores de producción y el intervalo basados en el número de trabajadores.
    /// </summary>
    private void ActualizarProduccionBasadaEnTrabajadores()
    {
        float trabajadoresActivos = _trabajadoresActuales.Count;

        // Calcular la producción por lote
        // Si no hay trabajadores, quizás la producción es 0 o muy baja.
        if (trabajadoresActivos == 0)
        {
            unitsPerProductionBatch = 0; // No produce nada sin trabajadores
        }
        else
        {
            // Producción base + (trabajadores - 1) * bonus por trabajador adicional
            unitsPerProductionBatch = unidadesAlimentoPorLoteBase + (trabajadoresActivos * bonusProduccionPorTrabajador);
        }

        // Calcular el intervalo de producción (se reduce con más trabajadores)
        // Asegurarse de que el intervalo no sea negativo o cero.
        intervalBetweenProduction = Mathf.Max(1f, intervaloProduccionAlimentoBase - (trabajadoresActivos * reduccionIntervaloPorTrabajador));

        // Debug.Log($"Granja '{buildingName}' actualizada. Trabajadores: {trabajadoresActivos}, Producción/Lote: {unitsPerProductionBatch:F2}, Intervalo: {intervalBetweenProduction:F2}s");
    }
    #endregion
}
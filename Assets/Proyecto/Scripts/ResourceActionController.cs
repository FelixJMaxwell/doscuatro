// Archivo: ResourceActionController.cs (Panel de Trucos para Desarrolladores)
using UnityEngine;
using UnityEngine.UI;    // Necesario para Button
using System.Collections.Generic; // Necesario para List
// Asegúrate de tener using para tu namespace si RecurSO está en uno.

// Esta clase se puede definir al principio del archivo ResourceActionController.cs o en su propio archivo.
[System.Serializable]
public class ResourceActionConfig
{
    [Tooltip("Descripción para identificar esta acción en el Inspector (ej. 'Añadir Madera').")]
    public string actionDescription = "Añadir Recurso X";

    [Tooltip("El botón de la UI que disparará esta acción.")]
    public Button actionButton;

    [Header("Recurso a Añadir")]
    [Tooltip("El ScriptableObject del recurso que se añadirá.")]
    public RecurSO resourceToAddSO; // El RecurSO que define el tipo de recurso
    [Tooltip("La cantidad de 'resourceToAddSO' que se añadirá al hacer clic.")]
    public float amountToAdd = 100f; // Cantidad a añadir, configurable
}

public class ResourceActionController : MonoBehaviour
{
    #region Inspector Configuration
    // =================================================================================================================
    // CONFIGURACIÓN DE ACCIONES DESDE EL INSPECTOR
    // =================================================================================================================
    [Header("Configuración de Botones para Añadir Recursos (Modo Desarrollador)")]
    [Tooltip("Define cada botón, el recurso que añade y la cantidad.")]
    public List<ResourceActionConfig> resourceActions;
    #endregion

    #region Unity Lifecycle Methods
    // =================================================================================================================
    // MÉTODOS DEL CICLO DE VIDA DE UNITY
    // =================================================================================================================
    void Start()
    {
        ConfigureActionButtons();
    }
    #endregion

    #region Button Configuration
    // =================================================================================================================
    // CONFIGURACIÓN DE LOS BOTONES
    // =================================================================================================================
    private void ConfigureActionButtons()
    {
        if (resourceActions == null || resourceActions.Count == 0)
        {
            Debug.LogWarning("ResourceActionController: No hay acciones de recurso configuradas.");
            return;
        }

        foreach (ResourceActionConfig actionConfig in resourceActions)
        {
            if (actionConfig.actionButton == null)
            {
                Debug.LogWarning($"ResourceActionController: Botón no asignado para la acción '{actionConfig.actionDescription}'.");
                continue;
            }
            if (actionConfig.resourceToAddSO == null)
            {
                Debug.LogWarning($"ResourceActionController: 'resourceToAddSO' no asignado para la acción '{actionConfig.actionDescription}' en el botón '{actionConfig.actionButton.name}'. El botón será desactivado.");
                actionConfig.actionButton.interactable = false; // Desactivar si no hay recurso que añadir
                continue;
            }

            // Limpiar listeners anteriores
            actionConfig.actionButton.onClick.RemoveAllListeners();

            // Capturar la configuración localmente para la clausura de la lambda
            ResourceActionConfig currentConfig = actionConfig;

            // Añadir el listener
            actionConfig.actionButton.onClick.AddListener(() =>
            {
                ExecuteResourceAction(currentConfig); // Llamar al método simplificado
            });

            // En modo desarrollador, los botones estarán siempre activos si están bien configurados
            actionConfig.actionButton.interactable = true;
        }
    }
    #endregion

    #region Action Logic
    // =================================================================================================================
    // LÓGICA DE EJECUCIÓN DE LA ACCIÓN (SIMPLIFICADA)
    // =================================================================================================================
    private void ExecuteResourceAction(ResourceActionConfig config)
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceActionController: ResourceManager.Instance no encontrado.");
            return;
        }

        if (config.resourceToAddSO == null)
        {
            Debug.LogError($"Acción '{config.actionDescription}': resourceToAddSO es nulo. No se puede añadir recurso.");
            return;
        }

        // Añadir el recurso especificado sin verificar ni gastar costos
        if (config.amountToAdd > 0)
        {
            ResourceManager.Instance.Añadir(config.resourceToAddSO.Nombre, config.amountToAdd);
            Debug.Log($"MODO DESARROLLADOR: Se añadieron {config.amountToAdd} de '{config.resourceToAddSO.Nombre}'.");
        }
        else if (config.amountToAdd < 0) // Permitir restar recursos si se configura cantidad negativa
        {
            ResourceManager.Instance.Gastar(config.resourceToAddSO.Nombre, -config.amountToAdd);
            Debug.Log($"MODO DESARROLLADOR: Se restaron {-config.amountToAdd} de '{config.resourceToAddSO.Nombre}'.");
        }
        else // amountToAdd es 0
        {
            Debug.Log($"MODO DESARROLLADOR: Acción '{config.actionDescription}' configurada para añadir 0 de '{config.resourceToAddSO.Nombre}'. No se hizo nada.");
        }

        // No es necesario actualizar el estado de otros botones basado en costos,
        // ya que no hay costos.
    }
    #endregion

    // Las secciones de UI Update (para interactividad basada en costos) ya no son necesarias.
}

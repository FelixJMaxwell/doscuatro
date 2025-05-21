using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Enum for different resource types (optional, but good for organization)
public enum ResourceType
{
    None,
    Food,
    Water,
    Wood,
    Stone,
    Metal,
    Cloth,
    Medicine,
    Knowledge,
    Faith
}

// Centralized resource manager in the game
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // List of initial RecurSO ScriptableObjects to be loaded at startup
    public List<RecurSO> recursosIniciales;

    // Dictionary to store active resource instances, keyed by resource name
    [SerializeField] public Dictionary<string, RecursoInstancia> recursos = new();

    // UI TextMeshProUGUI element to display Faith amount
    public TextMeshProUGUI FeUI;

    // Transform for visual pillars related to Faith (e.g., in MonolitoBehaviour)
    [SerializeField] private Transform FaithHolder;

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject); to persist between scenes
            InicializarRecursos();
        }
        else
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }

    // Initializes resources from the list of RecurSO ScriptableObjects
    private void InicializarRecursos()
    {
        recursos.Clear(); // Ensure the dictionary is empty before initializing

        if (recursosIniciales == null)
        {
            Debug.LogWarning("ResourceManager: The initial resources list is null. No resources will be initialized.");
            return;
        }

        foreach (var recursoSO in recursosIniciales)
        {
            // Validate the RecurSO before attempting to use it
            if (ValidarRecurSO(recursoSO))
            {
                if (!recursos.ContainsKey(recursoSO.Nombre))
                {
                    // Create a new RecursoInstancia and add it to the dictionary
                    recursos[recursoSO.Nombre] = new RecursoInstancia
                    {
                        data = recursoSO,
                        actual = recursoSO.ValorBase // Initialize with base value
                    };
                    //Debug.Log($"ResourceManager: Resource initialized: {recursoSO.Nombre}, Amount: {recursoSO.ValorBase}, Max: {recursoSO.Maximo}");
                }
                else
                {
                    Debug.LogWarning($"ResourceManager: Resource '{recursoSO.Nombre}' has already been initialized. Duplication will be ignored.");
                }
            }
        }
    }

    // Method to validate basic configuration of a RecurSO
    private bool ValidarRecurSO(RecurSO recurso)
    {
        if (recurso == null)
        {
            Debug.LogError("ResourceManager: Attempted to use a null RecurSO. Please assign a valid ScriptableObject in the Inspector.");
            return false;
        }
        if (string.IsNullOrEmpty(recurso.Nombre))
        {
            Debug.LogError($"ResourceManager: RecurSO '{recurso.name}' has a null or empty name. Please assign a valid name to the ScriptableObject!");
            return false;
        }
        // Check if Maximo is zero or less, which would prevent adding resources
        // The 'Faith' exception is kept here as per previous discussion, adjust if needed.
        if (recurso.Maximo <= 0 && recurso.Nombre != "Faith")
        {
            Debug.LogWarning($"ResourceManager: RecurSO '{recurso.Nombre}' has a Maximo value of 0 or less ({recurso.Maximo}). This will prevent resources from being added and might cause unexpected behavior. Please set a positive Maximo value if this resource should be storable.");
            // You might decide if this should be a fatal error or just a warning based on game design.
        }
        return true;
    }

    // Resets all resources to their initial maximum or base value
    public void ResetearRecursos()
    {
        foreach (var recursoSO in recursosIniciales)
        {
            if (recursos.ContainsKey(recursoSO.Nombre))
            {
                recursos[recursoSO.Nombre].actual = recursoSO.Maximo; // Reset to Maximo
                // Or: recursos[recursoSO.Nombre].actual = recursoSO.ValorBase; // Reset to BaseValue
            }
        }
        // Update all relevant UI elements after resetting
        ActualizarRecursoUI("Fe"); // Example for Fe, extend for others
    }

    // Adds a specified quantity to a resource
    public void Añadir(string nombre, float cantidad)
    {
        if (recursos.TryGetValue(nombre, out var instancia))
        {
            instancia.Añadir(cantidad);
            ActualizarRecursoUI(nombre); // Update UI for this resource
        }
        else
        {
            Debug.LogError($"ResourceManager: Resource '{nombre}' not found. Cannot add {cantidad}.");
        }
    }

    // Spends a specified quantity from a resource
    public void Gastar(string nombre, float cantidad)
    {
        if (recursos.TryGetValue(nombre, out var instancia))
        {
            // The Gastar method in RecursoInstancia already handles the check for sufficient quantity
            instancia.Gastar(cantidad);
            ActualizarRecursoUI(nombre); // Update UI for this resource

            // Specific visual logic for "Fe" related to pillars
            if (nombre == "Fe")
            {
                if (FaithHolder != null)
                {
                    // Calculate how many pillars to affect based on the quantity of Fe spent
                    int pillarsToAffect = Mathf.FloorToInt(cantidad) * 3; 

                    for (int i = 0; i < pillarsToAffect && i < FaithHolder.childCount; i++)
                    {
                        PilarBehaviour tempPilar = FaithHolder.GetChild(i).GetComponent<PilarBehaviour>();
                        if (tempPilar != null)
                        {
                            tempPilar.Bajar = true; // Set pillar to go down (assuming PilarBehaviour has this property)
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("ResourceManager: FaithHolder is not assigned for pillar animation. Assign it in the Inspector.");
                }
            }
        }
        else
        {
            Debug.LogError($"ResourceManager: Resource '{nombre}' not found. Cannot spend {cantidad}.");
        }
    }

    // Gets the current quantity of a resource
    public float GetCantidad(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.actual : 0f;
    }

    // Gets the maximum capacity of a resource
    public float GetMaximo(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.Maximo : 0f;
    }

    // Gets the base value of a resource (from its RecurSO data)
    public float GetValorBase(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.BaseValue : 0f;
    }

    // Helper method to update UI for specific resources
    private void ActualizarRecursoUI(string nombreRecurso)
    {
        // This is a simplified example. In a real game, you'd have a more robust UI manager
        // that maps resource names to specific TextMeshProUGUI elements.
        if (nombreRecurso == "Fe" && FeUI != null)
        {
            FeUI.text = $"Fe: {GetCantidad("Fe").ToString("F0")} / {GetMaximo("Fe").ToString("F0")}";
        }
        // Add more else if blocks here for other resources if they have dedicated UI elements
        // For example:
        // else if (nombreRecurso == "Fragmento" && FragmentoUI != null)
        // {
        //     FragmentoUI.text = $"Fragmentos: {GetCantidad("Fragmento").ToString("F0")}";
        // }
    }
}
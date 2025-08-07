using UnityEngine;

public class Cheats : MonoBehaviour
{
    public static Cheats Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Opcional, si quieres que persista en escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Añade una cantidad específica de un recurso al sistema de recursos.
    /// Este método se enlaza directamente a los botones de la UI en el Inspector.
    /// </summary>
    /// <param name="resourceName">El nombre del recurso a añadir.</param>
    /// <param name="amountToAdd">La cantidad del recurso que se va a añadir.</param>
    public void AddResourceCheat(string resourceName, float amountToAdd)
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.Añadir(resourceName, amountToAdd);
            Debug.Log($"Cheat activado: Añadidos {amountToAdd} de {resourceName}.");
        }
        else
        {
            Debug.LogError("ResourceManager no disponible para añadir recursos.");
        }
    }
}

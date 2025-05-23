using System.Collections.Generic;
using UnityEngine;

// Gestiona las casas disponibles en el juego para asignar NPCs.
public class HousingManager : MonoBehaviour
{
    // Implementación del patrón Singleton.
    public static HousingManager Instance { get; private set; }

    // Lista de todas las instancias de Building_Casa que están activas y registradas.
    // Considera hacerla privada y exponer métodos para obtener información si es necesario.
    [SerializeField] // Para verla en Inspector (debug).
    private List<Building_Casa> casasDisponibles = new List<Building_Casa>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Si debe persistir entre escenas.
        }
        else
        {
            Debug.LogWarning("Ya existe una instancia de HousingManager. Destruyendo este duplicado.");
            Destroy(gameObject);
        }
    }

    // Registra una nueva casa en el sistema. Llamado por Building_Casa.Start() o al ser construida.
    public void RegistrarCasa(Building_Casa casa)
    {
        if (casa != null && !casasDisponibles.Contains(casa))
        {
            casasDisponibles.Add(casa);
            // Debug.Log($"Casa '{casa.buildingName}' registrada. Total de casas: {casasDisponibles.Count}");
        }
        else if (casa == null)
        {
            Debug.LogWarning("HousingManager: Se intentó registrar una casa nula.");
        }
        // else if (casasDisponibles.Contains(casa))
        // {
        //     Debug.LogWarning($"HousingManager: La casa '{casa.buildingName}' ya está registrada.");
        // }
    }

    // Busca una casa que tenga espacio disponible.
    public Building_Casa BuscarCasaDisponible()
    {
        foreach (var casa in casasDisponibles)
        {
            // Asegurarse que la casa no sea nula (podría haber sido destruida y la lista aún no actualizada).
            if (casa != null && casa.HayEspacio())
            {
                return casa; // Devuelve la primera casa encontrada con espacio.
            }
        }
        return null; // No se encontró ninguna casa con espacio.
    }

    // Quita una casa del sistema (ej. si es destruida). Llamado por Building_Casa.OnDestroy().
    public void QuitarCasa(Building_Casa casa)
    {
        if (casa != null && casasDisponibles.Contains(casa))
        {
            casasDisponibles.Remove(casa);
            // Debug.Log($"Casa '{casa.buildingName}' quitada del registro. Total de casas: {casasDisponibles.Count}");
        }
        // else if (casa == null)
        // {
        //     Debug.LogWarning("HousingManager: Se intentó quitar una casa nula.");
        // }
    }

    // Podrías añadir métodos útiles como:
    // public int GetTotalCapacidadVivienda() { ... }
    // public int GetTotalHabitantes() { ... }
    // public int GetEspaciosDisponiblesTotales() { ... }
}
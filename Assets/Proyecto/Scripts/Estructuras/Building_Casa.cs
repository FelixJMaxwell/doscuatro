using System.Collections.Generic;
using UnityEngine;

// Se asume que BaseBuilding es una clase de la que esta hereda,
// similar a tus otros scripts de edificios (ej. Building_Granja).
public class Building_Casa : BaseBuilding
{
    [Header("Configuración de Casa")]
    public int CapacidadMaxima = 1; // Capacidad por defecto, ajústala en el Inspector.
    // Lista para almacenar los PersonajeBehaviour que actualmente habitan esta casa.
    public List<PersonajeBehaviour> Habitantes = new List<PersonajeBehaviour>();

    // Llamado cuando el edificio es inicializado.
    protected override void Start()
    {
        base.Start(); // Ejecuta la lógica de Start de la clase BaseBuilding.
        // Asigna un nombre por defecto si no se ha especificado uno.
        if (string.IsNullOrEmpty(buildingName))
        {
            buildingName = "Casa";
        }
        // Define el tipo de estructura (debes tener este enum definido en BuildingType.cs).
        TipoEstructura = BuildingType.Estructura.CasaBasica; // Ejemplo, asegúrate que el enum coincida.
        RegistrarCasa(); // Registra esta casa en el HousingManager.
    }

    // Verifica si hay espacio disponible en la casa.
    public bool HayEspacio()
    {
        return Habitantes.Count < CapacidadMaxima;
    }

    // Asigna un personaje a esta casa.
    // Devuelve true si el personaje fue asignado, false en caso contrario.
    public bool AsignarHabitante(PersonajeBehaviour personaje)
    {
        // Verifica si hay espacio y si el personaje no es nulo y no está ya en la lista.
        if (personaje != null && HayEspacio() && !Habitantes.Contains(personaje))
        {
            Habitantes.Add(personaje);
            personaje.AsignarCasa(this); // Informa al personaje sobre su nueva casa.
            Debug.Log($"Personaje '{personaje.nombre}' asignado a la casa '{buildingName}'.");
            return true;
        }
        Debug.LogWarning($"No se pudo asignar a '{personaje?.nombre ?? "Personaje Nulo"}' a la casa '{buildingName}'. Espacio disponible: {HayEspacio()}, ¿Ya es habitante?: {Habitantes.Contains(personaje)}");
        return false;
    }

    // Quita un personaje de esta casa.
    // Devuelve true si el personaje fue quitado, false en caso contrario.
    public bool QuitarHabitante(PersonajeBehaviour personaje)
    {
        // Verifica si el personaje no es nulo y está en la lista de habitantes.
        if (personaje != null && Habitantes.Contains(personaje))
        {
            Habitantes.Remove(personaje);
            // Si esta era la casa del personaje, informa al personaje que ya no tiene casa.
            if (personaje.casaAsignada == this)
            {
                personaje.DesasignarCasa();
            }
            Debug.Log($"Personaje '{personaje.nombre}' quitado de la casa '{buildingName}'.");
            return true;
        }
        Debug.LogWarning($"Personaje '{personaje?.nombre ?? "Personaje Nulo"}' no es habitante de '{buildingName}', no se puede quitar.");
        return false;
    }

    // Método llamado por un PersonajeBehaviour (ej. en su método Morir)
    // para asegurar que se elimina de la lista de habitantes de esta casa.
    public void RemoverPersonaPorComportamiento(PersonajeBehaviour personaje)
    {
        if (personaje != null && Habitantes.Contains(personaje))
        {
            Habitantes.Remove(personaje);
            // No se llama a personaje.DesasignarCasa() aquí, ya que el personaje
            // está manejando su propio estado (ej. está muriendo).
            Debug.Log($"Personaje '{personaje.nombre}' removido de '{buildingName}' debido a su propio comportamiento (ej. Morir).");
        }
    }

    // Registra esta casa en el HousingManager.
    public void RegistrarCasa()
    {
        if (HousingManager.Instance != null)
        {
            HousingManager.Instance.RegistrarCasa(this);
        }
        else
        {
            // Es importante manejar este caso, quizás reintentar o tener un fallback.
            Debug.LogError($"HousingManager.Instance no encontrado. No se pudo registrar la casa: '{buildingName ?? gameObject.name}'.");
        }
    }

    // Llamado automáticamente por Unity cuando el GameObject es destruido.
    protected virtual void OnDestroy()
    {
        // Si HousingManager existe, quita esta casa de su lista.
        if (HousingManager.Instance != null)
        {
            HousingManager.Instance.QuitarCasa(this);
        }

        // Asegura que todos los habitantes sean desasignados de esta casa.
        // Es importante hacer una copia de la lista si se modifica durante la iteración,
        // o iterar de forma segura. En este caso, solo leemos y llamamos a un método en otro objeto.
        foreach (PersonajeBehaviour habitante in new List<PersonajeBehaviour>(Habitantes)) // Itera sobre una copia para evitar problemas si DesasignarCasa modifica la lista indirectamente.
        {
            if (habitante != null && habitante.casaAsignada == this)
            {
                habitante.DesasignarCasa();
            }
        }
        Habitantes.Clear(); // Limpia la lista de habitantes.
        // Debug.Log($"Casa '{buildingName}' destruida y quitada del HousingManager.");
    }
}
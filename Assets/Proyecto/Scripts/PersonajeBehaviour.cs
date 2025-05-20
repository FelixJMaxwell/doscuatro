using UnityEngine;
using System.Collections.Generic;

public class PersonajeBehaviour : MonoBehaviour
{
    // Variables básicas
    public string nombre { get; private set; }
    public int edad { get; private set; }
    public string genero { get; private set; }

    // Estado
    public float salud { get; private set; }
    public string ropa { get; private set; }
    public string herramienta { get; private set; }
    public int felicidad { get; private set; }

    //Trabajo
    public string trabajo {get; private set;}
    //Educacion
    public string educacion {get; private set;}

    // Inventario
    public List<GameObject> inventario = new List<GameObject>();

    // Relaciones
    private Dictionary<PersonajeBehaviour, int> relaciones = new Dictionary<PersonajeBehaviour, int>();

    private void Start()
    {
        // Inicialización
        felicidad = 100; // Valor inicial de felicidad
    }

    public void Inicializar(NPCDataSO data)
    {
        // Información general
        nombre = data.npcNombre;
        edad = data.edad;
        genero = data.genero;

        // Estado
        salud = data.saludBase;
        ropa = "Ropa Básica"; // Valor inicial
        herramienta = "Ninguna"; // Valor inicial
        trabajo = data.trabajo;
        educacion = data.educacion;
    }

    public void RecibirDaño(float daño)
    {
        salud -= daño;
        if (salud <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        // Aquí también deberías limpiar las relaciones del NPC muerto con otros NPCs
        foreach (var relacion in relaciones)
        {
            if (relacion.Key != null) // Verifica que el NPC relacionado no sea nulo (ya destruido)
            {
                relacion.Key.EliminarRelacion(this); // Llama a un método para eliminar la relación en el otro NPC
            }
        }
        relaciones.Clear(); // Limpia las relaciones de este NPC
        Destroy(gameObject);
    }

    public void CambiarRopa(string nuevaRopa)
    {
        ropa = nuevaRopa;
    }

    public void CambiarHerramienta(string nuevaHerramienta)
    {
        herramienta = nuevaHerramienta;
    }

     public void ModificarFelicidad(int cantidad)
    {
        felicidad += cantidad;
        // Asegurar que la felicidad esté dentro de un rango válido (ejemplo: 0-100)
        felicidad = Mathf.Clamp(felicidad, 0, 100);
    }

    // Métodos para gestionar relaciones
    public void ConocerNuevoNPC(PersonajeBehaviour otroNPC, int relacionInicial = 0)
    {
        if (otroNPC != null && !relaciones.ContainsKey(otroNPC))
        {
            relaciones.Add(otroNPC, relacionInicial);
            otroNPC.ConocerNuevoNPC(this, relacionInicial); // Establecer la relación en ambos sentidos
        }
    }

    public void ModificarRelacion(PersonajeBehaviour otroNPC, int cantidad)
    {
        if (otroNPC != null && relaciones.ContainsKey(otroNPC))
        {
            relaciones[otroNPC] += cantidad;
            // Puedesclamp la relación dentro de un rango (ejemplo: -100 a 100)
            relaciones[otroNPC] = Mathf.Clamp(relaciones[otroNPC], -100, 100);
        }
    }

     public int ObtenerRelacion(PersonajeBehaviour otroNPC)
    {
        if (otroNPC != null && relaciones.ContainsKey(otroNPC))
        {
            return relaciones[otroNPC];
        }
        return 0; // Valor por defecto si no hay relación
    }

    public void EliminarRelacion(PersonajeBehaviour otroNPC)
    {
        if (otroNPC != null && relaciones.ContainsKey(otroNPC))
        {
            relaciones.Remove(otroNPC);
        }
    }
}

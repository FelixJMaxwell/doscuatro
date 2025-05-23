// Archivo: NivelEdificioPersonajeSO.cs
using UnityEngine;
using System.Collections.Generic; // Si 'ObjetoProfesion' usa List o necesitas otras colecciones

[CreateAssetMenu(fileName = "NuevoNivelEdificioPersonaje", menuName = "Culto del Monolito/Configuración de Nivel/Edificio Personaje")]
public class NivelEdificioPersonajeSO : ScriptableObject
{
    [Header("Identificación del Nivel")]
    public int numeroDeNivel; // Ej. 1, 2, 3... para referencia.
    public string nombreDelNivel; // Ej. "Santuario Básico", "Altar de Invocación Avanzado"

    [Header("Parámetros de Generación EN este Nivel")]
    public float tiempoEntreGeneraciones;
    public int maxPersonajesPermitidos;
    public float costoFeGeneracionNormal;
    public float costoFragmentoGeneracionLegendario;

    [Header("Costos para MEJORAR AL SIGUIENTE Nivel (desde este nivel)")]
    public float costoMejoraFeAlSiguienteNivel;
    public float costoMejoraFragmentoAlSiguienteNivel;
    // La struct ObjetoProfesion puede seguir definida en Building_Personajes.cs o moverla a un scope más global si es necesario.
    public List<Building_Personajes.ObjetoProfesion> costoMejoraHerramientasAlSiguienteNivel;

    [Header("Desbloqueos AL LLEGAR a este Nivel")]
    public bool requiereObjetosProfesionAlLlegar;
    public bool desbloqueaLegendariosAlLlegar;

    // Podrías añadir aquí incluso referencias a prefabs de modelos diferentes para el edificio en este nivel,
    // o efectos especiales, etc.
    // public GameObject modeloEdificioNivel;
}
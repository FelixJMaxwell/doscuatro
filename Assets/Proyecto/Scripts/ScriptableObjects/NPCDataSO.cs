using UnityEngine;
// using System.Collections.Generic; // No se usa actualmente.

[CreateAssetMenu(fileName = "NuevoNPCData", menuName = "Culto del Monolito/Configuración NPC")] // Nombre de menú mejorado
public class NPCDataSO : ScriptableObject
{
    [Header("Información General")]
    [Tooltip("Nombre del tipo de NPC o nombre base si se generan múltiples.")]
    public string npcNombre;
    // 'profesion' parece redundante si tienes 'trabajo'. Considera unificar.
    // public string profesion; // Podría ser un enum o estar definido por el 'trabajo'.

    [Tooltip("Prefab del modelo 3D/2D que se instanciará para este NPC.")]
    public GameObject prefabModelo; // Clave para la instanciación.

    [Tooltip("Marcar si este tipo de NPC es inherentemente legendario/raro.")]
    public bool esLegendario = false; // Cambiado a booleano para dos estados de rareza.

    [Header("Estado Base")]
    [Tooltip("Salud inicial del NPC al ser generado.")]
    public float saludBase = 100f;
    [Tooltip("Edad inicial del NPC.")]
    public int edad; // Considera rangos de edad Min/Max si quieres variabilidad.
    [Tooltip("Género del NPC (ej. Masculino, Femenino, Otro).")]
    public string genero; // Podría ser un enum para consistencia.

    [Header("Ocupación")] // Unificado 'Trabajo' y 'Educacion' bajo 'Ocupación'.
    [Tooltip("Trabajo o rol principal que este tipo de NPC puede desempeñar.")]
    public string trabajo; // Podría ser un enum o referenciar otro SO de "Trabajo".
    [Tooltip("Nivel de educación o tipo de conocimiento que este NPC posee o puede adquirir.")]
    public string educacion; // Similar a 'trabajo', podría ser más estructurado.
}
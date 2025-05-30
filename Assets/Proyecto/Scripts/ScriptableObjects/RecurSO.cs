// Archivo: RecurSO.cs
using UnityEngine;

// Define los datos base para un tipo de recurso en el juego.
// Permite crear assets de recursos desde el menú de Unity.
[CreateAssetMenu(fileName = "NuevoRecurso", menuName = "Culto del Monolito/Recurso")]
public class RecurSO : ScriptableObject
{
    #region Propiedades Principales del Recurso
    // =================================================================================================================
    // PROPIEDADES FUNDAMENTALES DEL RECURSO
    // =================================================================================================================
    [Header("Definición Básica del Recurso")]
    [Tooltip("Nombre único identificador del recurso (ej. Fe, Madera, Comida). Usado como clave en ResourceManager.")]
    public string Nombre;

    [Tooltip("Cantidad máxima de este recurso que se puede almacenar. Considera usar float.MaxValue si es ilimitado.")]
    public float Maximo = 1000f; // Valor por defecto

    [Tooltip("Valor inicial del recurso al empezar el juego o cuando el recurso se introduce por primera vez.")]
    public float ValorBase = 0f; // Valor por defecto
    #endregion

    #region Sistema de Niveles del Recurso (Opcional)
    // =================================================================================================================
    // CONFIGURACIÓN PARA RECURSOS QUE PUEDEN SUBIR DE NIVEL
    // (Ejemplo: La "Fe" podría tener niveles que afecten al Monolito o a su capacidad de almacenamiento)
    // =================================================================================================================
    [Header("Sistema de Niveles (Opcional)")]
    [Tooltip("Marcar si este recurso específico tiene un sistema de niveles asociado. Si no, los siguientes campos se ignoran.")]
    public bool requiereNivel; // Si este recurso puede ser "mejorado" o tener niveles

    [Tooltip("Si 'requiereNivel' es true, este es el NOMBRE de otro recurso (ej. 'Fragmentos', 'PuntosDeCulto') necesario para pagar la subida de nivel de ESTE recurso.")]
    public string recursoRequeridoParaNivel; // El nombre del recurso que se consume para subir de nivel este.

    [Tooltip("Factor que ajusta la curva de costo para subir de nivel. <1 suaviza, >1 endurece. (Usado en la fórmula de costo).")]
    public float factorNivel = 0.75f;

    [Tooltip("Costo base (en 'recursoRequeridoParaNivel') para subir ESTE recurso al primer nivel efectivo (o de nivel 0 a 1).")]
    public float costoBaseNivel = 10f;

    [Tooltip("Multiplicador del costo por cada nivel adicional de ESTE recurso (ej. 1.5 = 50% más caro cada nivel).")]
    public float incrementoPorNivel = 1.5f;

    /// <summary>
    /// Calcula la cantidad necesaria del 'recursoRequeridoParaNivel' para subir ESTE recurso
    /// desde su 'nivelActual' al siguiente.
    /// </summary>
    /// <param name="nivelActual">El nivel actual del recurso (ej. Nivel de Fe del Monolito, 0-indexed o 1-indexed según tu sistema).</param>
    /// <returns>El costo para alcanzar el siguiente nivel, o 0 si 'requiereNivel' es falso.</returns>
    public float CalcularCostoSubirNivel(float nivelActual)
    {
        if (!requiereNivel)
        {
            // Si este recurso no usa el sistema de niveles, no hay costo para "subirlo".
            // Devolver float.MaxValue podría ser más indicativo de "no se puede subir"
            // si el contexto de llamada espera un costo. O 0f si es "no cuesta nada".
            return 0f; // O float.MaxValue si prefieres indicar "imposible/no aplica"
        }

        // Asegurar que el nivel actual no sea negativo para la fórmula de potencia.
        if (nivelActual < 0) nivelActual = 0;

        // Fórmula: Costo = CostoBaseNivel * (IncrementoPorNivel ^ NivelActual) * FactorNivel
        // Esta fórmula calcula el costo para pasar del 'nivelActual' al 'nivelActual + 1'.
        // Ejemplo: Si nivelActual es 0, calcula el costo para llegar al nivel 1.
        // Si nivelActual es 1, calcula el costo para llegar al nivel 2.
        return costoBaseNivel * Mathf.Pow(incrementoPorNivel, nivelActual) * factorNivel;
    }
    #endregion
}
using UnityEngine;

[CreateAssetMenu(fileName = "NuevoRecurso", menuName = "Culto del Monolito/Recurso")] // Nombre de menú mejorado
public class RecurSO : ScriptableObject
{
    [Tooltip("Nombre único identificador del recurso (ej. Fe, Madera, Comida).")]
    public string Nombre; // Bien, clave para el diccionario en ResourceManager.

    [Tooltip("Cantidad máxima de este recurso que se puede almacenar.")]
    public float Maximo; // Considera si todos los recursos deben tener un máximo (ej. Fe ilimitada conceptualmente?).

    [Tooltip("Valor inicial del recurso al empezar o al ser creado si aplica.")]
    public float ValorBase; // Buena idea para un valor inicial.

    [Header("Requisitos de Nivel (Opcional)")]
    [Tooltip("Marcar si este recurso tiene un sistema de niveles asociado (ej. Nivel de Fe del Monolito).")]
    public bool requiereNivel; // Indica si este recurso tiene niveles.

    [Tooltip("Nombre del recurso (ej. 'Fragmentos') necesario para subir de nivel ESTE recurso (si 'requiereNivel' es true).")]
    public string recursoRequeridoParaNivel; // Nombre del recurso necesario para subir de nivel (ej: "Fe").

    [Tooltip("Factor que ajusta la curva de costo por nivel. Valores menores a 1 suavizan la curva, mayores la endurecen.")]
    public float factorNivel = 0.75f; // Factor multiplicativo para el costo por nivel.

    [Tooltip("Costo base en 'recursoRequeridoParaNivel' para alcanzar el nivel 1 (si aplica).")]
    public float costoBaseNivel = 10f; // Costo base para el primer nivel.

    [Tooltip("Cuánto se multiplica el costo por cada nivel adicional (ej. 1.5 significa 50% más caro cada nivel).")]
    public float incrementoPorNivel = 1.5f; // Multiplicador del costo por cada nivel adicional.

    // Método para calcular la cantidad necesaria del 'recursoRequeridoParaNivel'
    // para subir ESTE recurso (si tiene niveles) al 'siguienteNivelDesdeActual'.
    // 'nivelActual' es el nivel presente antes de intentar subir.
    public float CalcularCostoSubirNivel(float nivelActual)
    {
        if (!requiereNivel)
        {
            return 0f; // Si no requiere nivel, el costo es cero.
        }

        // Asegurarse que el nivel actual no sea negativo para evitar problemas con Mathf.Pow.
        if (nivelActual < 0) nivelActual = 0;

        // Fórmula para un avance gradual pero no sencillo.
        // Costo = CostoBase * (IncrementoPorNivel ^ NivelActual) * FactorNivel.
        // Nota: NivelActual aquí se refiere al nivel que se quiere superar.
        // Si nivelActual es 0, se calcula el costo para pasar a nivel 1.
        // Si nivelActual es 1, se calcula el costo para pasar a nivel 2.
        // La fórmula parece calcular el costo para alcanzar el *siguiente* nivel *después* del actual.
        return costoBaseNivel * Mathf.Pow(incrementoPorNivel, nivelActual) * factorNivel;
    }
}
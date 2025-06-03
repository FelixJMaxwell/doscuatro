using UnityEngine;

[System.Serializable]
public class RecursoInstancia
{
    [Tooltip("Referencia al ScriptableObject que define las propiedades base de este recurso.")]
    public RecurSO data; // El SO con la definición (nombre, máximo base, etc.)
    [Tooltip("Cantidad actual de este recurso que posee el jugador/entidad.")]
    public float actual; // La cantidad que se tiene.

    // Propiedades de conveniencia
    public string Nombre => data != null ? data.Nombre : "RECURSO_INVALIDO";
    public float Maximo => data != null ? data.Maximo : 0f;
    public float BaseValue => data != null ? data.ValorBase : 0f;

    // --- ESTE ES EL CONSTRUCTOR QUE NECESITAS ---
    /// <summary>
    /// Constructor para crear una nueva instancia de recurso.
    /// </summary>
    /// <param name="soData">El ScriptableObject (RecurSO) que define este recurso.</param>
    /// <param name="cantidadInicial">La cantidad inicial para este recurso.</param>
    public RecursoInstancia(RecurSO soData, float cantidadInicial)
    {
        this.data = soData;
        if (this.data != null)
        {
            // Asegurar que la cantidad inicial no exceda el máximo definido en el SO
            // y no sea menor que cero.
            this.actual = Mathf.Clamp(cantidadInicial, 0, this.data.Maximo);
        }
        else
        {
            // Si no hay datos de SO, simplemente asigna la cantidad inicial (o podrías poner 0)
            // y advierte del problema.
            this.actual = cantidadInicial;
            Debug.LogError("¡RecursoInstancia creada con un RecurSO nulo! Esto puede causar problemas.");
        }
    }
    // --- FIN DEL CONSTRUCTOR ---

    public void Añadir(float cantidad)
    {
        if (data == null) {
            Debug.LogError($"Intento de añadir a una RecursoInstancia sin RecurSO ({Nombre}).");
            return;
        }
        if (cantidad < 0) {
            // Considerar si esto debería llamar a Gastar o simplemente ser un error/advertencia
            Debug.LogWarning($"Intentando añadir cantidad negativa ({cantidad}) al recurso '{Nombre}'.");
            return;
        }
        actual = Mathf.Clamp(actual + cantidad, 0, Maximo);
    }

    public bool Gastar(float cantidad)
    {
        if (data == null) {
            Debug.LogError($"Intento de gastar de una RecursoInstancia sin RecurSO ({Nombre}).");
            return false;
        }
        if (cantidad < 0) {
            Debug.LogWarning($"Intentando gastar cantidad negativa ({cantidad}) del recurso '{Nombre}'.");
            return true; // No se gastó, pero la operación de "gastar negativo" podría considerarse "no fallida"
        }
        if (cantidad == 0) return true;


        float valorPrevio = actual;
        actual = Mathf.Clamp(actual - cantidad, 0, Maximo);
        return valorPrevio >= cantidad; // Devuelve true si había suficiente para cubrir el gasto solicitado
    }

    public bool TieneSuficiente(float cantidadNecesaria)
    {
        if (cantidadNecesaria <= 0) return true; // Si no se necesita nada o una cantidad negativa, se considera que sí tiene.
        return actual >= cantidadNecesaria;
    }
}
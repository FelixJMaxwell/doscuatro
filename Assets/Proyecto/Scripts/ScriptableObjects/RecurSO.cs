using UnityEngine;

[CreateAssetMenu(fileName = "RecurSO", menuName = "Scriptable Objects/RecurSO")]
public class RecurSO : ScriptableObject
{
    public string Nombre;
    public float Maximo;
    public float ValorBase;

    [Header("Requisitos de Nivel")]
    public bool requiereNivel; // Indica si este recurso tiene niveles
    public string recursoRequeridoParaNivel; // Nombre del recurso necesario para subir de nivel (ej: "Fe")
    public float factorNivel = 0.75f; // Factor multiplicativo para el costo por nivel
    public float costoBaseNivel = 10f; // Costo base para el primer nivel
    public float incrementoPorNivel = 1.5f; // Multiplicador del costo por cada nivel adicional

    // Método para calcular la cantidad necesaria del recurso requerido para subir al siguiente nivel
    public float CalcularCostoSubirNivel(float nivelActual)
    {
        if (!requiereNivel)
        {
            return 0f; // Si no requiere nivel, el costo es cero
        }

        // Fórmula para un avance gradual pero no sencillo
        // Costo = CostoBase * (IncrementoPorNivel ^ NivelActual) * FactorNivel
        return costoBaseNivel * Mathf.Pow(incrementoPorNivel, nivelActual) * factorNivel;
    }
}
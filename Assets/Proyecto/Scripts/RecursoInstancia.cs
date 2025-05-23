using UnityEngine;

// Esta clase representa una instancia de un recurso en el juego,
// manteniendo su estado actual y referenciando sus datos base (RecurSO).
[System.Serializable] // Es útil si planeas serializarlo, aunque como clase normal también funciona.
public class RecursoInstancia
{
    public RecurSO data; // Referencia al ScriptableObject que define este recurso.
    public float actual; // Cantidad actual de este recurso.

    // Propiedad para acceder fácilmente al máximo definido en el RecurSO.
    public float Maximo => data != null ? data.Maximo : 0f; // Añadido chequeo de nulidad para 'data'.
    // Propiedad para acceder fácilmente al valor base definido en el RecurSO.
    public float BaseValue => data != null ? data.ValorBase : 0f; // Añadido chequeo de nulidad para 'data'.

    // Añade una cantidad al recurso, sin exceder el máximo.
    public void Añadir(float cantidad)
    {
        if (data == null) {
            Debug.LogError("Datos (RecurSO) no asignados para esta instancia de recurso. No se puede añadir.");
            return;
        }
        if (cantidad < 0) {
            Debug.LogWarning("Intentando añadir una cantidad negativa. Usar Gastar() en su lugar o verificar la lógica.");
            // Opcionalmente, tratar cantidad negativa como gasto: Gastar(-cantidad);
            // return;
        }
        actual = Mathf.Clamp(actual + cantidad, 0, Maximo);
    }

    // Gasta una cantidad del recurso. No permite que baje de cero.
    // Devuelve true si se pudo gastar la cantidad completa, false si no había suficiente (aunque igual gasta lo que hay).
    public bool Gastar(float cantidad) // Cambiado a bool para indicar éxito/fallo de gasto completo.
    {
        if (data == null) {
            Debug.LogError("Datos (RecurSO) no asignados para esta instancia de recurso. No se puede gastar.");
            return false;
        }
        if (cantidad < 0) {
            Debug.LogWarning("Intentando gastar una cantidad negativa. Usar Añadir() en su lugar o verificar la lógica.");
            // return false; // Opcional: tratar como añadir.
        }

        float valorPrevio = actual;
        actual = Mathf.Clamp(actual - cantidad, 0, Maximo); // Maximo aquí es por si 'cantidad' es negativa y grande.
                                                         // Realmente, el clamp superior no debería ser necesario si 'cantidad' es positiva.

        return valorPrevio - actual >= cantidad || valorPrevio == cantidad; // Indica si se pudo gastar lo solicitado.
    }

    // Comprueba si hay suficiente cantidad de este recurso.
    public bool TieneSuficiente(float cantidadNecesaria)
    {
        return actual >= cantidadNecesaria;
    }
}
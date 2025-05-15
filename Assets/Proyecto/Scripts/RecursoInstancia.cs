using UnityEngine;


[System.Serializable]
public class RecursoInstancia
{
    public RecurSO data;
    public float actual;

    public float Maximo => data.Maximo;
    public float BaseValue => data.ValorBase;

    public void Añadir(float cantidad) {
        actual = Mathf.Clamp(actual + cantidad, 0, Maximo);
    }

    public void Gastar(float cantidad) {
        actual = Mathf.Clamp(actual - cantidad, 0, Maximo);
    }
}

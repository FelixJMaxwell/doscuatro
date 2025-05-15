using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    None,
    Food,
    Water,
    Wood,
    Stone,
    Metal,
    Cloth,
    Medicine,
    Knowledge,
    Faith
}

// Gestor centralizado de recursos en el juego
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<RecurSO> recursosIniciales;
    [SerializeField] public Dictionary<string, RecursoInstancia> recursos = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcional: DontDestroyOnLoad(gameObject); para que persista entre escenas
            InicializarRecursos();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InicializarRecursos()
    {
        recursos.Clear(); // Asegurarse de que el diccionario esté vacío antes de inicializar
        foreach (var recursoSO in recursosIniciales)
        {
            recursos[recursoSO.Nombre] = new RecursoInstancia
            {
                data = recursoSO,
                // actual = recursoSO.Maximo // O recursoSO.ValorBase si quieres iniciar con el valor base
            };
        }
    }

    // Método para reiniciar todos los recursos a su valor máximo inicial (o base)
    public void ResetearRecursos()
    {
        foreach (var recursoSO in recursosIniciales)
        {
            if (recursos.ContainsKey(recursoSO.Nombre))
            {
                recursos[recursoSO.Nombre].actual = recursoSO.Maximo; // O recursoSO.ValorBase
            }
        }
    }

    public void Añadir(string nombre, float cantidad)
    {
        if (recursos.TryGetValue(nombre, out var instancia))
        {
            instancia.Añadir(cantidad);
        }
        else
        {
            Debug.LogError($"Recurso '{nombre}' no encontrado.");
        }
    }

    public void Gastar(string nombre, float cantidad)
    {
        if (recursos.TryGetValue(nombre, out var instancia))
        {
            instancia.Gastar(cantidad);
        }
        else
        {
            Debug.LogError($"Recurso '{nombre}' no encontrado.");
        }
    }

    public float GetCantidad(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.actual : 0;
    }

    public float GetMaximo(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.Maximo : 0;
    }

    public float GetValorBase(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.BaseValue : 0;
    }
}
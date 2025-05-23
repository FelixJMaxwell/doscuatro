using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Enum para diferentes tipos de recursos (opcional, pero bueno para organización).
// Ya lo tienes en un archivo separado, lo cual está bien. Si no, podría ir aquí.
/* public enum ResourceType { ... } */

// Gestiona de forma centralizada todos los recursos del juego.
public class ResourceManager : MonoBehaviour
{
    // Implementación del patrón Singleton para fácil acceso.
    public static ResourceManager Instance { get; private set; }

    // Lista de ScriptableObjects (RecurSO) que definen los recursos iniciales
    // a cargar al inicio del juego. Asignar desde el Inspector.
    [Tooltip("Arrastra aquí los ScriptableObjects de los recursos iniciales del juego.")]
    public List<RecurSO> recursosIniciales;

    // Diccionario para almacenar las instancias activas de los recursos,
    // usando el nombre del recurso (desde RecurSO.Nombre) como clave.
    // Es serializable para poder verlo en el Inspector (útil para debug).
    [SerializeField]
    public Dictionary<string, RecursoInstancia> recursos = new Dictionary<string, RecursoInstancia>();

    // --- UI Específica de Recursos ---
    // Sugerencia: Idealmente, la gestión directa de elementos UI específicos
    // debería estar en un UIManager para separar responsabilidades.
    // ResourceManager podría emitir eventos cuando los recursos cambian, y UIManager escucharía.
    [Header("UI (Considerar mover a un UIManager)")]
    [Tooltip("Elemento TextMeshPro para mostrar la cantidad de Fe. Asignar desde el Inspector.")]
    public TextMeshProUGUI FeUI;
    // Considera una forma más genérica de actualizar la UI si tienes muchos recursos.
    // Ejemplo: Un diccionario de string (nombreRecurso) a TextMeshProUGUI.

    [Tooltip("Transform que contiene los GameObjects de los pilares de Fe (usado para animaciones).")]
    [SerializeField] private Transform FaithHolder; // Esta referencia parece muy específica de MonolitoBehaviour.
                                                 // ¿Debería estar aquí o ser pasada/manejada por MonolitoBehaviour directamente?

    private void Awake()
    {
        // Implementación del Singleton.
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Descomentar si el ResourceManager debe persistir entre escenas.
            InicializarRecursos();
        }
        else
        {
            Debug.LogWarning("Ya existe una instancia de ResourceManager. Destruyendo este duplicado.");
            Destroy(gameObject); // Destruye este objeto si ya existe una instancia.
        }
    }

    // Inicializa los recursos basándose en la lista 'recursosIniciales'.
    private void InicializarRecursos()
    {
        recursos.Clear(); // Limpia el diccionario por si se llama múltiples veces (aunque Awake lo previene).

        if (recursosIniciales == null || recursosIniciales.Count == 0)
        {
            Debug.LogWarning("ResourceManager: La lista 'recursosIniciales' está vacía o es nula. No se inicializarán recursos.");
            return;
        }

        foreach (var recursoSO in recursosIniciales)
        {
            if (ValidarRecurSO(recursoSO)) // Valida la configuración del ScriptableObject.
            {
                if (!recursos.ContainsKey(recursoSO.Nombre))
                {
                    // Crea una nueva instancia del recurso y la añade al diccionario.
                    recursos[recursoSO.Nombre] = new RecursoInstancia
                    {
                        data = recursoSO,
                        actual = recursoSO.ValorBase // Inicializa con el valor base.
                    };
                    // Actualiza la UI para este recurso recién inicializado.
                    ActualizarRecursoUI(recursoSO.Nombre);
                    // Debug.Log($"Recurso inicializado: {recursoSO.Nombre}, Cantidad: {recursoSO.ValorBase}, Max: {recursoSO.Maximo}");
                }
                else
                {
                    Debug.LogWarning($"ResourceManager: El recurso '{recursoSO.Nombre}' ya ha sido inicializado. Se ignorará la duplicación.");
                }
            }
        }
    }

    // Valida la configuración básica de un ScriptableObject de recurso (RecurSO).
    private bool ValidarRecurSO(RecurSO recurso)
    {
        if (recurso == null)
        {
            Debug.LogError("ResourceManager: Se intentó usar un RecurSO nulo. Asigna un ScriptableObject válido en el Inspector.");
            return false;
        }
        if (string.IsNullOrEmpty(recurso.Nombre))
        {
            Debug.LogError($"ResourceManager: El RecurSO '{recurso.name}' (nombre de asset) tiene un campo 'Nombre' vacío o nulo. ¡Asigna un nombre válido!");
            return false;
        }
        // La excepción para 'Faith' con Maximo <= 0 podría ser un indicativo de que
        // 'Faith' se maneja de forma diferente o no debería tener un máximo aquí.
        // Si Faith no tiene máximo, su propiedad 'Maximo' en el SO debería reflejarlo (ej. float.MaxValue).
        if (recurso.Maximo <= 0 && recurso.Nombre != "Faith") // Ajustar esta lógica si es necesario.
        {
            Debug.LogWarning($"ResourceManager: RecurSO '{recurso.Nombre}' tiene Maximo <= 0 ({recurso.Maximo}). Esto puede impedir añadir recursos. Considera un Maximo positivo.");
        }
        return true;
    }

    // Resetea todos los recursos a su valor máximo o base (según diseño).
    public void ResetearRecursos()
    {
        foreach (var kvp in recursos) // Itera sobre el diccionario de instancias.
        {
            RecursoInstancia instancia = kvp.Value;
            if (instancia.data != null)
            {
                // Decide si resetear al máximo o al valor base.
                // instancia.actual = instancia.Maximo;
                instancia.actual = instancia.BaseValue; // Ejemplo: resetear al valor base.
                ActualizarRecursoUI(kvp.Key); // Actualiza la UI para cada recurso reseteado.
            }
        }
        Debug.Log("Todos los recursos han sido reseteados.");
    }

    // Añade una cantidad a un recurso específico.
    public void Añadir(string nombre, float cantidad)
    {
        if (cantidad < 0) {
             Debug.LogWarning($"Intentando añadir cantidad negativa ({cantidad}) al recurso '{nombre}'. Se usará Gastar() en su lugar.");
             Gastar(nombre, -cantidad); // Trata cantidad negativa como gasto.
             return;
        }

        if (recursos.TryGetValue(nombre, out var instancia))
        {
            instancia.Añadir(cantidad);
            ActualizarRecursoUI(nombre); // Actualiza la UI del recurso modificado.
        }
        else
        {
            Debug.LogError($"ResourceManager: Recurso '{nombre}' no encontrado. No se pudo añadir {cantidad}.");
        }
    }

    // Gasta una cantidad de un recurso específico.
    // Devuelve true si se pudo gastar la cantidad, false en caso contrario.
    public bool Gastar(string nombre, float cantidad) // Cambiado a bool
    {
        if (cantidad < 0) {
             Debug.LogWarning($"Intentando gastar cantidad negativa ({cantidad}) del recurso '{nombre}'. Se usará Añadir() en su lugar.");
             Añadir(nombre, -cantidad); // Trata cantidad negativa como adición.
             return true; // Asumiendo que la adición siempre es "exitosa" en este contexto.
        }

        if (recursos.TryGetValue(nombre, out var instancia))
        {
            float cantidadPrevia = instancia.actual;
            bool pudoGastarTodo = instancia.Gastar(cantidad); // RecursoInstancia.Gastar ahora también podría devolver bool.
                                                     // O simplemente verificar aquí:
            if (cantidadPrevia < cantidad && instancia.actual == 0) {
                // No había suficiente, se gastó todo lo que había.
                pudoGastarTodo = false;
            } else if (cantidadPrevia >= cantidad) {
                pudoGastarTodo = true;
            }


            ActualizarRecursoUI(nombre);

            // Lógica específica para 'Fe' y pilares.
            // Considera si esta lógica tan específica debería estar aquí o ser manejada por
            // un sistema que observe los cambios en 'Fe'.
            if (nombre == "Fe" && pudoGastarTodo) // Solo animar si el gasto fue exitoso (o como se desee).
            {
                GestionarPilaresDeFe(cantidad);
            }
            return pudoGastarTodo;
        }
        else
        {
            Debug.LogError($"ResourceManager: Recurso '{nombre}' no encontrado. No se pudo gastar {cantidad}.");
            return false;
        }
    }

    // Método privado para la lógica de los pilares de Fe.
    // Dentro de ResourceManager.cs

    // ... otros métodos y variables ...

    // Método privado para la lógica de los pilares de Fe.
    // Se llama cuando se gasta Fe.
    private void GestionarPilaresDeFe(float cantidadFeGastada)
    {
        if (FaithHolder == null)
        {
            // Es una advertencia leve porque el juego puede funcionar sin esto, pero el efecto visual no ocurrirá.
            // Debug.LogWarning("ResourceManager: FaithHolder no asignado. No se animarán pilares de Fe.");
            return;
        }

        if (cantidadFeGastada <= 0)
        {
            // No hacer nada si no se gastó Fe o se gastó una cantidad no positiva.
            return;
        }

        // Calcular cuántos pilares afectar: 3 pilares por cada unidad de Fe gastada.
        // Usamos Mathf.FloorToInt para asegurar que solo unidades completas de Fe contribuyan.
        // Ejemplo: si se gastan 10.5 de Fe, se afectan 10 * 3 = 30 pilares.
        // Si se gastan 0.5 de Fe, se afectan 0 * 3 = 0 pilares.
        int pilaresAAfectar = Mathf.FloorToInt(cantidadFeGastada) * 3;

        if (pilaresAAfectar == 0 && cantidadFeGastada > 0)
        {
            // Consideración: ¿Qué pasa si se gasta menos de 1 Fe (ej. 0.5)?
            // Actualmente, no se afectaría ningún pilar. Si quieres que incluso una fracción
            // afecte al menos a un grupo de 3 pilares (o 1 pilar), la lógica necesitaría ajustarse.
            // Por ahora, seguimos la regla de "3 por cada unidad entera de Fe".
            // Debug.Log($"Se gastó {cantidadFeGastada} de Fe, pero es menos de 1 unidad entera. No se afectarán pilares.");
        }


        // Ahora, iterar y afectar los pilares.
        // Es importante decidir cómo se seleccionan los pilares a afectar.
        // Opción 1: Afectar los últimos 'N' hijos activos del FaithHolder (de arriba hacia abajo).
        // Opción 2: Tener una lista de pilares "llenos" y "vaciar" los primeros 'N'.
        // La Opción 1 es más simple si los pilares se destruyen o desactivan al bajar.

        int pilaresRealmenteAfectados = 0;
        for (int i = 0; i < pilaresAAfectar; i++)
        {
            // Para afectar los pilares "visualmente más altos" o "más recientemente añadidos" primero,
            // si los nuevos pilares se añaden al final de la jerarquía de FaithHolder.
            // Iteramos desde el final de los hijos hacia el principio.
            int childIndex = FaithHolder.childCount - 1 - pilaresRealmenteAfectados;

            if (childIndex < 0)
            {
                // No hay más pilares hijos que afectar, incluso si 'pilaresAAfectar' era mayor.
                // Debug.LogWarning("Se intentó afectar más pilares de los disponibles en FaithHolder.");
                break;
            }

            Transform pilarTransform = FaithHolder.GetChild(childIndex);
            if (pilarTransform != null)
            {
                PilarBehaviour pilarBehaviour = pilarTransform.GetComponent<PilarBehaviour>();
                if (pilarBehaviour != null && !pilarBehaviour.Bajar) // Solo afecta pilares que no estén ya bajando.
                {
                    pilarBehaviour.EstablecerEstadoBajada(true); // Usa el método más explícito.
                    pilaresRealmenteAfectados++;
                }
                // Si el pilar no tiene PilarBehaviour o ya está bajando, se salta y se intenta el siguiente
                // (el contador 'i' avanza, pero 'pilaresRealmenteAfectados' no, así que se buscará más atrás).
                // Para evitar un bucle si todos los restantes ya están bajando o no tienen script,
                // es mejor que 'pilaresRealmenteAfectados' siempre incremente o que el bucle tenga otra condición.
                // Con la lógica actual, si se encuentran pilares no válidos, simplemente no se cuentan
                // y se intentará afectar 'pilaresAAfectar' válidos.
            }
        }

        // if (pilaresRealmenteAfectados > 0)
        // {
        //     Debug.Log($"{pilaresRealmenteAfectados} pilares de Fe han comenzado a bajar debido al gasto de {cantidadFeGastada} de Fe.");
        // }
    }

    // Obtiene la cantidad actual de un recurso.
    public float GetCantidad(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.actual : 0f;
    }

    // Obtiene la capacidad máxima de un recurso.
    public float GetMaximo(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.Maximo : 0f;
    }

    // Obtiene el valor base de un recurso.
    public float GetValorBase(string nombre)
    {
        return recursos.TryGetValue(nombre, out var instancia) ? instancia.BaseValue : 0f;
    }

    // Comprueba si hay suficiente cantidad de un recurso.
    public bool TieneSuficiente(string nombre, float cantidadNecesaria)
    {
        return GetCantidad(nombre) >= cantidadNecesaria;
    }


    // Método para actualizar la UI de un recurso específico.
    // Sugerencia: Abstraer esto a un UIManager o usar un sistema de eventos.
    private void ActualizarRecursoUI(string nombreRecurso)
    {
        if (string.IsNullOrEmpty(nombreRecurso)) return;

        if (nombreRecurso == "Fe" && FeUI != null)
        {
            // Formatea los números a enteros si no necesitas decimales para la Fe.
            FeUI.text = $"Fe: {GetCantidad("Fe").ToString("F0")} / {GetMaximo("Fe").ToString("F0")}";
        }
        // Ejemplo para otros recursos:
        // else if (nombreRecurso == "Madera" && MaderaUI != null)
        // {
        //     MaderaUI.text = $"Madera: {GetCantidad("Madera").ToString("F0")}";
        // }

        // Alternativa más genérica (requiere que GameManager o un UIManager tenga referencias a los textos):
        // GameManager.Instance?.ActualizarTextoRecurso(nombreRecurso, GetCantidad(nombreRecurso), GetMaximo(nombreRecurso));
    }
}
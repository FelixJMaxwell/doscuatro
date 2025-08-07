// Archivo: ResourceManager.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Configuración de Recursos Iniciales")]
    [Tooltip("Arrastra aquí todos los ScriptableObjects de RecurSO que se inicializarán al inicio del juego.")]
    public List<RecurSO> recursosInicialesSOs; // EL ResourceManager gestiona esta lista

    [SerializeField]
    private Dictionary<string, RecursoInstancia> _recursos = new Dictionary<string, RecursoInstancia>();

    public static event Action<string, float, float> OnRecursoActualizado;
    public static event Action<float> OnFaithLimitChanged;

    // Volvemos a colocar estas referencias aquí, ya que ResourceManager las usará
    // (Aún se recomienda mover la lógica de UI a UIManager o un script de Monolito)
    [Header("UI Específica (Temporal - Mover a UIManager)")]
    public TextMeshProUGUI FeUIText; // Aún presente si ResourceManager sigue actualizando la UI directamente
    [SerializeField] private Transform faithPillarsHolder;
    [Header("Configuración de SOs (para lógica interna/UI temporal)")]
    [SerializeField] private RecurSO feDataSO; // ResourceManager necesita esta referencia para su lógica interna
    // Si otros recursos tienen lógica específica aquí, también puedes añadir sus SOs.

    [System.Serializable]
    public class RecursoInstancia
    {
        public RecurSO data;
        [SerializeField] private float _actual;
        [SerializeField] private float _maximo;

        public RecursoInstancia(RecurSO dataSO, float cantidadInicial)
        {
            data = dataSO;
            _actual = cantidadInicial;
            _maximo = dataSO.esLimitado ? dataSO.LimiteInicial : float.MaxValue;
        }

        public float actual
        {
            get => _actual;
            set
            {
                float oldAmount = _actual;
                _actual = Mathf.Clamp(value, 0, _maximo);
            }
        }

        public float Maximo
        {
            get => _maximo;
            set
            {
                float oldMax = _maximo;
                _maximo = Mathf.Max(0, value);

                if (_actual > _maximo)
                {
                    actual = _maximo;
                }

                if (_maximo != oldMax)
                {
                    if (data != null && data.Nombre == "Fe")
                    {
                        OnFaithLimitChanged?.Invoke(_maximo);
                    }
                }
            }
        }

        public void Añadir(float cantidad) { actual += cantidad; }
        public bool Gastar(float cantidad)
        {
            if (_actual >= cantidad)
            {
                actual -= cantidad;
                return true;
            }
            return false;
        }
    }

    public IReadOnlyDictionary<string, RecursoInstancia> GetRuntimeRecursos()
    {
        return _recursos;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InicializarRecursos();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InicializarRecursos()
    {
        _recursos.Clear();
        if (recursosInicialesSOs == null || recursosInicialesSOs.Count == 0) return;
        foreach (var recursoSO in recursosInicialesSOs)
        {
            if (ValidarRecurSO(recursoSO))
            {
                if (!_recursos.ContainsKey(recursoSO.Nombre))
                {
                    _recursos[recursoSO.Nombre] = new RecursoInstancia(recursoSO, recursoSO.CantidadInicial);

                    OnRecursoActualizado?.Invoke(recursoSO.Nombre, _recursos[recursoSO.Nombre].actual, _recursos[recursoSO.Nombre].Maximo);
                    if (recursoSO.Nombre == "Fe")
                    {
                        OnFaithLimitChanged?.Invoke(_recursos[recursoSO.Nombre].Maximo);
                    }
                    ActualizarRecursoUI(recursoSO.Nombre); // Temporal, a mover a UIManager
                }
            }
        }
    }

    private bool ValidarRecurSO(RecurSO r) { return r != null && !string.IsNullOrEmpty(r.Nombre); }

    public void Añadir(string nombreRecurso, float cantidad)
    {
        if (string.IsNullOrEmpty(nombreRecurso)) return;
        if (cantidad < 0) { Gastar(nombreRecurso, -cantidad); return; }
        if (cantidad == 0) return;
        if (_recursos.TryGetValue(nombreRecurso, out RecursoInstancia instancia))
        {
            float cantidadPrevia = instancia.actual;
            instancia.Añadir(cantidad);
            if (instancia.actual != cantidadPrevia) OnRecursoActualizado?.Invoke(nombreRecurso, instancia.actual, instancia.Maximo);
            ActualizarRecursoUI(nombreRecurso); // Temporal, a mover a UIManager
        }
        else Debug.LogError($"Recurso '{nombreRecurso}' no encontrado en ResourceManager.");
    }

    public bool Gastar(string nombreRecurso, float cantidad)
    {
        if (string.IsNullOrEmpty(nombreRecurso)) return false;
        if (cantidad < 0) { Añadir(nombreRecurso, -cantidad); return true; }
        if (cantidad == 0) return true;
        if (_recursos.TryGetValue(nombreRecurso, out RecursoInstancia instancia))
        {
            float cantidadPrevia = instancia.actual;
            bool pudoGastarSolicitadoCompletamente = instancia.Gastar(cantidad);

            if (instancia.actual != cantidadPrevia)
            {
                OnRecursoActualizado?.Invoke(nombreRecurso, instancia.actual, instancia.Maximo);
            }
            ActualizarRecursoUI(nombreRecurso); // Temporal, a mover a UIManager

            // Usa 'feDataSO' que está directamente en ResourceManager
            if (feDataSO != null && nombreRecurso == feDataSO.Nombre)
            {
                float cantidadRealmenteGastada = cantidadPrevia - instancia.actual;
                if (cantidadRealmenteGastada > 0)
                {
                    GestionarPilaresDeFe(cantidadRealmenteGastada); // Temporal, a mover
                }
            }
            return pudoGastarSolicitadoCompletamente;
        }
        Debug.LogError($"Recurso '{nombreRecurso}' no encontrado en ResourceManager.");
        return false;
    }

    public void IncrementarLimiteMaximoRecurso(string nombreRecurso, float cantidadAIncrementar)
    {
        if (string.IsNullOrEmpty(nombreRecurso))
        {
            Debug.LogError("ResourceManager: Nombre de recurso no válido para incrementar el límite máximo.");
            return;
        }

        if (_recursos.TryGetValue(nombreRecurso, out RecursoInstancia instancia))
        {
            if (instancia.data.esLimitado)
            {
                float oldMax = instancia.Maximo;
                instancia.Maximo += cantidadAIncrementar;

                if (instancia.Maximo != oldMax)
                {
                    OnRecursoActualizado?.Invoke(nombreRecurso, instancia.actual, instancia.Maximo);
                    Debug.Log($"Límite máximo de {nombreRecurso} incrementado a {instancia.Maximo:F0}.");
                }
            }
            else
            {
                Debug.LogWarning($"El recurso '{nombreRecurso}' no está configurado como limitado en su ScriptableObject. No se puede incrementar su límite.");
            }
        }
        else
        {
            Debug.LogError($"Recurso '{nombreRecurso}' no encontrado en el ResourceManager. No se puede incrementar su límite.");
        }
    }

    // Método para disminuir el límite
    public void DisminuirLimiteMaximoRecurso(string nombreRecurso, float cantidadADisminuir)
    {
        if (nombreRecurso == "Fe" && _recursos.TryGetValue(nombreRecurso, out RecursoInstancia instancia))
        {
            instancia.Maximo -= cantidadADisminuir;
            if (instancia.Maximo < instancia.data.LimiteInicial)
            {
                instancia.Maximo = instancia.data.LimiteInicial;
            }
            OnFaithLimitChanged?.Invoke(instancia.Maximo);
        }
    }

    // Dejar aquí si ResourceManager sigue actualizando la UI directamente (no recomendado a largo plazo)
    private void ActualizarRecursoUI(string nombreRecurso)
    {
        if (feDataSO != null && nombreRecurso == feDataSO.Nombre && FeUIText != null)
        {
            FeUIText.text = $"Fe: {GetCantidad(feDataSO.Nombre):F0} / {GetMaximo(feDataSO.Nombre):F0}";
        }
    }

    // Dejar aquí si ResourceManager sigue gestionando los pilares directamente (no recomendado a largo plazo)
    private void GestionarPilaresDeFe(float cantidadFeGastadaReal)
    {
        if (faithPillarsHolder == null) return;
        if (cantidadFeGastadaReal <= 0) return;

        int pilaresAAfectar = Mathf.FloorToInt(cantidadFeGastadaReal) * 3;
        if (pilaresAAfectar == 0) return;

        int pilaresRealmenteAfectados = 0;
        for (int i = 0; i < pilaresAAfectar; i++)
        {
            int childIndex = faithPillarsHolder.childCount - 1 - pilaresRealmenteAfectados;
            if (childIndex < 0) break;

            Transform pilarTransform = faithPillarsHolder.GetChild(childIndex);
            if (pilarTransform != null)
            {
                PilarBehaviour pilarBehaviour = pilarTransform.GetComponent<PilarBehaviour>();
                if (pilarBehaviour != null && !pilarBehaviour.Bajar)
                {
                    pilarBehaviour.EstablecerEstadoBajada(true);
                    pilaresRealmenteAfectados++;
                }
            }
        }
    }

    public float GetCantidad(string nombreRecurso) { return _recursos.TryGetValue(nombreRecurso, out var i) ? i.actual : 0f; }
    public float GetMaximo(string nombreRecurso) { return _recursos.TryGetValue(nombreRecurso, out var i) ? i.Maximo : 0f; }
    public bool TieneSuficiente(string nombreRecurso, float cantidadNecesaria)
    {
        if (cantidadNecesaria <= 0) return true;
        return GetCantidad(nombreRecurso) >= cantidadNecesaria;
    }
    
    public RecursoInstancia ObtenerRecurso(string nombre)
    {
        if (_recursos.TryGetValue(nombre, out RecursoInstancia instancia))
        {
            return instancia;
        }
        return null; // O lanza una excepción, dependiendo de tu manejo de errores.
    }
}
// Archivo: ResourceManager.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    // ... (Instance, recursosInicialesSOs, _recursos, OnRecursoActualizado, etc. como antes) ...
    public static ResourceManager Instance { get; private set; }

    public List<RecurSO> recursosInicialesSOs;
    [SerializeField]
    private Dictionary<string, RecursoInstancia> _recursos = new Dictionary<string, RecursoInstancia>();

    public static event Action<string, float, float> OnRecursoActualizado;

    // ... (UI Temporal: FeUIText, faithPillarsHolder) ...
    [Header("UI Específica (Temporal - Mover a UIManager)")]
    public TextMeshProUGUI FeUIText;
    [SerializeField] private Transform faithPillarsHolder;
    [Header("Configuración de SOs (para UI interna)")]
    [SerializeField] private RecurSO feDataSO;


    // --- NUEVO MÉTODO PÚBLICO PARA EL EDITOR ---
    /// <summary>
    /// Devuelve una copia de solo lectura del diccionario de recursos actual.
    /// Ideal para que el Custom Editor pueda mostrar la información en Play Mode.
    /// </summary>
    public IReadOnlyDictionary<string, RecursoInstancia> GetRuntimeRecursos()
    {
        // Devolver una nueva instancia o una interfaz de solo lectura previene
        // que el editor modifique directamente la colección interna.
        // Para una simple visualización, esto es seguro.
        return _recursos; // Dictionary<K,V> implementa IReadOnlyDictionary<K,V>
    }
    // --- FIN NUEVO MÉTODO ---


    private void Awake()
    {
        if (Instance == null) { Instance = this; InicializarRecursos(); }
        else { Destroy(gameObject); }
    }

    private void InicializarRecursos()
    {
        _recursos.Clear();
        if (recursosInicialesSOs == null || recursosInicialesSOs.Count == 0) return;
        foreach (var recursoSO in recursosInicialesSOs) {
            // Asumiendo que ValidarRecurSO existe y funciona
            // if (ValidarRecurSO(recursoSO)) {
                if (!_recursos.ContainsKey(recursoSO.Nombre)) {
                    _recursos[recursoSO.Nombre] = new RecursoInstancia(recursoSO, recursoSO.ValorBase);
                    OnRecursoActualizado?.Invoke(recursoSO.Nombre, _recursos[recursoSO.Nombre].actual, _recursos[recursoSO.Nombre].Maximo);
                    ActualizarRecursoUI(recursoSO.Nombre);
                }
            // }
        }
    }
    
    // Dummy ValidarRecurSO si no lo tienes completo
    private bool ValidarRecurSO(RecurSO r) { return r!= null && !string.IsNullOrEmpty(r.Nombre); }


    // ... (tus métodos Añadir, Gastar, GetCantidad, GetMaximo, TieneSuficiente, ActualizarRecursoUI, GestionarPilaresDeFe) ...
    // Estos métodos deben usar '_recursos' internamente.
    public void Añadir(string nombreRecurso, float cantidad) {
        if (string.IsNullOrEmpty(nombreRecurso)) return;
        if (cantidad < 0) { Gastar(nombreRecurso, -cantidad); return; }
        if (cantidad == 0) return;
        if (_recursos.TryGetValue(nombreRecurso, out RecursoInstancia instancia)) {
            float cantidadPrevia = instancia.actual;
            instancia.Añadir(cantidad);
            if (instancia.actual != cantidadPrevia) OnRecursoActualizado?.Invoke(nombreRecurso, instancia.actual, instancia.Maximo);
            ActualizarRecursoUI(nombreRecurso);
        } else Debug.LogError($"Recurso '{nombreRecurso}' no encontrado.");
    }

    public bool Gastar(string nombreRecurso, float cantidad) {
        if (string.IsNullOrEmpty(nombreRecurso)) return false;
        if (cantidad < 0) { Añadir(nombreRecurso, -cantidad); return true; }
        if (cantidad == 0) return true;
        if (_recursos.TryGetValue(nombreRecurso, out RecursoInstancia instancia)) {
            float cantidadPrevia = instancia.actual;
            bool pudoGastar = instancia.Gastar(cantidad);
            if (instancia.actual != cantidadPrevia) OnRecursoActualizado?.Invoke(nombreRecurso, instancia.actual, instancia.Maximo);
            ActualizarRecursoUI(nombreRecurso);
            if (feDataSO != null && nombreRecurso == feDataSO.Nombre && (cantidadPrevia - instancia.actual) > 0) {
                GestionarPilaresDeFe(cantidadPrevia - instancia.actual);
            }
            return pudoGastar;
        }
        Debug.LogError($"Recurso '{nombreRecurso}' no encontrado.");
        return false;
    }
    private void ActualizarRecursoUI(string nombreRecurso) {
        if (feDataSO != null && nombreRecurso == feDataSO.Nombre && FeUIText != null) {
            FeUIText.text = $"Fe: {GetCantidad(feDataSO.Nombre):F0} / {GetMaximo(feDataSO.Nombre):F0}";
        }
    }
    private void GestionarPilaresDeFe(float cantidadFeGastadaReal) { /* ... tu lógica ... */ }
    public float GetCantidad(string nombreRecurso) { return _recursos.TryGetValue(nombreRecurso, out var i) ? i.actual : 0f; }
    public float GetMaximo(string nombreRecurso) { return _recursos.TryGetValue(nombreRecurso, out var i) ? i.Maximo : 0f; }
    public bool TieneSuficiente(string nombreRecurso, float cantidadNecesaria) {
        if (cantidadNecesaria <= 0) return true;
        return GetCantidad(nombreRecurso) >= cantidadNecesaria;
    }
}
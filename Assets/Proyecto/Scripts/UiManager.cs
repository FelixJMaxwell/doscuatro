// Archivo: UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UiManager : MonoBehaviour
{
    #region Singleton
    public static UiManager Instance { get; private set; }
    #endregion

    #region Referencias a Paneles UI Principales
    [Header("Paneles Principales de UI")]
    [SerializeField] private GameObject panelArquitectura;
    [SerializeField] private GameObject panelMonolito;
    [Tooltip("Panel genérico para mostrar información del edificio seleccionado.")]
    [SerializeField] private GameObject panelInformacionEdificio; // NUEVA REFERENCIA GENÉRICA
    [SerializeField] private GameObject panelControlCrisol;
    #endregion

    #region Referencias a Elementos UI Comunes
    [Header("Displays de Recursos Globales")]
    [SerializeField] private TextMeshProUGUI textoRecursoFe;
    [SerializeField] private TextMeshProUGUI textoRecursoFragmentos;
    [Header("Configuración de Recursos (para UI)")]
    [SerializeField] private RecurSO feDataSO; // Asignar el SO de Fe
    [SerializeField] private RecurSO fragmentoSO; // Asignar el SO de Fragmento
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ControlarVisibilidadPanel(panelArquitectura, false);
        ControlarVisibilidadPanel(panelMonolito, false);
        ControlarVisibilidadPanel(panelInformacionEdificio, false); // Asegurar que esté oculto
        ControlarVisibilidadPanel(panelControlCrisol, false);

        ResourceManager.OnRecursoActualizado += ActualizarDisplayRecursoEspecifico;
        InicializarDisplaysDeRecursos();
    }

    private void OnDestroy()
    {
        ResourceManager.OnRecursoActualizado -= ActualizarDisplayRecursoEspecifico;
    }

    #region Panel Management
    private void ControlarVisibilidadPanel(GameObject panel, bool mostrar)
    {
        if (panel != null)
        {
            panel.SetActive(mostrar); // Simplemente establece el estado deseado
        }
    }
    // Corrección de la línea anterior:
    // private void ControlarVisibilidadPanel(GameObject panel, bool mostrar)
    // {
    //     if (panel != null)
    //     {
    //         if (panel.activeInHierarchy != mostrar)
    //         {
    //             panel.SetActive(mostrar); // La asignación debe ser 'mostrar'
    //         }
    //     }
    // }
    // ¡Ups! Corrección más simple y correcta para ControlarVisibilidadPanel:
    // private void ControlarVisibilidadPanel(GameObject panel, bool mostrar)
    // {
    //     if (panel != null)
    //     {
    //         panel.SetActive(mostrar);
    //     }
    // }


    public void MostrarPanelArquitectura(bool mostrar)
    {
        ControlarVisibilidadPanel(panelArquitectura, mostrar);
        if (mostrar && GameManager.Instance != null) GameManager.Instance.DeseleccionarEstructuraActual();
    }

    public void MostrarPanelContextualMonolito(bool mostrar) // Ya no necesita el MonolitoBehaviour aquí
    {
        ControlarVisibilidadPanel(panelMonolito, mostrar);
        // El MonolitoBehaviour mismo maneja su prompt de "Presiona F" y su panel de opciones
        // si 'panelMonolito' aquí es diferente al 'panelOpcionInteraccion' del Monolito.
        // Si es el mismo, entonces esta llamada es redundante con la lógica del MonolitoBehaviour.
        // Asumamos que este es un panel más general si es necesario.
    }

    // Método para el panel de información de edificio genérico
    public void MostrarPanelInformacionEdificio(bool mostrar) // Ya no toma BaseBuilding
    {
        ControlarVisibilidadPanel(panelInformacionEdificio, mostrar);
    }
    
    public void MostrarPanelControlCrisol(bool mostrar) // Ya no necesita Building_Personajes aquí
    {
        ControlarVisibilidadPanel(panelControlCrisol, mostrar);
    }
    #endregion

    #region UI Data Updates
        private void InicializarDisplaysDeRecursos()
    {
        if (ResourceManager.Instance == null) return;
        if (feDataSO != null) ActualizarDisplayRecursoEspecifico(feDataSO.Nombre, ResourceManager.Instance.GetCantidad(feDataSO.Nombre), ResourceManager.Instance.GetMaximo(feDataSO.Nombre));
        if (fragmentoSO != null) ActualizarDisplayRecursoEspecifico(fragmentoSO.Nombre, ResourceManager.Instance.GetCantidad(fragmentoSO.Nombre), ResourceManager.Instance.GetMaximo(fragmentoSO.Nombre));
    }
    
    private void ActualizarDisplayRecursoEspecifico(string nombreRecurso, float cantidad, float maximo)
    {
        if (feDataSO != null && nombreRecurso == feDataSO.Nombre && textoRecursoFe != null)
        {
             textoRecursoFe.text = $"Fe: {cantidad.ToString("F0")}/{maximo.ToString("F0")}";
        }
        else if (fragmentoSO != null && nombreRecurso == fragmentoSO.Nombre && textoRecursoFragmentos != null)
        {
            textoRecursoFragmentos.text = $"Fragmentos: {cantidad.ToString("F0")}";
        }
    }
    #endregion
}
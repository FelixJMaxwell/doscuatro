// Archivo: ConfiguracionEntradaArquitecturaUI.cs
using UnityEngine;
using System; // Asegúrate de tener esto para [Serializable]

[Serializable] // Necesario para que aparezca en el Inspector como una lista de objetos.
public class ConfiguracionEntradaArquitecturaUI
{
    [Tooltip("Nombre descriptivo para esta entrada en el Inspector (ej. 'Entrada UI Casa').")]
    public string nombreEditor; // Para tu organización en el Inspector.

    [Tooltip("El GameObject raíz de la 'tarjeta' o 'entrada' de UI para este edificio. Sus hijos deben seguir un orden específico.")]
    public GameObject elementoRaizUI;

    [Tooltip("El ScriptableObject que contiene todos los datos (nombre, descripción, icono, prefab del edificio) para esta entrada.")]
    public EdificioDataSO datosDelEdificioSO;
}
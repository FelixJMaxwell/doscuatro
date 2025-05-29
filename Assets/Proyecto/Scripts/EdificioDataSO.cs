// Archivo: EdificioDataSO.cs
using UnityEngine;
using System.Collections.Generic; // Para List

// Reutiliza la struct que definimos en BaseBuilding.cs o define una similar aquí.
// Si está en BaseBuilding.cs, asegúrate que sea accesible.
// [System.Serializable]
// public struct ConstructionCostEntry { ... }

[CreateAssetMenu(fileName = "NuevoEdificioData", menuName = "Culto del Monolito/Datos de Edificio")]
public class EdificioDataSO : ScriptableObject
{
    [Header("Información Básica")]
    public string nombreMostrado = "Edificio sin Nombre"; // Nombre para la UI
    public Sprite icono;
    [TextArea(3,5)]
    public string descripcion;

    [Header("Prefab y Costos")]
    [Tooltip("El Prefab del edificio que se instanciará. Debe tener un componente que herede de BaseBuilding.")]
    public BaseBuilding prefabDelEdificio; // El prefab real con el script BaseBuilding
    
    // Los costos también podrían estar aquí para mostrarlos en la UI,
    // y BaseBuilding.constructionCosts se usaría para la lógica interna.
    // O, si BaseBuilding.constructionCosts es la fuente de verdad, la UI podría leerlos del prefab.
    // Por simplicidad, podemos duplicarlos aquí para la UI o asumir que la UI los lee del prefab.
    // Para este ejemplo, asumiré que el BaseBuilding en el prefab ya tiene sus costos configurados.

    // Podrías añadir más datos aquí: tiempo de construcción, requisitos de tecnología, etc.
}
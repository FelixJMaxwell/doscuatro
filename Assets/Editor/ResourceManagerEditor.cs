// Archivo: ResourceManagerEditor.cs (En carpeta "Editor")
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ResourceManager))]
public class ResourceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Dibuja las propiedades públicas y [SerializeField] por defecto

        ResourceManager resourceManager = (ResourceManager)target;

        if (Application.isPlaying && resourceManager != null)
        {
            // Usar el nuevo método para obtener los recursos
            IReadOnlyDictionary<string, RecursoInstancia> runtimeRecursos = resourceManager.GetRuntimeRecursos();

            if (runtimeRecursos != null && runtimeRecursos.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Recursos Almacenados (En Ejecución)", EditorStyles.boldLabel);

                // Iterar sobre la copia de solo lectura
                foreach (KeyValuePair<string, RecursoInstancia> pair in runtimeRecursos)
                {
                    if (pair.Value != null && pair.Value.data != null) // Chequeo adicional de nulidad
                    {
                        EditorGUILayout.LabelField($"{pair.Key} ({pair.Value.data.Nombre}): {pair.Value.actual.ToString("F2")} / {pair.Value.Maximo.ToString("F0")}");
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"{pair.Key}: (Datos de instancia o RecurSO nulos)");
                    }
                }
                Repaint(); // Solicitar que se redibuje el inspector para ver cambios en tiempo real
            }
            else if (runtimeRecursos != null && runtimeRecursos.Count == 0)
            {
                EditorGUILayout.HelpBox("No hay recursos instanciados actualmente en ejecución.", MessageType.Info);
            }
            else if (runtimeRecursos == null) // Si GetRuntimeRecursos devolviera null por alguna razón
            {
                 EditorGUILayout.HelpBox("El diccionario de recursos en ejecución es nulo.", MessageType.Warning);
            }
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en modo de ejecución para ver las cantidades de recursos almacenados.", MessageType.Info);
        }
        // El caso de resourceManager == null ya lo maneja el !Application.isPlaying o el primer if.
    }
}
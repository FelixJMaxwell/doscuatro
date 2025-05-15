using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ResourceManager))]
public class ResourceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Dibuja las propiedades públicas por defecto

        ResourceManager resourceManager = (ResourceManager)target;

        if (Application.isPlaying && resourceManager != null && resourceManager.recursos != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recursos Almacenados", EditorStyles.boldLabel);

            foreach (KeyValuePair<string, RecursoInstancia> pair in resourceManager.recursos)
            {
                EditorGUILayout.LabelField($"{pair.Key}: {pair.Value.actual}");
            }
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en modo de ejecución para ver las cantidades de recursos.", MessageType.Info);
        }
        else if (resourceManager == null || resourceManager.recursos == null)
        {
            EditorGUILayout.HelpBox("El ResourceManager o su diccionario de recursos son nulos.", MessageType.Warning);
        }
    }
}
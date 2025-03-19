using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ReloadScriptsButtonInSceneView
{
    static ReloadScriptsButtonInSceneView()
    {
        // Suscribirse al evento duringSceneGUI que se llama cuando se dibuja el Scene View
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // Definir la posición y tamaño del botón en la esquina inferior izquierda
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, sceneView.position.height - 50, 150, 40));

        // Dibujar el botón y recargar scripts cuando se presione
        if (GUILayout.Button("Recargar Scripts"))
        {
            AssetDatabase.Refresh(); // Recarga los scripts
        }

        GUILayout.EndArea();
        Handles.EndGUI();
    }
}

// Archivo: MonolitoParticlesEditor.cs (Debe estar en una carpeta "Editor")
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MonolitoParticles))]
public class MonolitoParticlesEditor : Editor
{
    private MonolitoParticles scriptTarget;
    private SerializedProperty configuracionParticulasProp;
    private SerializedProperty amplitudMovimientoXGlobalProp;
    private SerializedProperty velocidadMinimaGlobalProp;
    private SerializedProperty velocidadMaximaGlobalProp;
    private SerializedProperty zonaDeTeleportProp;
    private SerializedProperty velocidadMovimientosEspecialesProp;
    private SerializedProperty dispersionXAntesDeRetornoProp; // NUEVA PROPIEDAD SERIALIZADA

    private GameObject parentDeParticulasFuente;

    private void OnEnable()
    {
        scriptTarget = (MonolitoParticles)target;
        // Encontrar todas las propiedades serializadas que queremos dibujar
        configuracionParticulasProp = serializedObject.FindProperty("configuracionParticulas");
        amplitudMovimientoXGlobalProp = serializedObject.FindProperty("amplitudMovimientoXGlobal");
        velocidadMinimaGlobalProp = serializedObject.FindProperty("velocidadMinimaGlobal");
        velocidadMaximaGlobalProp = serializedObject.FindProperty("velocidadMaximaGlobal");
        zonaDeTeleportProp = serializedObject.FindProperty("zonaDeTeleport");
        velocidadMovimientosEspecialesProp = serializedObject.FindProperty("velocidadMovimientosEspeciales");
        dispersionXAntesDeRetornoProp = serializedObject.FindProperty("dispersionXAntesDeRetorno"); // BUSCAR LA NUEVA VARIABLE
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Asignación Rápida de Partículas", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Arrastra un GameObject aquí. Todos sus hijos directos se añadirán a la lista 'Configuración Partículas', reemplazando los existentes. Luego podrás configurar cada uno individualmente.", MessageType.Info);

        parentDeParticulasFuente = (GameObject)EditorGUILayout.ObjectField(
            new GUIContent("Padre de Partículas Fuente", "Arrastra un GameObject aquí. Sus hijos se usarán como partículas."),
            parentDeParticulasFuente,
            typeof(GameObject),
            true
        );

        if (GUILayout.Button("Cargar Hijos como Partículas a Configuración"))
        {
            if (parentDeParticulasFuente != null)
            {
                AsignarParticulasDesdeFuente();
            }
            else
            {
                EditorUtility.DisplayDialog("Error de Asignación", "Por favor, asigna un 'Padre de Partículas Fuente' primero.", "OK");
            }
        }
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Configuración Global y Movimientos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(amplitudMovimientoXGlobalProp);
        EditorGUILayout.PropertyField(velocidadMinimaGlobalProp);
        EditorGUILayout.PropertyField(velocidadMaximaGlobalProp);
        EditorGUILayout.PropertyField(zonaDeTeleportProp);
        EditorGUILayout.PropertyField(velocidadMovimientosEspecialesProp, new GUIContent("Velocidad Mov. Especiales", "Velocidad para convergencia y retorno al origen."));
        
        // DIBUJAR LA NUEVA PROPIEDAD dispersionXAntesDeRetorno AQUÍ
        EditorGUILayout.PropertyField(dispersionXAntesDeRetornoProp, new GUIContent("Dispersión X Antes de Retorno", "Distancia máxima en X para la dispersión aleatoria antes del retorno."));

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Configuración Individual de Partículas", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(configuracionParticulasProp, true);

        serializedObject.ApplyModifiedProperties();
    }

    private void AsignarParticulasDesdeFuente()
    {
        if (parentDeParticulasFuente == null) return;
        Undo.RecordObject(scriptTarget, "Asignar Partículas desde Fuente");
        configuracionParticulasProp.ClearArray();
        Transform parentTransform = parentDeParticulasFuente.transform;
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            configuracionParticulasProp.InsertArrayElementAtIndex(i);
            SerializedProperty elementProp = configuracionParticulasProp.GetArrayElementAtIndex(i);
            
            elementProp.FindPropertyRelative("particleObject").objectReferenceValue = child.gameObject;
            // Establecer valores por defecto para las nuevas propiedades de MonolitoParticleConfig
            elementProp.FindPropertyRelative("amplitudOscilacionXIndividual").floatValue = 0f;
            elementProp.FindPropertyRelative("velocidadOscilacionIndividual").floatValue = 0f;
            elementProp.FindPropertyRelative("puedeTeleportar").boolValue = false;
            elementProp.FindPropertyRelative("teleportIntervaloMin").floatValue = 5f;
            elementProp.FindPropertyRelative("teleportIntervaloMax").floatValue = 15f;
        }
        EditorUtility.SetDirty(scriptTarget);
        Debug.Log($"Se asignaron {parentTransform.childCount} partículas desde '{parentDeParticulasFuente.name}' a la lista de configuración.");
    }
}
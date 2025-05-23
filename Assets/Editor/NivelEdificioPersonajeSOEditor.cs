// Archivo: NivelEdificioPersonajeSOEditor.cs (Debe estar en una carpeta "Editor")
using UnityEngine;
using UnityEditor; // Necesario para scripts de Editor
using System.Collections.Generic; // Para List

[CustomEditor(typeof(NivelEdificioPersonajeSO))] // Indica a Unity que este script es un Inspector custom para NivelEdificioPersonajeSO
public class NivelEdificioPersonajeSOEditor : Editor
{
    // Referencia al objeto NivelEdificioPersonajeSO que estamos inspeccionando
    private NivelEdificioPersonajeSO configSO;

    // Propiedades serializadas para un manejo robusto de Undo/Redo y Prefab Overrides
    SerializedProperty numeroDeNivelProp;
    SerializedProperty nombreDelNivelProp;
    SerializedProperty tiempoEntreGeneracionesProp;
    SerializedProperty maxPersonajesPermitidosProp;
    SerializedProperty costoFeGeneracionNormalProp;
    SerializedProperty costoFragmentoGeneracionLegendarioProp;
    SerializedProperty costoMejoraFeAlSiguienteNivelProp;
    SerializedProperty costoMejoraFragmentoAlSiguienteNivelProp;
    SerializedProperty costoMejoraHerramientasAlSiguienteNivelProp; // Esta es una lista
    SerializedProperty requiereObjetosProfesionAlLlegarProp;
    SerializedProperty desbloqueaLegendariosAlLlegarProp;

    private void OnEnable()
    {
        // Obtiene el objeto target que se está inspeccionando.
        configSO = (NivelEdificioPersonajeSO)target;

        // Encuentra las propiedades serializadas por su nombre de variable en NivelEdificioPersonajeSO.
        numeroDeNivelProp = serializedObject.FindProperty("numeroDeNivel");
        nombreDelNivelProp = serializedObject.FindProperty("nombreDelNivel");
        tiempoEntreGeneracionesProp = serializedObject.FindProperty("tiempoEntreGeneraciones");
        maxPersonajesPermitidosProp = serializedObject.FindProperty("maxPersonajesPermitidos");
        costoFeGeneracionNormalProp = serializedObject.FindProperty("costoFeGeneracionNormal");
        costoFragmentoGeneracionLegendarioProp = serializedObject.FindProperty("costoFragmentoGeneracionLegendario");
        costoMejoraFeAlSiguienteNivelProp = serializedObject.FindProperty("costoMejoraFeAlSiguienteNivel");
        costoMejoraFragmentoAlSiguienteNivelProp = serializedObject.FindProperty("costoMejoraFragmentoAlSiguienteNivel");
        costoMejoraHerramientasAlSiguienteNivelProp = serializedObject.FindProperty("costoMejoraHerramientasAlSiguienteNivel");
        requiereObjetosProfesionAlLlegarProp = serializedObject.FindProperty("requiereObjetosProfesionAlLlegar");
        desbloqueaLegendariosAlLlegarProp = serializedObject.FindProperty("desbloqueaLegendariosAlLlegar");
    }

    public override void OnInspectorGUI()
    {
        // Actualiza el objeto serializado al inicio de OnInspectorGUI. Esencial para que los cambios se reflejen.
        serializedObject.Update();

        EditorGUILayout.LabelField("Configuración del Nivel para Generador de Personajes", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // --- Sección de Identificación del Nivel ---
        EditorGUILayout.LabelField("Identificación del Nivel", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(numeroDeNivelProp, new GUIContent("Número de Nivel", "Número identificador de este nivel (ej. 1, 2, 3)."));
        EditorGUILayout.PropertyField(nombreDelNivelProp, new GUIContent("Nombre Descriptivo", "Nombre temático o descriptivo para este nivel (ej. 'Santuario Básico')."));
        EditorGUILayout.Space(5);

        // --- Sección de Parámetros de Generación ---
        EditorGUILayout.LabelField("Parámetros de Generación (EN este Nivel)", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(tiempoEntreGeneracionesProp, new GUIContent("Tiempo Entre Generaciones (s)", "Segundos entre cada intento de generación de personaje."));
        EditorGUILayout.PropertyField(maxPersonajesPermitidosProp, new GUIContent("Max Personajes Permitidos", "Límite de personajes que este edificio puede haber generado o mantener en este nivel."));
        EditorGUILayout.PropertyField(costoFeGeneracionNormalProp, new GUIContent("Costo Fe (Normal)", "Cantidad de Fe para generar un NPC normal."));
        EditorGUILayout.PropertyField(costoFragmentoGeneracionLegendarioProp, new GUIContent("Costo Fragmento (Legendario)", "Cantidad de Fragmentos para generar un NPC legendario."));
        EditorGUILayout.Space(5);

        // --- Sección de Costos de Mejora ---
        EditorGUILayout.LabelField("Costos para MEJORAR AL SIGUIENTE Nivel (desde este nivel)", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(costoMejoraFeAlSiguienteNivelProp, new GUIContent("Costo Fe para Mejorar", "Fe necesaria para pasar de este nivel al siguiente."));
        EditorGUILayout.PropertyField(costoMejoraFragmentoAlSiguienteNivelProp, new GUIContent("Costo Fragmento para Mejorar", "Fragmentos necesarios para pasar de este nivel al siguiente."));

        // Mostrar la lista de herramientas/recursos para mejorar.
        // EditorGUILayout.PropertyField maneja listas automáticamente.
        EditorGUILayout.PropertyField(costoMejoraHerramientasAlSiguienteNivelProp, new GUIContent("Herramientas para Mejorar", "Lista de objetos/recursos y sus cantidades para mejorar al siguiente nivel."), true); // 'true' para mostrar hijos/elementos de la lista.
        EditorGUILayout.Space(5);

        // --- Sección de Desbloqueos ---
        EditorGUILayout.LabelField("Desbloqueos (AL LLEGAR a este Nivel)", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(requiereObjetosProfesionAlLlegarProp, new GUIContent("Requiere Objetos Profesión", "Si al llegar a este nivel, la generación de NPCs comienza a requerir objetos de profesión."));
        EditorGUILayout.PropertyField(desbloqueaLegendariosAlLlegarProp, new GUIContent("Desbloquea Legendarios", "Si al llegar a este nivel, se desbloquea la capacidad de generar NPCs legendarios."));
        EditorGUILayout.Space(10);


        // --- Validaciones y Ayudas Visuales (Ejemplos) ---
        if (configSO.tiempoEntreGeneraciones <= 0)
        {
            EditorGUILayout.HelpBox("El 'Tiempo Entre Generaciones' debe ser mayor que cero.", MessageType.Warning);
        }
        if (configSO.maxPersonajesPermitidos < 0) // Podría ser 0 si un nivel no permite generar
        {
            EditorGUILayout.HelpBox("El 'Max Personajes Permitidos' no debería ser negativo.", MessageType.Warning);
        }

        if (configSO.desbloqueaLegendariosAlLlegar && configSO.costoFragmentoGeneracionLegendario <= 0)
        {
            EditorGUILayout.HelpBox("Si se desbloquean legendarios, el 'Costo Fragmento (Legendario)' debería ser mayor que cero.", MessageType.Info);
        }

        // Botón de ejemplo para alguna acción custom
        if (GUILayout.Button("Validar Datos del Nivel"))
        {
            Debug.Log($"Validando datos para: {configSO.nombreDelNivel} (Nivel {configSO.numeroDeNivel})");
            // Aquí podrías poner lógica de validación más compleja si fuera necesario.
            if (configSO.costoMejoraFeAlSiguienteNivel < 0)
            {
                 Debug.LogWarning($"El costo de mejora de Fe para {configSO.name} es negativo, ¡revisar!");
            }
        }

        // Aplica los cambios al objeto serializado. Esencial para que los cambios se guarden.
        serializedObject.ApplyModifiedProperties();

        // Opcional: Si haces cambios directos al 'configSO' (no a través de SerializedProperty),
        // necesitarías marcarlo como 'dirty' para que Unity sepa que debe guardarlo:
        // if (GUI.changed) // Si algún control que no usa SerializedProperty cambió
        // {
        //     EditorUtility.SetDirty(configSO);
        // }
    }
}
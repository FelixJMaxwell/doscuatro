using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConstruirMapa))]
public class PoblarMapa : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Poblar mapa"))
        {
            ConstruirMapa construirMapa = (ConstruirMapa)target;
            construirMapa.PoblarMapaMini();
        }
    }
}

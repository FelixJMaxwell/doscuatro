using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCData", menuName = "Entidades/NPC Data")]
public class NPCDataSO : ScriptableObject
{
    [Header("Información General")]
    public string npcNombre;
    public string profesion;
    public GameObject prefabModelo;

    [Header("Estado")]
    public float saludBase = 100f;
    public int edad;
    public string genero;

    [Header("Trabajo")]
    public string trabajo;

     [Header("Educacion")]
    public string educacion;
}
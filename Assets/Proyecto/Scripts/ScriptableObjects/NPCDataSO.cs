using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Entidades/NPC Data")]
public class NPCDataSO : ScriptableObject
{
    [Header("Información General")]
    public string npcNombre;
    public string profesion;
    [TextArea(3, 10)]
    public string descripcion;
    public Sprite icono;
    public GameObject prefabModelo;

    [Header("Rareza")]
    public string rareza = "F";  // "SS", "S", "A", "B", "C", "D", "E", "F" - Valor por defecto

    [Header("Basico")]
    public int edad;
    public string genero;

    [Header("Estadísticas Básicas (D&D)")]
    public int fuerza;
    public int destreza;
    public int constitucion;
    public int inteligencia;
    public int sabiduria;
    public int carisma;

    [Header("Estado")]
    public float saludBase = 100f;
    public float energiaBase = 100f;
    public float manaBase = 50f;
    public float velocidadMovimientoBase = 3f;

    [Header("Habilidades")]
    [TextArea(3, 10)]
    public string[] habilidades;

    [Header("Diálogo")]
    public bool puedeDialogar = true;
    [TextArea(3, 10)]
    public string dialogoInicial;
    [TextArea(3, 10)]
    public string[] frasesAleatorias;

    // Puedes añadir más datos específicos del NPC aquí
}
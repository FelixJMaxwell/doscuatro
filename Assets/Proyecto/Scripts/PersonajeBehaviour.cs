using UnityEngine;
using System.Collections.Generic;

public class PersonajeBehaviour : MonoBehaviour
{
    // Variables básicas generales
    public string nombre { get; private set; }
    public int edad { get; private set; }
    public string genero { get; private set; }
    public string rareza { get; private set; }  // "SS", "S", "A", "B", "C", "D", "E", "F"
    public string profesion { get; private set; }

    // Estadísticas
    public float salud { get; private set; }
    public float energia { get; private set; }
    public float mana { get; private set; }
    public float velocidadMovimiento { get; private set; }

    public Dictionary<string, int> estadisticas = new Dictionary<string, int>();

    // Inventario
    public List<GameObject> inventario = new List<GameObject>();
    public List<string> nombresNPCsConocidos = new List<string>();

    // Relaciones
    public Dictionary<string, float> relaciones = new Dictionary<string, float>();

    // Eventos
    public delegate void OnPersonajeNivelUp(PersonajeBehaviour personaje);
    public static event OnPersonajeNivelUp PersonajeNivelUp;

    private float tiempoVivo = 0f;
    [SerializeField] private float tiempoParaNuevaRelacion = 30f;  // Ejemplo: cada 30 segundos

    private void Start()
    {
        // Aquí podrías agregar lógica adicional de inicialización.
        tiempoVivo = 0f;
    }

    public void Inicializar(NPCDataSO data, Dictionary<string, int> stats, string rarezaAsignada)
    {
        // Información general
        nombre = data.npcNombre;
        edad = data.edad;
        genero = data.genero;
        profesion = data.profesion;
        rareza = rarezaAsignada; // Usar la rareza asignada por el edificio

        // Estadísticas
        salud = data.saludBase;
        energia = data.energiaBase;
        mana = data.manaBase;
        velocidadMovimiento = data.velocidadMovimientoBase;

        estadisticas = stats;

        // Inventario
        for (int i = 0; i < Random.Range(10, 16); i++)
        {
            //Generar un objeto aleatorio y añadirlo al inventario
            //GameObject objeto = GenerarObjetoAleatorio();
            //inventario.Add(objeto);
        }
        nombresNPCsConocidos.Add(nombre);
    }

    public bool EsLegendario()
    {
        return rareza == "SS" || rareza == "S";
    }


    public void AgregarRelacion(string nombreNPC, float valorInicial)
    {
        if (!relaciones.ContainsKey(nombreNPC))
        {
            relaciones.Add(nombreNPC, valorInicial);
        }
        else
        {
            relaciones[nombreNPC] = valorInicial;
        }
    }

    public void ModificarRelacion(string nombreNPC, float cantidad)
    {
        if (relaciones.ContainsKey(nombreNPC))
        {
            relaciones[nombreNPC] += cantidad;
        }
        else
        {
            AgregarRelacion(nombreNPC, cantidad);
        }
    }

    private void Update()
    {
        // Lógica de comportamiento del personaje.
        tiempoVivo += Time.deltaTime;

        if (tiempoVivo >= tiempoParaNuevaRelacion)
        {
            tiempoVivo = 0f; // Reiniciar el contador

            // Lógica para conocer un nuevo NPC y establecer una relación.
            ConocerNuevoNPC();
        }
    }

     private void ConocerNuevoNPC()
    {
        // 1. Encontrar otros NPCs cercanos (esto es un ejemplo, necesitarás tu propia lógica)
        PersonajeBehaviour[] otrosNPCs = FindObjectsOfType<PersonajeBehaviour>();
        PersonajeBehaviour npcMasCercano = null;
        float distanciaMinima = float.MaxValue;

        foreach (PersonajeBehaviour otroNPC in otrosNPCs)
        {
            if (otroNPC != this && !nombresNPCsConocidos.Contains(otroNPC.nombre)) // No conocerse a sí mismo y no conocer ya
            {
                float distancia = Vector3.Distance(transform.position, otroNPC.transform.position);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    npcMasCercano = otroNPC;
                }
            }
        }

        // 2. Si se encuentra un NPC cercano, establecer una relación.
        if (npcMasCercano != null)
        {
            nombresNPCsConocidos.Add(npcMasCercano.nombre);
            float relacionInicial = Random.Range(-10f, 10f); // Valor de relación inicial aleatorio
            AgregarRelacion(npcMasCercano.nombre, relacionInicial);
            Debug.Log($"{nombre} conoce a {npcMasCercano.nombre}. Relación inicial: {relacionInicial}");
        }
    }


    public void RecibirDaño(float daño)
    {
        salud -= daño;
        if (salud <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Destroy(gameObject);
    }
}
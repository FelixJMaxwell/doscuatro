using UnityEngine;
using System.Collections.Generic;

public class Building_Personajes : BaseBuilding
{
    [Header("Generación de Personajes")]
    [SerializeField] private NPCDataSO tipoPersonajeAGenerar; // ScriptableObject del tipo de NPC
    [SerializeField] private Transform puntoDeGeneracion; // Punto donde aparecerá el NPC
    [SerializeField] private float tiempoEntreGeneraciones = 5f; // Tiempo entre generación de NPCs
    private float tiempoTranscurrido;
    [SerializeField] private int maxPersonajesGenerados = 10;
    private int personajesGeneradosCount = 0;

    [Header("Requisitos de Fe")]
    [SerializeField] private float feNecesariaParaGenerar = 20f; // Cantidad de Fe necesaria para generar un NPC normal
    [SerializeField] private RecurSO feDataSO; // Referencia al ScriptableObject de Fe

    [Header("Generación de Legendarios")]
    [SerializeField] private bool puedeGenerarLegendarios = false; // Se desbloquea con progreso
    [SerializeField] private float fragmentosNecesariosParaLegendario = 5f; // Cantidad de fragmentos
    [SerializeField] private RecurSO fragmentoDataSO;
    [SerializeField] private NPCDataSO tipoPersonajeLegendario; //SO del personaje legendario

    [Header("Requisitos de Profesión")]
    [SerializeField] private bool requiereEducacionEspecializada = true;
    [SerializeField] private List<ObjetoProfesion> objetosProfesionNecesarios;

    [Header("Sacrificio")]
    [SerializeField] private bool puedeSacrificar = true;
    [SerializeField] private float relacionNegativaPorSacrificio = -10f; // Ejemplo de impacto en la relación

    // Evento para la indicación audiovisual
    public delegate void OnPersonajeGenerado(GameObject nuevoPersonaje);
    public static event OnPersonajeGenerado PersonajeGenerado;

    [System.Serializable]
    public struct ObjetoProfesion
    {
        public string nombreObjeto;
        public int cantidadNecesaria;
    }

    protected override void Start()
    {
        base.Start();
        buildingName = "Generador de Personajes";
        if (puntoDeGeneracion == null)
        {
            Debug.LogError("No se ha asignado el punto de generación en " + gameObject.name);
        }
        if (feDataSO == null)
        {
            Debug.LogError("No se ha asignado el RecurSO de Fe en " + gameObject.name);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isActive && tipoPersonajeAGenerar != null && puntoDeGeneracion != null && (personajesGeneradosCount < maxPersonajesGenerados))
        {
            tiempoTranscurrido += Time.deltaTime;
            if (tiempoTranscurrido >= tiempoEntreGeneraciones)
            {
                if (ResourceManager.Instance != null && ResourceManager.Instance.GetCantidad(feDataSO.Nombre) >= feNecesariaParaGenerar)
                {
                    // Verifica si se cumplen los requisitos de profesión
                    if (!requiereEducacionEspecializada || CumplirRequisitosProfesion())
                    {
                         GenerarPersonaje(false); // Genera un personaje normal
                         tiempoTranscurrido = 0f;
                    }
                    else
                    {
                        Debug.Log("No se cumplen los requisitos de profesión para generar personaje en " + gameObject.name);
                    }
                   
                }
                else
                {
                    Debug.Log("No hay suficiente Fe para generar personaje en " + gameObject.name);
                }
            }
        }
    }

    private void GenerarPersonaje(bool esLegendario)
    {
        GameObject nuevoPersonaje;
        string rarezaAsignada; // Variable para almacenar la rareza

        if (esLegendario)
        {
            if (ResourceManager.Instance.GetCantidad(fragmentoDataSO.Nombre) >= fragmentosNecesariosParaLegendario && puedeGenerarLegendarios)
            {
                nuevoPersonaje = Instantiate(tipoPersonajeLegendario.prefabModelo, puntoDeGeneracion.position, puntoDeGeneracion.rotation);
                ResourceManager.Instance.Gastar(fragmentoDataSO.Nombre, fragmentosNecesariosParaLegendario);
                rarezaAsignada = Random.Range(0, 2) == 0 ? "SS" : "S"; // Asignar "SS" o "S" aleatoriamente
            }
            else
            {
                Debug.Log("No se puede generar un personaje legendario");
                return;
            }
        }
        else
        {
            nuevoPersonaje = Instantiate(tipoPersonajeAGenerar.prefabModelo, puntoDeGeneracion.position, puntoDeGeneracion.rotation);
            ResourceManager.Instance.Gastar(feDataSO.Nombre, feNecesariaParaGenerar);
            rarezaAsignada = AsignarRarezaComun(); // Asignar rareza común
        }

        personajesGeneradosCount++;

        PersonajeBehaviour personajeBehaviour = nuevoPersonaje.GetComponent<PersonajeBehaviour>();
        if (personajeBehaviour != null)
        {
            personajeBehaviour.Inicializar(esLegendario ? tipoPersonajeLegendario : tipoPersonajeAGenerar, GenerarStatsAleatorios(), rarezaAsignada); // Pasar la rareza asignada
        }
        else
        {
            Debug.LogError("El prefab del personaje generado no tiene un script PersonajeBehaviour en " + gameObject.name);
        }

        // Invocar el evento de generación
        PersonajeGenerado?.Invoke(nuevoPersonaje);
    }

    private string AsignarRarezaComun()
    {
        // Lógica para asignar rarezas comunes (A, B, C, D, E, F)
        int randomValue = Random.Range(0, 100); // Rango del 0 al 99

        if (randomValue < 5)        return "A"; // 5%
        else if (randomValue < 15)   return "B"; // 10% (15-5)
        else if (randomValue < 30)  return "C"; // 15% (30-15)
        else if (randomValue < 50)  return "D"; // 20% (50-30)
        else if (randomValue < 75)  return "E"; // 25% (75-50)
        else                       return "F"; // 25% (100-75)
    }

     private Dictionary<string, int> GenerarStatsAleatorios()
    {
        Dictionary<string, int> statsAleatorios = new Dictionary<string, int>();
        statsAleatorios.Add("Fuerza", Random.Range(10, 20));
        statsAleatorios.Add("Destreza", Random.Range(10, 20));
        statsAleatorios.Add("Constitucion", Random.Range(10, 20));
        statsAleatorios.Add("Inteligencia", Random.Range(10, 20));
        statsAleatorios.Add("Sabiduria", Random.Range(10, 20));
        statsAleatorios.Add("Carisma", Random.Range(10, 20));
        return statsAleatorios;
    }

    private bool CumplirRequisitosProfesion()
    {
        if (objetosProfesionNecesarios == null || objetosProfesionNecesarios.Count == 0)
        {
            return true; // No hay requisitos
        }

        foreach (ObjetoProfesion requisito in objetosProfesionNecesarios)
        {
            if (ResourceManager.Instance == null || ResourceManager.Instance.GetCantidad(requisito.nombreObjeto) < requisito.cantidadNecesaria)
            {
                return false; // No se cumple un requisito
            }
        }
        //Si llego hasta aqui, se cumplen todos los requisitos
        return true;
    }

    public void SacrificarPersonaje(GameObject personajeASacrificar)
    {
        if (!puedeSacrificar) return;

        // Aquí iría la lógica para eliminar al personaje de la escena y del sistema de juego
        Destroy(personajeASacrificar);
        personajesGeneradosCount--;

        // Lógica para afectar las relaciones (ejemplo)
        //GameManager.Instance.ModificarRelaciones(relacionNegativaPorSacrificio);

        // Generar evento de sacrificio (opcional)
        //OnPersonajeSacrificado?.Invoke();
    }

    public void GenerarPersonajeLegendario()
    {
        GenerarPersonaje(true);
    }

    protected override void ProduceResources()
    {
        // Este edificio no produce recursos
    }
}

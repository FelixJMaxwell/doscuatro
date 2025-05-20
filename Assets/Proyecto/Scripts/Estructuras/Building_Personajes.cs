using UnityEngine;
using System.Collections.Generic;

public class Building_Personajes : BaseBuilding
{
    [Header("Generación de Personajes")]
    [SerializeField] private NPCDataSO tipoPersonajeAGenerar;
    [SerializeField] private Transform puntoDeGeneracion;
    [SerializeField] private float tiempoEntreGeneraciones = 5f;
    private float tiempoTranscurrido;
    [SerializeField] private int maxPersonajesGenerados = 10;
    private int personajesGeneradosCount = 0;

    [Header("Requisitos de Fe")]
    [SerializeField] private float feNecesariaParaGenerar = 20f;
    [SerializeField] private RecurSO feDataSO;

    [Header("Generación de Legendarios")]
    [SerializeField] private bool puedeGenerarLegendarios = false;
    [SerializeField] private float fragmentosNecesariosParaLegendario = 5f;
    [SerializeField] private RecurSO fragmentoDataSO;
    [SerializeField] private NPCDataSO tipoPersonajeLegendario;

    [Header("Requisitos de Profesión")]
    [SerializeField] private bool PuedeGenerarProfesiones = false; // Nuevo nombre
    [SerializeField] private List<ObjetoProfesion> objetosProfesionNecesarios;

    public delegate void OnPersonajeGenerado(GameObject nuevoPersonaje);
    public static event OnPersonajeGenerado PersonajeGenerado;

    [System.Serializable]
    public struct ObjetoProfesion
    {
        public string nombreObjeto;
        public int cantidadNecesaria;
    }

    [Header("Actualizaciones del Edificio")]
    [SerializeField] private int nivelMaximo = 10;
    [SerializeField] private float[] tiempoEntreGeneracionesPorNivel;
    [SerializeField] private int[] maxPersonajesGeneradosPorNivel;
    [SerializeField] private float[] costoFePorNivel;
    [SerializeField] private float[] costoFragmentoPorNivel;
    [SerializeField] private ObjetoProfesion[][] herramientasNecesariasPorNivel;
    private int nivelActual = 1;

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

        if (tiempoEntreGeneracionesPorNivel.Length != nivelMaximo ||
            maxPersonajesGeneradosPorNivel.Length != nivelMaximo ||
            costoFePorNivel.Length != nivelMaximo ||
            costoFragmentoPorNivel.Length != nivelMaximo ||
            herramientasNecesariasPorNivel.Length != nivelMaximo)
        {
            Debug.LogError("Los arrays de costos y capacidades en " + gameObject.name + " no tienen la longitud correcta. Deben tener " + nivelMaximo + " elementos.");
            nivelMaximo = Mathf.Min(tiempoEntreGeneracionesPorNivel.Length, maxPersonajesGeneradosPorNivel.Length, costoFePorNivel.Length, herramientasNecesariasPorNivel.Length);
        }
        InicializarNiveles();
        AplicarNivel();
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
                    if (!PuedeGenerarProfesiones || CumplirRequisitosProfesion()) // Usa el nuevo nombre
                    {
                        GenerarPersonaje(false);
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
        string rarezaAsignada;

        if (esLegendario)
        {
            if (ResourceManager.Instance.GetCantidad(fragmentoDataSO.Nombre) >= fragmentosNecesariosParaLegendario && puedeGenerarLegendarios)
            {
                nuevoPersonaje = Instantiate(tipoPersonajeLegendario.prefabModelo, puntoDeGeneracion.position, puntoDeGeneracion.rotation);
                ResourceManager.Instance.Gastar(fragmentoDataSO.Nombre, fragmentosNecesariosParaLegendario);
                rarezaAsignada = Random.Range(0, 2) == 0 ? "SS" : "S";
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
            rarezaAsignada = AsignarRarezaComun();
        }

        personajesGeneradosCount++;

        PersonajeBehaviour personajeBehaviour = nuevoPersonaje.GetComponent<PersonajeBehaviour>();
        if (personajeBehaviour != null)
        {
            personajeBehaviour.Inicializar(esLegendario ? tipoPersonajeLegendario : tipoPersonajeAGenerar);
        }
        else
        {
            Debug.LogError("El prefab del personaje generado no tiene un script PersonajeBehaviour en " + gameObject.name);
        }

        PersonajeGenerado?.Invoke(nuevoPersonaje);
    }

    private string AsignarRarezaComun()
    {
        int randomValue = Random.Range(0, 100);
        if (randomValue < 5) return "A";
        else if (randomValue < 15) return "B";
        else if (randomValue < 30) return "C";
        else if (randomValue < 50) return "D";
        else if (randomValue < 75) return "E";
        else return "F";
    }

    private bool CumplirRequisitosProfesion()
    {
        if (!PuedeGenerarProfesiones) return true; // Usa el nuevo nombre
        if (objetosProfesionNecesarios == null || objetosProfesionNecesarios.Count == 0)
        {
            return true;
        }

        foreach (ObjetoProfesion requisito in objetosProfesionNecesarios)
        {
            if (ResourceManager.Instance == null || ResourceManager.Instance.GetCantidad(requisito.nombreObjeto) < requisito.cantidadNecesaria)
            {
                return false;
            }
        }
        return true;
    }

    public void DesbloquearGeneracionLegendarios()
    {
        puedeGenerarLegendarios = true;
    }

    public void DesbloquearRequisitosProfesion()
    {
        PuedeGenerarProfesiones = true; // Usa el nuevo nombre
    }

    public void ActualizarEdificio()
    {
        if (nivelActual < nivelMaximo)
        {
            if (ResourceManager.Instance.GetCantidad(feDataSO.Nombre) >= costoFePorNivel[nivelActual - 1] &&
                ResourceManager.Instance.GetCantidad(fragmentoDataSO.Nombre) >= costoFragmentoPorNivel[nivelActual - 1] &&
                CumplirRequisitosHerramientas(herramientasNecesariasPorNivel[nivelActual - 1]))
            {
                ResourceManager.Instance.Gastar(feDataSO.Nombre, costoFePorNivel[nivelActual - 1]);
                ResourceManager.Instance.Gastar(fragmentoDataSO.Nombre, costoFragmentoPorNivel[nivelActual - 1]);

                nivelActual++;
                AplicarNivel();
            }
            else
            {
                Debug.Log("No hay suficientes recursos para actualizar el edificio " + gameObject.name);
            }
        }
        else
        {
            Debug.Log("El edificio " + gameObject.name + " ya está al máximo nivel.");
        }
    }

    private void AplicarNivel()
    {
        tiempoEntreGeneraciones = tiempoEntreGeneracionesPorNivel[nivelActual - 1];
        maxPersonajesGenerados = maxPersonajesGeneradosPorNivel[nivelActual - 1];
        // Aquí podrías añadir más lógica para aplicar cambios específicos del nivel.
        if (nivelActual >= 2)
        {
            PuedeGenerarProfesiones = true;
        }
        if (nivelActual >= 5)
        {
            puedeGenerarLegendarios = true;
        }
    }

    private bool CumplirRequisitosHerramientas(ObjetoProfesion[] herramientasNecesarias)
    {
        if (herramientasNecesarias == null || herramientasNecesarias.Length == 0)
        {
            return true;
        }

        foreach (ObjetoProfesion requisito in herramientasNecesarias)
        {
            if (ResourceManager.Instance == null || ResourceManager.Instance.GetCantidad(requisito.nombreObjeto) < requisito.cantidadNecesaria)
            {
                return false;
            }
        }
        return true;
    }

    private void InicializarNiveles()
    {
        tiempoEntreGeneracionesPorNivel = new float[nivelMaximo];
        maxPersonajesGeneradosPorNivel = new int[nivelMaximo];
        costoFePorNivel = new float[nivelMaximo];
        costoFragmentoPorNivel = new float[nivelMaximo];
        herramientasNecesariasPorNivel = new ObjetoProfesion[nivelMaximo][];

        for (int i = 0; i < nivelMaximo; i++)
        {
            tiempoEntreGeneracionesPorNivel[i] = Mathf.Max(1f, 5f - i * 0.5f);
            if (i < 9)
            {
                maxPersonajesGeneradosPorNivel[i] = 10000;
            }
            else
            {
                maxPersonajesGeneradosPorNivel[i] = int.MaxValue;
            }

            costoFePorNivel[i] = 10f * Mathf.Pow(2, i);
            costoFragmentoPorNivel[i] = 2f * Mathf.Pow(1.5f, i);

            herramientasNecesariasPorNivel[i] = new ObjetoProfesion[i + 1];
            for (int j = 0; j < herramientasNecesariasPorNivel[i].Length; j++)
            {
                herramientasNecesariasPorNivel[i][j].nombreObjeto = "HerramientaNivel" + (j + 1);
                herramientasNecesariasPorNivel[i][j].cantidadNecesaria = (int)Mathf.Pow(2, j);
            }
        }
        PuedeGenerarProfesiones = false;
        puedeGenerarLegendarios = false;
    }
}


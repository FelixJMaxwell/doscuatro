using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Building_Personajes : BaseBuilding
{
    [Header("Generación de Personajes")]
    [SerializeField] private NPCDataSO tipoPersonajeAGenerar;
    [SerializeField] private Transform puntoDeGeneracion;
    [SerializeField] private int maxPersonajesGenerados = 10;
    private int personajesGeneradosCount = 0;

    [SerializeField] private float tiempoEntreGeneraciones = 5f;
    private float tiempoTranscurrido;

    [Header("Requisitos de Fe")]
    [SerializeField] private float feNecesariaParaGenerar = 20f;
    [SerializeField] private RecurSO feDataSO;

    [Header("Generación de Legendarios")]
    [SerializeField] private bool puedeGenerarLegendarios = false;
    [SerializeField] private float fragmentosNecesariosParaLegendario = 5f;
    [SerializeField] private RecurSO fragmentoDataSO;
    [SerializeField] private NPCDataSO tipoPersonajeLegendario;

    [Header("Requisitos de Profesión")]
    [SerializeField] private bool PuedeGenerarProfesiones = false;
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

    [Header("Referencias de la UI")]
    [SerializeField] private Button generarNormalButton;
    [SerializeField] private Toggle generarLegendarioToggle;

    private bool generarLegendario;

    private void Awake()
    {
        tiempoEntreGeneracionesPorNivel = new float[nivelMaximo];
        maxPersonajesGeneradosPorNivel = new int[nivelMaximo];
        costoFePorNivel = new float[nivelMaximo];
        costoFragmentoPorNivel = new float[nivelMaximo];
        herramientasNecesariasPorNivel = new ObjetoProfesion[nivelMaximo][];
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

        if (generarNormalButton != null)
        {
            generarNormalButton.onClick.AddListener(() => GenerarPersonaje(generarLegendario));
        }
        else
        {
            Debug.LogError("No se ha asignado el botón para generar personajes normales en " + gameObject.name);
        }

        if (generarLegendarioToggle != null)
        {
            generarLegendarioToggle.onValueChanged.AddListener(SetGenerarLegendario);
            generarLegendarioToggle.interactable = puedeGenerarLegendarios;
        }
        else
        {
            Debug.LogError("No se ha asignado el toggle para generar personajes legendarios en " + gameObject.name);
        }

        tiempoTranscurrido = 0f;
    }

    protected override void Update()
    {
        base.Update();
        tiempoTranscurrido += Time.deltaTime;

        if (generarNormalButton != null)
        {
            generarNormalButton.interactable = tiempoTranscurrido >= tiempoEntreGeneraciones && personajesGeneradosCount < maxPersonajesGenerados;
        }
        if (generarLegendarioToggle != null)
        {
            generarLegendarioToggle.interactable = puedeGenerarLegendarios && personajesGeneradosCount < maxPersonajesGenerados;
        }
    }

    public void GenerarPersonaje(bool esLegendario)
    {
        if (tiempoTranscurrido < tiempoEntreGeneraciones)
        {
            Debug.Log("Todavía no se puede generar otro personaje en " + gameObject.name);
            return;
        }

        if (personajesGeneradosCount >= maxPersonajesGenerados)
        {
            Debug.Log("Se ha alcanzado el máximo de personajes generados en " + gameObject.name);
            return;
        }

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
            if (ResourceManager.Instance.GetCantidad(feDataSO.Nombre) >= feNecesariaParaGenerar)
            {
                nuevoPersonaje = Instantiate(tipoPersonajeAGenerar.prefabModelo, puntoDeGeneracion.position, puntoDeGeneracion.rotation);
                ResourceManager.Instance.Gastar(feDataSO.Nombre, feNecesariaParaGenerar);
                rarezaAsignada = AsignarRarezaComun();
            }
            else
            {
                Debug.Log("No hay suficiente Fe para generar personaje en " + gameObject.name);
                return;
            }
        }

        personajesGeneradosCount++;
        tiempoTranscurrido = 0f;

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
        if (!PuedeGenerarProfesiones) return true;
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
        if (generarLegendarioToggle != null)
            generarLegendarioToggle.interactable = true;
    }

    public void DesbloquearRequisitosProfesion()
    {
        PuedeGenerarProfesiones = true;
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
        if (nivelActual >= 2)
        {
            PuedeGenerarProfesiones = true;
        }
        if (nivelActual >= 5)
        {
            puedeGenerarLegendarios = true;
            if (generarLegendarioToggle != null)
                generarLegendarioToggle.interactable = true;
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

    public void SetGenerarLegendario(bool value)
    {
        generarLegendario = value;
    }
}


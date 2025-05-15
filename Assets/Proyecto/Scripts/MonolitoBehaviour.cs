using UnityEngine;
using System.Collections.Generic;

public class MonolitoBehaviour : MonoBehaviour
{
    [Header("Fragmentos")]
    public float CostoFragmentoBase = 10f; // Costo base para el primer fragmento
    public float IncrementoCostoFragmento = 1.2f; // Multiplicador del costo por fragmento
    public float CostoFragmento { get; private set; } // Costo actual de un fragmento
    private int ContadorFragmentos = 0; // Contador de fragmentos construidos

    // Porcentaje de que pueda pasar algo en el monolito (sin usar directamente para el nivel)
    [Range(0f, 1f)]
    public float PorcentajeSingularidad = 0.1f;

    [Header("Sistema de Fe")]
    [SerializeField] private RecurSO FeDataSO; // Referencia al ScriptableObject de Fe
    public float FeActual { get; private set; }
    public float NivelDeFe { get; private set; } = 1f; // Nivel inicial de Fe
    public float FeMaxima => FeDataSO != null ? FeDataSO.Maximo : 100f; // Valor máximo de Fe
    public float FeBase => FeDataSO != null ? FeDataSO.ValorBase : 10f; // Valor base de Fe
    public float FeNecesariaParaSubirNivel => FeDataSO != null ? FeDataSO.CalcularCostoSubirNivel(NivelDeFe) : 0f;

    [Space(10)]
    public GameObject Pilar;
    public float RadioMinimo = 2f;
    public float RadioMaximo = 5f;
    public Transform FaithHolder;

    [Header("Configuraciones")]
    public List<GameObject> EstructurasConectadas;
    public float Ticker = 0f;
    public float TickerLimit = 5f;

    public GameManager gameManager;
    public float GizmoRadio = 5f;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (!gameManager.Estructuras.Contains(transform))
        {
            gameManager.Estructuras.Add(transform);
        }

        // Inicializar la Fe actual al valor máximo o base al inicio
        /* FeActual = FeMaxima; // O FeActual = FeBase; según tu diseño */
        ActualizarCostoFragmento();
    }

    private void Update()
    {
        if (EstructurasConectadas.Count <= 0)
        {
            return;
        }

        Ticker += Time.deltaTime;

        if (Ticker >= TickerLimit)
        {
            Ticker = 0;
            // Aquí podrías tener lógica para generar Fe gradualmente si está conectado a estructuras
            // Ejemplo: FeActual = Mathf.Min(FeActual + (TasaGeneracionFe * Time.deltaTime), FeMaxima);
        }
    }

    public void GenerarPilar()
    {
        float angulo = Random.Range(0f, 360f);
        float radianes = angulo * Mathf.Deg2Rad;

        float distancia = Random.Range(RadioMinimo, RadioMaximo);
        float x = transform.position.x + Mathf.Cos(radianes) * distancia;
        float z = transform.position.z + Mathf.Sin(radianes) * distancia;

        float RandomY = Random.Range(-0.3f, 0.5f);

        Vector3 position = new Vector3(x, RandomY, z);

        GameObject tempPilar = Instantiate(Pilar, new Vector3(position.x, -0.5f, position.z), Quaternion.identity);
        tempPilar.transform.SetParent(FaithHolder);
        tempPilar.transform.name = "Pilar_ " + FaithHolder.childCount;
        tempPilar.GetComponent<PilarBehaviour>().Posicion = position;
    }

    public void IntentarSubirNivelFe()
    {
        if (FeDataSO != null && ResourceManager.Instance != null)
        {
            float costoSubida = FeNecesariaParaSubirNivel;
            float recursosFeActual = ResourceManager.Instance.GetCantidad(FeDataSO.recursoRequeridoParaNivel);

            if (recursosFeActual >= costoSubida)
            {
                ResourceManager.Instance.Gastar(FeDataSO.recursoRequeridoParaNivel, costoSubida);
                NivelDeFe++;
                Debug.Log($"Nivel de Fe del Monolito subido a {NivelDeFe}. Costo: {costoSubida} {FeDataSO.recursoRequeridoParaNivel}");
                // Recalcular la Fe necesaria para el siguiente nivel
            }
            else
            {
                Debug.Log($"No suficiente {FeDataSO.recursoRequeridoParaNivel} para subir el nivel de Fe. Necesitas {costoSubida}, tienes {recursosFeActual}");
            }
        }
        else
        {
            Debug.LogError("FeDataSO no asignado o ResourceManager no encontrado.");
        }
    }

    private void ActualizarCostoFragmento()
    {
        CostoFragmento = CostoFragmentoBase * Mathf.Pow(IncrementoCostoFragmento, ContadorFragmentos);
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && !gameManager.EstructuraSeleccionada)
        {
            gameManager.EstructuraSeleccionada = transform.gameObject;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GizmoRadio);
    }

    public void AñadirFeManualmente(float cantidad)
    {
        if (FeDataSO != null && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.Añadir(FeDataSO.Nombre, cantidad);
            Debug.Log($"{cantidad} de Fe añadida al sistema.");
        }
        else
        {
            Debug.LogError("FeDataSO no asignado o ResourceManager no encontrado.");
        }
    }
}
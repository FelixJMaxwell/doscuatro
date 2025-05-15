using UnityEngine;
using System.Collections.Generic;



public class DadoBehaviour : MonoBehaviour
{
    public float ValorActual;
    public int CantCaras;
    public float LimiteTicker;
    public float RadioDeteccion;
    public List<GameObject> ObjetosConectados;
    public int ContadorPulsar = 0;

    [Header("Configuraciones")]
    public Transform Ligado;
    public float ticker;
    public GameManager gameManager;
    public bool DadoEstablecido;
    public bool Selected;
    public Vector3 offset;
    public bool Clicked;
    public bool ColocarDado;
    
    [Space(10)]
    private int IndiceActual = 0;
    public float DelayScroll = 0.2f;
    private float TiempoUltimoScroll;
    public List<GameObject> Elementos;
    public GameObject LigadoA;
    public Transform Plataforma;
    public List<Material> MaterialesPlataforma;

    private LineRenderer lineaDado;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        lineaDado = GetComponentInChildren<LineRenderer>();
        lineaDado.positionCount = 2;
    }

    private void Update() {
        if (Clicked && DadoEstablecido)
        {
            ticker += Time.deltaTime;
            if (ticker >= LimiteTicker)
            {
                Clicked = false;
                ticker = 0;
            }
        } else {
            Clicked = false;
        }

        if (!DadoEstablecido && gameManager.EstructuraConstruyendo)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DadoEstablecido = true;
                gameManager.EstructuraConstruyendo = null;
                ColocarDado = true;

                if (!gameManager.Estructuras.Contains(transform))
                {
                    gameManager.Estructuras.Add(transform);
                }
            }
        }

        if(ColocarDado){
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 0.5f, transform.position.z), 2.5f * Time.deltaTime);
            
            float Distancia = Vector3.Distance(transform.position, new Vector3(transform.position.x, 0.5f, transform.position.z));

            if(Distancia <= 0.01f){
                ColocarDado = false;
            }
        }

        if (Selected)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10);
            
            foreach (Collider item in hitColliders)
            {
                float distancia = Vector3.Distance(transform.position, item.transform.position);

                if (item.CompareTag("Monolito") && distancia > 4f && distancia < 6f)
                {
                    if (!Elementos.Contains(item.gameObject))
                    {
                        Elementos.Add(item.gameObject);
                    }
                }
                if (item.CompareTag("Dado") && distancia <= 3.8f)
                {
                    if (!Elementos.Contains(item.gameObject) && item != transform.GetComponent<Collider>())
                    {
                        Elementos.Add(item.gameObject);
                    }
                }
            }

            float ScrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (Time.time - TiempoUltimoScroll > DelayScroll) {
                if (ScrollInput > 0)
                {
                    IndiceActual = Elementos.Count - 1;
                }

                ActualizarSeleccion();
            } else if (ScrollInput < 0) {
                IndiceActual++;
                if (IndiceActual >= Elementos.Count)
                {
                    IndiceActual = 0; // Volver al primero si se pasa del final
                }
                ActualizarSeleccion();
            }

            TiempoUltimoScroll = Time.time;
        }
    }

    void ActualizarSeleccion()
    {
        if(gameManager.EstructuraSeleccionada == transform.gameObject) {
            if (Elementos.Count == 0)
            {
                return;
            }

            if (LigadoA != null)
            {
                if (LigadoA.GetComponent<MonolitoBehaviour>())
                {
                    MonolitoBehaviour monolitoAnterior = LigadoA.GetComponent<MonolitoBehaviour>();

                    if (monolitoAnterior.EstructurasConectadas.Contains(gameObject))
                    {
                        monolitoAnterior.EstructurasConectadas.Remove(gameObject);
                    }
                }

                if (LigadoA.GetComponent<DadoBehaviour>())
                {
                    DadoBehaviour dadoAnterior = LigadoA.GetComponent<DadoBehaviour>();

                    if (dadoAnterior.ObjetosConectados.Contains(gameObject))
                    {
                        dadoAnterior.ObjetosConectados.Remove(gameObject);
                    }
                }
            }

            LigadoA = Elementos[IndiceActual].gameObject;

            float distance = Vector3.Distance(transform.position, LigadoA.transform.position);

            if (LigadoA.CompareTag("Monolito") && distance <= 10)
            {
                lineaDado.gameObject.SetActive(true);
                lineaDado.SetPosition(1, new Vector3(transform.position.x, 0.1f, transform.position.z));
                lineaDado.SetPosition(0, new Vector3(LigadoA.transform.position.x, 0.1f, LigadoA.transform.position.z));
            } else {
                lineaDado.gameObject.SetActive(false);
            }

            if (LigadoA.CompareTag("Dado"))
            {
                if (distance <= RadioDeteccion)
                {
                    lineaDado.gameObject.SetActive(true);
                    lineaDado.SetPosition(1, new Vector3(transform.position.x, 0.1f, transform.position.z));
                    lineaDado.SetPosition(0, new Vector3(LigadoA.transform.position.x, 0.1f, LigadoA.transform.position.z));
                } else {
                    lineaDado.gameObject.SetActive(false);
                }
            }
        }



        if (LigadoA.GetComponent<MonolitoBehaviour>())
        {
            MonolitoBehaviour monolito = LigadoA.GetComponent<MonolitoBehaviour>();

            if (!monolito.EstructurasConectadas.Contains(gameObject))
            {
                monolito.EstructurasConectadas.Add(gameObject);
            }
        }

        if (LigadoA.GetComponent<DadoBehaviour>())
        {
            DadoBehaviour dado = LigadoA.GetComponent<DadoBehaviour>();

            if (!dado.ObjetosConectados.Contains(gameObject))
            {
                dado.ObjetosConectados.Add(gameObject);
            }
        }
    }

    private void OnMouseOver() {
        if (gameManager.EstructuraConstruyendo == transform.gameObject) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!gameManager.EstructuraSeleccionada)
            {
                gameManager.EstructuraSeleccionada = transform.gameObject;
                Selected = true;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Clicked)
            {
                return;
            }

            EstablecerValorActual();
        }
    }

    public void EstablecerValorActual(){
        ValorActual = UnityEngine.Random.Range(1,CantCaras);
    }

    /// <summary>
    /// Callback to draw gizmos only if the object is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, RadioDeteccion);
    }
}

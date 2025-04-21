using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

public class MonolitoBehaviour : MonoBehaviour
{
    public int ConexionesSimultaneas;
    public float CadenciaDisparo = 1;
    public float LimiteCadenciaDisparo = 5;
    public GameObject Pulsar;

    [Header("Fragmentos")]
    //El costo de los fragmentos se basa en adoración por parte de los humanos o de la cantidad de clicks que haga el usuario
    public float CostoFragmento;
    private int ContadorUpgradeCostoFragmento;
    public int PartesMonolito;
    private int ContadorUpgradePartesMonolito;
    //Porcentaje de que pueda pasar algo en el monolito
    public float PorcentajeSingularidad;

    [Header("Sistema de Fe")]
    public float FeActual;
    public float FeMaxima;
    public float NivelDeFe;
    public float FeBase;
    public float FeNecesariaParaSubirNivel;

    [Space(10)]
    public GameObject Pilar;
    public float RadioMinimo;
    public float RadioMaximo;
    public Transform FaithHolder;

    [Header("Configuraciones")]
    public List<GameObject> ObjetosConectados;
    public float Ticker;
    public float TickerLimit = 5;

    public int ContadorPulsar;
    public List<GameObject> Pulsos;
    public GameManager gameManager;
    public float GizmoRadio;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    
        if (!gameManager.Estructuras.Contains(transform))
        {
            gameManager.Estructuras.Add(transform);
        }
    }

    private void Update() {
        if (ObjetosConectados.Count <= 0)
        {
            return;
        }
        
        Ticker += Time.deltaTime;

        if (Ticker >= TickerLimit)
        {
            Ticker = 0;
            EnviarPulso();
        }
    }

    public void EnviarPulso(){
        ContadorPulsar++;
        Pulsos = new List<GameObject>();
        
        for (int i = 0; i < (CadenciaDisparo * ObjetosConectados.Count); i++)
        {
            GameObject tempPulsar = Instantiate(Pulsar, transform.position, Quaternion.identity);
            PulsarBehaviour _tempPulsar = tempPulsar.GetComponent<PulsarBehaviour>();
            _tempPulsar.VinoDesde = transform.gameObject;
            Vector3 posicion = Random.insideUnitCircle * 0.25f;
            _tempPulsar.transform.position = posicion + transform.position;

            if (!Pulsos.Contains(tempPulsar))
            {
                Pulsos.Add(tempPulsar);
            }
        }

        int cantidadObjetos = ObjetosConectados.Count;
        int cantidadPulsos = Pulsos.Count;

        if (cantidadObjetos == 0 || cantidadPulsos == 0) return;

        // Calcular cuántos pulsos se asignan por objeto
        int pulsosPorObjeto = cantidadPulsos / cantidadObjetos;

        // Asignar los pulsos a cada objeto conectado
        int pulsoIndex = 0;
        for (int i = 0; i < cantidadObjetos; i++)
        {
            for (int e = 0; e < pulsosPorObjeto; e++)
            {
                if (pulsoIndex < cantidadPulsos)
                {
                    PulsarBehaviour tempPulso = Pulsos[pulsoIndex].GetComponent<PulsarBehaviour>();
                    tempPulso.Objetivo = ObjetosConectados[i].transform;
                    pulsoIndex++;
                }
            }
        }

        // Si sobran pulsos, asignarlos a los primeros objetos conectados
        for (int i = 0; pulsoIndex < cantidadPulsos; i++)
        {
            PulsarBehaviour tempPulso = Pulsos[pulsoIndex].GetComponent<PulsarBehaviour>();
            tempPulso.Objetivo = ObjetosConectados[i % cantidadObjetos].transform;
            pulsoIndex++;
        }
    }

    public void GenerarPilar(){
        float angulo = Random.Range(0f, 360f);
        float radianes = angulo * Mathf.Deg2Rad;

        float distancia = Random.Range(RadioMinimo, RadioMaximo);
        float x = transform.position.x + Mathf.Cos(radianes) * distancia;
        float z = transform.position.z + Mathf.Sin(radianes) * distancia;

        float RandomY = Random.Range(-0.1f, 0.5f);

        Vector3 position = new Vector3(x, RandomY, z);

        GameObject tempPilar = Instantiate(Pilar, new Vector3(position.x,-0.5f, position.z), Quaternion.identity);
        tempPilar.transform.SetParent(FaithHolder);
        tempPilar.transform.name = "Pilar_ " + FaithHolder.childCount;
        tempPilar.GetComponent<PilarBehaviour>().Posicion = position;
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0) && !gameManager.EstructuraSeleccionada)
        {
            gameManager.EstructuraSeleccionada = transform.gameObject;
        }
    }

    /// <summary>
    /// Callback to draw gizmos only if the object is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GizmoRadio);
    }

}

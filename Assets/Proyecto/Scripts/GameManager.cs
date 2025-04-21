using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float Puntos;
    public TextMeshProUGUI Saldo;
    public GameObject Monolito;

    [Space(10)]
    public float VelocidadMovimientoDado;
    public GameObject DadoConstruyendo;
    public GameObject EstructuraSeleccionada;
    public GameObject DadoGO;
    public Transform DadoHolder;
    public Transform PulsarHolder;
    public Transform ParticleHolder;
    public List<Transform> Estructuras;

    [Space(10)]
    public GameObject PlataformaActual;
    
    [Space(10)]
    public List<GameObject> TextosUI;


    /* [Space(10)]
    public int pelotas;
    public float Multiplo; */

    public void ActualizarUI(TextMeshProUGUI ElementoUI, string texto){
        ElementoUI.text = texto;
        Estructuras = new List<Transform>();
    }

    public void ComprarDado(){
        if (!DadoConstruyendo)
        {
            GameObject tempDado = Instantiate(DadoGO, Monolito.transform.position + new Vector3(0,5,0), quaternion.identity);
            DadoConstruyendo = tempDado;
            tempDado.transform.SetParent(DadoHolder);
            tempDado.name = "Dado_" + DadoHolder.childCount;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (EstructuraSeleccionada)
            {
                EstructuraSeleccionada = null;
            }
        }

        if (DadoConstruyendo)
        {
            float MouseX = Input.GetAxis("Mouse X");
            float MouseY = Input.GetAxis("Mouse Y");

            Vector3 movimiento = new Vector3(MouseX, 0f, MouseY) * VelocidadMovimientoDado;

            DadoConstruyendo.transform.position += movimiento;

            DadoBehaviour tempDadoConstruyendo = DadoConstruyendo.GetComponent<DadoBehaviour>();

            foreach (Transform estructura in Estructuras)
            {
                float distancia = Vector3.Distance(tempDadoConstruyendo.Plataforma.position, estructura.position);

                if (distancia <= 3.8f)
                {
                    tempDadoConstruyendo.Plataforma.GetComponent<MeshRenderer>().material = tempDadoConstruyendo.MaterialesPlataforma[0];
                } else {
                    tempDadoConstruyendo.Plataforma.GetComponent<MeshRenderer>().material = tempDadoConstruyendo.MaterialesPlataforma[1];
                }
            }
        }

        if (EstructuraSeleccionada != null && EstructuraSeleccionada.name.Contains("Monolito"))
        {
            TextosUI[0].gameObject.SetActive(true);
            MonolitoBehaviour tempMonolito = EstructuraSeleccionada.GetComponent<MonolitoBehaviour>();

            if (Input.GetKeyDown(KeyCode.F))
            {
                tempMonolito.FeActual++;
                for (int i = 0; i < 3; i++)
                {
                    tempMonolito.GenerarPilar();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EstructuraSeleccionada = null;
                TextosUI[0].gameObject.SetActive(false);
            }
        }


    }

    public void SubirFireRateMonolito(){
        MonolitoBehaviour monolito = Monolito.GetComponent<MonolitoBehaviour>();

        if (monolito.CadenciaDisparo >= monolito.LimiteCadenciaDisparo) return;

        monolito.CadenciaDisparo++;
    }



    /* public void GenerarPuntos(){
        Debug.Log("1");
        StartCoroutine(GenerarObjetosConDelay());
        Debug.Log("2");
    } */

    /* public IEnumerator GenerarObjetosConDelay(){
        GameObject tempPulsar = Monolito.GetComponent<MonolitoBehaviour>().Pulsar;
        tempPulsar.GetComponent<PulsarBehaviour>().enabled = false;    
        
        for (int i = 0; i < pelotas; i++)
        {
            GameObject temp  = Instantiate(tempPulsar, Vector3.zero, quaternion.identity);
            Vector3 Posicion = UnityEngine.Random.insideUnitSphere * Multiplo;
            temp.transform.position = new Vector3(Posicion.x, Posicion.y, Posicion.z);

            yield return new WaitForSeconds(0.01f);
            Debug.Log("3");
        }
    } */
}

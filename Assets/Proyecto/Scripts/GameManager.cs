using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float Puntos;
    public TextMeshProUGUI Saldo;
    public GameObject Monolito;

    [Space(10)]
    public GameObject DadoConstruyendo;
    public GameObject DadoSeleccionado;
    public GameObject DadoGO;
    public Transform DadoHolder;

    [Space(10)]
    public GameObject PlataformaActual;

    /* [Space(10)]
    public int pelotas;
    public float Multiplo; */

    public void ActualizarUI(TextMeshProUGUI ElementoUI, string texto){
        ElementoUI.text = texto;
    }

    public void ComprarDado(){
        GameObject tempDado = Instantiate(DadoGO, new Vector3(0,-10,0), quaternion.identity);
        DadoConstruyendo = tempDado;
        tempDado.transform.SetParent(DadoHolder);
        tempDado.name = "Dado_" + DadoHolder.childCount;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (DadoSeleccionado)
            {
                DadoSeleccionado = null;
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

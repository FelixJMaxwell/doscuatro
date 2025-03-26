using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Collections;

public class MonolitoBehaviour : MonoBehaviour
{
    public int ConexionesSimultaneas;
    public float FireRate = 1;
    public float LimitFireRate = 5;
    public GameObject Pulsar;

    [Header("Configuraciones")]
    public List<GameObject> ObjetosConectados;
    public float Ticker;
    public float TickerLimit = 5;

    public int ContadorPulsar;

    private void Start() {
        
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

        StartCoroutine(GenerarPulso(ObjetosConectados, transform.position));

    }

    public IEnumerator GenerarPulso(List<GameObject> _ObjectosConectados, Vector3 _Posicion){
        foreach (GameObject objetoConectado in _ObjectosConectados)
        {
            if (objetoConectado.tag == "Dado")
            {
                DadoBehaviour DadoConectado = objetoConectado.GetComponent<DadoBehaviour>();

                // Generar 'FireRate' cantidad de pulsos con un pequeño retraso entre ellos
                for (int e = 0; e < FireRate; e++)
                {
                    // Instanciar el Pulsar en la posición del objeto
                    GameObject PulsarTemp = Instantiate(Pulsar, _Posicion, Quaternion.identity);

                    // Ajustar la posición del Pulsar para alinearlo al Dado conectado
                    PulsarTemp.transform.position += new Vector3(0, DadoConectado.transform.position.y, 0);

                    // Asignar el objetivo al Pulsar
                    PulsarTemp.GetComponent<PulsarBehaviour>().Objetivo = DadoConectado.transform;

                    // Nombrar el Pulsar para diferenciarlo
                    PulsarTemp.transform.name = "Pulsar_" + ContadorPulsar;
                    ContadorPulsar++;

                    // Esperar antes de generar el siguiente pulso
                    yield return new WaitForSeconds(0.2f); // Tiempo de espera entre pulsos (ajustable)
                }
            }
        }
    }

}

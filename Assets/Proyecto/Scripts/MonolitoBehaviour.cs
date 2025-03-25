using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class MonolitoBehaviour : MonoBehaviour
{
    public int ConexionesSimultaneas;
    public GameObject Pulsar;

    [Header("Configuraciones")]
    public List<GameObject> ObjetosConectados;
    public float Ticker;
    public float TickerLimit;

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
        for (int i = 0; i < ObjetosConectados.Count; i++)
        {
            if (ObjetosConectados[i].tag == "Dado")
            {
                DadoBehaviour DadoConectado = ObjetosConectados[i].GetComponent<DadoBehaviour>();

                GameObject PulsarTemp = Instantiate(Pulsar, transform.position, quaternion.identity);
                PulsarTemp.transform.position += new Vector3(0,DadoConectado.transform.position.y, 0);
                PulsarTemp.GetComponent<PulsarBehaviour>().Objetivo = DadoConectado.transform;
                PulsarTemp.transform.name = "Pulsar_" + ContadorPulsar;
            }
        }
    }

}

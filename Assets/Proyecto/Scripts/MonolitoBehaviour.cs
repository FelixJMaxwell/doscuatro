using UnityEngine;
using System.Collections.Generic;

public class MonolitoBehaviour : MonoBehaviour
{
    public int ConexionesSimultaneas;
    public float CadenciaDisparo = 1;
    public float LimiteCadenciaDisparo = 5;
    public GameObject Pulsar;

    [Header("Configuraciones")]
    public List<GameObject> ObjetosConectados;
    public float Ticker;
    public float TickerLimit = 5;

    public int ContadorPulsar;
    public List<GameObject> Pulsos;

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
        Pulsos = new List<GameObject>();
        
        for (int i = 0; i < (CadenciaDisparo * ObjetosConectados.Count); i++)
        {
            GameObject tempPulsar = Instantiate(Pulsar, transform.position, Quaternion.identity);
            PulsarBehaviour _tempPulsar = tempPulsar.GetComponent<PulsarBehaviour>();
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

}

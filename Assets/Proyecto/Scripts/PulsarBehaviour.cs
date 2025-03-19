using System;
using UnityEngine;

public class PulsarBehaviour : MonoBehaviour
{
    public Transform Objetivo;
    public float VelocidadMovimiento = 1;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Objetivo.position, VelocidadMovimiento * Time.deltaTime);

        float Distancia = Vector3.Distance(transform.position, Objetivo.transform.position);

        if (Distancia <= 0.2f)
        {
            Objetivo.GetComponent<DadoBehaviour>().GenerarPunto();
            DestroyImmediate(transform.gameObject);
        }
    }
}

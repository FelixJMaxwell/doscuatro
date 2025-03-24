using System;
using Unity.Mathematics;
using UnityEngine;

public class PulsarBehaviour : MonoBehaviour
{
    public Transform Objetivo;
    public float VelocidadMovimiento = 2;
    public GameObject Particula;
    public Transform ParticulaPos;

    void Start()
    {
        ParticulaPos = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Objetivo.position, VelocidadMovimiento * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name.Contains("Dado"))
        {
            collision.transform.GetComponent<DadoBehaviour>().GenerarPunto();
            Destroy(transform.gameObject);
            Quaternion rotacion = Quaternion.Euler(0,90,-90);
            GameObject _Particula = Instantiate(Particula, ParticulaPos.position, rotacion);
            _Particula.gameObject.SetActive(true);
        }
    }
}

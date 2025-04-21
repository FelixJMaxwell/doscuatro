using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PulsarBehaviour : MonoBehaviour
{
    public Transform Objetivo;
    public float VelocidadMovimiento = 2;
    public GameObject Particula;
    public Transform ParticulaPos;
    public GameManager gameManager;
    public GameObject VinoDesde;

    void Start()
    {
        ParticulaPos = transform.GetChild(0);
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Objetivo.position, VelocidadMovimiento * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.name.Contains("Dado") && other.transform == Objetivo)
        {
            other.transform.GetComponent<DadoBehaviour>().GenerarPunto();
            Destroy(transform.gameObject);

            Vector3 direccion = VinoDesde.transform.position - transform.position;

            Quaternion rotacion = Quaternion.LookRotation(direccion);
            GameObject _Particula = Instantiate(Particula, ParticulaPos.position, rotacion);
            _Particula.gameObject.SetActive(true);

            other.transform.GetComponent<DadoBehaviour>().EnviarPulso();
        }
    }
}

using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float Puntos;
    public GameObject Monolito;

    [Space(10)]
    public float VelocidadMovimientoDado;
    public GameObject EstructuraConstruyendo;
    public GameObject EstructuraSeleccionada;
    public GameObject EstructurAConstruir;

    public Transform EstructurasHolder;
    public List<Transform> Estructuras;
    
    [Space(10)]
    public List<GameObject> TextosUI;

    public void ActualizarUI(TextMeshProUGUI ElementoUI, string texto){
        ElementoUI.text = texto;
        Estructuras = new List<Transform>();
    }

    public void ComprarDado(){
        if (!EstructuraConstruyendo)
        {
            GameObject tempDado = Instantiate(EstructurAConstruir, Monolito.transform.position + new Vector3(0,5,0), quaternion.identity);
            EstructuraConstruyendo = tempDado;
            tempDado.transform.SetParent(EstructurasHolder);
            tempDado.name = "Dado_" + EstructurasHolder.childCount;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (EstructuraSeleccionada)
            {

                if (EstructuraSeleccionada.GetComponent<MonolitoBehaviour>())
                {
                    TextosUI[0].gameObject.SetActive(false);
                }
                
                EstructuraSeleccionada = null;
            }
        }

        if (EstructuraConstruyendo)
        {
            float MouseX = Input.GetAxis("Mouse X");
            float MouseY = Input.GetAxis("Mouse Y");

            Vector3 movimiento = new Vector3(MouseX, 0f, MouseY) * VelocidadMovimientoDado;
            EstructuraConstruyendo.transform.position += movimiento;

            DadoBehaviour tempDadoConstruyendo = EstructuraConstruyendo.GetComponent<DadoBehaviour>();

            bool estaCercaDeAlguna = false;

            foreach (Transform estructura in Estructuras)
            {
                float distancia = Vector3.Distance(tempDadoConstruyendo.Plataforma.position, estructura.position);

                bool tieneMonolit = estructura.GetComponent<MonolitoBehaviour>() != null;

                if (
                    (tieneMonolit && distancia > 4f && distancia < 6f) ||
                    (!tieneMonolit && distancia <= 3.8f)
                )
                {
                    estaCercaDeAlguna = true;
                    break; // Ya con una estructura cercana es suficiente
                }
            }

            MeshRenderer renderer = tempDadoConstruyendo.Plataforma.GetComponent<MeshRenderer>();
            renderer.material = estaCercaDeAlguna
                ? tempDadoConstruyendo.MaterialesPlataforma[0]
                : tempDadoConstruyendo.MaterialesPlataforma[1];
        }


        if (EstructuraSeleccionada != null && EstructuraSeleccionada.name.Contains("Monolito"))
        {
            TextosUI[0].gameObject.SetActive(true);
            MonolitoBehaviour tempMonolito = EstructuraSeleccionada.GetComponent<MonolitoBehaviour>();

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (tempMonolito != null && ResourceManager.Instance != null)
                {
                    // Añade 1 unidad de Fe al ResourceManager
                    tempMonolito.AñadirFeManualmente(1f);

                    // Genera 3 pilares (esto consume Fe según el costo actual del fragmento)
                    for (int i = 0; i < 3; i++)
                    {
                        tempMonolito.GenerarPilar();
                    }
                } else {
                    Debug.LogError("MonolitoBehaviour, FeDataSO o ResourceManager no encontrados en la estructura seleccionada.");
                }

                TextosUI[1].GetComponent<TextMeshProUGUI>().text = ResourceManager.Instance.GetCantidad("Fe").ToString() + " / " + ResourceManager.Instance.GetMaximo("Fe");
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EstructuraSeleccionada = null;
                TextosUI[0].gameObject.SetActive(false);
            }
        }

        if (EstructuraSeleccionada != null && EstructuraSeleccionada.name.Contains("Granja"))
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                Building_Granja tempGranja = EstructuraSeleccionada.GetComponent<Building_Granja>();
                tempGranja.ActivateBuilding();
            }
        }


    }
}

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ConstruirMapa : MonoBehaviour
{
    public int Largo;
    public int Ancho;
    public float Espacio;
    public GameObject Plataforma;
    public Transform HolderPlataforma;

    [Space(10)]
    public List<GameObject> ListaPlataformas;

    public void PoblarMapaMini(){

        // Calcular el centro de la cuadrícula
        float offsetX = (Largo - 1) * Espacio / 2f;
        float offsetZ = (Ancho - 1) * Espacio / 2f;

        for (int i = 0; i < Largo; i++)
        {
            for (int e = 0; e < Ancho; e++)
            {
                // Calcular posición centrada
                float posX = (i * Espacio) - offsetX;
                float posZ = (e * Espacio) - offsetZ;

                GameObject tempPlatform = Instantiate(Plataforma, new Vector3( posX, 0, posZ), quaternion.identity);
                tempPlatform.transform.SetParent(HolderPlataforma);
                tempPlatform.name = "Plataforma_" + i + e;

                if (!ListaPlataformas.Contains(tempPlatform))
                {
                    ListaPlataformas.Add(tempPlatform);
                }
            }
        }
    }

    public void BorrarLista(){
        if (ListaPlataformas.Count <= 0) return;

        for (int i = 0; i < ListaPlataformas.Count; i++)
        {
            DestroyImmediate(ListaPlataformas[i]);
        }

        ListaPlataformas = new List<GameObject>();
    }
}

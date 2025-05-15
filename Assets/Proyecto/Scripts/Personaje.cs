using UnityEngine;

public class Personaje : MonoBehaviour
{
    public Building_Casa casaAsignada;

    public void AsignarCasa(Building_Casa casa)
    {
        casaAsignada = casa;
    }
}

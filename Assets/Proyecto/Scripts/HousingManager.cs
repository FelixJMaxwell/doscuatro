using System.Collections.Generic;
using UnityEngine;

public class HousingManager : MonoBehaviour
{
    //Cada que se construye una nueva casa esta se debe registrar en el housingmanager
    //HousingManager.Instance.RegistrarCasa(this);

    public static HousingManager Instance { get; private set; }

    private List<Building_Casa> casasDisponibles = new List<Building_Casa>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegistrarCasa(Building_Casa casa)
    {
        casasDisponibles.Add(casa);
    }

    public Building_Casa BuscarCasaDisponible()
    {
        foreach (var casa in casasDisponibles)
        {
            if (casa.HayEspacio())
                return casa;
        }
        return null; // No hay casas disponibles
    }

    public void QuitarCasa(Building_Casa casa)
    {
        if (casasDisponibles.Contains(casa))
            casasDisponibles.Remove(casa);
    }
}

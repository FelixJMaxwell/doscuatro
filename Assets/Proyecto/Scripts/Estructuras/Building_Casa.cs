using System.Collections.Generic;

public class Building_Casa : BaseBuilding
{
    public int CapacidadMaxima;
    public List<Personaje> Habitantes = new List<Personaje>();

    public bool HayEspacio()
    {
        return Habitantes.Count < CapacidadMaxima;
    }

    public void AsignarPersona(Personaje persona)
    {
        if (HayEspacio())
        {
            Habitantes.Add(persona);
            persona.AsignarCasa(this);
        }
    }

    public void RemoverPersona(Personaje persona)
    {
        if (Habitantes.Contains(persona))
        {
            Habitantes.Remove(persona);
        }
    }

    public void RegistrarCasa(){
        HousingManager.Instance.RegistrarCasa(this);
    }
}

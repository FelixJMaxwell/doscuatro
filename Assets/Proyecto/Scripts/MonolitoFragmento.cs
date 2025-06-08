using UnityEngine;

public class MonolitoFragmento : MonoBehaviour
{
    public MonolitoBehaviour monolitoDeOrigen;
    private bool _yaRecolectado = false;

    private void OnMouseEnter()
    {
        if (!_yaRecolectado && monolitoDeOrigen != null)
        {
            _yaRecolectado = true;
            
            // Llamar al nuevo método que solo se encarga de la recolección del recurso
            monolitoDeOrigen.ConfirmarRecoleccionDelFragmento();

            Destroy(gameObject);
        }
    }
}

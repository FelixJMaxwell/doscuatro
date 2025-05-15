using UnityEngine;

public class Building_Granja : BaseBuilding
{
    [Header("Cobfiguraciones independientes")]
    public GameManager gameManager;

    private void OnMouseOver() {
        /* if (gameManager.EstructuraConstruyendo == transform.gameObject) return; */

        if (Input.GetMouseButtonDown(0))
        {
            if (!gameManager.EstructuraSeleccionada)
            {
                gameManager.EstructuraSeleccionada = transform.gameObject;
            }
        }
    }
}

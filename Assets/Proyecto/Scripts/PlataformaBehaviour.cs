using UnityEngine;

public class PlataformaBehaviour : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject DadoAsignado;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void OnMouseOver()
    {
        if (gameManager.DadoConstruyendo && !gameManager.DadoConstruyendo.GetComponent<DadoBehaviour>().DadoEstablecido && !DadoAsignado) 
        {
            gameManager.DadoConstruyendo.transform.position = transform.position + new Vector3(0,0.6f,0);
        }

        gameManager.PlataformaActual = gameObject;
    }
}

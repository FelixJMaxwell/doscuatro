using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float Puntos;
    public TextMeshProUGUI Saldo;
    public GameObject Monolito;

    [Space(10)]
    public GameObject DadoConstruyendo;
    public GameObject DadoSeleccionado;
    public GameObject DadoGO;

    public void ActualizarUI(TextMeshProUGUI ElementoUI, string texto){
        ElementoUI.text = texto;
    }

    public void ComprarDado(){
        GameObject tempDado = Instantiate(DadoGO, new Vector3(0,-10,0), quaternion.identity);
        DadoConstruyendo = tempDado;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (DadoSeleccionado)
            {
                DadoSeleccionado = null;
            }
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float Puntos;
    public TextMeshProUGUI Saldo;
    public GameObject Monolito;

    [Space(10)]
    public GameObject DadoActual;

    public void ActualizarUI(TextMeshProUGUI ElementoUI, string texto){
        ElementoUI.text = texto;
    }
}

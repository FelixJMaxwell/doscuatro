using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Collections.Specialized;
using UnityEngine.AI;
using System.Transactions;
using UnityEngine.Rendering;

public class DadoBehaviour : MonoBehaviour
{
    public float ValorActual;
    public int CantCaras;
    public float LimiteTicker;
    public float RadioDeteccion;

    [Header("Configuraciones")]
    public Transform Ligado;
    public float ticker;
    public bool Clicked;
    public GameManager gameManager;
    public bool DadoEstablecido;
    public Vector3 offset;
    
    public LineRenderer lineRenderer;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    private void Update() {
        if (Clicked && DadoEstablecido)
        {
            ticker += Time.deltaTime;
            if (ticker >= LimiteTicker)
            {
                Clicked = false;
                ticker = 0;
            }
        } else {
            Clicked = false;
        }

        if (!DadoEstablecido && gameManager.DadoActual)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, RadioDeteccion);

            float distancia = Vector3.Distance(transform.position, gameManager.Monolito.transform.position);

            if (distancia <= RadioDeteccion)
            {
                lineRenderer.gameObject.SetActive(true);
                lineRenderer.SetPosition(1, new Vector3(transform.position.x, 0.1f, transform.position.z));
                lineRenderer.SetPosition(0, new Vector3(gameManager.Monolito.transform.position.x, 0.1f, gameManager.Monolito.transform.position.z));
            } else {
                lineRenderer.gameObject.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0))
            {
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if (hitColliders[i].name == "Monolito")
                    {
                        Ligado = hitColliders[i].transform;
                        Ligado.GetComponent<MonolitoBehaviour>().ObjetosConectados.Add(transform.gameObject);
                    }
                }

                DadoEstablecido = true;
                gameManager.DadoActual = null;
            }
        }
    }

    /* private Vector3 GetMouseWorldPosition()
    {
        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    } */

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0))
        {
            if (Clicked)
            {
                return;
            }

            EstablecerValorActual();
        }
    }

    public void EstablecerValorActual(){
        Clicked = true;
        ValorActual = UnityEngine.Random.Range(1,CantCaras);
    }

    public void GenerarPunto(){
        gameManager.Puntos += ValorActual;

        gameManager.ActualizarUI(gameManager.Saldo, "$$$: " + gameManager.Puntos.ToString());
    }

    /// <summary>
    /// Callback to draw gizmos only if the object is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, RadioDeteccion);
    }
}

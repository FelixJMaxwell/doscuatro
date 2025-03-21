using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Collections.Specialized;
using UnityEngine.AI;
using System.Transactions;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class DadoBehaviour : MonoBehaviour
{
    public float ValorActual;
    public int CantCaras;
    public float LimiteTicker;
    public float RadioDeteccion;

    [Header("Configuraciones")]
    public Transform Ligado;
    public float ticker;
    public GameManager gameManager;
    public bool DadoEstablecido;
    public bool Selected;
    public Vector3 offset;
    public bool Clicked;
    
    private LineRenderer lineRenderer;
    
    private int IndiceActual = 0;
    public float DelayScroll = 0.2f;
    private float TiempoUltimoScroll;
    public List<GameObject> Elementos;

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

        if (!DadoEstablecido && gameManager.DadoConstruyendo)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DadoEstablecido = true;
                gameManager.DadoConstruyendo = null;
            }
        }

        if (Selected)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, RadioDeteccion);
            
            foreach (Collider item in hitColliders)
            {
                if (item.CompareTag("Monolito"))
                {
                    if (!Elementos.Contains(item.gameObject))
                    {
                        Elementos.Add(item.gameObject);
                    }
                }
                if (item.CompareTag("Dado"))
                {
                    if (!Elementos.Contains(item.gameObject) && item != transform.GetComponent<Collider>())
                    {
                        Elementos.Add(item.gameObject);
                    }
                }
            }

            float ScrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (Time.time - TiempoUltimoScroll > DelayScroll)
            {
                if (ScrollInput > 0)
                {
                    IndiceActual = Elementos.Count - 1;
                }

                ActualizarSeleccion();
            } else if (ScrollInput < 0) // Scroll hacia abajo
            {
                IndiceActual++;
                if (IndiceActual >= Elementos.Count)
                {
                    IndiceActual = 0; // Volver al primero si se pasa del final
                }
                ActualizarSeleccion();
            }

            TiempoUltimoScroll = Time.time;
        }
    }

    void ActualizarSeleccion()
    {
        for (int i = 0; i < Elementos.Count; i++)
        {
            if (i == IndiceActual)
            {
                float distance = Vector3.Distance(transform.position, Elementos[i].transform.position);

                if (distance <= RadioDeteccion)
                {
                    lineRenderer.gameObject.SetActive(true);
                    lineRenderer.SetPosition(1, new Vector3(transform.position.x, 0.1f, transform.position.z));
                    lineRenderer.SetPosition(0, new Vector3(gameManager.Monolito.transform.position.x, 0.1f, gameManager.Monolito.transform.position.z));
                }
            } else {
                lineRenderer.gameObject.SetActive(false);
            }
        }

        Debug.Log("Elemento actual: " + Elementos[IndiceActual].name);
    }

    private void OnMouseOver() {
        if (gameManager.DadoConstruyendo == transform.gameObject) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!gameManager.DadoSeleccionado)
            {
                gameManager.DadoSeleccionado = transform.gameObject;
                Selected = true;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Clicked)
            {
                return;
            }

            EstablecerValorActual();
        }
    }

    public void EstablecerValorActual(){
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

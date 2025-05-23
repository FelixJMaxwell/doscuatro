using UnityEngine;

// Controla el comportamiento de un objeto "Pilar", probablemente usado
// como efecto visual para representar la Fe u otro recurso/estado.
public class PilarBehaviour : MonoBehaviour
{
    // 'Posicion' es la posición objetivo a la que el pilar se moverá cuando NO está 'Bajar'.
    [Tooltip("Posición final a la que el pilar se moverá al emerger.")]
    public Vector3 PosicionObjetivoAlSubir; // Renombrado de 'Posicion' para mayor claridad.

    // 'Bajar' indica si el pilar debe moverse hacia abajo (retraerse/desaparecer).
    [Tooltip("Marcar para que el pilar inicie el movimiento de descenso.")]
    public bool Bajar = false; // Inicialmente, un pilar no debería estar bajando.

    [Tooltip("Velocidad de movimiento del pilar (unidades por segundo).")]
    public float VelocidadMovimiento = 1f;

    // Podría ser útil tener una posición objetivo para cuando baja.
    [Tooltip("Posición Y objetivo cuando el pilar está bajando/oculto.")]
    [SerializeField] private float PosicionYAlBajar = -3f; // Hacerla configurable.

    private Vector3 _posicionObjetivoActual; // Posición hacia la que se mueve actualmente.

    private void Start()
    {
        // Al inicio, si no está bajando, su objetivo es la posición de subida.
        // Si está marcado para bajar desde el inicio (poco común), su objetivo es la posición de bajada.
        if (!Bajar)
        {
            _posicionObjetivoActual = PosicionObjetivoAlSubir;
        }
        else
        {
            _posicionObjetivoActual = new Vector3(transform.position.x, PosicionYAlBajar, transform.position.z);
        }
    }

    private void Update()
    {
        // Determina la posición objetivo actual basada en el estado 'Bajar'.
        if (Bajar)
        {
            // Si debe bajar, el objetivo es la posición Y de bajada, manteniendo X y Z actuales.
            _posicionObjetivoActual = new Vector3(transform.position.x, PosicionYAlBajar, transform.position.z);
        }
        else
        {
            // Si no debe bajar (es decir, debe subir o mantenerse arriba), el objetivo es 'PosicionObjetivoAlSubir'.
            _posicionObjetivoActual = PosicionObjetivoAlSubir;
        }

        // Mueve el pilar hacia su posición objetivo actual.
        transform.position = Vector3.MoveTowards(transform.position, _posicionObjetivoActual, VelocidadMovimiento * Time.deltaTime);

        // Opcional: Si el pilar que está bajando alcanza su destino, podría destruirse o desactivarse.
        if (Bajar && Vector3.Distance(transform.position, _posicionObjetivoActual) < 0.01f)
        {
            // Debug.Log($"Pilar {gameObject.name} ha terminado de bajar y será destruido.");
            // Destroy(gameObject); // O desactivarlo: gameObject.SetActive(false);
            // Si se desactiva, necesitarás una forma de reactivarlo/reutilizarlo (object pooling).
        }
    }

    // Método público para cambiar el estado de 'Bajar' y actualizar el objetivo.
    // Esto permite que otros scripts (como ResourceManager o MonolitoBehaviour) controlen el pilar.
    public void EstablecerEstadoBajada(bool debeBajar)
    {
        if (Bajar == debeBajar) return; // No hacer nada si el estado ya es el solicitado.

        Bajar = debeBajar;
        if (Bajar)
        {
            _posicionObjetivoActual = new Vector3(transform.position.x, PosicionYAlBajar, transform.position.z);
            // Debug.Log($"Pilar {gameObject.name} ahora está bajando.");
        }
        else
        {
            _posicionObjetivoActual = PosicionObjetivoAlSubir; // Debería volver a su posición original de subida.
            // Debug.Log($"Pilar {gameObject.name} ahora está subiendo (o manteniéndose arriba).");
        }
    }
}
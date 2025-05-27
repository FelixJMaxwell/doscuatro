using System.Collections.Generic;
using TMPro; // Necesario para TextMeshProUGUI
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Necesario para EventSystem.current
// using UnityEngine.UI; // No parece usarse directamente (Button es referenciado por GameObject y luego se accede al componente)

public class GameManager : MonoBehaviour
{
    // Instancia estática del Singleton
    public static GameManager Instance { get; private set; }

    // 'Puntos' no se usa actualmente. ¿Es una moneda global o puntuación?
    public float Puntos;
    [Tooltip("Referencia al objeto principal del Monolito en la escena.")]
    public GameObject Monolito; // Podría ser MonolitoBehaviour para acceso directo a sus métodos.

    [Space(10)]
    [Header("Construcción y Selección")]
    [Tooltip("Referencia al GameObject de la estructura que se está colocando actualmente.")]
    public GameObject EstructuraConstruyendo; // El objeto que el jugador está moviendo para colocar.
    [Tooltip("Referencia al GameObject de la estructura actualmente seleccionada por el jugador.")]
    public GameObject EstructuraSeleccionada; // El objeto sobre el que se pueden realizar acciones.
    [Tooltip("Prefab de la estructura que se instanciará al 'ComprarDado' o iniciar construcción.")]
    public GameObject EstructurAConstruir; // Prefab a instanciar.

    [Tooltip("Transform padre que contendrá todas las estructuras instanciadas en el juego.")]
    public Transform EstructurasHolder; // Bueno para organizar la jerarquía.

    [Space(10)]
    [Header("UI General")]
    // Esta lista de GameObjects para textos es poco flexible.
    // Es mejor tener referencias directas a los TextMeshProUGUI con nombres descriptivos
    // o un sistema de UIManager más robusto.
    [Tooltip("Lista de GameObjects que contienen elementos de texto UI. Usar con precaución.")]
    public List<GameObject> TextosUI; // Ejemplo: TextosUI[0] es para panel Monolito, TextosUI[1] para Fe.

    [Header("Paneles UI")]
    [Tooltip("Panel UI para interacciones con NPCs seleccionados.")]
    public GameObject NPCPanel;
    [Tooltip("Panel UI para interacciones con el Monolito seleccionado.")]
    public GameObject MonolitoPanel;
    [Tooltip("Panel UI para el menú de construcción de arquitecturas.")]
    public GameObject ArquitecturaPanel;

    public GameObject CrisolDeAlmasGO;
    public Button GenerarAldeanoBtn;
    public Toggle ToggleLegendario;
    public Button ActualizarCrisolBtn;

    private void Awake()
    {
        // Implementación del Singleton
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Descomentar si necesitas que persista entre escenas
        }
        else
        {
            Debug.LogWarning("Ya existe una instancia de GameManager. Destruyendo este duplicado.");
            Destroy(gameObject);
        }
    }

    // Este método tiene un efecto secundario extraño: resetea la lista 'Estructuras'.
    // Debería solo actualizar el texto.
    public void ActualizarUI(TextMeshProUGUI ElementoUI, string texto)
    {
        if (ElementoUI != null)
        {
            ElementoUI.text = texto;
        }
        else
        {
            Debug.LogError("ElementoUI es nulo. No se puede actualizar el texto.");
        }
    }

    // El nombre 'ComprarDado' es muy específico. Si es genérico para comprar/colocar estructuras,
    // el nombre podría ser más general como 'IniciarColocacionEstructura'.
    public void ComprarDado() // Renombrar a IniciarConstruccionEstructura(GameObject prefabAConstruir)?
    {
        if (EstructurAConstruir == null) {
            Debug.LogError("EstructurAConstruir no está asignada en GameManager. No se puede instanciar.");
            return;
        }

        // Solo permite una estructura en construcción a la vez.
        if (!EstructuraConstruyendo)
        {
            // Instancia la estructura en una posición inicial (ej. cerca del Monolito).
            // 'quaternion.identity' es de Unity.Mathematics, puede ser 'Quaternion.identity' de UnityEngine.
            GameObject tempDado = Instantiate(EstructurAConstruir, Monolito.transform.position + new Vector3(0, 5, 0), Quaternion.identity);
            EstructuraConstruyendo = tempDado; // Asigna la nueva instancia.

            if (EstructurasHolder != null)
            {
                tempDado.transform.SetParent(EstructurasHolder); // Organiza en la jerarquía.
            }
            else
            {
                Debug.LogWarning("EstructurasHolder no asignado en GameManager. La estructura se instanciará en la raíz.");
            }
            // Nombrar el objeto instanciado puede ser útil para debugging.
            tempDado.name = EstructurAConstruir.name + "_" + (EstructurasHolder != null ? EstructurasHolder.childCount : Random.Range(0,1000));
            // Sugerencia: El objeto 'EstructuraConstruyendo' debería tener un script que maneje su lógica de
            // seguimiento del ratón, validación de posición y colocación final.
        }
        else
        {
            Debug.Log("Ya hay una estructura en proceso de construcción.");
        }
    }

    void Update()
    {
        // --- Lógica de Deselección con Escape ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (EstructuraConstruyendo != null) // Si está colocando una estructura, cancela la colocación.
            {
                Destroy(EstructuraConstruyendo);
                EstructuraConstruyendo = null;
                Debug.Log("Construcción cancelada.");
            }
            else if (EstructuraSeleccionada != null) // Si hay una estructura seleccionada, la deselecciona.
            {
                // Cierra paneles asociados a la estructura deseleccionada.
                // Esta lógica de string.Contains es frágil. Es mejor usar componentes o tags.
                if (EstructuraSeleccionada.GetComponent<MonolitoBehaviour>() != null && MonolitoPanel != null)
                {
                    // TextosUI[0] parece ser un título o info del panel Monolito.
                    if (TextosUI.Count > 0 && TextosUI[0] != null) TextosUI[0].gameObject.SetActive(false);
                    MonolitoPanel.SetActive(false);
                }
                else if (EstructuraSeleccionada.GetComponent<PersonajeBehaviour>() != null && NPCPanel != null) // Mejor usar GetComponent.
                {
                    NPCPanel.SetActive(false);
                }
                // Añadir lógica para otros tipos de estructuras y sus paneles si es necesario.

                EstructuraSeleccionada = null; // Deselecciona.
                Debug.Log("Estructura deselecccionada.");
            }
            else if (ArquitecturaPanel != null && ArquitecturaPanel.activeInHierarchy) // Si no hay nada seleccionado y el panel de arquitectura está abierto, ciérralo.
            {
                ArquitecturaPanel.SetActive(false);
            }
            // Considerar cerrar otros paneles modales aquí también.
        }

        // --- Lógica para la estructura que se está construyendo/colocando ---
        if (EstructuraConstruyendo != null)
        {
            // El código de movimiento y validación de posición está comentado.
            // Esta lógica debería estar en un script en el prefab de 'EstructuraConstruyendo'
            // o en un sistema de colocación más dedicado.
            /*
            // Ejemplo de cómo podría funcionar el seguimiento del ratón (requiere un Plane o Raycasting):
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, layerMaskParaElSuelo)) // Necesitas un LayerMask
            {
                EstructuraConstruyendo.transform.position = hitInfo.point; // Ajustar Y si es necesario.
                // Aquí iría la lógica de validación de posición (colisiones, cercanía a otras, etc.)
                // bool puedeConstruirAqui = ValidarPosicionConstruccion(EstructuraConstruyendo.transform.position);
                // Cambiar material para feedback visual (rojo/verde).

                if (Input.GetMouseButtonDown(0)) // Click para colocar.
                {
                    if (puedeConstruirAqui)
                    {
                        FinalizarConstruccion(EstructuraConstruyendo);
                        EstructuraConstruyendo = null; // Limpia la referencia.
                    }
                    else
                    {
                        Debug.Log("No se puede construir aquí."); // Feedback al jugador.
                    }
                }
            }
            */
        }

        // --- Lógica para la estructura seleccionada ---
        if (EstructuraSeleccionada != null)
        {
            // Esta sección maneja interacciones específicas (como presionar 'F' para el Monolito)
            // Sería mejor si la propia estructura (MonolitoBehaviour, Building_Granja) manejara
            // sus interacciones cuando está seleccionada, en lugar de tener esta lógica en GameManager.
            // GameManager podría notificar a la estructura que ha sido seleccionada/deseleccionada.

            MonolitoBehaviour monolitoBehaviour = EstructuraSeleccionada.GetComponent<MonolitoBehaviour>();
            if (monolitoBehaviour != null)
            {
                // Abre el panel del Monolito si no está ya activo.
                if (MonolitoPanel != null && !MonolitoPanel.activeInHierarchy)
                {
                    if (TextosUI.Count > 0 && TextosUI[0] != null) TextosUI[0].gameObject.SetActive(true);
                    MonolitoPanel.SetActive(true);
                    // Actualiza la UI de Fe al seleccionar el Monolito
                    if (TextosUI.Count > 1 && TextosUI[1] != null && TextosUI[1].GetComponent<TextMeshProUGUI>() != null && ResourceManager.Instance != null)
                    {
                        TextosUI[1].GetComponent<TextMeshProUGUI>().text = "Fe: " + ResourceManager.Instance.GetCantidad("Fe").ToString("F0") + " / " + ResourceManager.Instance.GetMaximo("Fe").ToString("F0");
                    }

                }

                if (Input.GetKeyDown(KeyCode.F)) // Interacción específica del Monolito.
                {
                    // Verifica si la cantidad actual de Fe es menor que el máximo.
                    if (ResourceManager.Instance != null && ResourceManager.Instance.GetCantidad("Fe") < ResourceManager.Instance.GetMaximo("Fe"))
                    {
                        monolitoBehaviour.AñadirFeManualmente(1f); // Llama al método del Monolito.
                        for (int i = 0; i < 3; i++) // Genera 3 pilares.
                        {
                            monolitoBehaviour.GenerarPilar();
                        }
                        // La UI de Fe se actualiza a través de ResourceManager.ActualizarRecursoUI("Fe")
                        // llamado desde MonolitoBehaviour.AñadirFeManualmente si éste llama a ResourceManager.Instance.Añadir("Fe", ...)
                        // Si no, hay que actualizarla aquí explícitamente.
                        if (TextosUI.Count > 1 && TextosUI[1] != null && TextosUI[1].GetComponent<TextMeshProUGUI>() != null) {
                             TextosUI[1].GetComponent<TextMeshProUGUI>().text = "Fe: " + ResourceManager.Instance.GetCantidad("Fe").ToString("F0") + " / " + ResourceManager.Instance.GetMaximo("Fe").ToString("F0");
                        }
                    }
                    else
                    {
                        Debug.Log("Máximo de Fe alcanzado o ResourceManager no disponible.");
                    }
                }
            }

            Building_Granja granja = EstructuraSeleccionada.GetComponent<Building_Granja>();
            if (granja != null)
            {
                if (Input.GetKeyDown(KeyCode.G)) // Interacción específica de la Granja.
                {
                    granja.ActivateBuilding(); // Llama al método de la Granja.
                }
            }

            // Lógica para PersonajeBehaviour (NPC)
            PersonajeBehaviour personaje = EstructuraSeleccionada.GetComponent<PersonajeBehaviour>();
            if (personaje != null) {
                if (NPCPanel != null && !NPCPanel.activeInHierarchy) {
                    NPCPanel.SetActive(true);
                    // Aquí podrías popular el NPCPanel con la info del 'personaje'.
                    // Ejemplo: NPCPanel.GetComponent<NPCPanelUI>().MostrarInfo(personaje);
                }
            }
        }
    }

    // Maneja los clics en los botones de la UI del lado derecho.
    public void BotonesDerecha()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            // No hay ningún objeto UI seleccionado (esto puede pasar si el clic no fue en un botón).
            return;
        }
        GameObject UISeleccionada = EventSystem.current.currentSelectedGameObject;

        // Comparar por nombre de GameObject es frágil. Si renombras el botón en el editor, esto se rompe.
        // Es mejor asignar los métodos directamente a los eventos onClick de los botones en el Inspector.
        // Ejemplo: En el Inspector, para ArquitecturaBtn, en su evento OnClick(), arrastras este GameManager
        // y seleccionas una función como ToggleArquitecturaPanel().
        if (UISeleccionada.name == "ArquitecturaBtn" && ArquitecturaPanel != null)
        {
            ArquitecturaPanel.SetActive(!ArquitecturaPanel.activeInHierarchy); // Toggle.
        }
        else if (UISeleccionada.name == "AdministracionBtn")
        {
            Debug.Log("Menu de administración solicitado.");
            // Aquí abrirías el panel de administración.
        }
        // ... y así para los otros botones.
    }

    // Método para comprar/colocar un edificio. Parece incompleto o un stub.
    public void ComprarEdificio() // Podría tomar un ScriptableObject de Edificio como parámetro.
    {
        // Lógica para seleccionar qué edificio comprar (quizás desde el ArquitecturaPanel).
        // Luego, instanciarlo y entrar en modo de colocación (asignar a EstructuraConstruyendo).
        // Ejemplo:
        // BuildingDataSO edificioAComprar = UIManager.Instance.GetEdificioSeleccionadoParaCompra();
        // if (ResourceManager.Instance.TieneSuficiente(edificioAComprar.costoRecurso, edificioAComprar.costoCantidad))
        // {
        //     ResourceManager.Instance.Gastar(edificioAComprar.costoRecurso, edificioAComprar.costoCantidad);
        //     EstructurAConstruir = edificioAComprar.prefab; // Asigna el prefab del SO.
        //     IniciarColocacionEstructura(); // Reutiliza la lógica de ComprarDado.
        // }

        GameObject tempEdificioAConstruir = GameObject.Instantiate(CrisolDeAlmasGO, new Vector3(-5,0,0), Quaternion.identity);
        tempEdificioAConstruir.GetComponent<Building_Personajes>().ActivateBuilding();
        tempEdificioAConstruir.GetComponent<Building_Personajes>().generarPersonajeButton = GenerarAldeanoBtn;
        tempEdificioAConstruir.GetComponent<Building_Personajes>().generarLegendarioToggle = ToggleLegendario;
        tempEdificioAConstruir.GetComponent<Building_Personajes>().mejorarEdificioButton = ActualizarCrisolBtn;
    }

    // Sugerencia: Un método para finalizar la construcción.
    /*
    public void FinalizarConstruccion(GameObject edificioConstruido)
    {
        // Lógica para "fijar" el edificio.
        // Activar sus componentes de producción, añadirlo a listas de gestión, etc.
        BaseBuilding buildingScript = edificioConstruido.GetComponent<BaseBuilding>();
        if (buildingScript != null)
        {
            buildingScript.ActivateBuilding(); // Activa el edificio.
            if (!Estructuras.Contains(edificioConstruido.transform)) // Asegúrate de que 'Estructuras' exista o se inicialice.
            {
                Estructuras.Add(edificioConstruido.transform);
            }
        }
    }
    */
}
using UnityEngine;

// Representa una Granja, un tipo específico de BaseBuilding.
public class Building_Granja : BaseBuilding
{
    // 'gameManager' se usaba para la selección. Si BaseBuilding o un sistema global
    // maneja la selección, esta referencia podría no ser necesaria aquí.
    // public GameManager gameManager; // Considerar obtenerlo vía Singleton o pasarlo si es necesario.
    private GameManager _gameManagerInstance; // Caché local si se usa frecuentemente.

    protected override void Start()
    {
        base.Start(); // Importante llamar al Start de la clase base.

        // Configuración específica de la Granja.
        buildingName = "Granja de Cultivos"; // Nombre por defecto.
        TipoEstructura = BuildingType.Estructura.Granja; // Asigna el tipo correcto.
        producesResources = true; // Las granjas suelen producir recursos.
        // 'RecursoProducir' y 'TasaDeProduccion' deberían asignarse en el Inspector
        // o aquí si esta Granja siempre produce lo mismo.
        // Ejemplo:
        // RecursoProducir = ResourceManager.Instance.GetRecurSOByName("Comida"); // Necesitarías un método para esto.
        // TasaDeProduccion = 0.5f; // Produce 0.5 Comida por segundo.

        // Obtener referencia al GameManager si es necesario para alguna lógica específica de la granja.
        // _gameManagerInstance = GameManager.Instance; // Asumiendo que GameManager es Singleton.
    }

    // La lógica de OnMouseOver para selección es muy genérica.
    // Podría estar en BaseBuilding si todos los edificios se seleccionan igual,
    // o manejada por un sistema de input global.
    private void OnMouseOver()
    {
        // Si se requiere referencia a GameManager (ej. si es para ESTRUCTURA_SELECCIONADA global)
        // es mejor obtenerla una vez en Start o usar un Singleton.
        GameManager gm = GameManager.Instance; // Asumiendo Singleton.
                                                // Si GameManager no es Singleton, necesitarías asignarlo.

        if (gm == null) return; // No hacer nada si no hay GameManager.

        // El comentario original /* if (gameManager.EstructuraConstruyendo == transform.gameObject) return; */
        // evitaría la selección si este objeto es el que se está construyendo. Buena idea.
        if (gm.EstructuraConstruyendo == this.gameObject) return;

        if (Input.GetMouseButtonDown(0)) // Clic izquierdo.
        {
            if (gm.EstructuraSeleccionada == null) // Si no hay nada seleccionado aún.
            {
                gm.EstructuraSeleccionada = this.gameObject; // Selecciona esta granja.
                Debug.Log($"Edificio '{buildingName}' seleccionado.");
                // Aquí podrías abrir un panel de UI específico para la granja.
                // Ejemplo: UIManager.Instance.AbrirPanelGranja(this);
            }
            // Si ya estaba seleccionada esta misma granja, un segundo clic podría deseleccionarla
            // o abrir un menú más detallado.
            // else if (gm.EstructuraSeleccionada == this.gameObject) { /* Lógica de segundo clic */ }
        }
    }

    // Override ActivateBuilding para añadir lógica específica de la granja al activarse.
    public override void ActivateBuilding()
    {
        base.ActivateBuilding(); // Llama a la lógica base (pone isActive = true).
        Debug.Log($"La granja '{buildingName}' ha comenzado su producción.");
        // Aquí podrías iniciar animaciones, asignar trabajadores si tuvieras ese sistema, etc.
    }

    // Override ProduceResources si la granja tiene una forma particular de producir.
    // protected override void ProduceResources(float cantidadPorLote)
    // {
    //     base.ProduceResources(cantidadPorLote); // Llama a la lógica base.
    //     // Lógica adicional: ¿Quizás la granja necesita agua para producir comida?
    //     // if (ResourceManager.Instance.TieneSuficiente("Agua", 0.1f * cantidadPorLote)) {
    //     //    ResourceManager.Instance.Gastar("Agua", 0.1f * cantidadPorLote);
    //     //    ResourceManager.Instance.Añadir(RecursoProducir.Nombre, cantidadPorLote);
    //     // } else { // No hay suficiente agua, la producción falla o se reduce. }
    // }
}
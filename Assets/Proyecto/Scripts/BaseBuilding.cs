using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour
{
    [Header("General Info")]
    public string buildingName;
    public Sprite buildingIcon;
    public int constructionCost;
    public BuildingType TipoEstructura;

    [Header("Production")]
    public bool producesResources;
    [SerializeField] protected RecurSO RecursoProducir;
    [SerializeField] protected float TasaDeProduccion = 1f; // unidades por segundo

    protected bool isActive = false;
    private float productionTimer = 0f;

    protected virtual void Start()
    {
        InitializeBuilding();
    }

    protected virtual void Update()
    {
        if (isActive && producesResources && RecursoProducir != null && ResourceManager.Instance != null)
        {
            productionTimer += Time.deltaTime;
            if (productionTimer >= 1f / TasaDeProduccion) // Produce cada cierto intervalo basado en la tasa
            {
                ProduceResources();
                productionTimer = 0f;
            }
        }
    }

    protected virtual void InitializeBuilding()
    {
        // Inicializar componentes o estados específicos
    }

    protected virtual void ProduceResources()
    {
        if (RecursoProducir != null && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.Añadir(RecursoProducir.Nombre, TasaDeProduccion * Time.deltaTime);
        }
    }

    public virtual void ActivateBuilding()
    {
        isActive = true;
    }

    public virtual void DeactivateBuilding()
    {
        isActive = false;
    }
}
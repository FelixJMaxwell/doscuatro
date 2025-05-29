// Archivo: BuildingManager.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    #region Singleton & Core References
    // =================================================================================================================
    // SINGLETON Y REFERENCIAS PRINCIPALES
    // =================================================================================================================
    public static BuildingManager Instance { get; private set; }
    private Camera _mainCamera;
    #endregion

    #region Inspector Configuration
    // =================================================================================================================
    // CONFIGURACIÓN ASIGNABLE DESDE EL INSPECTOR
    // =================================================================================================================
    [Header("Capas para Raycasting y Colisiones")]
    [SerializeField] private LayerMask placementLayerMask; // Capa del suelo donde se puede construir
    [SerializeField] private LayerMask obstructionLayerMask; // Capa de objetos que obstruyen

    [Header("Materiales del Fantasma")]
    [SerializeField] private Material ghostValidMaterial;   // Material para cuando la posición es válida
    [SerializeField] private Material ghostInvalidMaterial; // Material para cuando la posición es inválida
    #endregion

    #region State Variables
    // =================================================================================================================
    // VARIABLES DE ESTADO INTERNO DEL MANAGER
    // =================================================================================================================
    private GameObject _currentBuildingGhost;
    private BaseBuilding _buildingToPlacePrefabData;
    private bool _isInPlacementMode = false;
    private bool _canPlaceCurrentGhost = false;
    private List<Renderer> _ghostRenderers = new List<Renderer>();
    public bool IsInPlacementMode => _isInPlacementMode; // Esto es una propiedad de solo lectura (getter)
    #endregion

    #region Unity Lifecycle Methods
    // =================================================================================================================
    // MÉTODOS DEL CICLO DE VIDA DE UNITY (AWAKE, START, UPDATE)
    // =================================================================================================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("BuildingManager: No se encontró la cámara principal (Camera.main).");
            enabled = false; // Desactivar el manager si no hay cámara.
        }
    }

    private void Update()
    {
        if (!_isInPlacementMode || _currentBuildingGhost == null)
        {
            return;
        }
        ProcessPlacementModeInput();
        HandleGhostMovementAndValidation();
    }
    #endregion

    #region Public API - Mode Control
    // =================================================================================================================
    // API PÚBLICA PARA INICIAR Y CANCELAR EL MODO DE COLOCACIÓN
    // =================================================================================================================

    /// <summary>
    /// Inicia el modo de colocación para un tipo de edificio específico.
    /// Llamado desde la UI u otro sistema de juego.
    /// </summary>
    /// <param name="buildingPrefab">El prefab del edificio (que contiene BaseBuilding) a colocar.</param>
    public void StartPlacementMode(BaseBuilding buildingPrefab)
    {
        if (buildingPrefab == null)
        {
            Debug.LogError("BuildingManager: Se intentó iniciar la colocación con un prefab nulo.");
            return;
        }
        if (_isInPlacementMode) // Si ya se está colocando algo, cancelarlo primero
        {
            CancelPlacementMode();
        }

        _buildingToPlacePrefabData = buildingPrefab;
        _currentBuildingGhost = Instantiate(buildingPrefab.gameObject); // Instanciar el prefab como fantasma
        _currentBuildingGhost.name = buildingPrefab.name + "_FantasmaEnColocacion";

        SetGhostAppearance(_currentBuildingGhost, true); // Configurar el fantasma (scripts, colliders, material inicial)

        _isInPlacementMode = true;
        // Debug.Log($"BuildingManager: Iniciando modo colocación para '{_buildingToPlacePrefabData.buildingName}'.");
    }

    /// <summary>
    /// Cancela el modo de colocación actual.
    /// </summary>
    public void CancelPlacementMode()
    {
        if (_currentBuildingGhost != null)
        {
            Destroy(_currentBuildingGhost);
        }
        _currentBuildingGhost = null;
        _buildingToPlacePrefabData = null;
        _isInPlacementMode = false;
        _ghostRenderers.Clear();
        // Debug.Log("BuildingManager: Modo colocación cancelado.");
    }
    #endregion

    #region Placement Mode Logic (Input & Ghost Update)
    // =================================================================================================================
    // LÓGICA DEL MODO DE COLOCACIÓN (MANEJO DE INPUT Y ACTUALIZACIÓN DEL FANTASMA)
    // =================================================================================================================

    private void ProcessPlacementModeInput()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            AttemptToPlaceBuilding();
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacementMode();
        }
    }

    private void HandleGhostMovementAndValidation()
    {
        if (_mainCamera == null || _currentBuildingGhost == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 200f, placementLayerMask))
        {
            _currentBuildingGhost.transform.position = hitInfo.point; // Ajustar Y si el pivote no está en la base

            _canPlaceCurrentGhost = CheckPlacementValidity(_currentBuildingGhost);
            UpdateGhostMaterial(_canPlaceCurrentGhost);
        }
        else
        {
            _canPlaceCurrentGhost = false; // Cursor no está sobre superficie válida
            UpdateGhostMaterial(false);
        }
    }
    #endregion

    #region Placement Attempt, Validation & Finalization
    // =================================================================================================================
    // INTENTO DE COLOCACIÓN, VALIDACIÓN DE RECURSOS/POSICIÓN Y FINALIZACIÓN
    // =================================================================================================================

    private void AttemptToPlaceBuilding() // Renombrado de TryPlaceBuilding
    {
        if (!_canPlaceCurrentGhost)
        {
            Debug.Log("BuildingManager: Posición de construcción inválida (según _canPlaceCurrentGhost).");
            // Sonido de error / Feedback visual
            return;
        }

        if (ResourceManager.Instance == null) {
             Debug.LogError("BuildingManager: ResourceManager no disponible para verificar/gastar recursos.");
             return;
        }
        if (_buildingToPlacePrefabData == null) {
            Debug.LogError("BuildingManager: _buildingToPlacePrefabData es nulo. No se puede construir.");
            CancelPlacementMode();
            return;
        }

        if (_buildingToPlacePrefabData.CanBeBuilt())
        {
            _buildingToPlacePrefabData.SpendConstructionResources();
            // Debug.Log($"BuildingManager: Recursos gastados para '{_buildingToPlacePrefabData.buildingName}'.");
            FinalizeBuildingConstruction(); // Renombrado de FinalizeBuildingPlacement
        }
        else
        {
            Debug.Log($"BuildingManager: Recursos insuficientes para '{_buildingToPlacePrefabData.buildingName}'.");
            // Feedback al jugador
        }
    }

    private bool CheckPlacementValidity(GameObject ghostObject) // Renombrado de IsValidPlacementPosition
    {
        // Esta es una validación simple. Puede expandirse mucho.
        Collider ghostCollider = ghostObject.GetComponent<Collider>(); // Asumimos que el fantasma tiene UN collider principal para sus bounds.
        if (ghostCollider == null)
        {
            Bounds bounds = GetCombinedBoundsOfRenderers(ghostObject);
            if (bounds.size == Vector3.zero) return true; // No se puede validar sin bounds/collider, asumir válido por ahora.
            return !Physics.CheckBox(bounds.center, bounds.extents, ghostObject.transform.rotation, obstructionLayerMask);
        }

        if (ghostCollider is BoxCollider box) {
            Vector3 center = ghostObject.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, ghostObject.transform.lossyScale);
            return !Physics.CheckBox(center, halfExtents, ghostObject.transform.rotation, obstructionLayerMask);
        }
        else if (ghostCollider is SphereCollider sphere) {
            Vector3 center = ghostObject.transform.TransformPoint(sphere.center);
            float radius = sphere.radius * GetMaxScaleDimension(ghostObject.transform.lossyScale);
            return !Physics.CheckSphere(center, radius, obstructionLayerMask);
        }
        // Añadir más tipos de collider si es necesario (CapsuleCollider, MeshCollider (más complejo))
        Debug.LogWarning($"BuildingManager: Validación para tipo de collider '{ghostCollider.GetType()}' no implementada completamente en '{ghostObject.name}'.");
        return true; // Fallback si el tipo de collider no está manejado explícitamente.
    }

    private void FinalizeBuildingConstruction()
    {
        GameObject finalBuilding = _currentBuildingGhost;
        finalBuilding.name = _buildingToPlacePrefabData.name; // Nombre limpio

        SetGhostAppearance(finalBuilding, false); // Reactivar scripts, colliders, material original (si se manejara)

        BaseBuilding buildingScript = finalBuilding.GetComponent<BaseBuilding>();
        if (buildingScript != null)
        {
            buildingScript.InitializePostConstruction();
            buildingScript.ActivateBuilding();

            if (GameManager.Instance != null && GameManager.Instance.TodasLasEstructurasActivas != null)
            {
                GameManager.Instance.TodasLasEstructurasActivas.Add(buildingScript);
            }
        }

        // Debug.Log($"BuildingManager: Edificio '{buildingScript?.buildingName ?? finalBuilding.name}' construido en {finalBuilding.transform.position}.");

        // Limpiar estado actual para permitir nueva colocación o salir del modo
        _currentBuildingGhost = null; // Importante: el objeto 'finalBuilding' persiste en la escena.
        BaseBuilding prefabOriginal = _buildingToPlacePrefabData; // Guardar referencia por si se quiere construir otro igual
        _buildingToPlacePrefabData = null;
        _isInPlacementMode = false; // Salir del modo colocación por defecto
        _ghostRenderers.Clear();

        // Opcional: Si el jugador mantiene presionada una tecla (ej. Shift), iniciar colocación del mismo edificio.
        // if (Input.GetKey(KeyCode.LeftShift) && prefabOriginal != null)
        // {
        //     StartPlacementMode(prefabOriginal);
        // }
    }
    #endregion

    #region Ghost Management Utilities
    // =================================================================================================================
    // UTILIDADES PARA MANEJAR LA APARIENCIA Y ESTADO DEL FANTASMA DEL EDIFICIO
    // =================================================================================================================

    private void SetGhostAppearance(GameObject ghost, bool isGhostModeActive)
    {
        BaseBuilding buildingScript = ghost.GetComponent<BaseBuilding>();
        if (buildingScript != null)
        {
            buildingScript.enabled = !isGhostModeActive; // Activar/desactivar lógica principal
        }
        // Aquí podrías desactivar otros scripts específicos del edificio si es necesario

        Collider[] colliders = ghost.GetComponentsInChildren<Collider>(true); // Incluir inactivos
        foreach (Collider col in colliders)
        {
            col.isTrigger = isGhostModeActive; // Hacerlos trigger en modo fantasma
        }

        _ghostRenderers.Clear();
        if (isGhostModeActive)
        {
            ghost.GetComponentsInChildren<Renderer>(true, _ghostRenderers); // Obtener renderers
            UpdateGhostMaterial(true); // Asumir inicialmente válido
        }
        else
        {
            // Aquí se restauraría el material original si se guardó.
            // Por ahora, simplemente dejamos de actualizarlo.
            // Si ghostValidMaterial/ghostInvalidMaterial son instancias, no habrá problema.
            // Si son sharedMaterials, necesitas restaurar el original.
        }
    }

    private void UpdateGhostMaterial(bool isValid)
    {
        Material materialToSet = isValid ? ghostValidMaterial : ghostInvalidMaterial;
        if (materialToSet == null || _ghostRenderers.Count == 0)
        {
            // Si no hay materiales de fantasma o renderers, no hacer nada.
            // Esto evita que el edificio se vuelva invisible si los materiales no están asignados.
            return;
        }

        foreach (Renderer rend in _ghostRenderers)
        {
            if (rend != null) rend.material = materialToSet; // Asigna el material (crea instancia)
        }
    }

    private Bounds GetCombinedBoundsOfRenderers(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);
        Bounds bounds = renderers[0].bounds;
        for(int i = 1; i < renderers.Length; i++)
        {
            if(renderers[i].enabled) bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private float GetMaxScaleDimension(Vector3 scale) {
        return Mathf.Max(scale.x, scale.y, scale.z);
    }
    #endregion
}
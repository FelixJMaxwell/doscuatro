// Archivo: MonolitoParticles.cs
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Archivo: MonolitoParticleConfig.cs (o dentro de MonolitoParticles.cs)
// Asegúrate que esté así o similar:
[System.Serializable]
public class MonolitoParticleConfig
{
    public GameObject particleObject;

    [Header("Oscilación X")]
    public float amplitudOscilacionXIndividual = 0f;
    public float velocidadOscilacionIndividual = 0f;

    [Header("Teletransporte (Opcional)")]
     public bool puedeTeleportar = false;
    public float teleportIntervaloMin = 5f;
    public float teleportIntervaloMax = 15f;

    // Datos de Runtime
    [System.NonSerialized] public Vector3 posicionLocalOriginalCompleta; // NUEVO: Para guardar la X, Y, Z local original
    [System.NonSerialized] public float posicionXBaseActualParaOscilacion;
    [System.NonSerialized] public float velocidadOscilacionReal;
    [System.NonSerialized] public float tiempoAcumuladoOscilacion;
    [System.NonSerialized] public float proximoTeleportTimer;

    // Constructor por defecto (opcional, pero útil para claridad)
    public MonolitoParticleConfig() {
        puedeTeleportar = false;
        teleportIntervaloMin = 5f;
        teleportIntervaloMax = 15f;
        amplitudOscilacionXIndividual = 0f; // Usará global
        velocidadOscilacionIndividual = 0f; // Usará global
    }

    public void InicializarRuntimeData(float velMinGlobal, float velMaxGlobal, float ampXGlobal)
    {
        if (particleObject != null)
        {
            // Guardar la posición local original completa (X, Y, Z)
            posicionLocalOriginalCompleta = particleObject.transform.localPosition;
            posicionXBaseActualParaOscilacion = particleObject.transform.localPosition.x;
        }
        velocidadOscilacionReal = (velocidadOscilacionIndividual > 0.001f) ? velocidadOscilacionIndividual : Random.Range(velMinGlobal, velMaxGlobal);
        tiempoAcumuladoOscilacion = Random.Range(0f, 2f * Mathf.PI);
        ResetTeleportTimer();
    }

    public void ResetTeleportTimer() {
        if (puedeTeleportar) {
            proximoTeleportTimer = Random.Range(teleportIntervaloMin, teleportIntervaloMax);
        }
    }
}

public class MonolitoParticles : MonoBehaviour
{
    [Header("Configuración Global de Partículas")]
    public List<MonolitoParticleConfig> configuracionParticulas;

    [Tooltip("Amplitud de movimiento X global (si la individual es 0).")]
    public float amplitudMovimientoXGlobal = 1.0f;
    [Tooltip("Velocidad mínima global de oscilación (si la individual es 0).")]
    public float velocidadMinimaGlobal = 0.5f;
    [Tooltip("Velocidad máxima global de oscilación (si la individual es 0).")]
    public float velocidadMaximaGlobal = 2.0f;

    [Header("Configuración de Teletransporte")]
    [Tooltip("Collider que define la zona donde las partículas especiales pueden teletransportarse.")]
    public Collider zonaDeTeleport;

    [Header("Configuración de Movimientos Especiales")]
    [Tooltip("Velocidad a la que las partículas se moverán hacia el punto de convergencia o de retorno a su origen.")]
    public float velocidadMovimientosEspeciales = 5.0f;
    [Tooltip("Distancia máxima en el eje X local para la dispersión aleatoria antes de que las partículas regresen a su origen.")]
    public float dispersionXAntesDeRetorno = 1.5f; // NUEVA VARIABLE

    private bool _modoEspecialActivo = false;
    private Coroutine _corutinaMovimientoEspecial = null;


    void Start()
    {
        if (configuracionParticulas == null || configuracionParticulas.Count == 0)
        {
            Debug.LogWarning("MonolitoParticles: No se han asignado partículas en 'configuracionParticulas'. El script se desactivará.");
            enabled = false;
            return;
        }

        if (zonaDeTeleport == null)
        {
            bool algunaPuedeTeleportar = false;
            foreach (MonolitoParticleConfig config in configuracionParticulas)
            {
                if (config.particleObject != null && config.puedeTeleportar)
                {
                    algunaPuedeTeleportar = true;
                    break;
                }
            }
            if (algunaPuedeTeleportar)
            {
                Debug.LogError("MonolitoParticles: Hay partículas configuradas para teletransportar pero 'zonaDeTeleport' no está asignada.");
            }
        }

        foreach (MonolitoParticleConfig config in configuracionParticulas)
        {
            if (config.particleObject != null)
            {
                config.InicializarRuntimeData(velocidadMinimaGlobal, velocidadMaximaGlobal, amplitudMovimientoXGlobal);
            }
            else
            {
                Debug.LogWarning("MonolitoParticles: Un GameObject en 'configuracionParticulas' es nulo y será ignorado.");
            }
        }
    }

    void Update()
    {
        if (_modoEspecialActivo || configuracionParticulas == null || configuracionParticulas.Count == 0)
        {
            return;
        }

        foreach (MonolitoParticleConfig config in configuracionParticulas)
        {
            if (config.particleObject == null || !config.particleObject.activeInHierarchy) continue;

            if (config.puedeTeleportar && zonaDeTeleport != null)
            {
                config.proximoTeleportTimer -= Time.deltaTime;
                if (config.proximoTeleportTimer <= 0)
                {
                    TeleportParticle(config);
                    config.ResetTeleportTimer();
                }
            }

            config.tiempoAcumuladoOscilacion += Time.deltaTime * config.velocidadOscilacionReal;
            float amplitudActual = (config.amplitudOscilacionXIndividual > 0.001f) ? config.amplitudOscilacionXIndividual : amplitudMovimientoXGlobal;
            float desplazamientoX = Mathf.Sin(config.tiempoAcumuladoOscilacion) * amplitudActual;
            float nuevaPosXLocal = config.posicionXBaseActualParaOscilacion + desplazamientoX;

            config.particleObject.transform.localPosition = new Vector3(
                nuevaPosXLocal,
                config.particleObject.transform.localPosition.y,
                config.particleObject.transform.localPosition.z
            );
        }
    }

    void TeleportParticle(MonolitoParticleConfig particleConfig)
    {
        if (particleConfig.particleObject == null || zonaDeTeleport == null) return;
        Vector3 nuevoPuntoDestinoMundial = GetRandomPointInColliderBounds(zonaDeTeleport);
        particleConfig.particleObject.transform.position = nuevoPuntoDestinoMundial;
        particleConfig.posicionXBaseActualParaOscilacion = particleConfig.particleObject.transform.localPosition.x;
    }

    private Vector3 GetRandomPointInColliderBounds(Collider coll)
    {
        Bounds bounds = coll.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public Coroutine CongelarYConvergerParticulas(Transform puntoDeConvergencia)
    {
        if (puntoDeConvergencia == null) { Debug.LogError("PuntoDeConvergencia es nulo."); return null; }
        if (!enabled || configuracionParticulas == null || configuracionParticulas.Count == 0) { return null; }
        if (_corutinaMovimientoEspecial != null) StopCoroutine(_corutinaMovimientoEspecial);
        _corutinaMovimientoEspecial = StartCoroutine(ProcesoCongelarYConverger(puntoDeConvergencia));
        return _corutinaMovimientoEspecial;
    }

    private IEnumerator ProcesoCongelarYConverger(Transform puntoDeConvergencia)
    {
        _modoEspecialActivo = true;
        // Debug.Log($"MonolitoParticles: ({Time.time:F2}s) Iniciando congelación...");
        yield return new WaitForSeconds(1.0f);
        // Debug.Log($"MonolitoParticles: ({Time.time:F2}s) Fin congelación. Moviendo gradualmente a: {puntoDeConvergencia.position}");

        if (configuracionParticulas != null && configuracionParticulas.Count > 0)
        {
            bool todasHanLlegado = false;
            int particulasActivasContador = 0;
            foreach(MonolitoParticleConfig config in configuracionParticulas) {
                if (config.particleObject != null && config.particleObject.activeInHierarchy) particulasActivasContador++;
            }
            if (particulasActivasContador == 0) todasHanLlegado = true;

            while (!todasHanLlegado)
            {
                todasHanLlegado = true;
                int particulasEnDestinoEsteFrame = 0;
                foreach (MonolitoParticleConfig config in configuracionParticulas)
                {
                    if (config.particleObject != null && config.particleObject.activeInHierarchy)
                    {
                        Transform particulaTransform = config.particleObject.transform;
                        if (Vector3.Distance(particulaTransform.position, puntoDeConvergencia.position) > 0.01f)
                        {
                            todasHanLlegado = false;
                            particulaTransform.position = Vector3.MoveTowards(
                                particulaTransform.position,
                                puntoDeConvergencia.position,
                                velocidadMovimientosEspeciales * Time.deltaTime
                            );
                        } else {
                            particulasEnDestinoEsteFrame++;
                        }
                    } else {
                         particulasEnDestinoEsteFrame++;
                    }
                }
                if(particulasEnDestinoEsteFrame >= particulasActivasContador){ // Si todas las que estaban activas llegaron (o eran nulas/inactivas)
                    todasHanLlegado = true;
                }
                yield return null;
            }

            foreach (MonolitoParticleConfig config in configuracionParticulas)
            {
                if (config.particleObject != null && config.particleObject.activeInHierarchy)
                {
                    config.particleObject.transform.position = puntoDeConvergencia.position;
                    config.posicionXBaseActualParaOscilacion = config.particleObject.transform.localPosition.x;
                    config.tiempoAcumuladoOscilacion = 0f;
                    config.ResetTeleportTimer();
                }
            }
        }
        yield return null;
        _modoEspecialActivo = false;
        _corutinaMovimientoEspecial = null;
    }

    public Coroutine DevolverParticulasASuOrigen()
    {
        if (!enabled || configuracionParticulas == null || configuracionParticulas.Count == 0) return null;
        if (_corutinaMovimientoEspecial != null) StopCoroutine(_corutinaMovimientoEspecial);
        _corutinaMovimientoEspecial = StartCoroutine(ProcesoDevolverAOrigenGradual());
        return _corutinaMovimientoEspecial;
    }

    // ANTERIORMENTE ProcesoDevolverAOrigen, AHORA ProcesoDevolverAOrigenGradual
    // En MonolitoParticles.cs

    private IEnumerator ProcesoDevolverAOrigenGradual()
    {
        _modoEspecialActivo = true;
        int coroutineInstanceID = Random.Range(1000, 9999);
        Debug.Log($"MonolitoParticles ({Time.frameCount}) [{coroutineInstanceID}]: INICIANDO ProcesoDevolverAOrigenGradual. Velocidad: {velocidadMovimientosEspeciales}");
        yield return null; // Frame para aplicar pausa de Update

        if (configuracionParticulas == null || configuracionParticulas.Count == 0)
        {
            // ... (lógica de salida si no hay partículas) ...
            _modoEspecialActivo = false; _corutinaMovimientoEspecial = null; yield break;
        }

        // --- NUEVO: Fase de Dispersión X Aleatoria ---
        Debug.Log($"MonolitoParticles ({Time.frameCount}) [{coroutineInstanceID}]: Aplicando dispersión X aleatoria antes del retorno.");
        foreach (MonolitoParticleConfig config in configuracionParticulas)
        {
            if (config.particleObject != null && config.particleObject.activeInHierarchy)
            {
                Transform particulaTransform = config.particleObject.transform;
                float offsetXAleatorio = Random.Range(-dispersionXAntesDeRetorno, dispersionXAntesDeRetorno);
                float nuevaXLocalConDispersion = particulaTransform.localPosition.x + offsetXAleatorio;

                particulaTransform.localPosition = new Vector3(
                    nuevaXLocalConDispersion,
                    particulaTransform.localPosition.y,
                    particulaTransform.localPosition.z
                );
                // No actualizamos posicionXBaseActualParaOscilacion aquí, ya que el objetivo final
                // es la posicionLocalOriginalCompleta, y la base de oscilación se reseteará allí.
            }
        }
        // Esperar un frame para que esta dispersión sea visible antes de que comience el movimiento de retorno.
        yield return null;
        // --- FIN NUEVA FASE ---


        int particulasActivasContador = 0;
        // ... (contar particulasActivasContador como antes) ...
        foreach (MonolitoParticleConfig config in configuracionParticulas) {
            if (config.particleObject != null && config.particleObject.activeInHierarchy) {
                particulasActivasContador++;
            }
        }

        if (particulasActivasContador == 0) {
            // ... (lógica de salida si no hay partículas activas) ...
             _modoEspecialActivo = false; _corutinaMovimientoEspecial = null; yield break;
        }
        // Debug.Log($"MonolitoParticles [{coroutineInstanceID}]: {particulasActivasContador} partículas activas comenzarán a moverse a su origen desde sus nuevas posiciones dispersas.");


        int framesDeMovimiento = 0;
        int particulasQueAunNecesitanMoverse = particulasActivasContador;

        while (particulasQueAunNecesitanMoverse > 0)
        {
            framesDeMovimiento++;
            particulasQueAunNecesitanMoverse = 0;

            float step = velocidadMovimientosEspeciales * Time.deltaTime;

            for (int i = 0; i < configuracionParticulas.Count; i++)
            {
                MonolitoParticleConfig config = configuracionParticulas[i];
                if (config.particleObject != null && config.particleObject.activeInHierarchy)
                {
                    Transform particulaTransform = config.particleObject.transform;
                    if (Vector3.Distance(particulaTransform.localPosition, config.posicionLocalOriginalCompleta) > 0.01f)
                    {
                        particulaTransform.localPosition = Vector3.MoveTowards(
                            particulaTransform.localPosition,
                            config.posicionLocalOriginalCompleta,
                            step
                        );
                        if (Vector3.Distance(particulaTransform.localPosition, config.posicionLocalOriginalCompleta) > 0.01f)
                        {
                            particulasQueAunNecesitanMoverse++;
                        }
                        else
                        {
                            particulaTransform.localPosition = config.posicionLocalOriginalCompleta; // Snap final
                        }
                    }
                }
            }
            if (particulasQueAunNecesitanMoverse == 0) break;
            yield return null;
        }

        Debug.Log($"MonolitoParticles ({Time.frameCount}) [{coroutineInstanceID}]: Movimiento gradual a origen completado después de {framesDeMovimiento} frames.");

        // Asegurar posiciones finales y actualizar bases de oscilación
        foreach (MonolitoParticleConfig config in configuracionParticulas)
        {
            if (config.particleObject != null && config.particleObject.activeInHierarchy)
            {
                config.particleObject.transform.localPosition = config.posicionLocalOriginalCompleta;
                config.posicionXBaseActualParaOscilacion = config.posicionLocalOriginalCompleta.x;
                config.tiempoAcumuladoOscilacion = Random.Range(0f, 2f * Mathf.PI);
                config.ResetTeleportTimer();
            }
        }

        yield return null;

        _modoEspecialActivo = false;
        _corutinaMovimientoEspecial = null;
        // Debug.Log($"MonolitoParticles ({Time.frameCount}) [{coroutineInstanceID}]: Modo especial (retorno gradual) desactivado.");
    }

    // Métodos opcionales para añadir/quitar partículas en runtime (si los necesitas)
    public void AddConfiguredParticle(MonolitoParticleConfig newConfig)
    {
        if (newConfig != null && newConfig.particleObject != null && configuracionParticulas != null && !configuracionParticulas.Contains(newConfig))
        {
            newConfig.InicializarRuntimeData(velocidadMinimaGlobal, velocidadMaximaGlobal, amplitudMovimientoXGlobal);
            configuracionParticulas.Add(newConfig);
        }
    }

    public bool RemoveParticle(GameObject particleToRemove)
    {
        // ... (implementación como antes) ...
        if (particleToRemove != null && configuracionParticulas != null) {
            for (int i = 0; i < configuracionParticulas.Count; i++) {
                if (configuracionParticulas[i].particleObject == particleToRemove) {
                    configuracionParticulas.RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }
}
using UnityEngine;

public class PilarBehaviour : MonoBehaviour
{
    public Vector3 Posicion;

    

    private void Update() {
        transform.position = Vector3.MoveTowards(transform.position, Posicion, 1 * Time.deltaTime);
    }
}
using UnityEngine;

public class PilarBehaviour : MonoBehaviour
{
    public Vector3 Posicion;
    public bool Bajar;

    private void Update()
    {
        if (!Bajar)
        {
            transform.position = Vector3.MoveTowards(transform.position, Posicion, 1 * Time.deltaTime);
        }

        if (Bajar)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, -3, transform.position.z), 1 * Time.deltaTime);
        }
    }
    

}
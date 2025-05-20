using UnityEngine;

public class FeNull : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.name.Contains("Pilar"))
        {
            Destroy(other.transform.gameObject);
        }
    }
}

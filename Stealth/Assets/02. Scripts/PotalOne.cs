using UnityEngine;
using UnityEngine.SceneManagement;

public class PotalOne : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            SceneManager.LoadScene(2);
        }
    }
}

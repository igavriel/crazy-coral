using System.Threading;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] GameObject coral;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void arrowPressed()
    {
        coral.SetActive(true);
        gameObject.SetActive(false);
    }
}

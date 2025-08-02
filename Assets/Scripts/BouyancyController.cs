using UnityEngine;

public class BouyancyController : MonoBehaviour
{
    [SerializeField] Vector2 density;
    [SerializeField] Vector2 flowVariation;
    [SerializeField] Vector2 timer;
    float currTimer;
    BuoyancyEffector2D effector;
    bool isMaximum = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        effector = GetComponent<BuoyancyEffector2D>();
        currTimer = Random.Range(timer.x, timer.y);
    }

    // Update is called once per frame
    void Update()
    {
        currTimer -= Time.deltaTime;
        if (currTimer < 0)
        {
            currTimer = Random.Range(timer.x, timer.y);
            effector.density = Random.Range(density.x, density.y);
            effector.flowVariation = Random.Range(flowVariation.x, flowVariation.y) ;
            isMaximum = !isMaximum;
        }
    }
}

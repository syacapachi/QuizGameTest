using UnityEngine;

public class ManagerLocater : MonoBehaviour
{
    public static ManagerLocater Instance { get; private set; }
    [SerializeField] public GameManager gameManager;
    [SerializeField] public QuizLoaderManager loader;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}

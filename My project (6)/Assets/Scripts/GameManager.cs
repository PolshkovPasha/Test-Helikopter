using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameSettings settings; 

    public GameObject InputManager { get; private set; }
    public GameObject Interactor { get; private set; }

    public CameraFollow camFollow;

    public Transform StartPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeGame();
    }

    public InputReader InputReader { get; private set; }

    private void InitializeGame()
    {
        if (settings == null)
        {
            Debug.LogError("GameSettings не призначено в GameManager!");
            return;
        }

        // 1. Ініціалізуємо Input Manager
        if (settings.inputManagerPrefab != null)
        {
            InputReader = Instantiate(settings.inputManagerPrefab);
            InputReader.Setup(settings.inputSettings);
        }

        // 2. Ініціалізуємо об'єкт реакції (Гелікоптер)
        if (settings.interactionPrefab != null)
        {
            Interactor = Instantiate(settings.interactionPrefab, StartPosition.position, StartPosition.rotation);

                if (camFollow != null)
                {
                    camFollow.SetTarget(Interactor.transform);
                }
                else
                {
                    Debug.LogWarning("Не доданий скрипт CameraFollow для контролю Main Camera!");
                }
            
        }

        Debug.Log("Гейм менеджер успішно ініціалізував усі системи та налаштував камеру!");
    }
}
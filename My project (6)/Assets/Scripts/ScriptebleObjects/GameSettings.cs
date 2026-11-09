using UnityEngine;

[CreateAssetMenu(fileName = "NewGameSettings", menuName = "Game/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Prefabs to Initialize")]
    
    public InputReader inputManagerPrefab;

    public GameObject interactionPrefab;

    [Header("Sub-Settings")]
    public InputSettings inputSettings;

    [Header("Gameplay Variables")]
    public float responseDelay = 0.1f;
}
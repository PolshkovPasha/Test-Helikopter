using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NewInputSettings", menuName = "Game/Input Settings")]
public class InputSettings : ScriptableObject
{
    [Header("Vector Actions (Hold/Continuous)")]
    public InputActionReference moveAction;       
    public InputActionReference turnAction;
    public InputActionReference flyUpAction;      
    public InputActionReference flyDownAction;    
}
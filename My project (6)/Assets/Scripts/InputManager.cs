using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    public static event Action<bool> OnFlyUpHold;
    public static event Action<bool> OnflyDownAction;

    private InputSettings _settings;

    private InputAction _moveAction;
    private InputAction _flyUpAction;
    private InputAction _flyDownAction;
    private InputAction _turnAction;

    // Повертає float від -1 (Q / Ліворуч) до 1 (E / Праворуч)
    public float TurnInput => _turnAction != null ? _turnAction.ReadValue<float>() : 0f;

    public Vector2 MovementInput => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Setup(InputSettings settings)
    {
        _settings = settings;
        if (_settings == null) return;

        // Ініціалізація руху WASD (Value / Vector2)
        if (_settings.moveAction != null)
        {
            _moveAction = _settings.moveAction.action;
            _moveAction.Enable();
        }

        // Ініціалізація злету (Hold)
        if (_settings.flyUpAction != null)
        {
            _flyUpAction = _settings.flyUpAction.action;
            _flyUpAction.performed += ctx => OnFlyUpHold?.Invoke(true);
            _flyUpAction.canceled += ctx => OnFlyUpHold?.Invoke(false);
            _flyUpAction.Enable();
        }

        // Ініціалізація зниження (Hold)
        if (_settings.flyDownAction != null)
        {
            _flyDownAction = _settings.flyDownAction.action;
            _flyDownAction.performed += ctx => OnflyDownAction?.Invoke(true);
            _flyDownAction.canceled += ctx => OnflyDownAction?.Invoke(false);
            _flyDownAction.Enable();
        }

        // Ініціалізація повороту Q/E
        if (settings.turnAction != null)
        {
            _turnAction = settings.turnAction.action;
            _turnAction.Enable();
        }
    }

    private void OnDestroy()
    {
        // Вимикаємо все при знищенні об'єкта
        _moveAction?.Disable();
        _flyUpAction?.Disable();
        _flyDownAction?.Disable();
    }
}
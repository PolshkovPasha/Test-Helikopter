using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectController : MonoBehaviour
{
    [Header("Thrust Settings (Налаштування коефіцієнтів)")]
    [Tooltip("Поточна підйомна сила двигуна (для моніторингу)")]
    public float currentThrust = 0f;
    [SerializeField] private float engenUpMultiplier = 1.5f;
    [SerializeField] private float engenDownMultiplier = 0.3f;
    [SerializeField] private float thrustChangeSpeed = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float maxHorizontalSpeed = 15f;

    [Header("Physics Tail Turn Settings (Фізичний поворот)")]
    [Tooltip("Точка на хвості, куди прикладається сила бічного гвинта")]
    [SerializeField] private Transform tailRotorPosition;
    [Tooltip("Сила тяги хвостового гвинта для повороту гравцем")]
    [SerializeField] private float tailTurnForce = 15f;

    [Header("Helicopter Tilt Settings (Нахили корпусу)")]
    [SerializeField] private Transform visualModel;
    [SerializeField] private float maxTiltAngle = 25f; 
    [Tooltip("Час згладжування нахилу в секундах. Чим більший, тим плавніший нахил.")]
    [SerializeField] private float tiltSmoothTime = 0.25f;

    [Header("Physics Damping (Контроль плавності)")]
    [SerializeField] private float linearDrag = 1.5f;
    [SerializeField] private float angularDrag = 6f;
   

    // Змінні для Mathf.SmoothDampAngle (зберігають швидкість зміни кутів)
    private float _currentForwardTilt = 0f;
    private float _currentSidewaysTilt = 0f;
    private float _forwardTiltVelocity = 0f;
    private float _sidewaysTiltVelocity = 0f;

    private float _turnInput = 0f;

    [SerializeField] private Rigidbody rbody;
    [SerializeField] private Transform helicopterPos;

    [Header("Анімації")]
    [SerializeField] Transform helicopterBlades;
    public float bladesSpeedRotation = 200;

    private bool _isHoldingFlyUp = false;
    private bool _isHoldingFlyDown = false;
    private Vector2 _movementInput = Vector2.zero;

    private void Start()
    {
        if (rbody != null) 
        { 
        // Автоматично вмикаємо інтерполяцію для ідеально плавного руху камери
        rbody.interpolation = RigidbodyInterpolation.Interpolate;

        rbody.linearDamping = linearDrag;
        rbody.angularDamping = angularDrag;
        rbody.useGravity = true;

        if (tailRotorPosition == null)
            {
                Debug.LogError("Додайте точку в полі Tail Rotor Position!", this);
            }
        }
        else 
        {
            Debug.LogError("Поле для Rigidbody пусте");
        }
    }

    private void OnEnable()
    {
        InputReader.OnFlyUpHold += HandleFlyUp;
        InputReader.OnflyDownAction += HandleFlyDown;
    }

    private void OnDisable()
    {
        InputReader.OnFlyUpHold -= HandleFlyUp;
        InputReader.OnflyDownAction -= HandleDownReset;
    }

    private void Update()
    {
        if (InputReader.Instance != null)
        {
            _movementInput = InputReader.Instance.MovementInput;
            _turnInput = InputReader.Instance.TurnInput;
        }

        // Визначаємо цільовий множник тяги
        float targetMultiplier = 1f; // Зависання

        if (_isHoldingFlyUp)
        {
            targetMultiplier = engenUpMultiplier;
        }
        else if (_isHoldingFlyDown)
        {
            targetMultiplier = engenDownMultiplier;
        }

        // Розраховуємо силу з урахуванням нахилу
        float targetThrust = HoverForce * targetMultiplier;
        currentThrust = Mathf.MoveTowards(currentThrust, targetThrust, Time.deltaTime * thrustChangeSpeed);
    }

    private void FixedUpdate()
    {
        HandlePhysicsMovement();
        helicopterBlades.Rotate(Vector3.up * bladesSpeedRotation * Time.deltaTime);
    }

    private void HandlePhysicsMovement()
    {
        // 1. ПЛАВНИЙ НАХИЛ (SmoothDampAngle)
        ApplyVisualTilt();

        // 2. ФІЗИЧНИЙ ПОВОРОТ ХВОСТОМ (Прикладання бічної сили)
        if (tailRotorPosition != null && Mathf.Abs(_turnInput) > 0.01f)
        {
            Vector3 forceDirection = transform.right * _turnInput;
            rbody.AddForceAtPosition(forceDirection * tailTurnForce, tailRotorPosition.position, ForceMode.Acceleration);
        }

        // 3. ЗАХИСТ ВІД ПЕРЕВЕРТАННЯ (Стабілізація фізичного горизонту)
        float currentYaw = transform.eulerAngles.y;
        Quaternion lockedHorizontalRotation = Quaternion.Euler(0f, currentYaw, 0f);
        rbody.MoveRotation(lockedHorizontalRotation);

        // 4. РУХ НА СИЛАХ (Вектор тяги береться від нахиленої візуальної моделі)
        Vector3 totalThrustDirection = visualModel != null ? visualModel.up : transform.up;
        Vector3 combinedFlightForce = totalThrustDirection * currentThrust;

        rbody.AddForce(combinedFlightForce, ForceMode.Acceleration);

        // 5. М'ЯКЕ ОБМЕЖЕННЯ МАКСИМАЛЬНОЇ ГОРИЗОНТАЛЬНОЇ ШВИДКОСТІ
        Vector3 horizontalVelocity = new Vector3(rbody.linearVelocity.x, 0f, rbody.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxHorizontalSpeed)
        {
            Vector3 limitedVel = horizontalVelocity.normalized * maxHorizontalSpeed;
            rbody.linearVelocity = new Vector3(limitedVel.x, rbody.linearVelocity.y, limitedVel.z);
        }
    }

    private void ApplyVisualTilt()
    {
        if (visualModel == null) return;

        // Цільові кути нахилу від введення WASD
        float targetForwardTilt = _movementInput.y * maxTiltAngle;
        float targetSidewaysTilt = -_movementInput.x * maxTiltAngle;

        // Плавне інтерполювання кутів без ривків за допомогою кубічного згладжування
        _currentForwardTilt = Mathf.SmoothDampAngle(_currentForwardTilt, targetForwardTilt, ref _forwardTiltVelocity, tiltSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        _currentSidewaysTilt = Mathf.SmoothDampAngle(_currentSidewaysTilt, targetSidewaysTilt, ref _sidewaysTiltVelocity, tiltSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        // Застосовуємо обертання тільки локально для візуальної моделі
        visualModel.localRotation = Quaternion.Euler(_currentForwardTilt, 0f, _currentSidewaysTilt);
    }

    private void HandleFlyUp(bool isHolding) => _isHoldingFlyUp = isHolding;
    private void HandleFlyDown(bool isHolding) => _isHoldingFlyDown = isHolding;
    private void HandleDownReset(bool isHolding) => _isHoldingFlyDown = isHolding;

    public float HoverForce
    {
        get
        {
            // Беремо нахил саме візуальної моделі, бо вона тепер задає напрямок вектора тяги
            Transform checkTransform = visualModel != null ? visualModel : transform;

            float baseHover = Mathf.Abs(Physics.gravity.y);
            float angleRaw = Vector3.Dot(checkTransform.up, Vector3.up);

            // Уникаємо ділення на нуль або занадто малих значень при екстремальних нахилах
            if (angleRaw <= 0.1f) return baseHover * 2f;

            // Збільшуємо загальну тягу пропорційно нахилу для утримання висоти
            return baseHover / angleRaw;
        }
    }
}
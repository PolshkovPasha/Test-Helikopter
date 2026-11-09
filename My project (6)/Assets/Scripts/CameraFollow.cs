using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Ціль для слідування")]
    [SerializeField] private Transform target; // Сюди перетягнемо наш гелікоптер

    [Header("Налаштування позиції")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // Відступ камери (вище і позаду гелікоптера)
    [SerializeField] private float smoothSpeed = 5f; // Швидкість згладжування руху

    [Header("Налаштування обертання")]
    [SerializeField] private bool followRotation = true; // Чи повинна камера повертатися за гелікоптером
    [SerializeField] private float rotationSmoothSpeed = 5f; // Швидкість згладжування повороту

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. РОЗРАХУНОК ПОЗИЦІЇ
        Vector3 targetPosition;

        if (followRotation)
        {
            // Якщо камера має повертатися разом із гелікоптером (літати за його хвостом)
            targetPosition = target.transform.TransformPoint(offset);
        }
        else
        {
            // Якщо камера зафіксована в одному напрямку і просто ковзає слідом
            targetPosition = target.position + offset;
        }

        // Плавно переміщуємо камеру з поточної позиції в цільову
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 2. РОЗРАХУНОК ПОВОРОТУ
        if (followRotation)
        {
            // Рахуємо правильний поворот у бік гелікоптера
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
        else
        {
            // Камера просто завжди дивиться на гелікоптер, не обертаючись навколо своєї осі
            transform.LookAt(target);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
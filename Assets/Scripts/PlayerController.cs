using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    private Rigidbody rb;
    private Animator animator;
    public Joystick joystick;

    public bool IsMoving { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }


    private void FixedUpdate()
    {
        Move();
        Animate();
    }

    private void Move()
    {
        float moveX = joystick.Horizontal; 
        float moveZ = joystick.Vertical;

        // Отримуємо напрямок камери
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        // Нормалізуємо, щоб не було прискорення по діагоналі
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Рух в напрямку камери
        Vector3 moveDir = forward * moveZ + right * moveX;

        if (moveDir.magnitude > 0.1f || moveX != 0 || moveZ != 0)  // Перевіряємо чи натискаються клавіші руху
        {
            IsMoving = true;
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            rb.velocity = moveDir.normalized * speed + new Vector3(0, rb.velocity.y, 0);
            // Розвертаємо гравця в напрямку руху
            transform.forward = Vector3.Slerp(transform.forward, moveDir.normalized, 0.1f);
        }
        else
        {
            IsMoving = false;
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Зупиняємо рух по горизонталі, якщо не рухаємось
        }
    }

    private void Animate()
    {
        Vector3 flatVelocity = rb.velocity;
        flatVelocity.y = 0;

        // Використовуємо більше чутливий метод для перевірки руху
        float currentSpeed = flatVelocity.magnitude;

        bool isMoving = currentSpeed > 0.01f || (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)); // Перевірка на натискання клавіш
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            // Створення більш плавних анімацій для швидкості
            animator.SetFloat("Speed", currentSpeed / (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed));
        }
        else
        {
            animator.SetFloat("Speed", 0f); // Якщо гравець не рухається, встановлюємо значення 0 для анімації
        }
    }
}

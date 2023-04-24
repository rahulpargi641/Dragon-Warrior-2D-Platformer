using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_JumpForce;
    [SerializeField] private float m_WallJumpForce;
    [SerializeField] private float m_WallPushForce;
    [SerializeField] private float m_Gravity;
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private LayerMask m_WallLayer;

    private Rigidbody2D m_RigidBody;
    private Animator m_Animator;
    private BoxCollider2D m_BoxCollider;

    private float m_HorizontalInput;
    private float m_WallJumpCooldown;

    private void Awake()
    {
        // Refrences
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_BoxCollider = GetComponent<BoxCollider2D>();

        if (m_RigidBody == null)
        {
            Debug.LogError("Rigidbody2D component is missing on the player!");
            return;
        }

        if (m_Animator == null)
        {
            Debug.LogError("Animator component is missing on the player!");
            return;
        }

        if (m_BoxCollider == null)
        {
            Debug.LogError("BoxCollider2D component is missing on the player!");
            return;
        }
    }

    private void Update()
    {
        m_HorizontalInput = Input.GetAxis("Horizontal");

        //Flip player when moving left-right
        if (m_HorizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (m_HorizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        //Set animator parameters
        m_Animator.SetBool("Run", m_HorizontalInput != 0);
        m_Animator.SetBool("Grounded", IsGrounded());

      
        //Wall jump logic
        if (m_WallJumpCooldown > 0.3f)
        {
            m_RigidBody.velocity = new Vector2(m_HorizontalInput * m_RunSpeed, m_RigidBody.velocity.y);

            if (OnWall() && !IsGrounded())
            {
                m_RigidBody.gravityScale = 0;
                m_RigidBody.velocity = Vector2.zero;

            }
            else
            {
                m_RigidBody.gravityScale = m_Gravity;
            }

            if (m_WallJumpCooldown < 0.3f) return;
            if (Input.GetKey(KeyCode.Space))
                Jump();
        }
        else
            m_WallJumpCooldown += Time.deltaTime;
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            m_RigidBody.velocity = new Vector2(m_RigidBody.velocity.x, m_JumpForce);
            m_Animator.SetTrigger("Jump");
        }
        else if (OnWall() && !IsGrounded()) // climb up code
        {
            if (m_HorizontalInput == 0)
            {
                m_RigidBody.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * m_WallPushForce*2, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
                m_RigidBody.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * m_WallPushForce, m_WallJumpForce);

            m_WallJumpCooldown = 0;   // wana to wait before performing the next jump
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(m_BoxCollider.bounds.center, m_BoxCollider.bounds.size, 0, Vector2.down, 0.1f, m_GroundLayer);
        return raycastHit.collider != null;
    }
    private bool OnWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(m_BoxCollider.bounds.center, m_BoxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, m_WallLayer);
        if (raycastHit.collider) Debug.Log("On the wall");
        return raycastHit.collider != null;
    }
}
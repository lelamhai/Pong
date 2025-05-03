using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private float m_Speed = 5f;

    private void Start()
    {
        if (m_Rigidbody2D == null)
        {
            LoadRigidbody();
        }

          

        
    }

    private void Reset()
    {
        LoadRigidbody();
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            bool isRight = Random.value >= 0.5f;
            float xVelocity = -1f;

            if (isRight == true)
            {
                xVelocity = 1f;
            }

            float yVelocity = Random.Range(-1, 1);
            m_Rigidbody2D.velocity = new Vector2(xVelocity, yVelocity) * m_Speed;
        }
    }

    void LoadRigidbody()
    {
        m_Rigidbody2D = this.GetComponent<Rigidbody2D>();
    }
}

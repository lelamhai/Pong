using UnityEngine;

public class AI : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;

    private void Update()
    {
        bool isPressingUp = Input.GetKey(KeyCode.UpArrow);
        bool isPressingDown = Input.GetKey(KeyCode.DownArrow);

        if (isPressingUp)
        {
            this.transform.Translate(Vector2.up * Time.deltaTime * m_Speed);
        }

        if (isPressingDown)
        {
            this.transform.Translate(Vector2.down * Time.deltaTime * m_Speed);
        }
    }
}

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 lastPosition;
    private float sendInterval = 0.1f;
    private float timer = 0f;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.position += movement * speed * Time.deltaTime;

        // Send position at intervals
        timer += Time.deltaTime;
        if (timer >= sendInterval || Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            NetworkManager.Instance.SendPosition(transform.position);
            lastPosition = transform.position;
            timer = 0f;
        }
    }
}
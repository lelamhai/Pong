using UnityEngine;

public class GameStarter : MonoBehaviour
{
    void Start()
    {
        // Initialize dispatcher
        UnityMainThreadDispatcher.Instance.Enqueue(() => { });

        // Connect to server
        NetworkManager.Instance.ConnectToServer();

        // Create local player cube
        GameObject playerCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerCube.transform.position = Vector3.zero;
        playerCube.AddComponent<PlayerMovement>();

        // Set distinct color for local player
        playerCube.GetComponent<Renderer>().material.color = Color.green;
        playerCube.name = "LocalPlayer";
    }
}
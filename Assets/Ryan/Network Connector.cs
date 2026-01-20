using UnityEngine;
using Unity.Netcode;

public class NetworkConnector : MonoBehaviour
{
    public void create()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void join()
    {
        NetworkManager.Singleton.StartClient();
    }
}

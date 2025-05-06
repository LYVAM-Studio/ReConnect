using System;
using System.Collections;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ReconnectNetworkManager : NetworkManager
{
    public static event Action OnClientConnectedEvent;
    public static event Action OnClientDisconnectedEvent;
    
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
        OnClientConnectedEvent?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        
        OnClientDisconnectedEvent?.Invoke();
    }
    
    public async Task<bool> StartClientAsync(float timeoutSeconds = 5f)
    {
        var tcs = new TaskCompletionSource<bool>();

        void OnConnected() => tcs.TrySetResult(true);
        void OnDisconnected() => tcs.TrySetResult(false);

        OnClientConnectedEvent += OnConnected;
        OnClientDisconnectedEvent += OnDisconnected;

        singleton.StartClient();

        Task delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
        Task completedTask = await Task.WhenAny(tcs.Task, delayTask);

        OnClientConnectedEvent -= OnConnected;
        OnClientDisconnectedEvent -= OnDisconnected;

        if (completedTask == tcs.Task)
            return tcs.Task.Result; // true if connected, false if disconnected
        // Timeout fallback
        singleton.StopClient();
        return false;
    }
}
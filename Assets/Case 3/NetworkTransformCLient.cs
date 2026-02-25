using UnityEngine;
using Unity.Netcode.Components;

public class NetworkTransformCLient : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}

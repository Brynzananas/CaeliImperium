using CaeliImperium.Components;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class MonsterChestControllerSpewNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
    }
    public void OnReceived()
    {
        MonsterChestController monsterChestController = networkInstanceId.GetMonsterChestController();
        if (!monsterChestController) return;
        monsterChestController.Spew();
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
    }
    public static void SendToServer(NetworkInstanceId networkInstanceId) => new MonsterChestControllerSpewNetMessage
    {
        networkInstanceId = networkInstanceId
    }.Send(R2API.Networking.NetworkDestination.Server);
}

using CaeliImperium.Components.Refinery;
using RoR2;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace CaeliImperium.Components;
public class ClientToServerMessenger : NetworkBehaviour
{
    public static ClientToServerMessenger authorityInstance;
    public static FixedConditionalWeakTable<PlayerCharacterMasterController, ClientToServerMessenger> keyValuePairs = [];
    public PlayerCharacterMasterController playerCharacterMasterController;
    public void Awake()
    {
        if (Util.HasEffectiveAuthority(netIdentity)) authorityInstance = this;
        playerCharacterMasterController = GetComponent<PlayerCharacterMasterController>();
        if (!playerCharacterMasterController) return;
        if (keyValuePairs.ContainsKey(playerCharacterMasterController))
        {
            keyValuePairs[playerCharacterMasterController] = this;
        }
        else
        {
            keyValuePairs.Add(playerCharacterMasterController, this);
        }
    }
    [Command]
    public void CmdPipelineBuilderPlacePreview(NetworkInstanceId networkInstanceId, Vector3 position, Vector3 normal, bool valid)
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        pipelineBuilder.RpcPlacePreview(position, normal, valid);
    }
    [Command]
    public void CmdPipelineBuilderPlaceNode(NetworkInstanceId networkInstanceId, Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool end)
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        pipelineBuilder.RpcPlaceNode(position, normal, p0, p1, p2, p3, end);
    }
    [Command]
    public void CmdWellExtractorControllerOnPipeConnected(NetworkInstanceId networkInstanceId, Vector3 rotateTo, NetworkInstanceId networkInstanceIdPipelineBuilder)
    {
        WellExtractorController wellExtractorController = networkInstanceId.GetWellExtractorController();
        if (!wellExtractorController) return;
        wellExtractorController.RpcOnPipeConnected(rotateTo, networkInstanceIdPipelineBuilder);
    }
    [Command]
    public void CmdPipelineBuilderDisablePreview(NetworkInstanceId networkInstanceId)
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        pipelineBuilder.RpcDisablePreview();
    }
    [Command]
    public void CmdPipelineBuilderRemoveCurrentInteractor(NetworkInstanceId networkInstanceId)
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        pipelineBuilder.RpcRemoveCurrentInteractor();
    }
    [Command]
    public void CmdMonsterChestSpew(NetworkInstanceId networkInstanceId)
    {
        MonsterChestController monsterChestController = networkInstanceId.GetMonsterChestController();
        if (!monsterChestController) return;
        monsterChestController.Spew();
    }
}

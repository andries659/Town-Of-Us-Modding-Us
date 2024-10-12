using BepInEx;
using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;
using System.Linq;
using Il2CppSystem.Collections.Generic;

// Define the Telepath Role
public class Telepath : CustomRole
{
    private PlayerControl linkedPlayer;
    private bool isLinked = false;
    private float stunDuration = 5.0f; // Stun duration in seconds

    public Telepath(PlayerControl player) : base(player)
    {
        Name = "Telepath";
        RoleColor = Palette.Cyan;
        RoleType = RoleType.Neutral; // Change based on your needs
    }

    // This method is called when a player links with another player
    public void LinkWithPlayer(PlayerControl targetPlayer)
    {
        if (isLinked)
        {
            HudManager.Instance.ShowNotification("Already linked to " + linkedPlayer.name);
            return;
        }

        linkedPlayer = targetPlayer;
        isLinked = true;

        // Show feedback to the player that the Telepath is linked
        HudManager.Instance.ShowNotification("Linked with " + linkedPlayer.name);

        // Sync with other clients using an RPC
        RpcLinkWithPlayer(targetPlayer.PlayerId);
    }

    // Synchronize the link with all other clients
    [Rpc(RpcTarget.All, RpcMode.IncludeSelf)]
    private void RpcLinkWithPlayer(byte playerId)
    {
        linkedPlayer = PlayerControl.AllPlayerControls.FirstOrDefault(p => p.PlayerId == playerId);
    }

    // Called every frame to monitor the linked player
    public override void PostFixedUpdate()
    {
        if (isLinked && linkedPlayer != null)
        {
            // Mimic the linked player's position (or other data)
            PlayerControl.LocalPlayer.transform.position = linkedPlayer.transform.position;
            // Example: Show the linked player's tasks or actions in real time
        }
    }

    // Called when the linked player dies
    public override void OnPlayerDeath(PlayerControl deadPlayer)
    {
        if (isLinked && linkedPlayer == deadPlayer)
        {
            StunTelepath();
        }
    }

    private void StunTelepath()
    {
        // Apply stun to the Telepath (disabling movement, actions, etc.)
        PlayerControl.LocalPlayer.moveable = false;
        HudManager.Instance.ShowNotification("You are stunned!");

        Coroutines.Start(StunDuration(stunDuration));
    }

    // Coroutine to release the stun after a delay
    private IEnumerator StunDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        PlayerControl.LocalPlayer.moveable = true;
        HudManager.Instance.ShowNotification("Stun ended!");
    }

    // Sync role details with all clients
    public override void RpcAssignRole(byte playerId)
    {
        // Call base method to sync role across players
        base.RpcAssignRole(playerId);
    }
}

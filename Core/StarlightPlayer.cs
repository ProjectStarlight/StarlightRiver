using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using StarlightRiver.GUI;
using StarlightRiver.Items.Armor;
using StarlightRiver.NPCs.Boss.SquidBoss;
using StarlightRiver.Tiles.Permafrost;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public partial class StarlightPlayer : ModPlayer
    {
        public int Timer { get; private set; }

        public bool JustHit = false;
        public int LastHit = 0;

        public bool DarkSlow = false;

        public int Shake = 0;

        public int ScreenMoveTime = 0;
        public Vector2 ScreenMoveTarget = new Vector2(0, 0);
        public Vector2 ScreenMovePan = new Vector2(0, 0);
        private int ScreenMoveTimer = 0;
        private int panDown = 0;

        public int platformTimer = 0;

        public int PickupTimer = 0;
        public int MaxPickupTimer = 0;
        public NPC PickupTarget;

        public float GuardDamage;
        public int GuardCrit;
        public float GuardBuff;
        public int GuardRad;

        public float itemSpeed;

        public override void PreUpdate()
        {
            if (PickupTarget != null)
            {
                PickupTimer++;

                player.immune = true;
                player.immuneTime = 5;
                player.immuneNoBlink = true;

                player.Center = PickupTarget.Center;
                if (PickupTimer >= MaxPickupTimer) PickupTarget = null;
            }
            else PickupTimer = 0;

            platformTimer--;

            if (Main.netMode != NetmodeID.Server)
            {
                AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

                Stamina.visible = false;
                Infusion.visible = false;

                if (mp.Abilities.Any(a => !a.Locked)) Stamina.visible = true;

                if (Main.playerInventory)
                {
                    if (player.chest == -1 && Main.npcShop == 0) Collection.visible = true;
                    else Collection.visible = false;

                    GUI.Codex.ButtonVisible = true;
                    if (mp.Abilities.Any(a => !a.Locked)) Infusion.visible = true;
                }
                else
                {
                    Collection.visible = false;
                    Collection.ActiveAbility = null;
                    GUI.Codex.ButtonVisible = false;
                    GUI.Codex.Open = false;
                    Infusion.visible = false;
                }
            }

            if (DarkSlow)
            {
                player.velocity.X *= 0.8f;
            }
            DarkSlow = false;
        }

        public delegate void ResetEffectsDelegate(StarlightPlayer player);
        public static event ResetEffectsDelegate ResetEffectsEvent;
        public override void ResetEffects()
        {
            ResetEffectsEvent?.Invoke(this);
            GuardDamage = 1;
            GuardCrit = 0;
            GuardBuff = 1;
            GuardRad = 0;
            itemSpeed = 1;
        }

        public override void PostUpdate()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && player == Main.LocalPlayer) StarlightWorld.rottime += (float)Math.PI / 60;
            Timer++;
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            JustHit = true;
            LastHit = Timer;
        }

        public override void PostUpdateEquips() => JustHit = false;

        public override void ModifyScreenPosition()
        {
            if (ScreenMoveTime > 0 && ScreenMoveTarget != Vector2.Zero)
            {
                Vector2 off = new Vector2(Main.screenWidth, Main.screenHeight) / -2;
                if (ScreenMoveTimer <= 30) //go out
                {
                    Main.screenPosition = Vector2.SmoothStep(Main.LocalPlayer.Center + off, ScreenMoveTarget + off, ScreenMoveTimer / 30f);
                }
                else if (ScreenMoveTimer >= ScreenMoveTime - 30) //go in
                {
                    Main.screenPosition = Vector2.SmoothStep((ScreenMovePan == Vector2.Zero ? ScreenMoveTarget : ScreenMovePan) + off, Main.LocalPlayer.Center + off, (ScreenMoveTimer - (ScreenMoveTime - 30)) / 30f);
                }
                else
                {
                    if (ScreenMovePan == Vector2.Zero) Main.screenPosition = ScreenMoveTarget + off; //stay on target
                    else if (ScreenMoveTimer <= ScreenMoveTime - 150) Main.screenPosition = Vector2.Lerp(ScreenMoveTarget + off, ScreenMovePan + off, ScreenMoveTimer / (float)(ScreenMoveTime - 150));
                    else Main.screenPosition = ScreenMovePan + off;
                }

                if (ScreenMoveTimer == ScreenMoveTime) { ScreenMoveTime = 0; ScreenMoveTimer = 0; ScreenMoveTarget = Vector2.Zero; ScreenMovePan = Vector2.Zero; }
                ScreenMoveTimer++;
            }

            if (Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].wall == ModContent.WallType<AuroraBrickWall>() &&
                Main.npc.Any(n => n.active && n.modNPC is SquidBoss && n.ai[0] == (int)SquidBoss.AIStates.SecondPhase && n.ai[1] > 300) && panDown < 150) //the worlds most ungodly check ever
            {
                panDown++;
            }
            else if (panDown > 0) panDown--;

            Main.screenPosition.Y += Main.rand.Next(-Shake, Shake) + panDown;
            Main.screenPosition.X += Main.rand.Next(-Shake, Shake);
            if (Shake > 0) { Shake--; }
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (player.HeldItem.modItem is Items.Vitric.VitricSword && (player.HeldItem.modItem as Items.Vitric.VitricSword).Broken) PlayerLayer.HeldItem.visible = false;

            Action<PlayerDrawInfo> layerTarget = DrawGlowmasks;
            PlayerLayer layer = new PlayerLayer("ItemLayer", "Starlight River Item Drawing Layer", layerTarget);
            layers.Insert(layers.IndexOf(layers.FirstOrDefault(n => n.Name == "Arms")), layer);

            void DrawGlowmasks(PlayerDrawInfo info)
            {
                if (info.drawPlayer.HeldItem.modItem is Items.IGlowingItem) (info.drawPlayer.HeldItem.modItem as Items.IGlowingItem).DrawGlowmask(info);
            }
            #region armor masks
            Action<PlayerDrawInfo> helmetTarget = DrawHelmetMask;
            layers.Insert(layers.IndexOf(layers.FirstOrDefault(n => n.Name == "Head")) + 1, new PlayerLayer("SLRHelmet", "Helmet mask layer", helmetTarget));

            Action<PlayerDrawInfo> chestTarget = DrawChestMask;
            layers.Insert(layers.IndexOf(layers.FirstOrDefault(n => n.Name == "Body")) + 1, new PlayerLayer("SLRChest", "Chest mask layer", chestTarget));

            Action<PlayerDrawInfo> legTarget = DrawLegMask;
            layers.Insert(layers.IndexOf(layers.FirstOrDefault(n => n.Name == "Legs")) + 1, new PlayerLayer("SLRLeg", "Leg mask layer", legTarget));


            void DrawHelmetMask(PlayerDrawInfo info)
            {
                if (info.drawPlayer.armor[10].IsAir && info.drawPlayer.armor[0].modItem is IArmorLayerDrawable) (info.drawPlayer.armor[0].modItem as IArmorLayerDrawable).DrawArmorLayer(info);
                else if (info.drawPlayer.armor[10].modItem is IArmorLayerDrawable) (info.drawPlayer.armor[10].modItem as IArmorLayerDrawable).DrawArmorLayer(info);
            }
            void DrawChestMask(PlayerDrawInfo info)
            {
                if (info.drawPlayer.armor[11].IsAir && info.drawPlayer.armor[1].modItem is IArmorLayerDrawable) (info.drawPlayer.armor[1].modItem as IArmorLayerDrawable).DrawArmorLayer(info);
                else if (info.drawPlayer.armor[11].modItem is IArmorLayerDrawable) (info.drawPlayer.armor[11].modItem as IArmorLayerDrawable).DrawArmorLayer(info);
            }
            void DrawLegMask(PlayerDrawInfo info)
            {
                if (info.drawPlayer.armor[12].IsAir && info.drawPlayer.armor[2].modItem is IArmorLayerDrawable) (info.drawPlayer.armor[2].modItem as IArmorLayerDrawable).DrawArmorLayer(info);
                else if (info.drawPlayer.armor[12].modItem is IArmorLayerDrawable) (info.drawPlayer.armor[12].modItem as IArmorLayerDrawable).DrawArmorLayer(info);
            }
            #endregion
        }

        public override void OnEnterWorld(Player player)
        {
            Collection.ShouldReset = true;
        }

        public override float UseTimeMultiplier(Item item) => itemSpeed;
    }
}

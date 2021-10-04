using Microsoft.Xna.Framework;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Items.Armor;
using StarlightRiver.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public partial class StarlightPlayer : ModPlayer
    {
        public int Timer { get; private set; }

        public bool JustHit = false;
        public int LastHit = 0;
        public bool trueInvisible = false;

        public bool DarkSlow = false;

        public int Shake = 0;

        public int ScreenMoveTime = 0;
        public Vector2 ScreenMoveTarget = new Vector2(0, 0);
        public Vector2 ScreenMovePan = new Vector2(0, 0);
        public bool ScreenMoveHold = false;
        private int ScreenMoveTimer = 0;
        private int panDown = 0;

        public int platformTimer = 0;

        public int PickupTimer = 0;
        public int MaxPickupTimer = 0;
        public NPC PickupTarget;

        public bool inTutorial;

        public float GuardDamage;
        public int GuardCrit;
        public float GuardBuff;
        public int GuardRad;

        public float itemSpeed;
        public float rotation;

        public static List<PlayerTicker> spawners = new List<PlayerTicker>();

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

            if (DarkSlow)
            {
                player.velocity.X *= 0.8f;
            }
            DarkSlow = false;

            if (!player.immune)
            {
                VitricSpike.CollideWithSpikes(player, out int damage);
                if (damage > 0)
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was impaled by glass shards."), damage, 0);
            }

            foreach (PlayerTicker ticker in spawners.Where(n => n.Active(player) && Timer % n.TickFrequency == 0))
                ticker.Tick(player);
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

            trueInvisible = false;
        }

        public override void PostUpdate()
        {
            PostUpdateEvent.Invoke(player);

            if (Main.netMode == NetmodeID.MultiplayerClient && player == Main.LocalPlayer) StarlightWorld.rottime += (float)Math.PI / 60;
            Timer++;
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            JustHit = true;
            LastHit = Timer;
        }

        public delegate void PostUpdateEquipsDelegate(StarlightPlayer player);
        public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
        public override void PostUpdateEquips()
        {
            PostUpdateEquipsEvent?.Invoke(this);
            JustHit = false;
        }

        private int AddExpansion()
        {
            return (int)Math.Floor(((Main.screenPosition.X + (Main.screenWidth * (1f / Core.ZoomHandler.ExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.X + Main.screenWidth) / 16f) + 2));
        }

        private int AddExpansionY()
        {
            return (int)Math.Floor(((Main.screenPosition.Y + (Main.screenHeight * (1f / Core.ZoomHandler.ExtraZoomTarget))) / 16f) + 2 - (((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2));
        }

        public override void ModifyScreenPosition()
        {
            var adj = new Vector2(AddExpansion(), AddExpansionY()) * 8;
            Main.screenPosition -= adj;

            if (ScreenMoveTime > 0 && ScreenMoveTarget != Vector2.Zero)
            {
                Vector2 off = (new Vector2(Main.screenWidth, Main.screenHeight) / -2) * 1 / ZoomHandler.ExtraZoomTarget;
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
                    if (ScreenMovePan == Vector2.Zero)
                        Main.screenPosition = ScreenMoveTarget + off; //stay on target

                    else if (ScreenMoveTimer <= ScreenMoveTime - 150)
                        Main.screenPosition = Vector2.Lerp(ScreenMoveTarget + off, ScreenMovePan + off, ScreenMoveTimer / (float)(ScreenMoveTime - 150));

                    else
                        Main.screenPosition = ScreenMovePan + off;
                }

                if (ScreenMoveTimer == ScreenMoveTime)
                {
                    ScreenMoveTime = 0;
                    ScreenMoveTimer = 0;
                    ScreenMoveTarget = Vector2.Zero;
                    ScreenMovePan = Vector2.Zero;
                }

                if (ScreenMoveTimer < ScreenMoveTime - 30 || !ScreenMoveHold)
                    ScreenMoveTimer++;
            }

            bool validTile = WorldGen.InWorld((int)Main.LocalPlayer.position.X / 16, (int)Main.LocalPlayer.position.Y / 16) && Framing.GetTileSafely(Main.LocalPlayer.Center)?.wall == ModContent.WallType<AuroraBrickWall>();
            if (validTile && Main.npc.Any(n => n.active && n.modNPC is SquidBoss && n.ai[0] == (int)SquidBoss.AIStates.SecondPhase && n.ai[1] > 300) && panDown < 150) // TODO: fix the worlds most ungodly check ever
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
            if (player.HeldItem.modItem is Content.Items.Vitric.VitricSword && (player.HeldItem.modItem as Content.Items.Vitric.VitricSword).Broken) PlayerLayer.HeldItem.visible = false;

            Action<PlayerDrawInfo> layerTarget = DrawGlowmasks;
            PlayerLayer layer = new PlayerLayer("ItemLayer", "Starlight River Item Drawing Layer", layerTarget);
            layers.Insert(layers.IndexOf(layers.FirstOrDefault(n => n.Name == "Arms")), layer);

            void DrawGlowmasks(PlayerDrawInfo info)
            {
                if (info.drawPlayer.HeldItem.modItem is IGlowingItem) (info.drawPlayer.HeldItem.modItem as IGlowingItem).DrawGlowmask(info);
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
            inTutorial = false;
        }

        public override void PlayerConnect(Player player)
        {
            AbilityProgress packet = new AbilityProgress(this.player.whoAmI, this.player.GetHandler());
            packet.Send();
        }

        public override float UseTimeMultiplier(Item item) => itemSpeed;

        public void DoubleTapEffects(int keyDir)
        {
            if (keyDir == (Main.ReversedUpDownArmorSetBonuses ? 1 : 0)) //double tap down
            {
                //Breacher drone
                var spotter = Main.projectile.Where(n => n.owner == player.whoAmI && n.modProjectile is SpotterDrone drone2).OrderBy(n => Vector2.Distance(n.Center, player.Center)).FirstOrDefault();
                if (spotter != default && spotter.modProjectile is SpotterDrone drone && drone.CanScan)
                {
                    BreacherPlayer modPlayer = player.GetModPlayer<BreacherPlayer>();
                    if (modPlayer.Charges >= 1)
                    {
                        Helper.PlayPitched("Effects/Chirp" + (Main.rand.Next(2) + 1).ToString(), 0.5f, 0);
                        drone.ScanTimer = SpotterDrone.ScanTime;
                        drone.Charges = player.GetModPlayer<BreacherPlayer>().Charges;
                        player.GetModPlayer<BreacherPlayer>().ticks = 0;
                    }
                }
            }
        }
    }
}

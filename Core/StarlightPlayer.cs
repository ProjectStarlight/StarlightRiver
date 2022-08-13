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
using StarlightRiver.Core.Loaders;
using Terraria.Graphics.CameraModifiers;
using StarlightRiver.Core.Systems;

namespace StarlightRiver.Core
{
    public partial class StarlightPlayer : ModPlayer
    {
        public int Timer { get; private set; }

        public bool JustHit = false;
        public int LastHit = 0;
        public bool trueInvisible = false;

        public bool DarkSlow = false;

        public int platformTimer = 0;

        public int PickupTimer = 0; //TODO: Move this into its own thing eventually
        public int MaxPickupTimer = 0;
        public NPC PickupTarget;
        public Vector2 oldPickupPos;

        public bool inTutorial;

        public float GuardDamage;
        public int GuardCrit;
        public float GuardBuff;
        public int GuardRad;

        public float ItemSpeed;
        public float rotation;

        public static List<PlayerTicker> spawners = new List<PlayerTicker>();

        public bool shouldSendHitPacket = false;
        public OnHitPacket hitPacket = null;

        public override void PreUpdate()
        {
            if (PickupTarget != null)
            {
                if (PickupTimer == 0)
                    oldPickupPos = Player.Center;

                PickupTimer++;

                Player.immune = true;
                Player.immuneTime = 5;
                Player.immuneNoBlink = true;

                Player.Center = Vector2.SmoothStep(oldPickupPos, PickupTarget.Center, PickupTimer / 30f);
                if (PickupTimer >= MaxPickupTimer) PickupTarget = null;
            }
            else PickupTimer = 0;

            platformTimer--;

            if (DarkSlow)
            {
                Player.velocity.X *= 0.8f;
            }
            DarkSlow = false;

            if (!Player.immune)
            {
                VitricSpike.CollideWithSpikes(Player, out int damage);
                if (damage > 0)
                    Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " was impaled by glass shards."), damage, 0);
            }

            foreach (PlayerTicker ticker in spawners.Where(n => n.Active(Player) && Timer % n.TickFrequency == 0))
                ticker.Tick(Player);
        }

        public delegate void ResetEffectsDelegate(StarlightPlayer Player);
        public static event ResetEffectsDelegate ResetEffectsEvent;
        public override void ResetEffects()
        {
            ResetEffectsEvent?.Invoke(this);
            GuardDamage = 1;
            GuardCrit = 0;
            GuardBuff = 1;
            GuardRad = 0;
            ItemSpeed = 1;

            trueInvisible = false;
            shouldSendHitPacket = false;
        }

        public override void PostUpdate()
        {
            PostUpdateEvent.Invoke(Player);

            if (Main.netMode == NetmodeID.MultiplayerClient && Player == Main.LocalPlayer) StarlightWorld.rottime += (float)Math.PI / 60;
            Timer++;
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            JustHit = true;
            LastHit = Timer;
        }

        public delegate void PostUpdateEquipsDelegate(StarlightPlayer Player);
        public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
        public override void PostUpdateEquips()
        {
            PostUpdateEquipsEvent?.Invoke(this);
            JustHit = false;
        }

        /// <summary>
        /// This is expected to be run PRIOR to the modify hit hooks so that when we send the data we can send it as it was prior to the edits so when it runs on the other client the modify hooks should be the same at the end
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        /// <param name="knockback"></param>
        /// <param name="crit"></param>
        public void addHitPacket(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                hitPacket = new OnHitPacket(Player, proj, target, damage, knockback, crit);
        }

        /// <summary>
        /// This is expected to run AFTER the on hit hooks so that if and only if any event during the modify and/or hit hooks wants the data to be synced we will do so
        /// </summary>
        public void sendHitPacket()
        {
            if (shouldSendHitPacket && hitPacket != null && Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
            {
                hitPacket.Send(-1, Main.myPlayer, false);
                shouldSendHitPacket = false;
                hitPacket = null;
            }
        }

        public override void OnEnterWorld(Player Player)
        {
            ZoomHandler.SetZoomAnimation(Main.GameZoomTarget, 1);
            StarlightRiver.LightingBufferInstance.ResizeBuffers(Main.screenWidth, Main.screenHeight);

            rotation = 0;

            BossBarOverlay.tracked = null;
            Collection.ShouldReset = true;
            inTutorial = false;

            DummyTile.dummies.Clear();

            if(Main.masterMode)
                UILoader.GetUIState<MessageBox>().Display("WARNING", "Starlight River has unique behavior for it's bosses in master mode. This behavior is intended to be immensely difficult over anything else, and assumes a high amount of knowldge about " +
					"both the mod and base game. Starlight River master mode is not intended for a first playthrough. Starlight River master mode is not intended to be fair. Starlight River master mode is not intended to be fun for everyone. " +
					"Please remember that the health, both physical and mental, of yourself and those around you is far more important than this game or anything inside of it."); 
        }

        public override void OnRespawn(Player Player)
        {
            if (Player == Main.LocalPlayer)
                CameraSystem.Reset();

            rotation = 0;
            inTutorial = false;
        }

        public override void PlayerConnect(Player Player)
        {
            AbilityProgress packet = new AbilityProgress(Main.myPlayer, Main.LocalPlayer.GetHandler());
            packet.Send(runLocally: false);
        }

        public override float UseTimeMultiplier(Item Item) => ItemSpeed;

        public void DoubleTapEffects(int keyDir) //TODO: Move this to breacher armor classes
        {
            if (keyDir == (Main.ReversedUpDownArmorSetBonuses ? 1 : 0)) //double tap down
            {
                //Breacher drone
                var spotter = Main.projectile.Where(n => n.owner == Player.whoAmI && n.ModProjectile is SpotterDrone drone2).OrderBy(n => Vector2.Distance(n.Center, Player.Center)).FirstOrDefault();
                if (spotter != default && spotter.ModProjectile is SpotterDrone drone && drone.CanScan)
                {
                    BreacherPlayer modPlayer = Player.GetModPlayer<BreacherPlayer>();
                    Projectile Projectile = spotter;
                    var target = Main.npc.Where(n => n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, Main.MouseWorld)).FirstOrDefault();
                    if (modPlayer.Charges >= 1 && target != default)
                    {
                        Helper.PlayPitched("Effects/Chirp" + (Main.rand.Next(2) + 1).ToString(), 0.5f, 0);
                        drone.ScanTimer = SpotterDrone.ScanTime;
                        drone.Charges = Player.GetModPlayer<BreacherPlayer>().Charges;
                        Player.GetModPlayer<BreacherPlayer>().ticks = 0;
                    }
                }
            }
        }
    }
}

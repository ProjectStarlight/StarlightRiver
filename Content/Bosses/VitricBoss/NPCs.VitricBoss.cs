using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Foregrounds;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static StarlightRiver.Helpers.Helper;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
    [AutoloadBossHead]
    public sealed partial class VitricBoss : ModNPC
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 homePos;
        public List<NPC> crystals = new List<NPC>();
        public List<Vector2> crystalLocations = new List<Vector2>();
        public Rectangle arena;

        public int twistTimer;
        public int maxTwistTimer;
        public int lastTwistState;
        public int twistTarget;
        public int shieldShaderTimer;

        public bool rotationLocked;
        public float lockedRotation;

        private int favoriteCrystal = 0;
        private bool altAttack = false;
        public Color glowColor = Color.Transparent;

        private List<VitricBossSwoosh> swooshes;
        private BodyHandler body;

        //Pain handler, possibly move this to a parent class at some point? Kind of a strange thing to parent for
        public float pain;
        public float painDirection;

        public Vector2 PainOffset => Vector2.UnitX.RotatedBy(painDirection) * (pain / 200f * 128);

        internal ref float GlobalTimer => ref npc.ai[0];
        internal ref float Phase => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];

        public override string Texture => AssetDirectory.VitricBoss + Name;

        #region tml hooks

        public override bool CheckActive() => Phase == (int)AIStates.Leaving;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Ceiros");

        public override bool Autoload(ref string name)
        {
            BodyHandler.LoadGores();
            return base.Autoload(ref name);
        }

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.lifeMax = 5750;
            npc.damage = 30;
            npc.defense = 10;
            npc.knockBackResist = 0f;
            npc.width = 140;
            npc.height = 120;
            npc.value = Item.buyPrice(0, 20, 0, 0);
            npc.npcSlots = 100f;
            npc.dontTakeDamage = true;
            npc.friendly = false;
            npc.boss = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.dontTakeDamageFromHostiles = true;
            npc.behindTiles = true;

            npc.HitSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/VitricBoss/ceramicimpact");

            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBoss1");

            swooshes = new List<VitricBossSwoosh>()
            {
            new VitricBossSwoosh(new Vector2(-16, -40), 6, this),
            new VitricBossSwoosh(new Vector2(16, -40), 6, this),
            new VitricBossSwoosh(new Vector2(-46, -34), 10, this),
            new VitricBossSwoosh(new Vector2(46, -34), 10, this)
            };

            body = new BodyHandler(this);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(8500 * bossLifeScale);
            npc.damage = 40;
            npc.defense = 12;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
            //return Phase == (int)AIStates.FirstPhase && AttackPhase == 4 && AttackTimer % 240 < 120;
        }

        public override bool CheckDead()
        {
            if (Phase == (int)AIStates.Dying && GlobalTimer >= 659)
            {
                foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.active = false; //reset arena
                StarlightWorld.Flag(WorldFlags.VitricBossDowned);
                return true;
            }

            if (Phase == (int)AIStates.SecondPhase || Phase == (int)AIStates.FirstPhase)
            {
                foreach (Player player in Main.player.Where(n => n.Hitbox.Intersects(arena)))
                {
                    player.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = homePos;
                    player.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 720;
                    player.immuneTime = 720;
                    player.immune = true;
                }

                foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) 
                    npc.ai[1] = 4;

                ChangePhase(AIStates.Dying, true);
                npc.dontTakeDamage = true;
                npc.life = 1;

                return false;
            }

            if (Phase == (int)AIStates.Dying)
                return true;

            else
                return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            swooshes.ForEach(n => n.Draw(spriteBatch));

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            swooshes.ForEach(n => n.DrawAdditive(spriteBatch));

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            body.DrawBody(spriteBatch);

            npc.frame.Width = 204;
            npc.frame.Height = 190;
            var effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0;
            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition + PainOffset, npc.frame, new Color(Lighting.GetSubLight(npc.Center)), npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);
            spriteBatch.Draw(GetTexture(Texture + "Glow"), npc.Center - Main.screenPosition + PainOffset, npc.frame, Color.White, npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);

            if (Phase == (int)AIStates.Dying) //death
            {
                var effect = Terraria.Graphics.Effects.Filters.Scene["MagmaCracks"].GetShader().Shader;
                effect.Parameters["sampleTexture2"].SetValue(GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/CrackMap"));
                effect.Parameters["sampleTexture3"].SetValue(GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/ProgressionMap"));
                effect.Parameters["uTime"].SetValue((GlobalTimer - 160) / 600f);

                effect.Parameters["sourceFrame"].SetValue(new Vector4(npc.frame.X, npc.frame.Y, npc.frame.Width, npc.frame.Height));
                effect.Parameters["texSize"].SetValue(GetTexture(Texture).Size());

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition + PainOffset, npc.frame, new Color(Lighting.GetSubLight(npc.Center)), npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(GetTexture(Texture + "Godray"), npc.Center - Main.screenPosition + PainOffset + new Vector2(2, -20), null, Color.White * ((GlobalTimer - 160) / 600f), npc.rotation, GetTexture(Texture + "Godray").Size() / 2, npc.scale, effects, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (Phase == (int)AIStates.FirstPhase && npc.dontTakeDamage) //draws the npc's shield when immune and in the first phase
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/Shield");
                var effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0;

                var effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
                effect.Parameters["sampleTexture2"].SetValue(GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap"));
                effect.Parameters["uTime"].SetValue(2 - (shieldShaderTimer / 120f) * 2);
                effect.Parameters["sourceFrame"].SetValue(new Vector4(npc.frame.X, npc.frame.Y, npc.frame.Width, npc.frame.Height));
                effect.Parameters["texSize"].SetValue(tex.Size());

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(tex, npc.Center - Main.screenPosition + PainOffset, npc.frame, Color.White, npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position.Y += 40;

            var spriteBatch = Main.spriteBatch;

            var tex = GetTexture(AssetDirectory.VitricBoss + "VitricBossBarUnder");
            var texOver = GetTexture(AssetDirectory.VitricBoss + "VitricBossBarOver");
            var progress = (float)npc.life / npc.lifeMax;

            Rectangle target = new Rectangle((int)(position.X - Main.screenPosition.X) + 2, (int)(position.Y - Main.screenPosition.Y), (int)(progress * tex.Width - 4), tex.Height);
            Rectangle source = new Rectangle(2, 0, (int)(progress * tex.Width - 4), tex.Height);

            var color = progress > 0.5f ?
                Color.Lerp(Color.Yellow, Color.LimeGreen, progress * 2 - 1) :
                Color.Lerp(Color.Red, Color.Yellow, progress * 2);

            spriteBatch.Draw(tex, position - Main.screenPosition, null, color, 0, tex.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(texOver, target, source, color, 0, tex.Size() / 2, 0, 0);

            return false;
        }

        public override void NPCLoot()
        {
            body.SpawnGores2();

            if (Main.expertMode)
                npc.DropItemInstanced(npc.Center, Vector2.One, ItemType<VitricBossBag>());

            else
            {
                int weapon = Main.rand.Next(4);
                switch (weapon)
                {
                    case 0: Item.NewItem(npc.Center, ItemType<BossSpear>()); break;
                    case 1: Item.NewItem(npc.Center, ItemType<VitricBossBow>()); break;
                    case 3: Item.NewItem(npc.Center, ItemType<Needler>()); break;
                    case 4: Item.NewItem(npc.Center, ItemType<RefractiveBlade>()); break;
                }

                Item.NewItem(npc.Center, ItemType<VitricOre>(), Main.rand.Next(30, 50));
                Item.NewItem(npc.Center, ItemType<MagmaCore>(), Main.rand.Next(1, 2));
                Item.NewItem(npc.Center, ItemType<Items.Misc.StaminaUp>());

                if(Main.rand.Next(10) == 0)
                    Item.NewItem(npc.Center, ItemType<Items.BarrierDye.VitricBossBarrierDye>());
            }
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            if (pain > 0)
                painDirection += Helper.CompareAngle((npc.Center - player.Center).ToRotation(), painDirection) * Math.Min(damage / 200f, 0.5f);
            else
                painDirection = (npc.Center - player.Center).ToRotation();

            pain += damage;

            if (crit)
                pain += 40;
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (pain > 0)
                painDirection += Helper.CompareAngle((npc.Center - projectile.Center).ToRotation(), painDirection) * Math.Min(damage / 200f, 0.5f);
            else
                painDirection = (npc.Center - projectile.Center).ToRotation();

            pain += damage;

            if (crit)
                pain += 40;
        }

        #endregion tml hooks

        #region helper methods

        //Used for the various differing passive animations of the different forms
        private void SetFrameX(int frame)
        {
            npc.frame.X = npc.frame.Width * frame;
        }

        private void SetFrameY(int frame)
        {
            npc.frame.Y = npc.frame.Height * frame;
        }

        //resets animation and changes phase
        private void ChangePhase(AIStates phase, bool resetTime = false)
        {
            npc.frame.Y = 0;
            Phase = (int)phase;
            if (resetTime) GlobalTimer = 0;
        }

        private int GetTwistDirection(float angle)
		{
            int direction = 0;

            if (angle > 1.57f && angle < 1.57f * 3)
                direction = -1;
            else
                direction = 1;

            if (Math.Abs(angle) > MathHelper.PiOver4 && Math.Abs(angle) < MathHelper.PiOver4 * 3)
                direction = 0;

            return direction;
        }

        private void Twist(int duration)
        {
            int direction = Main.player[npc.target].Center.X > npc.Center.X ? 1 : -1;

            float angle = (Main.player[npc.target].Center - npc.Center).ToRotation();
            if (Math.Abs(angle) > MathHelper.PiOver4 && Math.Abs(angle) < MathHelper.PiOver4 * 3)
                direction = 0;

            Twist(duration, direction);
        }

        private void Twist(int duration, int direction)
        {
            if (direction != lastTwistState)
            {
                twistTimer = 0;
                twistTarget = direction;
                maxTwistTimer = duration;
            }
        }

        #endregion helper methods

        #region AI
        public enum AIStates
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            Anger = 3,
            FirstToSecond = 4,
            SecondPhase = 5,
            Leaving = 6,
            Dying = 7
        }

        public override void PostAI()
        {
            //npc.life = 1;

            //if (Phase > (int)AIStates.SpawnAnimation && Phase < (int)AIStates.SecondPhase)
                //Phase = (int)AIStates.SecondPhase;

            //TODO: Remove later, debug only
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y)) //Boss Speed Up Key
            {
                for (int k = 0; k < 12; k++)
                    AI();
            }
        }

        public override void AI()
        {
            //Ticks the timer
            GlobalTimer++;
            AttackTimer++;

            //twisting
            if (twistTimer < maxTwistTimer)
                twistTimer++;

            if (twistTimer == maxTwistTimer)
            {
                lastTwistState = twistTarget;
            }

            //pain
            if (pain > 0)
                pain -= pain / 25f;

            pain = (int)MathHelper.Clamp(pain, 0, 100);

            //Main AI
            Lighting.AddLight(npc.Center, new Vector3(1, 0.8f, 0.4f)); //glow

            if (Phase != (int)AIStates.Leaving && arena != new Rectangle() && !Main.player.Any(n => n.active && n.statLife > 0 && n.Hitbox.Intersects(arena))) //if no valid players are detected
            {
                GlobalTimer = 0;
                Phase = (int)AIStates.Leaving; //begone thot!
                crystals.ForEach(n => n.ai[2] = 4);
                crystals.ForEach(n => n.ai[1] = 0);
            }

            float sin = (float)Math.Sin(Main.GameUpdateCount * 0.1f); //health bar glow color timer

            switch (Phase)
            {
                //on spawn effects
                case (int)AIStates.SpawnEffects:

                    for (int k = 0; k < Main.maxNPCs; k++) //finds all the large platforms to add them to the list of possible locations for the nuke attack
                    {
                        NPC npc = Main.npc[k];
                        if (npc?.active == true && (npc.type == NPCType<VitricBossPlatformUp>() || npc.type == NPCType<VitricBossPlatformDown>())) crystalLocations.Add(npc.Center + new Vector2(0, -48));
                    }

                    const int arenaWidth = 1280;
                    const int arenaHeight = 884;
                    arena = new Rectangle((int)npc.Center.X + 8 - arenaWidth / 2, (int)npc.Center.Y - 832 - arenaHeight / 2, arenaWidth, arenaHeight);

                    ChangePhase(AIStates.SpawnAnimation, true);
                    break;

                case (int)AIStates.SpawnAnimation: //the animation that plays while the boss is spawning and the title card is shown

                    SpawnAnimation();
                    DoRotation();

                    break;

                case (int)AIStates.FirstPhase:
                 
                    BootlegHealthbar.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0.2f) * Math.Min(1, GlobalTimer / 60f) * 0.7f;

                    if (shieldShaderTimer > 0)
                        shieldShaderTimer--;

                    int healthGateAmount = npc.lifeMax / 7;
                    if (npc.life <= npc.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount && !npc.dontTakeDamage)
                    {
                        shieldShaderTimer = 120;
                        npc.dontTakeDamage = true; //boss is immune at phase gate
                        npc.life = npc.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount - 1; //set health at phase gate
                        Main.PlaySound(SoundID.ForceRoar, npc.Center);
                    }

                    if (AttackTimer == 1) //switching out attacks
                        if (npc.dontTakeDamage) AttackPhase = 0; //nuke attack once the boss turns immortal for a chance to break a crystal
                        else //otherwise proceed with attacking pattern
                        {
                            AttackPhase++;
                            if (AttackPhase > 4) AttackPhase = 1;
                        }

                    switch (AttackPhase) //Attacks
                    {
                        case 0: MakeCrystalVulnerable(); break;
                        case 1: FireCage(); break;
                        case 2: CrystalSmash(); break;
                        case 3: SpikeMines(); break;
                        case 4: PlatformDash(); break;
                    }

                    DoRotation();

                    break;

                case (int)AIStates.Anger: //the short anger phase attack when the boss loses a crystal

                    BootlegHealthbar.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0) * 0.7f;

                    AngerAttack();
                    break;

                case (int)AIStates.FirstToSecond:

                    BootlegHealthbar.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0) * Math.Max(0, 1 - GlobalTimer / 60f) * 0.7f;

                    PhaseTransitionAnimation();
                    DoRotation();

                    break;

                case (int)AIStates.SecondPhase:

                    BootlegHealthbar.glowColor = new Color(0.9f + 0.1f * sin, 0.5f + 0.1f * sin, 0) * Math.Min(1, GlobalTimer / 60f) * 0.9f;

                    Vignette.offset = (npc.Center - Main.LocalPlayer.Center) * 0.8f;
                    Vignette.extraOpacity = 0.3f;

                    if (GlobalTimer == 60)
                    {
                        npc.dontTakeDamage = false; //damagable again
                        npc.friendly = false;
                        Vignette.visible = true;
                    }

                    if (AttackTimer == 1) //switching out attacks
                    {
                        AttackPhase++;
                        if (AttackPhase > 3)
                        {
                            if (!(AttackPhase == 4 && npc.life <= npc.lifeMax / 5)) //at low HP he can laser!
                                AttackPhase = 0;
                        }

                        altAttack = Main.rand.NextBool();
                        npc.netUpdate = true;
                    }

                    switch (AttackPhase) //switch for crystal behavior
                    {
                        case 0: if (altAttack) Darts(); else Volley(); break;
                        case 1: Mines(); break;
                        case 2: WhirlAndSmash(); break;
                        case 3: ResetPosition(); break;
                        case 4: Laser(); break;
                    }

                    DoRotation();

                    break;

                case (int)AIStates.Leaving:

                    BootlegHealthbar.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0) * Math.Max(0, 1 - GlobalTimer / 60f) * 0.7f;

                    npc.position.Y += 7;
                    Vignette.visible = false;

                    if (GlobalTimer >= 180)
                    {
                        npc.active = false; //leave
                        foreach (NPC npc in Main.npc.Where(n => n.modNPC is VitricBackdropLeft || n.modNPC is VitricBossPlatformUp)) npc.active = false; //arena reset
                    }
                    break;

                case (int)AIStates.Dying:

                    BootlegHealthbar.glowColor = new Color(0.9f + 0.1f * sin, 0.5f + 0.1f * sin, 0) * Math.Max(0, 1 - GlobalTimer / 60f) * 0.9f;

                    DeathAnimation();

                    break;
            }

            body.UpdateBody(); //update the physics on the body, last, so it can override framing
        }

		public override void ResetEffects()
		{
            rotationLocked = false;
        }

		private void DoRotation()
		{
            if (GlobalTimer % 30 == 0)
            {
                if (rotationLocked)
                    Twist(30, GetTwistDirection(lockedRotation));
                else
                    Twist(30);
            }

            if (twistTarget != 0)
            {
                float targetRot = rotationLocked ? lockedRotation : (Main.player[npc.target].Center - npc.Center).ToRotation();
                float speed = 0.07f;

                if (rotationLocked)
                    speed *= 2;

                if (twistTarget == 1)
                    npc.rotation += Helper.CompareAngle(targetRot, npc.rotation) * speed;
                if (twistTarget == -1)
                    npc.rotation += Helper.CompareAngle(targetRot + 3.14f, npc.rotation) * speed;
            }
            else
                npc.rotation = 0;
        }

        #endregion AI

        #region Networking
        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(favoriteCrystal);
            writer.Write(altAttack);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            favoriteCrystal = reader.ReadInt32();
            altAttack = reader.ReadBoolean();
        }
        #endregion Networking

        private int IconFrame = 0;
        private int IconFrameCounter = 0;
    }
}
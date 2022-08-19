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
using Terraria.Utilities;
using Terraria.GameContent.ItemDropRules;
using static StarlightRiver.Helpers.Helper;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent.Bestiary;
using StarlightRiver.Content.Biomes;
using Terraria.Audio;

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

        const int arenaWidth = 1280;
        const int arenaHeight = 884;

        public int twistTimer;
        public int maxTwistTimer;
        public int lastTwistState;
        public int twistTarget;
        public int shieldShaderTimer;

        public bool rotationLocked;
        public float lockedRotation;

        public Color glowColor = Color.Transparent;

        private int favoriteCrystal = 0;
        private bool altAttack = false;
        private int randSeed = 1923712512;
        private UnifiedRandom bossRand = new UnifiedRandom(1923712512);
        
        private List<VitricBossSwoosh> swooshes;
        private BodyHandler body;

        //Pain handler, possibly move this to a parent class at some point? Kind of a strange thing to parent for
        public float pain;
        public float painDirection;

        public Vector2 PainOffset => Vector2.UnitX.RotatedBy(painDirection) * (pain / 200f * 128);

        internal ref float GlobalTimer => ref NPC.ai[0];
        internal ref float Phase => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];

        private bool justRecievedPacket = false; //true for the frame this recieves a packet update to handle any syncronizing
        private float prevTickGlobalTimer; //since globalTimer can jump around from from to frame from recieving packets, we want to make sure we catch logic for every number in the cutscenes if it fastforwarded from a packet (reversed is ignored so we don't double up on sounds/shake)
        private float prevPhase = 0;
        private float prevAttackPhase = 0;

        public override string Texture => AssetDirectory.VitricBoss + Name;

        #region tml hooks

        public override bool CheckActive() => Phase == (int)AIStates.Leaving;

		public override ModNPC Clone(NPC npc)
		{
            var clone = base.Clone(npc) as VitricBoss;
            clone.crystals = new List<NPC>();
            clone.crystalLocations = new List<Vector2>();

            return clone;
        }

		public override void SetStaticDefaults()
        { 
            DisplayName.SetDefault("Ceiros");

            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {

            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void Load()
        {
            BodyHandler.LoadGores();
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 6000;
            NPC.damage = 30;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.width = 140;
            NPC.height = 120;
            NPC.value = Item.buyPrice(0, 40, 0, 0);
            NPC.dontTakeDamage = true;
            NPC.friendly = false;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamageFromHostiles = true;
            NPC.behindTiles = true;

            NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact");

            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/VitricBoss1");

            if (Main.netMode != NetmodeID.Server)
            {
                swooshes = new List<VitricBossSwoosh>()
                {
                new VitricBossSwoosh(new Vector2(-16, -40), 6, this),
                new VitricBossSwoosh(new Vector2(16, -40), 6, this),
                new VitricBossSwoosh(new Vector2(-46, -34), 10, this),
                new VitricBossSwoosh(new Vector2(46, -34), 10, this)
                };

                body = new BodyHandler(this);
            }
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(9000 * bossLifeScale);
            NPC.damage = 40;
            NPC.defense = 14;

            if(Main.masterMode)
			{
                NPC.lifeMax = (int)(14000 * bossLifeScale);
                NPC.damage = 60;
                NPC.defense = 14;
            }
        }

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] 
            {
                Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("Much like a regular sentinel, this sentinel works to protect settlements and remote outposts from enemies that would ruin the landscape. Unlike other sentinels, this sentinel has a rather sassy tone.")
            });
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
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    foreach (NPC npc in Main.npc.Where(n => n.ModNPC is VitricBackdropLeft || n.ModNPC is VitricBossPlatformUp))
                        npc.active = false; //reset arena      
                }
            }

            if (Phase == (int)AIStates.SecondPhase || Phase == (int)AIStates.FirstPhase)
            {
                foreach (Player Player in Main.player.Where(n => n.Hitbox.Intersects(arena)))
                {
                    Core.Systems.CameraSystem.DoPanAnimation(720, homePos);
                    Player.immuneTime = 720;
                    Player.immune = true;
                }

                foreach (NPC NPC in Main.npc.Where(n => n.ModNPC is VitricBackdropLeft || n.ModNPC is VitricBossPlatformUp)) 
                    NPC.ai[1] = 4;

                ChangePhase(AIStates.Dying, true);
                NPC.dontTakeDamage = true;
                NPC.life = 1;

                return false;
            }

            if (Phase == (int)AIStates.Dying)
                return true;

            else
                return false;
        }

        public void DrawBestiary(SpriteBatch spriteBatch, Vector2 screenPos)
		{
            NPC.frame.Width = 204;
            NPC.frame.Height = 190;

            SetFrameY(1);

            Vector2 offset;

            offset = new Vector2(60, 140);

            spriteBatch.Draw(Request<Texture2D>(Texture + "Body").Value, NPC.Center - screenPos + offset, new Rectangle(228, 0, 114, 232), Color.White, -0.1f, new Vector2(114, 232) / 2, 1, 0, 0);

            offset = new Vector2(20, 0);

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos + offset, NPC.frame, Color.White, -0.2f, NPC.frame.Size() / 2, 1, 0, 0);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if(NPC.IsABestiaryIconDummy)
			{
                DrawBestiary(spriteBatch, screenPos);
                return false;
			}

            swooshes.ForEach(n => n.Draw(spriteBatch));

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            swooshes.ForEach(n => n.DrawAdditive(spriteBatch));

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            body.DrawBody(spriteBatch);

            NPC.frame.Width = 204;
            NPC.frame.Height = 190;
            var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0;
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos + PainOffset, NPC.frame, new Color(Lighting.GetSubLight(NPC.Center)), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
            spriteBatch.Draw(Request<Texture2D>(Texture + "Glow").Value, NPC.Center - screenPos + PainOffset, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

            if (Phase == (int)AIStates.Dying) //death
            {
                var effect = Terraria.Graphics.Effects.Filters.Scene["MagmaCracks"].GetShader().Shader;
                effect.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/CrackMap").Value);
                effect.Parameters["sampleTexture3"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ProgressionMap").Value);
                effect.Parameters["uTime"].SetValue((GlobalTimer - 160) / 600f);
                effect.Parameters["drawColor"].SetValue(new Color(Lighting.GetSubLight(NPC.Center)).ToVector4());

                effect.Parameters["sourceFrame"].SetValue(new Vector4(NPC.frame.X, NPC.frame.Y, NPC.frame.Width, NPC.frame.Height));
                effect.Parameters["texSize"].SetValue(Request<Texture2D>(Texture).Value.Size());

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos + PainOffset, NPC.frame, new Color(Lighting.GetSubLight(NPC.Center)), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(Request<Texture2D>(Texture + "Godray").Value, NPC.Center - screenPos + PainOffset + new Vector2((NPC.spriteDirection == 1 ? 20 : -20), -30), null, new Color(255, 175, 100) * ((GlobalTimer - 160) / 600f), NPC.rotation, Request<Texture2D>(Texture + "Godray").Value.Size() / 2, NPC.scale, effects, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Phase == (int)AIStates.FirstPhase && NPC.dontTakeDamage) //draws the NPC's shield when immune and in the first phase
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/Shield").Value;
                var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0;

                var effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
                effect.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap").Value);
                effect.Parameters["uTime"].SetValue(2 - (shieldShaderTimer / 120f) * 2);
                effect.Parameters["sourceFrame"].SetValue(new Vector4(NPC.frame.X, NPC.frame.Y, NPC.frame.Width, NPC.frame.Height));
                effect.Parameters["texSize"].SetValue(tex.Size());

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(tex, NPC.Center - screenPos + PainOffset, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position.Y += 40;

            var spriteBatch = Main.spriteBatch;

            var tex = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBarUnder").Value;
            var texOver = Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossBarOver").Value;
            var progress = (float)NPC.life / NPC.lifeMax;

            Rectangle target = new Rectangle((int)(position.X - Main.screenPosition.X) + 2, (int)(position.Y - Main.screenPosition.Y), (int)(progress * tex.Width - 4), tex.Height);
            Rectangle source = new Rectangle(2, 0, (int)(progress * tex.Width - 4), tex.Height);

            var color = progress > 0.5f ?
                Color.Lerp(Color.Yellow, Color.LimeGreen, progress * 2 - 1) :
                Color.Lerp(Color.Red, Color.Yellow, progress * 2);

            spriteBatch.Draw(tex, position - Main.screenPosition, null, color, 0, tex.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(texOver, target, source, color, 0, tex.Size() / 2, 0, 0);

            return false;
        }

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
            npcLoot.Add(ItemDropRule.OneFromOptions(1, new int[]
            {
                ItemType<FacetAndLattice>(),
                ItemType<Coalescence>(),
                ItemType<Needler>(),
                ItemType<RefractiveBlade>(),
                ItemType<MagmiteVacpack>()
            }
            ));

            npcLoot.Add(ItemDropRule.Common(ItemType<MagmaCore>(), 1, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Misc.StaminaUp>(), 1, 1, 1));

            npcLoot.Add(ItemDropRule.Common(ItemType<Items.BarrierDye.VitricBossBarrierDye>(), 10, 1, 1));
            npcLoot.Add(ItemDropRule.Common(ItemType<Tiles.Trophies.CeirosTrophyItem>(), 10, 1, 1));
        }

		public override void BossLoot(ref string name, ref int potionType)
        {
            StarlightWorld.Flag(WorldFlags.VitricBossDowned);

            foreach (Player Player in Main.player.Where(n => n.active && arena.Contains(n.Center.ToPoint())))
            {
                Player.GetModPlayer<MedalPlayer>().ProbeMedal("Ceiros");
            }

            body.SpawnGores2();

            for(int k = 0; k < Main.rand.Next(30, 40); k++)
			{
                var rot = Main.rand.NextFloat(6.28f);
                Helper.NewItemPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 120, Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(5, 20), ItemType<VitricOre>());
            }
            for (int k = 0; k < Main.rand.Next(15, 25); k++)
            {
                var rot = Main.rand.NextFloat(6.28f);
                Helper.NewItemPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 120, Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(5, 20), ItemType<SandstoneChunk>());
            }
        }

        public override void OnHitByItem(Player Player, Item Item, int damage, float knockback, bool crit)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            if (pain > 0)
                painDirection += CompareAngle((NPC.Center - Player.Center).ToRotation(), painDirection) * Math.Min(damage / 200f, 0.5f);
            else
                painDirection = (NPC.Center - Player.Center).ToRotation();

            pain += damage;

            if (crit)
                pain += 40;
        }

        public override void OnHitByProjectile(Projectile Projectile, int damage, float knockback, bool crit)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            if (pain > 0)
                painDirection += CompareAngle((NPC.Center - Projectile.Center).ToRotation(), painDirection) * Math.Min(damage / 200f, 0.5f);
            else
                painDirection = (NPC.Center - Projectile.Center).ToRotation();

            pain += damage;

            if (crit)
                pain += 40;
        }

        #endregion tml hooks

        #region helper methods

        //Used for the various differing passive animations of the different forms
        private void SetFrameX(int frame)
        {
            NPC.frame.X = NPC.frame.Width * frame;
        }

        private void SetFrameY(int frame)
        {
            NPC.frame.Y = NPC.frame.Height * frame;
        }

        //resets animation and changes phase
        private void ChangePhase(AIStates phase, bool resetTime = false)
        {
            NPC.frame.Y = 0;
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
            int direction = Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1;

            float angle = (Main.player[NPC.target].Center - NPC.Center).ToRotation();
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
            if (!StarlightRiver.DebugMode)
                return;
        }

        public override void AI()
        {
            if (arena == new Rectangle())
                arena = new Rectangle((int)NPC.Center.X + 8 - arenaWidth / 2, (int)NPC.Center.Y - 832 - arenaHeight / 2, arenaWidth, arenaHeight);

            //find crystals
            if (crystals.Count < 4)
                findCrystals();

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
            Lighting.AddLight(NPC.Center, new Vector3(1, 0.8f, 0.4f)); //glow

            if (Phase != (int)AIStates.Leaving && Phase != (int)AIStates.Dying && arena != new Rectangle() && !Main.player.Any(n => n.active && !n.dead && arena.Contains(n.Center.ToPoint()))) //if no valid players are detected
            {
                GlobalTimer = 0;
                Phase = (int)AIStates.Leaving; //begone thot!
                crystals.ForEach(n => n.ai[2] = 4);
                crystals.ForEach(n => n.ai[1] = 0);
                NPC.netUpdate = true;
            }

            if (!BossBarOverlay.visible && Phase != (int)AIStates.Leaving && Phase != (int)AIStates.Dying && Main.netMode != NetmodeID.Server && arena.Contains(Main.LocalPlayer.Center.ToPoint()))
            {
                //in case the player joined late or something for the hp bar
                BossBarOverlay.SetTracked(NPC, ", Shattered Sentinel", ModContent.Request<Texture2D>(AssetDirectory.VitricBoss + "GUI/HealthBar", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
                BossBarOverlay.visible = true;
            }

            float sin = (float)Math.Sin(Main.GameUpdateCount * 0.1f); //health bar glow color timer

            int healthGateAmount = NPC.lifeMax / 7;

            switch (Phase)
            {
                //on spawn effects
                case (int)AIStates.SpawnEffects:

                    BuildCrystalLocations();

                    const int arenaWidth = 1280;
                    const int arenaHeight = 884;
                    arena = new Rectangle((int)NPC.Center.X + 8 - arenaWidth / 2, (int)NPC.Center.Y - 832 - arenaHeight / 2, arenaWidth, arenaHeight);

                    foreach (Player Player in Main.player.Where(n => n.active && arena.Contains(n.Center.ToPoint())))
                    {
                        Player.GetModPlayer<MedalPlayer>().QualifyForMedal("Ceiros", 1);
                    }

                    ChangePhase(AIStates.SpawnAnimation, true);
                    RebuildRandom();

                    break;

                case (int)AIStates.SpawnAnimation: //the animation that plays while the boss is spawning and the title card is shown

                    SpawnAnimation();
                    DoRotation();

                    break;

                case (int)AIStates.FirstPhase:
                 
                    BossBarOverlay.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0.2f) * Math.Min(1, GlobalTimer / 60f) * 0.7f;

                    if (shieldShaderTimer > 0)
                        shieldShaderTimer--;

                    if (NPC.life <= NPC.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount)
                    {
                        if (!NPC.dontTakeDamage)
                        {
                            shieldShaderTimer = 120;

                            NPC.life = NPC.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount - 1; //set health at phase gate
                        }

                        NPC.dontTakeDamage = true; //boss is immune at phase gate

                        RebuildRandom();
                    }

                    if (AttackTimer == 1) //switching out attacks
                    {
                        if (NPC.dontTakeDamage) //nuke attack once the boss turns immortal for a chance to break a crystal
                            AttackPhase = 0; 
                        else //otherwise proceed with attacking pattern
                        {
                            AttackPhase++;
                            altAttack = bossRand.NextBool();
                            RebuildRandom();

                            if (AttackPhase > 4)
                                AttackPhase = 1;
                        }
                    }

                    switch (AttackPhase) //Attacks
                    {
                        case 0: MakeCrystalVulnerable(); break;
                        case 1: FireCage(); break;
                        case 2: if (altAttack && BrokenCount >= 1) CrystalSmashSpaced(); else CrystalSmash(); break;
                        case 3: SpikeMines(); break;
                        case 4: if (altAttack) PlatformDashRain(); else PlatformDash(); break;
                    }

                    DoRotation();

                    break;

                case (int)AIStates.Anger: //the short anger phase attack when the boss loses a crystal

                    BossBarOverlay.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0) * 0.7f;

                    AngerAttack();
                    break;

                case (int)AIStates.FirstToSecond:


                    //health gate the anger phase still incase the player has absurd endgame damage
                    if (NPC.life <= NPC.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount)
                    {
                        if (!NPC.dontTakeDamage)
                        {
                            //first frame entering no take damage health gate so we do effects
                            shieldShaderTimer = 120;
                            NPC.life = NPC.lifeMax - (1 + crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * healthGateAmount - 1; //set health at phase gate
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                        }

                        NPC.dontTakeDamage = true; //boss is immune at phase gate TODO: If the boss gets butchered during anger attack he wont drop anything... and he can be butchered.
                    }
                    else
                        NPC.dontTakeDamage = false;

                    BossBarOverlay.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0) * Math.Max(0, 1 - GlobalTimer / 60f) * 0.7f;

                    NPC.dontTakeDamage = true;

                    PhaseTransitionAnimation();
                    DoRotation();

                    break;

                case (int)AIStates.SecondPhase:

                    BossBarOverlay.glowColor = new Color(0.9f + 0.1f * sin, 0.5f + 0.1f * sin, 0) * Math.Min(1, GlobalTimer / 60f) * 0.9f;

                    Vignette.offset = (NPC.Center - Main.LocalPlayer.Center) * 0.8f;
                    Vignette.opacityMult = 0.3f;
                    Vignette.visible = true;

                    if (GlobalTimer == 60)
                    {
                        NPC.dontTakeDamage = false; //damagable again
                        NPC.friendly = false;                    
                    }

                    if (AttackTimer == 1) //switching out attacks
                    {
                        AttackPhase++;
                        if (AttackPhase > 3)
                        {
                            if (!(AttackPhase == 4 && NPC.life <= (Main.masterMode ? NPC.lifeMax / 4 : NPC.lifeMax / 5) )) //at low HP he can laser!
                                AttackPhase = 0;
                        }

                        altAttack = bossRand.NextBool();
                        RebuildRandom();
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

                    BossBarOverlay.glowColor = new Color(0.6f + 0.1f * sin, 0.4f + 0.1f * sin, 0) * Math.Max(0, 1 - GlobalTimer / 60f) * 0.7f;

                    NPC.position.Y += 7;

                    if (GlobalTimer >= 180)
                    {
                        NPC.active = false; //leave
                        foreach (NPC NPC in Main.npc.Where(n => n.ModNPC is VitricBackdropLeft || n.ModNPC is VitricBossPlatformUp)) NPC.active = false; //arena reset
                    }
                    break;

                case (int)AIStates.Dying:

                    BossBarOverlay.glowColor = new Color(0.9f + 0.1f * sin, 0.5f + 0.1f * sin, 0) * Math.Max(0, 1 - GlobalTimer / 60f) * 0.9f;

                    DeathAnimation();

                    break;
            }

            body?.UpdateBody(); //update the physics on the body, last, so it can override framing

            if (Main.netMode == NetmodeID.Server)
            {
                //instantly switch targets if no longer valid
                Player target = Main.player[NPC.target];
                if (!target.active || target.dead || !arena.Contains(target.Center.ToPoint()))
                {
                    RandomizeTarget();
                    NPC.netUpdate = true;
                }
            }

            if (Main.netMode == NetmodeID.Server && (Phase != prevPhase || AttackPhase != prevAttackPhase))
            {
                prevPhase = Phase;
                prevAttackPhase = AttackPhase;
                NPC.netUpdate = true;
            }

            prevTickGlobalTimer = GlobalTimer; //potentially just shifted so we store the previous value in case of fastforwarding
            justRecievedPacket = false; //at end of frame set to no longer just recieved
        }

        public void findCrystals()
        {
            //finds the crystals for ceiros for mp if they haven't been found yet
            crystals.Clear(); // clear incase it was an edge case where fewer than all 4 were found on a frame since the npc spawn information hadn't arrived yet
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == ModContent.NPCType<VitricBossCrystal>() && !crystals.Contains(npc))
                    crystals.Add(npc);
            }
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
                float targetRot = rotationLocked ? lockedRotation : (Main.player[NPC.target].Center - NPC.Center).ToRotation();
                float speed = 0.07f;

                if (rotationLocked)
                    speed *= 2;

                if (twistTarget == 1)
                    NPC.rotation += CompareAngle(targetRot, NPC.rotation) * speed;
                if (twistTarget == -1)
                    NPC.rotation += CompareAngle(targetRot + 3.14f, NPC.rotation) * speed;
            }
            else
                NPC.rotation = 0;
        }

        #endregion AI

        #region Networking
        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(favoriteCrystal);
            writer.Write(altAttack);
            writer.Write(lockedRotation);

            writer.WritePackedVector2(startPos);
            writer.WritePackedVector2(endPos);
            writer.WritePackedVector2(homePos);

            writer.Write(NPC.dontTakeDamage);
            writer.Write(NPC.defense);

            writer.Write(NPC.target);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            justRecievedPacket = true;

            favoriteCrystal = reader.ReadInt32();
            altAttack = reader.ReadBoolean();
            lockedRotation = reader.ReadSingle();

            startPos = reader.ReadPackedVector2();
            endPos = reader.ReadPackedVector2();
            homePos = reader.ReadPackedVector2();

            NPC.dontTakeDamage = reader.ReadBoolean();
            NPC.defense = reader.ReadInt32();

            NPC.target = reader.ReadInt32();

            if (homePos != Vector2.Zero)
                arena = new Rectangle((int)homePos.X + 8 - arenaWidth / 2, (int)homePos.Y - 32 - arenaHeight / 2, arenaWidth, arenaHeight);
        }
        #endregion Networking

        private int IconFrame = 0;
        private int IconFrameCounter = 0;
    }
}
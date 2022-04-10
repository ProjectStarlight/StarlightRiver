using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Foregrounds;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Packets;
using Terraria.ID;
using System.IO;
using StarlightRiver.Content.NPCs.BaseTypes;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class VitricBossCrystal : ModNPC, IDrawAdditive
    {
        public Vector2 StartPos;
        public Vector2 TargetPos;
        public Vector2 prevTargetPos;
        public VitricBoss Parent;
		public bool shouldDrawArc;

		public ref float state => ref NPC.ai[0];
        public ref float timer => ref NPC.ai[1];
        public ref float phase => ref NPC.ai[2];
        public ref float altTimer => ref NPC.ai[3];

        public float prevState;
        public float prevPhase;

        public override string Texture => AssetDirectory.VitricBoss + Name;

        public override bool CheckActive() => phase == 4;

        public override bool? CanBeHitByProjectile(Projectile Projectile) => false;

        public override bool? CanBeHitByItem(Player Player, Item Item) => false;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Resonant Crystal");
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 10;
            NPC.damage = 40;
            NPC.defense = 0;
            NPC.knockBackResist = 0f;
            NPC.width = 32;
            NPC.height = 50;
            NPC.value = 0;
            NPC.friendly = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.dontTakeDamageFromHostiles = true;
            NPC.behindTiles = true;
            NPC.netAlways = true;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.damage = 50;
        }

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (phase == 2 && Parent != null && Parent.AttackTimer < 180)
                return false;

            if (phase == 3)
                return true;

            if (phase == 0 && NPC.velocity.Y <= 0) 
                return false; //can only do damage when moving downwards

            if (phase == 1 || phase == 5)
                return false;

            return !(state == 0 || state == 1); 
        }

        public bool findParent()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC NPC = Main.npc[i];
                if (NPC.active && NPC.type == ModContent.NPCType<VitricBoss>())
                {
                    Parent = NPC.ModNPC as VitricBoss;
                    return true;
                }
            }
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WritePackedVector2(StartPos);
            writer.WritePackedVector2(TargetPos);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            StartPos = reader.ReadPackedVector2();
            TargetPos = reader.ReadPackedVector2();
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state, lazy so: 0 = vulnerable, 1 = vulnerable broken, 2 = invulnerable, 3 = invulnerable broken
             * 1: timer
             * 2: phase
             * 3: alt timer
             */
            if (Parent == null)
            {
                if (!findParent())
                {
                    NPC.Kill();
                    return;
                }
            }

            if (!Parent.NPC.active)
            {
                NPC.Kill();
                return;
            }

            NPC.frame = new Rectangle(0, NPC.height * (int)state, NPC.width, NPC.height); //frame finding based on state

            timer++; //ticks the timers
            altTimer++;

            if (state == 0) //appears to be the "vulnerable" phase
            {
                if (Parent.arena.Contains(Main.LocalPlayer.Center.ToPoint()))
                {
                    Vignette.visible = true;
                    Vignette.offset = (NPC.Center - Main.LocalPlayer.Center) * 0.7f; //clientside vignette offset
                }


                if (Main.rand.Next(27) == 0)
                {
                    if (Main.rand.NextBool())
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<CrystalSparkle>(), 0, 0);
                    else
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<CrystalSparkle2>(), 0, 0);
                }

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player Player = Main.player[i];

                    if (Abilities.AbilityHelper.CheckDash(Player, NPC.Hitbox))
                    {
                        if (Parent.arena.Contains(Main.LocalPlayer.Center.ToPoint()))
                            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 20;

                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.DD2_WitherBeastCrystalImpact, (int)NPC.Center.X, (int)NPC.Center.Y);
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item70.SoundId, (int)NPC.Center.X, (int)NPC.Center.Y, Terraria.ID.SoundID.Item70.Style, 2, -0.5f);

                        Player.GetModPlayer<Abilities.AbilityHandler>().ActiveAbility?.Deactivate();
                        Player.velocity = Vector2.Normalize(Player.velocity) * -5f;

                        for (int k = 0; k < 20; k++)
                        {
                            Dust.NewDustPerfect(NPC.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8), 0, default, 2.2f); //Crystal
                            Dust.NewDustPerfect(NPC.Center, DustType<Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(150, 230, 255), 0.8f); //Crystal
                        }

                        for (int k = 0; k < 40; k++)
                            Dust.NewDustPerfect(Parent.NPC.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, 2.6f); //Boss
                    
                        if(Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient && Main.myPlayer == Player.whoAmI)
						{
                            var packet = new CeirosCrystal(Main.myPlayer, NPC.whoAmI, Parent.NPC.whoAmI, Player.velocity);
                            packet.Send(runLocally: false);
                            return;
						}
                        
                        state = 1; //It's all broken and on the floor!
                        phase = 0; //go back to doing nothing
                        timer = 0; //reset timer

                        Parent.NPC.ai[1] = (int)AIStates.Anger; //boss should go into it's angery phase
                        Parent.NPC.dontTakeDamage = false;  
                        Parent.ResetAttack();

                        NPC.netUpdate = true;

                        foreach (NPC NPC in (Parent.NPC.ModNPC as VitricBoss).crystals) //reset all our crystals to idle mode
                        {
                            phase = 0;
                            NPC.friendly = false; //damaging again
                            NPC.netUpdate = true;
                        }
                    }
                }
            }

            NPC.scale = 1; //resets scale, just incase

            switch (phase)
            {
                
                case 0: //nothing / spawning animation, sensitive to friendliness
                    if (NPC.rotation != 0) //normalize rotation
                    {
                        NPC.rotation += 0.5f;
                        if (NPC.rotation >= 5f) NPC.rotation = 0;
                    }
                    if (NPC.friendly && state != 0)
                    {
                        if (altTimer > 0 && altTimer <= 90)
                            NPC.Center = Vector2.Lerp(StartPos, TargetPos, Helper.SwoopEase(altTimer / 90f));

                        if (altTimer == 90)
                        {
                            NPC.friendly = false;
                            ResetTimers();
                            NPC.netUpdate = true;
                        }
                    }

                    break;

                case 1: //vulnerability 
                    NPC.velocity *= 0; //make sure we dont fall into oblivion

                    if (state == 0) 
                        NPC.friendly = true; //vulnerable crystal shouldnt do damage

                    if (NPC.rotation != 0) //normalize rotation
                    {
                        NPC.rotation += 0.5f;
                        if (NPC.rotation >= 5f)
                            NPC.rotation = 0;
                    }

                    if (timer > 60 && timer <= 120)
                        NPC.Center = Vector2.SmoothStep(StartPos, TargetPos, (timer - 60) / 60f); //go to the platform

                    if (timer >= 719) //when time is up... uh oh
                    {
                        if (state == 0) //only the vulnerable crystal
                        {
                            state = 2; //make invulnerable again
                            Parent.NPC.life += 250; //heal the boss
                            Parent.NPC.HealEffect(250, true);
                            Parent.NPC.dontTakeDamage = false; //make the boss vulnerable again so you can take that new 250 HP back off
                            Parent.RebuildRandom();
                            Parent.NPC.netUpdate = true;

                            for (float k = 0; k < 1; k += 0.03f) //dust visuals
                                Dust.NewDustPerfect(Vector2.Lerp(NPC.Center, Parent.NPC.Center, k), DustType<Dusts.Starlight>());
                        }

                        phase = 0; //go back to doing nothing
                        timer = 0; //reset timer
                        NPC.friendly = false; //damaging again
                    }
                    break;

                case 2: //circle attack
                    NPC.rotation = (NPC.Center - Parent.NPC.Center).ToRotation() + 1.57f; //sets the rotation appropriately for the circle attack

                    float dist = Vector2.Distance(NPC.Center, Parent.NPC.Center);

                    if (dist <= 100) //shrink the crystals for the rotation attack if they're near the boss so they properly hide in him
                        NPC.scale = Vector2.Distance(NPC.Center, Parent.NPC.Center) / 100f;
                    else NPC.scale = 1;

                    if(Parent.AttackPhase == 1 && dist > 100 && Main.rand.Next(4) == 0)
                        Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedByRandom(6.28f) * 10, DustType<Dusts.FireSparkle>(), Vector2.Zero);

                    break;

                case 3: //falling for smash attack

                    NPC.friendly = false;

                    int riseTime = 30 - Parent.BrokenCount * 4;

                    if (timer < riseTime)
                    {
                        NPC.position.Y -= (riseTime - timer) / 2.5f;
                        break;
                    }

                    NPC.velocity.Y += 0.9f;

                    if (NPC.rotation != 0) //normalize rotation
                    {
                        NPC.rotation += 0.5f;
                        if (NPC.rotation >= 5f) NPC.rotation = 0;
                    }

                    for (int k = 0; k < 3; k++)
                    {
                        var d = Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), DustType<PowerupDust>(), new Vector2(0, -Main.rand.NextFloat(3)), 0, new Color(255, 230, 100), 0.75f);
                        d.fadeIn = 10;
                    }

                    if (NPC.Center.Y > TargetPos.Y)
                    {
                        foreach (Vector2 point in Parent.crystalLocations) //Better than cycling througn Main.npc, still probably a better way to do this
                        {
                            Rectangle hitbox = new Rectangle((int)point.X - 110, (int)point.Y + 48, 220, 16); //grabs the platform hitbox
                            if (NPC.Hitbox.Intersects(hitbox))
                            {
                                NPC.position.Y = hitbox.Y - 40; //embed into the platform
                                Impact();
                            }
                        }
                    }

                    Tile tile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)(NPC.Center.Y + 24) / 16);

                    if ((tile.HasTile && tile.BlockType == BlockType.Solid) && tile.TileType != TileType<Tiles.Vitric.VitricBossBarrier>() && NPC.Center.Y > StarlightWorld.VitricBiome.Y * 16) //tile collision
                        Impact();
                    
                    break;

                case 4: //fleeing
                    shouldDrawArc = false;
                    NPC.velocity.Y += 0.7f;
                    if (timer >= 120) NPC.active = false;
                    break;

                case 5:
                case 6:

                    if (timer > 180)
                        NPC.scale = Math.Max(0, 1 - (timer - 180) / 60f);

                    break;
            }

            if (Main.netMode == NetmodeID.Server && (phase != prevPhase || state != prevState || TargetPos != prevTargetPos))
            {
                prevTargetPos = TargetPos;
                prevPhase = phase;
                prevState = state;
                NPC.netUpdate = true;
            }

        }
        
        private void Impact()
        {
            NPC.velocity *= 0;
            phase = 0; //turn it idle
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCHit42); //boom
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item70.SoundId, (int)NPC.Center.X, (int)NPC.Center.Y, Terraria.ID.SoundID.Item70.Style, 1, -1); //boom
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 17;

            if (state == 3 && Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(NPC.GetSpawnSourceForProjectileNPC(), NPC.Center, Vector2.Zero, ProjectileType<FireRingHostile>(), 20, 0, Main.myPlayer);

            for (int k = 0; k < 40; k++)
                Dust.NewDustPerfect(NPC.Center, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(7));
        }

        private void ResetTimers()
        {
            timer = 0;
            altTimer = 0;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, 0, 0);
            return false;
        }

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = Request<Texture2D>(Texture + "Glow").Value; //glowy outline
            if (state == 0)
                spriteBatch.Draw(tex, NPC.Center - screenPos, tex.Frame(), Helper.IndicatorColor, NPC.rotation, tex.Frame().Size() / 2, NPC.scale, 0, 0);

            if (phase == 3 && timer < 30)
            {
                float factor = timer / 30f;
                spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos + new Vector2(2, 0), NPC.frame, Color.White * (1 - factor), NPC.rotation, NPC.frame.Size() / 2, factor * 2 * NPC.scale, 0, 0);
            }

            spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossCrystalGlowOrange").Value, NPC.Center - screenPos, NPC.frame, Color.White * 0.8f, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, 0, 0);
            spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossCrystalGlowBlue").Value, NPC.Center - screenPos, NPC.frame, Color.White * 0.6f, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, 0, 0);

            if(phase >= 5)
			{
                spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricBoss + "VitricBossCrystalShape").Value, NPC.Center - screenPos, NPC.frame, Color.White * (timer / 120f), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, 0, 0);
            }
        }

        Trail trail;

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if(state == 0) //extra FX while vulnerable
			{
                var texGlow = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
                var pos = NPC.Center - Main.screenPosition;
                spriteBatch.Draw(texGlow, pos, null, new Color(200, 255, 255) * 0.7f * (0.9f + ((float)Math.Sin(Main.GameUpdateCount / 50f) * 0.1f)), 0, texGlow.Size() / 2, 2, 0, 0);

                var texShine = Request<Texture2D>("StarlightRiver/Assets/Keys/Shine").Value;

                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(0)), Main.GameUpdateCount / 100f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(0), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(34)), Main.GameUpdateCount / 90f + 2.2f, new Vector2(texShine.Width / 2, texShine.Height), 0.19f * GetProgress(34), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(70)), Main.GameUpdateCount / 80f + 5.4f, new Vector2(texShine.Width / 2, texShine.Height), 0.19f * GetProgress(70), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(15)), Main.GameUpdateCount / 90f + 3.14f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(15), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(98)), Main.GameUpdateCount / 100f + 4.0f, new Vector2(texShine.Width / 2, texShine.Height), 0.19f * GetProgress(98), 0, 0);
            }

            if(phase == 3)
			{
                var tex = Request<Texture2D>(AssetDirectory.VitricBoss + "GlassSpikeGlow").Value;
                var speed = NPC.velocity.Y / 15f;
                spriteBatch.Draw(tex, NPC.Center - Main.screenPosition + new Vector2(0, -45), null, new Color(255, 150, 50) * speed, -MathHelper.PiOver4, tex.Size() / 2, 3, 0, 0);
			}

            if (phase == 6 && timer > 220)
            {
                Texture2D texGlow2 = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
                Texture2D ballTex = Request<Texture2D>(AssetDirectory.VitricBoss + "FinalLaser").Value;

                var progress = Math.Min(1, ((timer - 220) / 60f));
                int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f);
                var color = new Color(255, 160 + sin, 40 + sin / 2) * progress;

                spriteBatch.Draw(texGlow2, NPC.Center - Main.screenPosition, null, color * progress, 0, texGlow2.Size() / 2, progress * 1.0f, default, default);
                spriteBatch.Draw(texGlow2, NPC.Center - Main.screenPosition, null, color * progress * 1.2f, 0, texGlow2.Size() / 2, progress * 1.6f, default, default);

                var effect1 = Filters.Scene["SunPlasma"].GetShader().Shader;
                effect1.Parameters["sampleTexture2"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallMap").Value);
                effect1.Parameters["sampleTexture3"].SetValue(Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);
                effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.01f);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(ballTex, NPC.Center - Main.screenPosition, null, Color.White * progress, 0, ballTex.Size() / 2, progress * 1.7f, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }

            if (shouldDrawArc)
			{
                var graphics = Main.graphics.GraphicsDevice;

                if (trail is null)
				{
                    trail = new Trail(graphics, 20, new NoTip(), ArcWidth, ArcColor);
				}

                Vector2[] positions = new Vector2[20];

                for (int k = 0; k < 20; k++)
                {
                    positions[k] = Parent.NPC.Center + (NPC.Center - Parent.NPC.Center).RotatedBy(k / 19f * MathHelper.PiOver2);
                    //spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)(positions[k].X - Main.screenPosition.X), (int)(positions[k].Y - Main.screenPosition.Y), 8, 8), Color.Green);
                }

                trail.Positions = positions;

                Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.ZoomMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
                effect.Parameters["time"].SetValue(Main.GameUpdateCount / 80f);
                effect.Parameters["repeats"].SetValue((1 - (Parent.AttackTimer - 360) / 480) * 4);

                effect.CurrentTechnique.Passes[0].Apply();

                trail.Render(effect);

                if(Parent.AttackTimer >= 760)
                    shouldDrawArc = false;
			}

            if(Parent != null && Parent.Phase == (float)AIStates.FirstPhase && Parent.AttackPhase == 1) //total bodge, these should draw on every crystal not just the oens that draw arcs. this detects the attack on the parent
			{
                if (Parent.AttackTimer > 360)
                {
                    float alpha = 0;

                    if (Parent.AttackTimer < 420)
                        alpha = (Parent.AttackTimer - 360) / 60f;
                    else if (Parent.AttackTimer > 720)
                        alpha = 1 - (Parent.AttackTimer - 720) / 40f;
                    else
                        alpha = 1;

                    var tex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
                    var tex2 = Request<Texture2D>(Texture + "Outline").Value;

                    spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, new Color(255, 160, 100) * alpha, 0, tex.Size() / 2, 2, 0, 0);
                    spriteBatch.Draw(tex2, NPC.Center - Main.screenPosition, null, Color.White * alpha, NPC.rotation, NPC.Size / 2, NPC.scale, 0, 0);

                    if (Parent.AttackTimer < 380)
                    {
                        float progress = (Parent.AttackTimer - 360) / 20f;
                        spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, new Color(255, 255, 150) * (4 - progress * 4), 0, tex.Size() / 2, 4 * progress, 0, 0);
                    }
                }
                else
				{
                    float alpha = 0;

                    if(Parent.AttackTimer <= 90)
                        alpha = (Parent.AttackTimer - 60) / 30f;
                    else if (Parent.AttackTimer > 300)
                        alpha = 1 - (Parent.AttackTimer - 300) / 60f;
                    else
                        alpha = 1;

                    var tex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
                    var tex2 = Request<Texture2D>(Texture + "Outline").Value;

                    spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, new Color(255, 160, 100) * alpha * 0.5f, 0, tex.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(tex2, NPC.Center - Main.screenPosition, null, Color.White * alpha * 0.8f, NPC.rotation, NPC.Size / 2, NPC.scale, 0, 0);
                }
            }
        }

        private float GetProgress(float off)
        {
            return (Main.GameUpdateCount + off * 3) % 80 / 80f;
        }

        private float ArcWidth(float progress)
		{
            float alpha = 0;

            if (Parent.AttackTimer < 420)
                alpha = (Parent.AttackTimer - 360) / 60f;//fadein speed (higher is slower)
            else if (Parent.AttackTimer > 720)
                alpha = 1 - (Parent.AttackTimer - 720) / 80f;//fadeout speed (higher is slower)
            else
                alpha = 1;

            return 36 * alpha;
        }

        private Color ArcColor(Vector2 coord)
		{
            if (coord.X > 0.95f)
                return Color.Transparent;

            float alpha;
            if (Parent.AttackTimer < 420)
                alpha = (Parent.AttackTimer - 360) / 60f;//fadein speed (higher is slower)
            else if (Parent.AttackTimer > 740)
                alpha = 1 - (Parent.AttackTimer - 740) / 30f;//fadeout speed (higher is slower)
            else
                alpha = 1;

            return Color.Lerp(new Color(255, 70, 40), new Color(255, 160, 60), (float)Math.Sin(coord.X * 6.28f + Main.GameUpdateCount / 20f)) * alpha;

            //return Color.Lerp(new Color(80, 160, 255), new Color(100, 255, 255), (float)Math.Sin(coord.X * 6.28f + Main.GameUpdateCount / 20f)) * alpha;
        }
    }
}
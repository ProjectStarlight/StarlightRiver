using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using static StarlightRiver.Content.Bosses.GlassBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Helpers;

using StarlightRiver.Core;
using StarlightRiver.Content.Foregrounds;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal class VitricBossCrystal : ModNPC, IDrawAdditive
    {
        public Vector2 StartPos;
        public Vector2 TargetPos;
        public VitricBoss Parent;
		public bool shouldDrawArc;

		public ref float state => ref npc.ai[0];
        public ref float timer => ref npc.ai[1];
        public ref float phase => ref npc.ai[2];
        public ref float altTimer => ref npc.ai[3];

        public override string Texture => AssetDirectory.GlassBoss + Name;

        public override bool CheckActive() => phase == 4;

        public override bool? CanBeHitByProjectile(Projectile projectile) => false;

        public override bool? CanBeHitByItem(Player player, Item item) => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Resonant Crystal");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.lifeMax = 2;
            npc.damage = 40;
            npc.defense = 0;
            npc.knockBackResist = 0f;
            npc.width = 32;
            npc.height = 50;
            npc.value = 0;
            npc.friendly = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.dontTakeDamage = true;
            npc.dontTakeDamageFromHostiles = true;
            npc.behindTiles = true;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.damage = 50;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (phase == 3)
                return true;

            if (phase == 0 && npc.velocity.Y <= 0) 
                return false; //can only do damage when moving downwards

            return !(state == 0 || state == 1); //too tired of dealing with this sheeeet
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state, lazy so: 0 = vulnerable, 1 = vulnerable broken, 2 = invulnerable, 3 = invulnerable broken
             * 1: timer
             * 2: phase
             * 3: alt timer
             */
            if (Parent == null) npc.Kill();
            npc.frame = new Rectangle(0, npc.height * (int)state, npc.width, npc.height); //frame finding based on state

            timer++; //ticks the timers
            altTimer++;

            if (state == 0) //appears to be the "vulnerable" phase
            {
                if(!Vignette.visible)
                    Vignette.visible = true;

                Vignette.offset = (npc.Center - Main.LocalPlayer.Center) * 0.7f; //clientside vignette offset
                

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (Abilities.AbilityHelper.CheckDash(player, npc.Hitbox))
                    {
                        Vignette.visible = false; //disable overlay
                        //Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = npc.Center;
                        //Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 60;
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 20;

                        Main.PlaySound(Terraria.ID.SoundID.DD2_WitherBeastCrystalImpact);
                        Main.PlaySound(Terraria.ID.SoundID.Item70.SoundId, (int)npc.Center.X, (int)npc.Center.Y, Terraria.ID.SoundID.Item70.Style, 2, -0.5f);

                        player.GetModPlayer<Abilities.AbilityHandler>().ActiveAbility?.Deactivate();
                        player.velocity = Vector2.Normalize(player.velocity) * -5f;

                        for (int k = 0; k < 20; k++)
                        {
                            Dust.NewDustPerfect(npc.Center, DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8), 0, default, 2.2f); //Crystal
                            Dust.NewDustPerfect(npc.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(150, 230, 255), 0.8f); //Crystal
                        }

                        for (int k = 0; k < 40; k++)
                            Dust.NewDustPerfect(Parent.npc.Center, DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, default, 2.6f); //Boss

                        for (int k = 0; k < 5; k++) 
                            Gore.NewGore(Parent.npc.Center, Vector2.One.RotatedBy(k / 4f * 6.28f) * 4, mod.GetGoreSlot("Gores/ShieldGore"));

                        state = 1; //It's all broken and on the floor!
                        phase = 0; //go back to doing nothing
                        timer = 0; //reset timer

                        Parent.npc.ai[1] = (int)AIStates.Anger; //boss should go into it's angery phase
                        Parent.ResetAttack();

                        foreach (NPC npc in (Parent.npc.modNPC as VitricBoss).crystals) //reset all our crystals to idle mode
                        {
                            phase = 0;
                            npc.friendly = false; //damaging again
                        }
                    }
                }
            }

            switch (phase)
            {
                case 0: //nothing / spawning animation, sensitive to friendliness
                    if (npc.rotation != 0) //normalize rotation
                    {
                        npc.rotation += 0.5f;
                        if (npc.rotation >= 5f) npc.rotation = 0;
                    }
                    if (npc.friendly && state != 0)
                    {
                        if (altTimer > 0 && altTimer <= 90)
                            npc.Center = Vector2.SmoothStep(StartPos, TargetPos, altTimer / 90);
                        if (altTimer == 90)
                        {
                            npc.friendly = false;
                            ResetTimers();
                        }
                    }
                    npc.scale = 1; //resets scale, just incase

                    break;

                case 1: //nuke attack
                    npc.velocity *= 0; //make sure we dont fall into oblivion

                    if (state == 0) 
                        npc.friendly = true; //vulnerable crystal shouldnt do damage

                    if (npc.rotation != 0) //normalize rotation
                    {
                        npc.rotation += 0.5f;
                        if (npc.rotation >= 5f)
                            npc.rotation = 0;
                    }

                    if (timer > 60 && timer <= 120)
                        npc.Center = Vector2.SmoothStep(StartPos, TargetPos, (timer - 60) / 60f); //go to the platform

                    if (timer >= 719) //when time is up... uh oh
                    {
                        if (state == 0) //only the vulnerable crystal
                        {
                            state = 2; //make invulnerable again
                            Parent.npc.life += 250; //heal the boss
                            Parent.npc.HealEffect(250, true);
                            Parent.npc.dontTakeDamage = false; //make the boss vulnerable again so you can take that new 250 HP back off

                            Vignette.visible = false; //reset vignette

                            for (float k = 0; k < 1; k += 0.03f) //dust visuals
                                Dust.NewDustPerfect(Vector2.Lerp(npc.Center, Parent.npc.Center, k), DustType<Dusts.Starlight>());
                        }

                        phase = 0; //go back to doing nothing
                        timer = 0; //reset timer
                        npc.friendly = false; //damaging again
                    }
                    break;

                case 2: //circle attack
                    npc.rotation = (npc.Center - Parent.npc.Center).ToRotation() + 1.57f; //sets the rotation appropriately for the circle attack

                    if (Vector2.Distance(npc.Center, Parent.npc.Center) <= 100) //shrink the crystals for the rotation attack if they're near the boss so they properly hide in him
                        npc.scale = Vector2.Distance(npc.Center, Parent.npc.Center) / 100f;
                    else npc.scale = 1;

                    break;

                case 3: //falling for smash attack

                    npc.friendly = false;

                    if (timer < 30)
                    {
                        npc.position.Y -= (30 - timer) / 2.5f;
                        break;
                    }

                    npc.velocity.Y += 0.9f;

                    if (npc.rotation != 0) //normalize rotation
                    {
                        npc.rotation += 0.5f;
                        if (npc.rotation >= 5f) npc.rotation = 0;
                    }

                    for (int k = 0; k < 3; k++)
                    {
                        var d = Dust.NewDustPerfect(npc.Center + new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), DustType<PowerupDust>(), new Vector2(0, -Main.rand.NextFloat(3)), 0, new Color(255, 230, 100), 0.75f);
                        d.fadeIn = 10;
                    }

                    if (npc.Center.Y > TargetPos.Y)
                        foreach (Vector2 point in Parent.crystalLocations) //Better than cycling througn Main.npc, still probably a better way to do this
                        {
                            Rectangle hitbox = new Rectangle((int)point.X - 110, (int)point.Y + 48, 220, 16); //grabs the platform hitbox
                            if (npc.Hitbox.Intersects(hitbox))
                            {
                                npc.position.Y = hitbox.Y - 40; //embed into the platform
                                Impact();
                            }
                        }

                    Tile tile = Framing.GetTileSafely((int)npc.Center.X / 16, (int)(npc.Center.Y + 24) / 16);

                    if (tile.collisionType == 1 && tile.type != TileType<Tiles.Vitric.VitricBossBarrier>() && npc.Center.Y > StarlightWorld.VitricBiome.Y * 16) //tile collision
                        Impact();
                    
                    break;

                case 4: //fleeing
                    npc.velocity.Y += 0.7f;
                    if (timer >= 120) npc.active = false;
                    break;

                case 5: //transforming the boss 
                        //TODO: Give this a new animation
                    break;
            }
        }
        
        private void Impact()
        {
            npc.velocity *= 0;
            phase = 0; //turn it idle
            Main.PlaySound(Terraria.ID.SoundID.NPCHit42); //boom
            Main.PlaySound(Terraria.ID.SoundID.Item70.SoundId, (int)npc.Center.X, (int)npc.Center.Y, Terraria.ID.SoundID.Item70.Style, 1, -1); //boom
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 17;

            for (int k = 0; k < 40; k++)
                Dust.NewDustPerfect(npc.Center, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(7));
        }

        private void ResetTimers()
        {
            timer = 0;
            altTimer = 0;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D tex = GetTexture(Texture + "Glow"); //glowy outline
            if (state == 0)
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition + new Vector2(0, 4), tex.Frame(), Helper.IndicatorColor, npc.rotation, tex.Frame().Size() / 2, npc.scale, 0, 0);

            if (phase == 3 && timer < 30)
            {
                float factor = timer / 30f;
                spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition + new Vector2(2, 0), npc.frame, Color.White * (1 - factor), npc.rotation, npc.frame.Size() / 2, factor * 2, 0, 0);
            }
        }

        Trail trail;

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if(state == 0) //extra FX while vulnerable
			{
                var texGlow = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
                var pos = npc.Center - Main.screenPosition;
                spriteBatch.Draw(texGlow, pos, null, new Color(200, 255, 255) * 0.7f * (0.9f + ((float)Math.Sin(Main.GameUpdateCount / 50f) * 0.1f)), 0, texGlow.Size() / 2, 2, 0, 0);

                var texShine = GetTexture("StarlightRiver/Assets/Keys/Shine");

                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(0)), Main.GameUpdateCount / 100f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(0), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(34)), Main.GameUpdateCount / 90f + 2.2f, new Vector2(texShine.Width / 2, texShine.Height), 0.19f * GetProgress(34), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(70)), Main.GameUpdateCount / 80f + 5.4f, new Vector2(texShine.Width / 2, texShine.Height), 0.19f * GetProgress(70), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(15)), Main.GameUpdateCount / 90f + 3.14f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(15), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(200, 255, 255) * 0.5f * (1 - GetProgress(98)), Main.GameUpdateCount / 100f + 4.0f, new Vector2(texShine.Width / 2, texShine.Height), 0.19f * GetProgress(98), 0, 0);
            }

            if(phase == 3)
			{
                var tex = GetTexture(AssetDirectory.GlassBoss + "GlassSpikeGlow");
                var speed = npc.velocity.Y / 15f;
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition + new Vector2(0, -45), null, new Color(255, 150, 50) * speed, -MathHelper.PiOver4, tex.Size() / 2, 3, 0, 0);
			}

            if(shouldDrawArc)
			{
                var graphics = Main.graphics.GraphicsDevice;

                if (trail is null)
				{
                    trail = new Trail(graphics, 20, new NoTip(), ArcWidth, ArcColor);
				}

                Vector2[] positions = new Vector2[20];

                for (int k = 0; k < 20; k++)
                {
                    positions[k] = Parent.npc.Center + (npc.Center - Parent.npc.Center).RotatedBy(k / 19f * MathHelper.PiOver2);
                }

                trail.Positions = positions;

                Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.ZoomMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/EnergyTrail"));
                effect.Parameters["time"].SetValue(Main.GameUpdateCount / 80f);
                effect.Parameters["repeats"].SetValue((1 - (Parent.AttackTimer - 360) / 480) * 4);

                effect.CurrentTechnique.Passes[0].Apply();

                trail.Render(effect);

                shouldDrawArc = false;
			}

            if(Parent.Phase == (float)AIStates.FirstPhase && Parent.AttackPhase == 1 && Parent.AttackTimer > 360) //total bodge shitcode, these should draw on every crystal not just the oens that draw arcs. this detects the attack on the parent
			{
                float alpha = 0;

                if (Parent.AttackTimer < 420)
                    alpha = (Parent.AttackTimer - 360) / 60f;
                else if (Parent.AttackTimer > 760)
                    alpha = 1 - (Parent.AttackTimer - 760) / 80f;
                else
                    alpha = 1;

                var tex = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
                var tex2 = GetTexture(Texture + "Outline");

                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, new Color(255, 160, 100) * alpha, 0, tex.Size() / 2, 2, 0, 0);
                spriteBatch.Draw(tex2, npc.Center - Main.screenPosition, null, new Color(255, 160, 100) * alpha * 0.5f, npc.rotation, tex2.Size() / 2, 1, 0, 0);

                if (Parent.AttackTimer < 380)
                {
                    float progress = (Parent.AttackTimer - 360) / 20f;
                    spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, new Color(255, 255, 150) * (4 - progress * 4), 0, tex.Size() / 2, 4 * progress, 0, 0);
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
                alpha = (Parent.AttackTimer - 360) / 60f;
            else if (Parent.AttackTimer > 760)
                alpha = 1 - (Parent.AttackTimer - 760) / 80f;
            else
                alpha = 1;

            return 36 * alpha;
        }

        private Color ArcColor(Vector2 coord)
		{
            float alpha = 0;

            if (Parent.AttackTimer < 420)
                alpha = (Parent.AttackTimer - 360) / 60f;
            else if (Parent.AttackTimer > 760)
                alpha = 1 - (Parent.AttackTimer - 760) / 80f;
            else
                alpha = 1;

            return Color.Lerp(new Color(255, 70, 40), new Color(255, 160, 60), (float)Math.Sin(coord.X * 6.28f + Main.GameUpdateCount / 20f)) * alpha;

            //return Color.Lerp(new Color(80, 160, 255), new Color(100, 255, 255), (float)Math.Sin(coord.X * 6.28f + Main.GameUpdateCount / 20f)) * alpha;
        }
    }
}
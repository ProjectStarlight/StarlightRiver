using ReLogic.Content;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class EchochainWhip : ModItem
	{
		internal int cooldown;
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain");
			Tooltip.SetDefault("Chains enemies together, sharing summon damage between them\nHold and release <right> to snare enemies near your mouse to the ground, chaining all effected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<EchochainWhipProjectile>(), 15, 3f, 3.75f, 40);
			Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));
		}

		public override bool AltFunctionUse(Player player)
		{
			return cooldown <= 0;
		}

		public override void UpdateInventory(Player player)
		{
			if (cooldown > 0)
				cooldown--;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.UseSound = SoundID.DD2_BetsyFireballImpact;
				Item.useStyle = ItemUseStyleID.Shoot;
			}
			else
			{
				Item.UseSound = SoundID.Item152;
				Item.useStyle = ItemUseStyleID.Swing;
			}

			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2 && cooldown <= 0)
			{
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<EchochainWhipAltProjectile>(), damage, knockback, player.whoAmI);
				cooldown = 120;
				return false;
			}

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.GoldBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.PlatinumBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class EchochainWhipAltProjectile : ModProjectile
	{
		public Point16?[] tiles = new Point16?[10];
		public NPC[] targets = new NPC[10];

		public Vector2 OwnerMouse => Owner.GetModPlayer<ControlsPlayer>().mouseWorld;
		public Player Owner => Main.player[Projectile.owner];

		public bool CanHold => Owner.GetModPlayer<ControlsPlayer>().mouseRight && !Owner.CCed && !Owner.noItems;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain Aura");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override bool? CanDamage()
		{
			return false;
		}

		public override bool PreAI()
		{ 
			Projectile.Center = Vector2.Lerp(Projectile.Center, OwnerMouse, 0.1f);
			PopulateTilesAndTargets();
			return true;
		}

		public override void AI()
		{
			Projectile.rotation = Owner.DirectionTo(OwnerMouse).ToRotation();

			if (!CanHold)
			{
				Owner.itemTime = 0;
				Owner.itemAnimation = 0;
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0f); // gotta set the composite arm here again otherwise the players front arm appears out for a frame and it looks bad
				Projectile.Kill();

				var targetsToChain = new List<NPC>();

				for (int i = 0; i < targets.Length; i++)
				{
					if (tiles[i] == null || targets[i] != null && targets[i].active)
						targetsToChain.Add(targets[i]);
				}

				if (targetsToChain.Count >= 2)
					EchochainSystem.AddNPCS(targetsToChain);

				for (int i = 0; i < tiles.Length; i++)
				{
					if (tiles[i] == null || targets[i] == null)
						return;

					int[] frames = new int[17];
					for (int a = 0; a < frames.Length; a++) // populate array
					{
						frames[a] = Main.rand.Next(3);
					}

					Vector2 pos = new Vector2(tiles[i].Value.X * 16, tiles[i].Value.Y * 16);
					var proj = Projectile.NewProjectileDirect(null, pos, Vector2.Zero, ModContent.ProjectileType<EchochainWhipAltProjectileChain>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

					(proj.ModProjectile as EchochainWhipAltProjectileChain).target = targets[i];
					(proj.ModProjectile as EchochainWhipAltProjectileChain).tilePosition = pos;
					(proj.ModProjectile as EchochainWhipAltProjectileChain).chainFrames = frames;
				}

				return;
			}

			Owner.ChangeDir(OwnerMouse.X < Owner.Center.X ? -1 : 1);
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Projectile.timeLeft = 2;

			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -2f * Owner.direction);
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0f);

			for (int i = 0; i < tiles.Length; i++)
			{
				if (tiles[i] == null)
					return;

				Vector2 pos = new Vector2(tiles[i].Value.X * 16, tiles[i].Value.Y * 16);
				pos += new Vector2(12f, 0f);

				if (Main.rand.NextBool(5))
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f) + Vector2.UnitY * -Main.rand.NextFloat(2f), 0, new Color(100, 200, 10), 0.35f);

				if (Main.rand.NextBool(5))
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f) + Vector2.UnitY * -Main.rand.NextFloat(3f), 0, new Color(150, 255, 25), 0.2f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		private void PopulateTilesAndTargets()
		{
			for (int i = 0; i < tiles.Length; i++)
			{
				tiles[i] = null;
				targets[i] = null;
			}

			Vector2 startPos = Projectile.Center + new Vector2(-50, 0);

			for (int x = 0; x < 10; x++)
			{
				int index = x;

				for (int y = 0; y < 10; y++) // search 10 tiles down
				{
					Vector2 worldPos = startPos + new Vector2(16f * x, y * 16f);
					Point16 tilePos = new Point16((int)worldPos.X / 16, (int)worldPos.Y / 16);
					Tile tile = Framing.GetTileSafely(tilePos);
					if (tile.HasTile && WorldGen.SolidOrSlopedTile(tile) && !tiles.Contains(tilePos))
					{
						targets[index] = Main.npc.Where(n => n.active && n.CanBeChasedBy() && n.Distance(worldPos) < 250f && !targets.Contains(n)).OrderBy(n => n.Distance(worldPos)).FirstOrDefault();
						tiles[index] = tilePos;
						Main.NewText(y);
						break;
					}				
				}
			}
		}
	}

	public class EchochainWhipAltProjectileChain : ModProjectile
	{
		public int[] chainFrames;

		public Vector2 tilePosition;

		public NPC target;

		public ref float StabTimer => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echo Chain");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 300;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (target == null || !target.active || tilePosition.Distance(target.Center) > 350f)
			{
				Projectile.Kill();
				return;
			}

			if (StabTimer < 60f)
				StabTimer++;

			float progress = StabTimer / 60f;

			if (progress < 0.5f)
			{
				Projectile.Center = Vector2.Lerp(tilePosition + new Vector2(0f, -20f), target.Center, EaseBuilder.EaseQuarticInOut.Ease(StabTimer / 30f));
			}
			else
			{
				Projectile.Center = Vector2.Lerp(target.Center, tilePosition + new Vector2(0f, -20f), EaseBuilder.EaseQuarticIn.Ease((StabTimer - 30f) / 30f));
				if (target.knockBackResist > 0f)
					target.Center = Projectile.Center;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (target == null || !target.active)
				return false;

			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Blur").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Vector2 chainStart = tilePosition;
			Vector2 chainEnd = Projectile.Center;

			float rotation = chainStart.DirectionTo(chainEnd).ToRotation() + MathHelper.PiOver2;

			float distance = Vector2.Distance(chainStart, chainEnd);

			for (int i = 0; i < (distance / 22); i += 1)
			{
				var pos = Vector2.Lerp(chainStart, chainEnd, i * 22 / distance);

				spriteBatch.Draw(bloomTex, pos - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f, 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

				Rectangle frame = tex.Frame(verticalFrames: 5, frameY: chainFrames[i]);
				spriteBatch.Draw(tex, pos - Main.screenPosition, frame, Color.White, rotation, frame.Size() / 2f, 1f, 0f, 0f);

				frame = texBlur.Frame(verticalFrames: 5, frameY: chainFrames[i]);
				spriteBatch.Draw(texBlur, pos - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f, rotation, frame.Size() / 2f, 1f, 0f, 0f);
			}

			return false;
		}
	}

	public class EchochainWhipProjectile : BaseWhip
	{
		public List<Vector2> tipPositions = new();
		public List<float> tipRotations = new();

		public List<NPC> hitTargets = new();
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public EchochainWhipProjectile() : base("Echochain", 40, 1.25f, new Color(150, 255, 20) * 0.5f) { }

		public override void ArcAI()
		{
			if (Projectile.ai[0] > flyTime * 0.45f)
			{
				var points = new List<Vector2>();
				points.Clear();
				SetPoints(points);

				Vector2 tipPos = points[39];

				tipPositions.Add(tipPos);
				if (tipPositions.Count > 15)
					tipPositions.RemoveAt(0);

				Vector2 difference = points[39] - points[38];
				float rotation = difference.ToRotation() - MathHelper.PiOver2;

				tipRotations.Add(rotation);
				if (tipRotations.Count > 15)
					tipRotations.RemoveAt(0);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(100, 200, 10), 0.5f);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(150, 255, 25), 0.35f);
			}
		}

		public override void DrawBehindWhip(ref Color lightColor)
		{
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_TipBlur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_TipGlow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
			Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
			int height = whipFrame.Height;

			float fadeOut = 1f;
			if (Projectile.ai[0] > flyTime * 0.4f && Projectile.ai[0] < flyTime * 0.9f)
				fadeOut *= 1f - (Projectile.ai[0] - flyTime * 0.4f) / (flyTime * 0.5f);
			else if (Projectile.ai[0] >= flyTime * 0.9f)
				fadeOut = 0f;

			for (int i = 15; i > 0; i--)
			{
				float fade = 1 - (15f - i) / 15f;

				if (i > 0 && i < tipPositions.Count)
				{
					whipFrame.Y = height * 4;
					var color = Color.Lerp(new Color(20, 135, 15, 0), new Color(100, 200, 10, 0), fade);
					Main.EntitySpriteDraw(texture.Value, tipPositions[i] - Main.screenPosition, whipFrame, Color.White * fade * fadeOut, tipRotations[i], whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

					Main.spriteBatch.Draw(texBlur, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut, tipRotations[i], texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut * 0.5f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut * 0.6f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);

					if (i < 15 && i + 1 < tipPositions.Count)
					{
						var newPosition = Vector2.Lerp(tipPositions[i], tipPositions[i + 1], 0.5f);
						float newRotation = MathHelper.Lerp(tipRotations[i], tipRotations[i + 1], 0.5f);

						Main.EntitySpriteDraw(texture.Value, newPosition - Main.screenPosition, whipFrame, Color.White * fade * fadeOut, newRotation, whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

						Main.spriteBatch.Draw(texBlur, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut, newRotation, texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut * 0.5f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut * 0.6f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);
					}
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitTargets.Add(target);

			var points = new List<Vector2>();
			points.Clear();
			SetPoints(points);

			Vector2 tipPos = points[39];

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(tipPos).RotatedByRandom(0.5f) * Main.rand.NextFloat(5f, 10f), 0, new Color(100, 200, 20), 0.5f);
			}
		}

		public override void Kill(int timeLeft)
		{
			if (hitTargets.Count >= 2)
				EchochainSystem.AddNPCS(hitTargets);

			hitTargets.Clear();
		}
	}

	/// <summary>
	/// This system acts as a handler for the echo chain damage graph
	/// </summary>
	class EchochainSystem : ModSystem
	{
		// These represent the graph for the echo chain's connection. Essentially
		// every NPC acts as a node, and are connected by edges which are created
		// when NPCs are struck by the weapon. Damage sharing is handled via a
		// graph traversal, and chain expiration happens as a simple iteration over
		// the edges collection and removing expired edges.
		public static List<EchochainNode> nodes = new();
		public static List<EchochainEdge> edges = new();

		public const int MAX_EDGE_TIMER = 300; // the maximum timer for each edge.

		public override void Load()
		{
			StarlightNPC.OnHitByItemEvent += OnHitChains;
			StarlightNPC.OnHitByProjectileEvent += OnHitChainsProj;

			On_Main.DrawNPCs += DrawEdges;
		}

		/// <summary>
		/// Calls the DrawEdge() method for all active edges, in the Main.DrawNPCs() hook
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="behindTiles"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void DrawEdges(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			foreach (EchochainEdge edge in edges)
			{
				edge.DrawEdge(Main.spriteBatch);
			}

			orig(self, behindTiles);
		}

		/// <summary>
		/// Triggers a traversal over the graph to damage all enemies connected to the hit enemy
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="hit"></param>
		/// <param name="damageDone"></param>
		private void OnHitChains(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			ResetTimers(npc);

			if (!item.CountsAsClass(DamageClass.Summon))
				return;

			//Traverse(npc, (n) => BuffInflictor.InflictStack<EchochainDamageDebuff, EchochainDamageStack>(n, 30, new EchochainDamageStack() { duration = 30, damage = hit.SourceDamage}), true);

			Traverse(npc, (n) =>
			{
				n.SimpleStrikeNPC(hit.SourceDamage, 0);

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(120, 255, 40), 0.65f);

					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2CircularEdge(5f, 5f), 0, new Color(120, 255, 40), 0.65f);
				}

				Dust.NewDustPerfect(n.Center, ModContent.DustType<EchochainBurstDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.5f, 0.75f));

				Helper.PlayPitched("Magic/Shadow1", 0.5f, 0f, npc.Center);
				CameraSystem.shake += 2;
			}, true);
		}

		/// <summary>
		/// Triggers a traversal over the graph to damage all enemies connected to the hit enemy
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projectile"></param>
		/// <param name="hit"></param>
		/// <param name="damageDone"></param>
		private void OnHitChainsProj(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			ResetTimers(npc);

			if (ProjectileID.Sets.IsAWhip[projectile.type] || !projectile.CountsAsClass(DamageClass.Summon)) // only minions should proc the chain hits
				return;

			//Traverse(npc, (n) => BuffInflictor.InflictStack<EchochainDamageDebuff, EchochainDamageStack>(n, 30, new EchochainDamageStack() { duration = 30, damage = hit.SourceDamage }), true);

			Traverse(npc, (n) =>
			{
				n.SimpleStrikeNPC(hit.SourceDamage, 0);

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(120, 255, 40), 0.65f);

					Dust.NewDustPerfect(n.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2CircularEdge(5f, 5f), 0, new Color(120, 255, 40), 0.65f);
				}

				Dust.NewDustPerfect(n.Center, ModContent.DustType<EchochainBurstDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.5f, 0.75f));

				Helper.PlayPitched("Magic/Shadow1", 0.5f, 0f, npc.Center);
				if (CameraSystem.shake < 6)
					CameraSystem.shake += 2;

			}, true);
		}

		public override void PreUpdateDusts()
		{
			List<EchochainEdge> edgesToRemove = new();

			foreach (EchochainEdge edge in edges)
			{
				edge.UpdateEdge();
				// removes ALL chains in which theyre timer has ran out, one of their npcs is inactive, one of their npcs is too far away from another, or somehow both of the edges npcs are the same.
				if (edge.timer <= 0 || !edge.end.npc.active || edge.end == null || !edge.start.npc.active || edge.start == null || Vector2.Distance(edge.start.npc.Center, edge.end.npc.Center) > 500f || edge.start == edge.end)
				{
					edgesToRemove.Add(edge);
				}
			}

			// this prevents "Collection was modified, enumeration may not execute"
			foreach (EchochainEdge edge in edgesToRemove)
			{
				edges.Remove(edge);
				edge.DestroyEdge();
			}
		}

		/*public override void PostUpdateEverything()
		{
			List<EchochainEdge> edgesToRemove = new();

			foreach (EchochainEdge edge in edges)
			{
				edge.UpdateEdge();
				// removes ALL chains in which theyre timer has ran out, one of their npcs is inactive, one of their npcs is too far away from another, or somehow both of the edges npcs are the same.
				if (edge.timer <= 0 || !edge.end.npc.active || edge.end == null || !edge.start.npc.active || edge.start == null || Vector2.Distance(edge.start.npc.Center, edge.end.npc.Center) > 500f || edge.start == edge.end)
				{
					edgesToRemove.Add(edge);
				}
			}

			// this prevents "Collection was modified, enumeration may not execute"
			foreach (EchochainEdge edge in edgesToRemove)
			{
				edges.Remove(edge);
				edge.DestroyEdge();
			}
		}*/

		/// <summary>
		/// This function will create appropriate nodes and edges for NPCs when they are
		/// struck by the weapon. All NPCs passed not currently connected will have connections
		/// added to them.
		/// </summary>
		/// <param name="npcsToAdd"></param>
		public static void AddNPCS(List<NPC> npcsToAdd)
		{
			List<EchochainNode> list = new();

			for (int i = 0; i < npcsToAdd.Count; i++)
			{
				NPC npc = npcsToAdd[i];

				if (npc.active) // don't add dead npcs to the list silly!
				{
					// First we check if a node already exists for this NPC, and if not, add it
					EchochainNode node = nodes.FirstOrDefault(n => n.npc == npc);

					if (node == null)
					{
						node = new EchochainNode(new List<EchochainEdge>(), npc);
						nodes.Add(node);
					}

					list.Add(node);
				}
			}

			// Now we check all unordered permutations of 2 NPCs, and generate chains between
			// them if they do not already exist
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = i; j < list.Count; j++)
				{
					// This is likely very slightly inefficient over performing the equality
					// check here, but this is sparsely run and this is far more readable

					int[] frames = new int[24];
					for (int a = 0; a < frames.Length; a++) // populate array
					{
						frames[a] = Main.rand.Next(3);
					}

					var potential = new EchochainEdge(list[i], list[j], MAX_EDGE_TIMER, frames);

					if (!edges.Any(n => n.Equals(potential)) && list[i] != list[j]) // check if it already exists, and if we are trying to add an edge that consists of the same npc, twice
					{
						edges.Add(potential);

						// We append the refferences to this edge to the nodes it is connecting aswell
						nodes.First(n => n == list[i]).edges.Add(potential);
						nodes.First(n => n == list[j]).edges.Add(potential);

						for (int d = 0; d < 20; d++)
						{
							Dust.NewDustPerfect(Vector2.Lerp(list[i].npc.Center, list[j].npc.Center, d / 20f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(3f, 3f), 0, new Color(150, 200, 20), 0.5f);
						}
					}
				}
			}
		}

		/// <summary>
		/// Triggers a DFS traversal of the graph starting with the node containing the given
		/// NPC, and performing the given action on any NPC connected.
		/// </summary>
		/// <param name="start">The NPC where the traversal should start from. If they are not present in the node colleciton, nothing happens</param>
		/// <param name="action">The action to be performed on every connected NPC</param>
		/// <param name="ignoreFirst">If the NPC passed should ignore the action passed or not</param>
		public static void Traverse(NPC start, Action<NPC> action, bool ignoreFirst = false)
		{
			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == start);

			// If the start isnt in the graph, give up
			if (startNode is null)
				return;

			Stack<EchochainNode> stack = new();
			HashSet<EchochainNode> visited = new();

			stack.Push(startNode);

			while (stack.Count > 0)
			{
				EchochainNode node = stack.Pop();

				if (!visited.Contains(node))
				{
					if (!(ignoreFirst && node == startNode))
						action(node.npc);

					foreach (EchochainEdge edge in node.edges)
					{
						EchochainNode adjNode = edge.start == node ? edge.end : edge.start;

						if (!visited.Contains(adjNode))
							stack.Push(adjNode);
					}

					visited.Add(node);
				}
			}
		}

		/// <summary>
		/// Helper Method which performs a DFS traversal of the graph, starting at start.
		/// </summary>
		/// <param name="start">The NPC where the traversal should start from. If they are not present in the node collection, nothing happens</param>
		public static void ResetTimers(NPC start)
		{
			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == start);

			// If the start isnt in the graph, give up
			if (startNode is null)
				return;

			Stack<EchochainNode> stack = new();
			HashSet<EchochainNode> visited = new();

			stack.Push(startNode);

			while (stack.Count > 0)
			{
				EchochainNode node = stack.Pop();

				if (!visited.Contains(node))
				{
					foreach (EchochainEdge edge in node.edges)
					{
						edge.timer = MAX_EDGE_TIMER; // reset the timer

						EchochainNode adjNode = edge.start == node ? edge.end : edge.start;

						if (!visited.Contains(adjNode))
							stack.Push(adjNode);
					}

					visited.Add(node);
				}
			}
		}

		/// <summary>
		/// Removes an NPC from the node list, and disconnects all edges connected to it.
		/// </summary>
		/// <param name="toRemove">The NPC to be removed</param>
		public static void RemoveNPC(NPC toRemove)
		{
			nodes.RemoveAll(n => n.npc == toRemove);
			edges.RemoveAll(n => n.start.npc == toRemove || n.end.npc == toRemove);
		}
	}

	/// <summary>
	/// Represents an edge in the echo chain graph, essentially a chain between two NPCs
	/// </summary>
	class EchochainEdge
	{
		public EchochainNode start;
		public EchochainNode end;
		public int timer;
		public int fadeInTimer;

		public int[] chainFrames; // frames for the chain link are randomized upon creation

		internal List<Vector2> cache;
		internal Trail trail;
		public EchochainEdge(EchochainNode start, EchochainNode end, int timer, int[] chainFrames)
		{
			this.start = start;
			this.end = end;
			this.timer = timer;
			this.chainFrames = chainFrames;
			fadeInTimer = 30;
		}

		public void UpdateEdge()
		{
			timer--;
			if (fadeInTimer > 0)
				fadeInTimer--;

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public void DestroyEdge()
		{
			start.edges.Remove(this);
			end.edges.Remove(this);

			for (int d = 0; d < 20; d++)
			{
				Dust.NewDustPerfect(Vector2.Lerp(start.npc.Center, end.npc.Center, d / 20f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(3f, 3f), 0, new Color(150, 200, 20), 0.5f);
			}
		}

		public void DrawEdge(SpriteBatch spriteBatch)
		{
			if (!start.npc.active || !end.npc.active || start.npc == null || end.npc == null)
				return;

			DrawPrimitives(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Blur").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Vector2 chainStart = start.npc.Center;
			Vector2 chainEnd = end.npc.Center;

			float rotation = chainStart.DirectionTo(chainEnd).ToRotation() + MathHelper.PiOver2;

			float distance = Vector2.Distance(chainStart, chainEnd);

			float fadeIn = 1f - fadeInTimer / 30f;

			for (int i = 0; i < (distance / 22); i += 1)
			{
				var pos = Vector2.Lerp(chainStart, chainEnd, i * 22 / distance);

				spriteBatch.Draw(bloomTex, pos - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f, 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

				Rectangle frame = tex.Frame(verticalFrames: 5, frameY: chainFrames[i]);	
				spriteBatch.Draw(tex, pos - Main.screenPosition, frame, Color.White * fadeIn, rotation, frame.Size() / 2f, 1f, 0f, 0f);

				frame = texBlur.Frame(verticalFrames: 5, frameY: chainFrames[i]);
				spriteBatch.Draw(texBlur, pos - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f * fadeIn, rotation, frame.Size() / 2f, 1f, 0f, 0f);
			}

			spriteBatch.Draw(bloomTex, chainStart - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f * fadeIn, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
			spriteBatch.Draw(bloomTex, chainEnd - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.1f * fadeIn, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
		}

		#region Primitive Drawing
		private void ManageCaches()
		{
			cache = new List<Vector2>();

			for (int i = 0; i < 10; i++)
			{
				cache.Add(Vector2.Lerp(start.npc.Center, end.npc.Center, i / 10f));
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(0), factor => 12.5f, factor =>
			{
				if (factor.X >= 0.85f)
					return Color.Transparent;

				return new Color(100, 200, 10) * 0.3f * factor.X;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[9];
		}
		public void DrawPrimitives(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.025f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

			trail?.Render(effect);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

			trail?.Render(effect);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
		#endregion Primitive Drawing


		/// <summary>
		/// Checks the equality of two edges. Since this graph is undirected, if the order is inversed
		/// it should still count as equal to that edge.
		/// </summary>
		/// <param name="obj">The other edge to compare to</param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is EchochainEdge edge)
			{
				return
					start == edge.start && end == edge.end ||
					start == edge.end && end == edge.start;
			}

			return false;	
		}
	}

	/// <summary>
	/// Represents a node in the echo chain graph, essentially a wrapper around an NPC
	/// </summary>
	class EchochainNode
	{
		public List<EchochainEdge> edges;
		public NPC npc;

		public EchochainNode(List<EchochainEdge> edges, NPC npc)
		{
			this.edges = edges;
			this.npc = npc;
		}
	}

	/*class EchochainDamageDebuff : StackableBuff<EchochainDamageStack>
	{
		public override string Name => "EchochainDamageBuff";

		public override string DisplayName => "Taking Damage"; // this should never be applied to a player so

		public override string Texture => AssetDirectory.Invisible;

		public override bool Debuff => true;

		public override string Tooltip => "Chained up...";

		public override EchochainDamageStack GenerateDefaultStackTyped(int duration)
		{
			return new EchochainDamageStack()
			{
				duration = 30, // duration of the stack should ALWAYS be 30 ticks
				damage = 1
			};
		}

		public override void PerStackEffectsNPC(NPC npc, EchochainDamageStack stack)
		{
			float lerper = EaseBuilder.EaseCubicOut.Ease(stack.duration / 30f);

			float power = MathHelper.Lerp(0f, 50f, lerper);

			float rotation = MathHelper.Pi * lerper;

			if (stack.duration % 2 == 0)
			{
				Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(rotation) * power, ModContent.DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(1.5f, 1.5f), 0, new Color(120, 255, 40), 0.65f);
				Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(rotation + MathHelper.Pi) * power, ModContent.DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(1.5f, 1.5f), 0, new Color(120, 255, 40), 0.65f);
			}

			if (stack.duration == 1)
			{
				npc.SimpleStrikeNPC(stack.damage, 0);

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(120, 255, 40), 0.65f);

					Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2CircularEdge(5f, 5f), 0, new Color(120, 255, 40), 0.65f);
				}

				Dust.NewDustPerfect(npc.Center, ModContent.DustType<EchochainBurstDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.75f, 1.25f));

				Helper.PlayPitched("Magic/Shadow1", 0.5f, 0f, npc.Center);
				CameraSystem.shake += 2;
			}
		}
	}

	class EchochainDamageStack : BuffStack
	{
		public int damage;
	}*/

	public class EchochainBurstDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
			dust.scale *= 0.045f;
			dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.Lerp(new Color(200, 40, 20, 0), new Color(25, 25, 25, 0), dust.alpha / 255f) * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 20;
			dust.scale += 0.015f;
			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Dust + Name).Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(135, 255, 10, 0) * 0.25f * lerper, 0f, bloomTex.Size() / 2f, dust.scale * 20f, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(135, 255, 10, 0) * 0.75f * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(90, 255, 130, 0) * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(255, 255, 150, 0) * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(90, 255, 130, 0) * 0.25f * lerper, 0f, bloomTex.Size() / 2f, dust.scale * 20f, 0f, 0f);

			return false;
		}
	}
}
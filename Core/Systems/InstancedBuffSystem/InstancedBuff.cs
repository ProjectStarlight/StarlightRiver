namespace StarlightRiver.Core.Systems.InstancedBuffSystem
{
	/// <summary>
	/// This class is to be used for buffs which require instanced data per enttiy it is inflicted on. For example, a different DoT value to apply.
	/// To inflict an instanced buff, call BuffInflictor.Inflict.
	/// </summary>
	internal abstract class InstancedBuff : ILoadable
	{
		/// <summary>
		/// The numeric ID of the backing traditional buff to indicate this buffs inflicted status
		/// </summary>
		public int BackingType => StarlightRiver.Instance.Find<ModBuff>(Name).Type;

		/// <summary>
		/// The internal name of the backing ModBuff
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The display name of the backing ModBuff
		/// </summary>
		public abstract string DisplayName { get; }

		/// <summary>
		/// The texture of the backing ModBuff
		/// </summary>
		public abstract string Texture { get; }

		/// <summary>
		/// If the backing ModBuff should be a buff or debuff
		/// </summary>
		public abstract bool Debuff { get; }

		/// <summary>
		/// The tooltip of the backing ModBuff
		/// </summary>
		public virtual string Tooltip => "";

		public void Load(Mod mod)
		{
			mod.AddContent(new InstancedBuffBacker(Name, DisplayName, Texture, Tooltip, Debuff));
			Load();
		}

		/// <summary>
		/// You can subscribe to StarlightPlayer/StarlightNPC events here
		/// </summary>
		public virtual void Load()
		{

		}

		/// <summary>
		/// You should unsub from all events you subscrive to in load here
		/// </summary>
		public virtual void Unload()
		{

		}

		/// <summary>
		/// If this buff is inflicted on a given NPC
		/// </summary>
		/// <param name="npc">The NPC to check</param>
		/// <returns>If that NPC has any instance of this buff</returns>
		public bool AnyInflicted(NPC npc)
		{
			return npc.HasBuff(BackingType);
		}

		/// <summary>
		/// If this buff is inflicted on a given player
		/// </summary>
		/// <param name="player">The player to check</param>
		/// <returns>If that player has any instance of this buff</returns>
		public bool AnyInflicted(Player player)
		{
			return player.HasBuff(BackingType);
		}

		/// <summary>
		/// Gets the inflicted instance of this instanced buff on a given NPC. To be used with StarlightNPC events
		/// </summary>
		/// <param name="npc">The NPC to check</param>
		/// <returns>The inflicted instance, or null if not inflicted</returns>
		public InstancedBuff? GetInstance(NPC npc)
		{
			return InstancedBuffNPC.GetInstance(npc, Name);
		}

		/// <summary>
		/// Gets the inflicted instance of this instanced buff on a given player. To be used with StarlightPlayer events
		/// </summary>
		/// <param name="player">The player to check</param>
		/// <returns>The inflicted instance, or null if not inflicted</returns>
		public InstancedBuff? GetInstance(Player player)
		{
			return InstancedBuffPlayer.GetInstance(player, Name);
		}

		/// <summary>
		/// The effects this buff should have while its inflicted on a player, in the default update loop
		/// </summary>
		/// <param name="player"></param>
		public virtual void UpdatePlayer(Player player) { }

		/// <summary>
		/// The effects this buff should have while its inflicted on an NPC, in the default update loop
		/// </summary>
		/// <param name="npc"></param>
		public virtual void UpdateNPC(NPC npc) { }
	}

	/// <summary>
	/// The auto-generated ModBuff backing type for instanced buffs
	/// </summary>
	[Autoload(false)]
	internal class InstancedBuffBacker : ModBuff
	{
		public string name;
		public string displayName;
		public string texture;
		public string tooltip;
		public bool debuff;

		public override string Name => name;

		public override string Texture => texture;

		public InstancedBuffBacker(string name, string displayName, string texture, string tooltip, bool debuff) : base()
		{
			this.name = name;
			this.displayName = displayName;
			this.texture = texture;
			this.tooltip = tooltip;
			this.debuff = debuff;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(displayName);
			Description.SetDefault(tooltip);

			Main.debuff[Type] = debuff;
		}
	}
}
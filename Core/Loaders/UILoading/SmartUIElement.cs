using Terraria.UI;

namespace StarlightRiver.Core.Loaders.UILoading
{
	/// <summary>
	/// Wrapper class that enforces Safe interoperability between event listeners and virtual methods for UI elements
	/// </summary>
	public class SmartUIElement : UIElement
	{
		#region XButton1
		/// <summary>
		/// A Safe wrapper around XButton1MouseUp that allows both an override and the OnXButton1MouseUp event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton1MouseUp(UIMouseEvent evt) { }

		public sealed override void XButton1MouseUp(UIMouseEvent evt)
		{
			base.XButton1MouseUp(evt);
			SafeXButton1MouseUp(evt);
		}

		/// <summary>
		/// A Safe wrapper around XButton1MouseDown that allows both an override and the OnXButton1MouseDown event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton1MouseDown(UIMouseEvent evt) { }

		public sealed override void XButton1MouseDown(UIMouseEvent evt)
		{
			base.XButton1MouseDown(evt);
			SafeXButton1MouseDown(evt);
		}

		/// <summary>
		/// A Safe wrapper around XButton1Click that allows both an override and the OnXButton1Click event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton1Click(UIMouseEvent evt) { }

		public sealed override void XButton1Click(UIMouseEvent evt)
		{
			base.XButton1Click(evt);
			SafeXButton1Click(evt);
		}

		/// <summary>
		/// A Safe wrapper around XButton1DoubleClick that allows both an override and the OnXButton1DoubleClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton1DoubleClick(UIMouseEvent evt) { }

		public sealed override void XButton1DoubleClick(UIMouseEvent evt)
		{
			base.XButton1DoubleClick(evt);
			SafeXButton1DoubleClick(evt);
		}
		#endregion

		#region XButton2
		/// <summary>
		/// A Safe wrapper around XButton2MouseUp that allows both an override and the OnXButton2MouseUp event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton2MouseUp(UIMouseEvent evt) { }

		public sealed override void XButton2MouseUp(UIMouseEvent evt)
		{
			base.XButton2MouseUp(evt);
			SafeXButton2MouseUp(evt);
		}

		/// <summary>
		/// A Safe wrapper around XButton2MouseDown that allows both an override and the OnXButton2MouseDown event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton2MouseDown(UIMouseEvent evt) { }

		public sealed override void XButton2MouseDown(UIMouseEvent evt)
		{
			base.XButton2MouseDown(evt);
			SafeXButton2MouseDown(evt);
		}

		/// <summary>
		/// A Safe wrapper around XButton2Click that allows both an override and the OnXButton2Click event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton2Click(UIMouseEvent evt) { }

		public sealed override void XButton2Click(UIMouseEvent evt)
		{
			base.XButton2Click(evt);
			SafeXButton2Click(evt);
		}

		/// <summary>
		/// A Safe wrapper around XButton2DoubleClick that allows both an override and the OnXButton2DoubleClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeXButton2DoubleClick(UIMouseEvent evt) { }

		public sealed override void XButton2DoubleClick(UIMouseEvent evt)
		{
			base.XButton2DoubleClick(evt);
			SafeXButton2DoubleClick(evt);
		}
		#endregion

		#region LMB
		/// <summary>
		/// A Safe wrapper around MouseUp that allows both an override and the OnMouseUp event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMouseUp(UIMouseEvent evt) { }

		public sealed override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);
			SafeMouseUp(evt);
		}

		/// <summary>
		/// A Safe wrapper around MouseDown that allows both an override and the OnMouseDown event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMouseDown(UIMouseEvent evt) { }

		public sealed override void MouseDown(UIMouseEvent evt)
		{
			base.MouseDown(evt);
			SafeMouseDown(evt);
		}

		/// <summary>
		/// A Safe wrapper around Click that allows both an override and the OnClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeClick(UIMouseEvent evt) { }

		public sealed override void Click(UIMouseEvent evt)
		{
			base.Click(evt);
			SafeClick(evt);
		}

		/// <summary>
		/// A Safe wrapper around DoubleClick that allows both an override and the OnDoubleClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeDoubleClick(UIMouseEvent evt) { }

		public sealed override void DoubleClick(UIMouseEvent evt)
		{
			base.DoubleClick(evt);
			SafeDoubleClick(evt);
		}
		#endregion

		#region RMB
		/// <summary>
		/// A Safe wrapper around RightMouseUp that allows both an override and the OnRightMouseUp event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeRightMouseUp(UIMouseEvent evt) { }

		public sealed override void RightMouseUp(UIMouseEvent evt)
		{
			base.RightMouseUp(evt);
			SafeRightMouseUp(evt);
		}

		/// <summary>
		/// A Safe wrapper around RightMouseDown that allows both an override and the OnRightMouseDown event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeRightMouseDown(UIMouseEvent evt) { }

		public sealed override void RightMouseDown(UIMouseEvent evt)
		{
			base.RightMouseDown(evt);
			SafeRightMouseDown(evt);
		}

		/// <summary>
		/// A Safe wrapper around RightClick that allows both an override and the OnRightClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeRightClick(UIMouseEvent evt) { }

		public sealed override void RightClick(UIMouseEvent evt)
		{
			base.RightClick(evt);
			SafeRightClick(evt);
		}

		/// <summary>
		/// A Safe wrapper around RightDoubleClick that allows both an override and the OnRightDoubleClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeRightDoubleClick(UIMouseEvent evt) { }

		public sealed override void RightDoubleClick(UIMouseEvent evt)
		{
			base.RightDoubleClick(evt);
			SafeRightDoubleClick(evt);
		}
		#endregion

		#region MMB
		/// <summary>
		/// A Safe wrapper around MiddleMouseUp that allows both an override and the OnMiddleMouseUp event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMiddleMouseUp(UIMouseEvent evt) { }

		public sealed override void MiddleMouseUp(UIMouseEvent evt)
		{
			base.MiddleMouseUp(evt);
			SafeMiddleMouseUp(evt);
		}

		/// <summary>
		/// A Safe wrapper around MiddleMouseDown that allows both an override and the OnMiddleMouseDown event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMiddleMouseDown(UIMouseEvent evt) { }

		public sealed override void MiddleMouseDown(UIMouseEvent evt)
		{
			base.MiddleMouseDown(evt);
			SafeMiddleMouseDown(evt);
		}

		/// <summary>
		/// A Safe wrapper around MiddleClick that allows both an override and the OnMiddleClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMiddleClick(UIMouseEvent evt) { }

		public sealed override void MiddleClick(UIMouseEvent evt)
		{
			base.MiddleClick(evt);
			SafeMiddleClick(evt);
		}

		/// <summary>
		/// A Safe wrapper around MiddleDoubleClick that allows both an override and the OnMiddleDoubleClick event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMiddleDoubleClick(UIMouseEvent evt) { }

		public sealed override void MiddleDoubleClick(UIMouseEvent evt)
		{
			base.MiddleDoubleClick(evt);
			SafeMiddleDoubleClick(evt);
		}
		#endregion

		#region Misc
		/// <summary>
		/// A Safe wrapper around MouseOver that allows both an override and the OnMouseOver event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeMouseOver(UIMouseEvent evt) { }

		public sealed override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			SafeMouseOver(evt);
		}

		/// <summary>
		/// A Safe wrapper around Update that allows both an override and the OnUpdate event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeUpdate(GameTime gameTime) { }

		public sealed override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			SafeUpdate(gameTime);
		}

		/// <summary>
		/// A Safe wrapper around ScrollWheel that allows both an override and the OnScrollWheel event to be used together
		/// </summary>
		/// <param name="evt">The mouse event that occured to fire this listener</param>
		public virtual void SafeScrollWheel(UIScrollWheelEvent evt) { }

		public sealed override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			SafeScrollWheel(evt);
		}
		#endregion
	}
}

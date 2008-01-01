using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Collections.Generic;

using Physics.Surfaces;
using Physics.Base;

namespace SilverStunts
{
	public class Editor : Control
	{
		bool gridEnabled = true;
		public bool GridEnabled { get { return gridEnabled && zoom == 1.0; } }
		public bool frozen = false;
		public bool names = true;

		public Canvas grid;
		public Canvas scroller;
		public Canvas workspace;
		private Game game;

		double scrollX = 0;
		double scrollY = 0;
		double zoom = 1.0;

		List<IGizmo> currentGizmos = new List<IGizmo>();
		List<IGizmo> dragGizmos = new List<IGizmo>();
		List<IGizmo> clipboard = new List<IGizmo>();

		Visual lastPick;
		bool wantAdditionalGizmos = false;
		bool inDragMode = false;

		double pasteShifX = 0;
		double pasteShifY = 0;

		public bool Active { get; set; }

		Visual visualToBeSelected;

		public Editor(Game game)
		{
			this.game = game;

			grid = game.content.FindName("grid") as Canvas;
			workspace = game.content.FindName("workspace") as Canvas;
			scroller = game.content.FindName("scroller") as Canvas;

			game.MouseLeftButtonDown += new MouseEventHandler(OnMouseLeftButtonDown);
			game.MouseLeftButtonUp += new MouseEventHandler(OnMouseLeftButtonUp);
			game.MouseMove += new MouseEventHandler(OnMouseMove);
		}

		public void Enable()
		{
			scrollX = - game.cameraX;
			scrollY = - game.cameraY + 100;
			Active = true;
			grid.Visibility = GridEnabled ? Visibility.Visible : Visibility.Collapsed;
			if (names) ShowNames();
			EnableOnionEffect();
			if (!frozen) game.DisableSimulation();
		}

		public void Disable()
		{
			Deselect();
			dragGizmos.Clear();
			clipboard.Clear();
			Active = false;
			grid.Visibility = Visibility.Collapsed;
			HideNames();
			DisableOnionEffect();
			game.EnableSimulation();
		}

		public void Reset()
		{
			Deselect();
			dragGizmos.Clear();
			clipboard.Clear();
			if (!Active) HideNames();
		}

		public void EnableOnionEffect()
		{
			foreach (Visual visual in game.level.visuals)
			{
				EnableOnionEffect(visual);
			}
			game.level.OnEntityCreated += new EventHandler<EntityCreatedArgs>(level_OnionHandler);
		}

		public static void EnableOnionEffect(Visual visual)
		{
			// apply to surfaces only
			if (visual.family != Visual.Family.Line &&
				visual.family != Visual.Family.Circle &&
				visual.family != Visual.Family.Rectangle) return;

			visual.content.Opacity = 0.8;
			int count = visual.content.Children.Count;
			bool firstLine = true;
			for (int i = 0; i < count; i++)
			{
				if (visual.content.Children[i] is System.Windows.Shapes.Rectangle)
				{
					System.Windows.Shapes.Rectangle rect = visual.content.Children[i] as System.Windows.Shapes.Rectangle;
					rect.Fill = Brushes.FromColor(Color.FromRgb(0xbb, 0x00, 0x00));
				}
				if (visual.content.Children[i] is System.Windows.Shapes.Ellipse)
				{
					System.Windows.Shapes.Ellipse circle = visual.content.Children[i] as System.Windows.Shapes.Ellipse;
					circle.Fill = Brushes.FromColor(Color.FromRgb(0xaa, 0x00, 0xaa));
				}
				if (visual.content.Children[i] is System.Windows.Shapes.Line)
				{
					System.Windows.Shapes.Line line = visual.content.Children[i] as System.Windows.Shapes.Line;
					if (firstLine)
						line.Stroke = Brushes.FromColor(Color.FromRgb(0x00, 0xbb, 0x00));
					else
					{
						line.Visibility = Visibility.Visible;
					}
					firstLine = false;
				}
			}
		}

		public void DisableOnionEffect()
		{
			foreach (Visual visual in game.level.visuals)
			{
				DisableOnionEffect(visual);
			}
			game.level.OnEntityCreated -= new EventHandler<EntityCreatedArgs>(level_OnionHandler);
		}

		public static void DisableOnionEffect(Visual visual)
		{
			// apply to surfaces only
			if (visual.family != Visual.Family.Line &&
				visual.family != Visual.Family.Circle &&
				visual.family != Visual.Family.Rectangle) return;

			visual.content.Opacity = 1.0;
			int count = visual.content.Children.Count;
			bool firstLine = true;
			for (int i = 0; i < count; i++)
			{
				if (visual.content.Children[i] is System.Windows.Shapes.Rectangle)
				{
					System.Windows.Shapes.Rectangle rect = visual.content.Children[i] as System.Windows.Shapes.Rectangle;
					rect.Fill = Brushes.Gray;
				}
				if (visual.content.Children[i] is System.Windows.Shapes.Ellipse)
				{
					System.Windows.Shapes.Ellipse circle = visual.content.Children[i] as System.Windows.Shapes.Ellipse;
					circle.Fill = Brushes.Gray;
				}
				if (visual.content.Children[i] is System.Windows.Shapes.Line)
				{
					System.Windows.Shapes.Line line = visual.content.Children[i] as System.Windows.Shapes.Line;
					if (firstLine)
						line.Stroke = Brushes.Gray;
					else
					{
						line.Visibility = Visibility.Collapsed;
					}
					firstLine = false;
				}
			}
		}

		void level_OnionHandler(object sender, EntityCreatedArgs e)
		{
			EnableOnionEffect(e.visual);
			if (!names) HideName(e.visual);
		}

		public void ShowNames()
		{
			foreach (Visual visual in game.level.visuals)
			{
				int i = visual.content.Children.Count;
				while (--i > 0)
				{
					if (visual.content.Children[i] is TextBlock)
					{
						TextBlock text = visual.content.Children[i] as TextBlock;
						text.Visibility = Visibility.Visible;
					}
				}
			}
		}

		public void HideNames()
		{
			foreach (Visual visual in game.level.visuals)
			{
				HideName(visual);
			}
		}

		private static void HideName(Visual visual)
		{
			int i = visual.content.Children.Count;
			while (--i > 0)
			{
				if (visual.content.Children[i] is TextBlock)
				{
					TextBlock text = visual.content.Children[i] as TextBlock;
					text.Visibility = Visibility.Collapsed;
				}
			}
		}

		public void UpdateScrolling()
		{
			TranslateTransform tt = new TranslateTransform();
			tt.X = scrollX + (500 / zoom);
			tt.Y = scrollY + (300 / zoom);

			ScaleTransform st = new ScaleTransform();
			st.ScaleX = zoom;
			st.ScaleY = zoom;

			TransformGroup tg = new TransformGroup();
			tg.Children.Add(tt);
			tg.Children.Add(st);

			scroller.RenderTransform = tg;
		}

		public void ProcessKey(KeyboardEventArgs e, bool pressed)
		{
			if (pressed) return;

			if (e.Key == 36) // G == grid
			{
				if (gridEnabled) DisableGrid(); else EnableGrid();
			}
			if (e.Key == 35) // F == freeze
			{
				if (frozen)
				{
					frozen = false;
					game.DisableSimulation();
				}
				else
				{
					frozen = true;
					game.EnableSimulation();
				}
			}
			if (e.Key == 47) // R = render XAML
			{
				RenderWorldXAML();
			}
			if (e.Key == 43) // N == names
			{
				names = !names;
				if (!names) HideNames(); else ShowNames();
			}
			if (e.Key == 13 || e.Key == 20) // HOME or 0
			{
				ResetScrollPane();
			}
			if (e.Key == 12 || e.Key == 29) // END = reset zoom or 9
			{
				ResetZoom();
			}
			if (e.Key == 19 || e.PlatformKeyCode == 46) // DELETE (need to use PlatformKeyCode on windows)
			{
				// delete selected objects
				foreach (IGizmo gizmo in currentGizmos)
				{
					if (gizmo is MoveGizmo) DeleteObject(gizmo as MoveGizmo);
					gizmo.Destroy();
				}
				currentGizmos.Clear();
				dragGizmos.Clear();
			}

			if (e.Ctrl && e.Key == 32) // CTRL+C == copy
			{
				CopySelectionIntoClipboard();
			}
			if (e.Ctrl && e.Key == 51) // CTRL+V == paste
			{
				PasteSelectionFromClipboard();
			}
		}

		public void SetZoom(double z)
		{
			zoom = z;
			if (z != 1.00)
			{
				grid.Visibility = Visibility.Collapsed;
			}
			else
			{
				if (gridEnabled) EnableGrid();
			}
		}

		private void RenderWorldXAML()
		{
			Page.Current.ShowWorldXAML();
		}

		void OnMouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (!Active) return;
			inDragMode = true;

			game.CaptureMouse();
			ResetPasteShift();

			IGizmo additionalHitGizmo = null;
			dragGizmos.Clear();
			bool anyGizmoHit = false;
			foreach (IGizmo gizmo in currentGizmos)
			{
				if (gizmo.HitTest(e))
				{
					anyGizmoHit = true;
					if (!(gizmo is MoveGizmo))
					{
						additionalHitGizmo = gizmo;
						break;
					}
				}
			}
			if (additionalHitGizmo != null)
			{
				// operate only with first hit additional gizmo
				dragGizmos.Clear();
				dragGizmos.Add(additionalHitGizmo);
			}
			else if (anyGizmoHit)
			{
				dragGizmos.Clear();
				// move all move gizmos in multiselection
				foreach (IGizmo gizmo in currentGizmos)
				{
					if (gizmo is MoveGizmo) dragGizmos.Add(gizmo);
				}
			}

			if (dragGizmos.Count > 0)
			{
				// in dragGizmos are active gizmos we operate upon
				foreach (IGizmo gizmo in dragGizmos)
				{
					gizmo.HandleMouseLeftButtonDown(sender, e);
				}
				return;
			}

			if (!e.Ctrl)
			{
				Deselect();
			}

			Point pos = GetWorldPos(e);
			Visual pick = game.PickSurface(pos.X, pos.Y);
			if (pick != null)
			{
				// click-to-move behavior
				IGizmo gizmo = InitMainGizmo(pick);
				gizmo.HandleMouseLeftButtonDown(null, e);
				wantAdditionalGizmos = true;
				lastPick = pick;
			}
			else
			{
				// start region selection
				IGizmo gizmo = new SelectionGizmo(this);
				gizmo.HandleMouseLeftButtonDown(this, e);
				dragGizmos.Add(gizmo);
			}
		}

		public Point GetWorldPos(MouseEventArgs e)
		{
			Point pos = e.GetPosition(game);
			TransformGroup group = scroller.RenderTransform as TransformGroup;
			TranslateTransform tt = group.Children[0] as TranslateTransform;
			pos.X /= zoom;
			pos.Y /= zoom;
			pos.X -= tt.X;
			pos.Y -= tt.Y;
			return pos;
		}

		void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (!Active) return;
			if (!inDragMode) return;

			foreach (IGizmo gizmo in dragGizmos)
			{
				// route message into gizmo
				gizmo.HandleMouseMove(sender, e);
			}
			UpdateGizmos();
		}

		void OnMouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (!Active) return;
			inDragMode = false;

			foreach (IGizmo gizmo in dragGizmos)
			{
				// route message into gizmo
				gizmo.HandleMouseLeftButtonUp(sender, e);
			}

			//Point pos = GetWorldPos(e);
			if (wantAdditionalGizmos)
			{
				InitAdditionalGizmos(lastPick);
				wantAdditionalGizmos = false;
			}
			else
			{
				// we were possibly in dragging/selection mode
				if (dragGizmos.Count == 1 && dragGizmos[0] is SelectionGizmo)
				{
					// rectangular selection mode
					SelectionGizmo gizmo = dragGizmos[0] as SelectionGizmo;
					List<Visual> picks = game.PickSurfaces(gizmo.x, gizmo.y, gizmo.w, gizmo.h);

					// select picks
					foreach (Visual pick in picks)
					{
						InitMainGizmo(pick);
						InitAdditionalGizmos(pick);
					}

					gizmo.Destroy();
					dragGizmos.Clear();
				}
			}
			game.ReleaseMouseCapture();
		}

		public void EnableGrid()
		{
			if (!Active) return;
			gridEnabled = true;
			grid.Visibility = Visibility.Visible;
		}

		public void DisableGrid()
		{
			if (!Active) return;
			gridEnabled = false;
			grid.Visibility = Visibility.Collapsed;
		}

		public void ResetScrollPane()
		{
			scrollX = 0;
			scrollY = 0;
			ResetZoom();
		}

		public void ResetZoom()
		{
			SetZoom(1.0);
		}

		public void UpdateGizmos()
		{
			foreach (IGizmo gizmo in currentGizmos)
			{
				gizmo.Update();
			}
		}

		public void InitAdditionalGizmos(Visual visual)
		{
			if (visual == null) return;

			if (visual.source is RectangleSurface)
			{
				currentGizmos.Add(new RectangleResizeTLGizmo(this, visual));
				currentGizmos.Add(new RectangleResizeTRGizmo(this, visual));
				currentGizmos.Add(new RectangleResizeBRGizmo(this, visual));
				currentGizmos.Add(new RectangleResizeBLGizmo(this, visual));
			}
			if (visual.source is CircleSurface)
			{
				currentGizmos.Add(new CircleRadiusGizmo(this, visual));
			}
			if (visual.source is LineSurface)
			{
				currentGizmos.Add(new LineSurfaceH1Gizmo(this, visual));
				currentGizmos.Add(new LineSurfaceH2Gizmo(this, visual));
			}
		}

		public IGizmo InitMainGizmo(Visual visual)
		{
			if (visual == null) return null;

			IGizmo gizmo = null;
			if (visual.source is RectangleSurface)
			{
				gizmo = new RectangleMoveGizmo(this, visual);
			}
			if (visual.source is CircleSurface)
			{
				gizmo = new CircleMoveGizmo(this, visual);
			}
			if (visual.source is LineSurface)
			{
				gizmo = new LineSurfaceMoveGizmo(this, visual);
			}

			if (gizmo == null) throw new Exception("Unsupported visual source");

			currentGizmos.Add(gizmo);
			dragGizmos.Add(gizmo);
			return gizmo;
		}

		private void Deselect()
		{
			foreach (IGizmo gizmo in currentGizmos)
			{
				gizmo.Destroy();
			}
			currentGizmos.Clear();
		}

		public void ResetPasteShift()
		{
			pasteShifX = 0;
			pasteShifY = 0;
		}

		public void CopySelectionIntoClipboard()
		{
			clipboard.Clear();
			clipboard.AddRange(currentGizmos);
		}

		public void PasteSelectionFromClipboard()
		{
			if (clipboard.Count == 0) return;
			Deselect();

			pasteShifX += 20;
			pasteShifY += 20;

			foreach (IGizmo gizmo in clipboard)
			{
				if (gizmo is MoveGizmo) CopyObjectUnderGizmo(gizmo as MoveGizmo, pasteShifX, pasteShifY);
			}
		}

		public string GenerateCode(PhysicsObject po)
		{
			if (po is RectangleSurface)
			{
				RectangleSurface o = po as RectangleSurface;
				return String.Format("{0} = Rectangle({1:g}, {2:g}, {3:g}, {4:g})", o.name, o.Center.X, o.Center.Y, o.Width, o.Height);
			}
			if (po is CircleSurface)
			{
				CircleSurface o = po as CircleSurface;
				return String.Format("{0} = Circle({1:g}, {2:g}, {3:g})", o.name, o.Center.X, o.Center.Y, o.Radius);
			}
			if (po is LineSurface)
			{
				LineSurface o = po as LineSurface;
				return String.Format("{0} = Line({1:g}, {2:g}, {3:g}, {4:g})", o.name, o.OP1.X, o.OP1.Y, o.OP2.X, o.OP2.Y);
			}
			throw new Exception("unsupported physics object");
		}

		public void CopyObjectUnderGizmo(MoveGizmo gizmo, double shiftx, double shifty)
		{
			visualToBeSelected = null;
			game.level.OnEntityCreated += new EventHandler<EntityCreatedArgs>(level_OnEntityCreated);

			if (gizmo.editable is RectangleSurface)
			{
				string name = game.level.GenerateUniqueName();
				RectangleSurface o = (RectangleSurface)gizmo.editable;
				string code = String.Format("{0} = Rectangle({1:g}, {2:g}, {3:g}, {4:g})", name, o.Center.X + shiftx, o.Center.Y + shifty, o.Width, o.Height);
				game.level.EvalExpression(code);
				game.level.AddEntitiesSource(code);
			}
			if (gizmo.editable is CircleSurface)
			{
				string name = game.level.GenerateUniqueName();
				CircleSurface o = (CircleSurface)gizmo.editable;
				string code = String.Format("{0} = Circle({1:g}, {2:g}, {3:g})", name, o.Center.X + shiftx, o.Center.Y + shifty, o.Radius);
				game.level.EvalExpression(code);
				game.level.AddEntitiesSource(code);
			}
			if (gizmo.editable is LineSurface)
			{
				string name = game.level.GenerateUniqueName();
				LineSurface o = (LineSurface)gizmo.editable;
				string code = String.Format("{0} = Line({1:g}, {2:g}, {3:g}, {4:g})", name, o.OP1.X + shiftx, o.OP1.Y + shifty, o.OP2.X + shiftx, o.OP2.Y + shifty);
				game.level.EvalExpression(code);
				game.level.AddEntitiesSource(code);
			}

			game.level.OnEntityCreated -= new EventHandler<EntityCreatedArgs>(level_OnEntityCreated);

			if (visualToBeSelected != null)
			{
				InitMainGizmo(visualToBeSelected);
				InitAdditionalGizmos(visualToBeSelected);
				visualToBeSelected = null;
			}
		}

		void level_OnEntityCreated(object sender, EntityCreatedArgs e)
		{
			visualToBeSelected = e.visual;
		}

		public void DeleteObject(MoveGizmo gizmo)
		{
			game.level.EvalExpression(String.Format("del {0}", gizmo.visual.source.name));
			game.level.EntityDestroyed(gizmo.visual);
			game.level.RemoveEntitiesSource(String.Format("{0} = \\w+\\(.*\\)", gizmo.visual.source.name));
		}

		public void ProcessInputs(bool[] keys)
		{
			int scrollStep = 10;
			if (keys[39]) scrollX += scrollStep; // J
			if (keys[41]) scrollX -= scrollStep; // L
			if (keys[38]) scrollY += scrollStep; // I
			if (keys[40]) scrollY -= scrollStep; // K

			if (keys[10] || keys[55]) // PAGE_UP = zoom out or Z
			{
				if (zoom < 5.0) SetZoom(zoom + 0.02);
			}
			if (keys[11] || keys[53]) // PAGE_DOWN = zoom in or X
			{
				if (zoom > 0.2) SetZoom(zoom - 0.02);
			}

		}
	}
}

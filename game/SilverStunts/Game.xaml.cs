using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;

using Physics.Util;

namespace SilverStunts
{
	public class Game : Control
	{
		public Editor editor;
		Physics.Engine physics;
		public Level level;

		public int cameraX;
		public int cameraY;

		public ClipCanvas content;

		public Canvas scroller;
		public Canvas world;
		public Canvas foreground;
		public Canvas background;
		public bool simulationEnabled = false;

		public Game(Canvas parent)
		{
			Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Game.xaml");
			content = this.InitializeFromXaml(new StreamReader(s).ReadToEnd()) as ClipCanvas;

			editor = new Editor(this);

			scroller = content.FindName("scroller") as Canvas;
			world = content.FindName("world") as Canvas;
			foreground = content.FindName("foreground") as Canvas;
			background = content.FindName("background") as Canvas;

			content.UpdateLayout();
		}

		public void Reset()
		{
			editor.Reset();
			ResetScrolling();
			UpdateScrolling();
		}

		public void AttachToLevel(Level level)
		{
			this.level = level;
			this.physics = level.physics;
			Reset();
		}

		public void ResetScrolling()
		{
			cameraX = 500;
			cameraY = 400;
		}

		public void EnableEditor()
		{
			editor.Enable();
		}

		public void DisableEditor()
		{
			editor.Disable();
		}

		public void EnableSimulation()
		{
			simulationEnabled = true;
		}

		public void DisableSimulation()
		{
			simulationEnabled = false;
		}

		public void Simulate()
		{
			if (!simulationEnabled) return;
			physics.TimeStep();
		}

		public void ProcessInputs(bool[] keys)
		{
			if (editor.Active) editor.ProcessInputs(keys);
			if (!simulationEnabled) return;
			if (level!=null) level.ProcessInputs(keys);
		}

		public void ProcessKey(KeyboardEventArgs e, bool pressed)
		{
			// handle editor behavior
			if (editor.Active) editor.ProcessKey(e, pressed);

			// no key processing in game ...
		}

		public void UpdateScrolling()
		{
			// update scroll pane
			if (editor.Active)
			{
				// editor handles scrolling
				editor.UpdateScrolling();
			}
			else
			{
				// game handles scrolling
				Vector apos = level.actor.GetPos();
				Vector dx = new Vector(apos.X - cameraX, apos.Y - cameraY);

				double mag = dx.Magnitude();
				if (mag > 400) dx.Mult(400 / mag);

				cameraX += (int)(dx.X / 2);
				cameraY += (int)(dx.Y / 2);

				TranslateTransform tt = new TranslateTransform();
				tt.X = -(cameraX - 500);
				tt.Y = -(cameraY - 400);
				scroller.RenderTransform = tt;
			}
		}

		internal string GetStats()
		{
			return String.Format("Game: CamX={0:0000} CamY={1:0000} World=#{2:0000}",
				cameraX,
				cameraY,
				world.Children.Count
			);
		}

		public Visual PickSurface(double x, double y)
		{
			return level.PickSurface(x, y);
		}

		internal List<Visual> PickSurfaces(double x, double y, double w, double h)
		{
			return level.PickSurfaces(x, y, w, h);
		}
	}
}

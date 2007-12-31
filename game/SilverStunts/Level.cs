using System;
using System.Windows.Controls;
using System.Text;

using System.IO;
using Microsoft.Scripting;
using System.Collections.Generic;

using SilverStunts.Entities;
using System.Text.RegularExpressions;

namespace SilverStunts
{
	public class EntityCreatedArgs : EventArgs
	{
		public Entity entity;
		public Visual visual;

		public EntityCreatedArgs(Entity entity, Visual visual)
		{
			this.visual = visual;
			this.entity = entity;
		}
	}

	public class LevelDescriptor
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public LevelDescriptor(string name)
		{
			this.Name = name;
		}
	}

	public class Level
	{
		public Physics.Engine physics;
		protected Canvas world;
		protected Canvas foreground;
		protected Canvas background;
		public IActor actor;
		string name;

		// source form
		public string ForegroundSource;
		public string BackgroundSource;
		public string EntitiesSource;
		public string LogicSource;

		Shell shell;
		public List<Visual> visuals = new List<Visual>(); // game visuals
		List<KeyValuePair<Visual, Entity>> toBeResolved = new List<KeyValuePair<Visual, Entity>>(); // entities toBeResolved for name resolution

		public event EventHandler<EntityCreatedArgs> OnEntityCreated;

		private delegate bool InitDelegate();
		private InitDelegate initDelegate;
		private delegate bool TickDelegate(int tick, int elapsed);
		private TickDelegate tickDelegate;

		public Level(string name, Canvas world, Canvas foreground, Canvas background)
		{
			this.name = name;
			this.world = world;
			this.foreground = foreground;
			this.background = background;

			actor = new Ghost();

			InitShell(name);
		}

		public Physics.Engine InitializePhysics()
		{
			Physics.Engine physics = new Physics.Engine();

			physics.SetDamping(0.99);
			physics.SetGravity(0.0, 0.5);
			physics.SetSurfaceBounce(0.0);
			physics.SetSurfaceFriction(0.2);

			return physics;
		}

		public void InitShell(string name)
		{
			this.shell = new Shell("py", name);
		}

		public void Prepare()
		{
			this.physics = InitializePhysics();
			this.world.Children.Clear();

			this.shell.SetVariable("world", this.world);
			this.shell.SetVariable("physics", this.physics);

			Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Level.py");
			string code = new StreamReader(s).ReadToEnd();
			shell.Execute(code);
		}

		bool IsOK(string res)
		{
			return res == "" || res.StartsWith("OK");
		}

		public bool EvalExpression(string expression)
		{
			return EvalExpression(expression, false);
		}

		public bool EvalExpression(string expression, bool force)
		{
			string res = shell.EvaluatePartialExpression(expression, force);
			return IsOK(res);
		}

		public void UpdateEntityName(Entity entity)
		{
			foreach (Object child in entity.xaml.Children)
			{
				if (child is TextBlock)
				{
					TextBlock tb = (TextBlock)child;
					tb.Text = entity.name + " (" + entity.type + ")";
				}
			}
		}

		public void EntityCreated(Entity entity, Visual visual)
		{
			// cannot resolve name now, because visual is yet not living in scripting engine
			toBeResolved.Add(new KeyValuePair<Visual, Entity>(visual, entity)); 

			// put visual into physics
			physics.Add(visual.source);

			// put visual into world
			world.Children.Add(visual);

			visuals.Add(visual);

			if (OnEntityCreated != null) OnEntityCreated(this, new EntityCreatedArgs(entity, visual));
		}

		public void EntityDestroyed(Visual visual)
		{
			visuals.Remove(visual);

			// remove visual from world
			world.Children.Remove(visual);

			// remove visual from physics
			physics.Remove(visual.source);
		}

		public void UpdateVisuals()
		{
			// lookup visual names
			ResolveWaitingEntities();

			foreach (Visual visual in visuals)
			{
				if (visual.source.IsDirty())
				{
					visual.Update();
					visual.source.ClearDirty();
				}
			}
		}

		private void ResolveWaitingEntities()
		{
			if (toBeResolved.Count == 0) return;
			foreach (KeyValuePair<Visual, Entity> pair in toBeResolved)
			{
				pair.Key.source.name = LookupEntityName(pair.Value);
				UpdateEntityName(pair.Value);
			}
			toBeResolved.Clear();
		}

		private string LookupEntityName(Entity entity)
		{
			foreach (KeyValuePair<SymbolId, object> item in shell.Module.Scope.Items)
			{
				object o = item.Value;
				if (o is Entity)
				{
					Entity e = o as Entity;
					if (ReferenceEquals(e, entity))
					{
						return item.Key.ToString();
					}
				}
			}
			return "?";
		}

		public bool EvalEntities()
		{
			string res = shell.ExecuteAsString(EntitiesSource);
			if (!IsOK(res)) return false;
			return true;
		}

		public bool EvalForeground()
		{
			Layer layer = new Layer(ForegroundSource, "foreground");
			foreground.Children.Clear();
			foreground.Children.Add(layer);
			return true;
		}

		public bool EvalBackground()
		{
			Layer layer = new Layer(BackgroundSource, "background");
			background.Children.Clear();
			background.Children.Add(layer);
			return true;
		}

		public bool EvalLogic()
		{
			string res = shell.ExecuteAsString(LogicSource);
			if (!IsOK(res)) return false;

			try
			{
				initDelegate = shell.Engine.EvaluateAs<InitDelegate>("init", shell.Module);
			}
			catch (Exception e)
			{
				res = shell.ExceptionToString(e);
				Page.Current.PrintConsole(res, Page.ConsoleOutputKind.Exception);
				initDelegate = null;
			}

			try
			{
				tickDelegate = shell.Engine.EvaluateAs<TickDelegate>("tick", shell.Module);
			}
			catch (Exception e)
			{
				res = shell.ExceptionToString(e);
				Page.Current.PrintConsole(res, Page.ConsoleOutputKind.Exception);
				tickDelegate = null;
			}

			return true;
		}

		public void Reset()
		{
			actor.Destroy();
			Clear();
			InitShell(this.name);
			Prepare();
			EvalBackground();
			EvalForeground();
			EvalEntities();
			EvalLogic();

			actor = new Bike(this, 100, 300);

			if (initDelegate != null)
			{
				// TODO: test result
				initDelegate();
			}

		}

		public void ProcessInputs(bool[] keys)
		{
			actor.ProcessInputs(keys);
		}

		public void Clear()
		{
			foreach (Visual visual in visuals)
			{
				// remove visual from world
				world.Children.Remove(visual);

				// remove visual from physics
				physics.Remove(visual.source);
			}

			visuals.Clear();
			toBeResolved.Clear();
		}

		internal string GetStats()
		{
			int entitiesCount = visuals.Count;
			int linesCount = 0;
			int rectanglesCount = 0;
			int circlesCount = 0;

			foreach (Visual visual in visuals)
			{
				if (visual.family == Visual.Family.Line) linesCount++;
				if (visual.family == Visual.Family.Rectangle) rectanglesCount++;
				if (visual.family == Visual.Family.Circle) circlesCount++;
			}

			return String.Format("Level: {0,-20} Entities={1:000} [L{2:000} R{3:000} C{4:000}]",
				name,
				entitiesCount,
				linesCount, rectanglesCount, circlesCount);
		}

		public Visual PickSurface(double x, double y)
		{
			Physics.Primitives.RectangleParticle particle = new Physics.Primitives.RectangleParticle(x, y, 2, 2);
			foreach (Visual visual in visuals)
			{
				if (visual.source is Physics.Surfaces.ISurface)
				{
					Physics.Surfaces.ISurface surface = visual.source as Physics.Surfaces.ISurface;
					if (surface.IsRectangleColliding(particle)) return visual;
				}
			}
			return null;
		}

		public List<Visual> PickSurfaces(double x, double y, double w, double h)
		{
			List<Visual> picks = new List<Visual>();
			Physics.Primitives.RectangleParticle particle = new Physics.Primitives.RectangleParticle(x, y, w, h);
			foreach (Visual visual in visuals)
			{
				if (visual.source is Physics.Surfaces.ISurface)
				{
					Physics.Surfaces.AbstractSurface surface = visual.source as Physics.Surfaces.AbstractSurface;
					if (surface.TestBoundingBox(x, y, x+w, y+h)) picks.Add(visual);
				}
			}
			return picks;
		}

		internal string RenderXAML()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<Page\n" +
"	xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
"	xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
"	x:Name=\""+name+"\"\n" +
"	WindowTitle=\""+name+"\">\n");

			sb.AppendLine("<Canvas x:Name=\"world\" Width=\"0\" Height=\"0\">");

			foreach (Visual visual in visuals)
			{
				if (visual.source is Physics.Surfaces.ISurface)
				{
					string xaml = visual.RenderXAML();
					sb.AppendLine(String.Format("<!-- entity {0} -->", visual.source.name));
					sb.Append(xaml);
					sb.AppendLine();
				}
			}

			sb.AppendLine("</Canvas>");
			sb.AppendLine("</Page>");
			return sb.ToString();
		}

		public bool IsNameTaken(string name)
		{
			foreach (KeyValuePair<SymbolId, object> item in shell.Module.Scope.Items)
			{
				if (item.Key.ToString() == name) return true;
			}
			return false;
		}


		public string GenerateUniqueName()
		{
			int index = 0;
			while (true)
			{
				int count = EntityNamer.Count();
				while (count>0)
				{
					string name = EntityNamer.RequestName();
					if (index > 0) name += index.ToString();
					if (!IsNameTaken(name)) return name;
				}
				index++;
			}
		}

		public void AddEntitiesSource(string source)
		{
			Page.Current.RequestEntitiesSource();
			EntitiesSource += "\n" + source;
			Page.Current.PublishEntitiesSource();
		}

		public void RemoveEntitiesSource(string pattern)
		{
			UpdateEntitiesSource(pattern, "");
		}

		public void UpdateEntitiesSource(string pattern, string content)
		{
			Page.Current.RequestEntitiesSource();
			Regex re = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
			EntitiesSource = re.Replace(EntitiesSource, content);
			Page.Current.PublishEntitiesSource();
		}

		public void Tick(int tick, int elapsed)
		{
			if (tickDelegate == null) return;
			tickDelegate(tick, elapsed);
		}
	}
}

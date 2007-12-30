using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Resources;
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
        public Visual binder;

        public EntityCreatedArgs(Entity entity, Visual binder)
        {
            this.binder = binder;
            this.entity = entity;
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

        ScriptShell shell;
        public List<Visual> binders = new List<Visual>(); // game entities
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

            actor = new DummyActor();

            InitShell(name);
        }

        public Physics.Engine InitializePhysics()
        {
            Physics.Engine physics = new Physics.Engine();

            physics.SetDamping(1.0);
            physics.SetGravity(0.0, 0.5);
            physics.SetSurfaceBounce(0.1);
            physics.SetSurfaceFriction(0.1);

            return physics;
        }

        public void InitShell(string name)
        {
            this.shell = new ScriptShell("py", name);
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

        public void EntityCreated(Entity entity, Visual binder)
        {
            toBeResolved.Add(new KeyValuePair<Visual, Entity>(binder, entity)); // cannot resolve name now, because binder is yet not living in scripting engine

            // put binder into physics
            physics.Add(binder.source);

            // put binder into world            
            world.Children.Add(binder);

            binders.Add(binder);

            if (OnEntityCreated != null) OnEntityCreated(this, new EntityCreatedArgs(entity, binder));
        }

        public void EntityDestroyed(Visual binder)
        {
            binders.Remove(binder);

            // remove binder from world            
            world.Children.Remove(binder);

            // remove binder from physics
            physics.Remove(binder.source);
        }

        public void UpdateBinders()
        {
            // lookup binder names
            ResolveWaitingEntities();

            foreach (Visual binder in binders)
            {
                if (binder.source.IsDirty())
                {
                    binder.Update();
                    binder.source.ClearDirty();
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
            foreach (Visual binder in binders)
            {
                // remove binder from world            
                world.Children.Remove(binder);

                // remove binder from physics
                physics.Remove(binder.source);
            }

            binders.Clear();
            toBeResolved.Clear();
        }

        internal string GetStats()
        {
            int entitiesCount = binders.Count;
            int linesCount = 0;
            int rectanglesCount = 0;
            int circlesCount = 0;

            foreach (Visual binder in binders)
            {
                if (binder.family == Visual.Family.Line) linesCount++;
                if (binder.family == Visual.Family.Rectangle) rectanglesCount++;
                if (binder.family == Visual.Family.Circle) circlesCount++;
            }

            return String.Format("Level: {0,-20} Entities={1:000} [L{2:000} R{3:000} C{4:000}]",
                name,
                entitiesCount,
                linesCount, rectanglesCount, circlesCount);
        }

        public Visual PickSurface(double x, double y)
        {
            Physics.Primitives.RectangleParticle particle = new Physics.Primitives.RectangleParticle(x, y, 2, 2);
            foreach (Visual binder in binders)
            {
                if (binder.source is Physics.Surfaces.ISurface)
                {
                    Physics.Surfaces.ISurface surface = binder.source as Physics.Surfaces.ISurface;
                    if (surface.IsRectangleColliding(particle)) return binder;
                }
            }
            return null;
        }

        public List<Visual> PickSurfaces(double x, double y, double w, double h)
        {
            List<Visual> picks = new List<Visual>();
            Physics.Primitives.RectangleParticle particle = new Physics.Primitives.RectangleParticle(x, y, w, h);
            foreach (Visual binder in binders)
            {
                if (binder.source is Physics.Surfaces.ISurface)
                {
                    Physics.Surfaces.AbstractSurface surface = binder.source as Physics.Surfaces.AbstractSurface;
                    if (surface.TestBoundingBox(x, y, x+w, y+h)) picks.Add(binder);
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

            foreach (Visual binder in binders)
            {
                if (binder.source is Physics.Surfaces.ISurface)
                {
                    string xaml = binder.RenderXAML();
                    sb.AppendLine(String.Format("<!-- entity {0} -->", binder.source.name));
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

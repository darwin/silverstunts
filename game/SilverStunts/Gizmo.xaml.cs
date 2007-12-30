using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Physics;
using Physics.Composites;
using Physics.Constraints;
using Physics.Primitives;
using Physics.Surfaces;
using Physics.Base;
using Physics.Util;

namespace SilverStunts
{
    public class EditGizmo : Control, IGizmo
    {
        protected Editor editor;
        public PhysicsObject editable;
        public Visual visual;
        protected Canvas content;

        bool dragging = false;
        Point startDrag;
        int gridX;
        int gridY;
        Canvas gridMarker;
        Canvas gridTarget;
        System.Windows.Shapes.Line gridLine;
        Point gridShift;

        public EditGizmo(Editor editor, Visual visual)
        {
            this.editor = editor;
            this.editable = visual.source;
            this.visual = visual;

            System.IO.Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Gizmo.xaml");
            content = this.InitializeFromXaml(new System.IO.StreamReader(s).ReadToEnd()) as Canvas;
            content.Opacity = 0.5;
            content.Background = Brushes.Gray;

            visual.content.Children.Add(this);
        }

        virtual public void Update() { }
        
        virtual public void StartDrag() 
        {
            content.Background = Brushes.Yellow;
        }

        virtual public void DragAction(Point shift) { }
        virtual public void EndDrag() 
        {
            content.Background = Brushes.Gray;
            Page.Current.level.UpdateEntitiesSource(String.Format("{0} = \\w+\\(.*\\)", editable.name), editor.GenerateCode(editable));
        }

        virtual public bool HitTest(MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            return p.X >= ((TranslateTransform)content.RenderTransform).X && p.X <= ((TranslateTransform)content.RenderTransform).X + content.Width &&
                p.Y >= ((TranslateTransform)content.RenderTransform).Y && p.Y <= ((TranslateTransform)content.RenderTransform).Y + content.Height;
        }

        public void HandleMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            DoneGridControl();
            this.dragging = false;
            EndDrag();
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (this.dragging == false) return;
            Point shift = new Point(e.GetPosition(editor.scroller).X - startDrag.X, e.GetPosition(editor.scroller).Y - startDrag.Y);
            shift.X = Math.Round(shift.X);
            shift.Y = Math.Round(shift.Y);
            DragAction(shift);
            visual.Update();
        }

        public void HandleMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.dragging = true;
            startDrag = e.GetPosition(editor.scroller);
            StartDrag();
        }

        public void Destroy()
        {
            visual.content.Children.Remove(this);
        }

        public void DoneGridControl()
        {
            if (gridMarker != null)
            {
                editor.workspace.Children.Remove(gridMarker);
                gridMarker = null;
            }
            if (gridTarget != null)
            {
                editor.workspace.Children.Remove(gridTarget);
                gridTarget = null;
            }
            if (gridLine != null)
            {
                editor.workspace.Children.Remove(gridLine);
                gridLine = null;
            }
        }

        public Canvas CreateGridGizmo()
        {
            Canvas gridGizmo = new Canvas();
            gridGizmo.Width = 7;
            gridGizmo.Height = 7;
            gridGizmo.Background = Brushes.Transparent;
            gridGizmo.Opacity = 0.5;

            System.Windows.Shapes.Line l1 = new System.Windows.Shapes.Line();
            l1.X1 = 0;
            l1.Y1 = 0;
            l1.X2 = 8;
            l1.Y2 = 8;
            l1.Stroke = Brushes.Black;

            System.Windows.Shapes.Line l2 = new System.Windows.Shapes.Line();
            l2.X1 = 0;
            l2.Y1 = 8;
            l2.X2 = 8;
            l2.Y2 = 0;
            l2.Stroke = Brushes.Black;

            gridGizmo.Children.Add(l1);
            gridGizmo.Children.Add(l2);

            return gridGizmo;
        }

        public int AlignToGridX(double tx)
        {
            int px = (int)tx;
            int rx = px % 10;
            int gx = px - rx;
            if (rx >= 5) gx += 10;
            return gx;
        }

        public int AlignToGridY(double ty)
        {
            int py = (int)ty;
            int ry = py % 10;
            int gy = py - ry;
            if (ry >= 5) gy += 10;
            return gy;
        }

        public void InitGridControl(double tx, double ty)
        {
            DoneGridControl();

            gridX = AlignToGridX(tx);
            gridY = AlignToGridX(ty);

            gridShift.X = tx - gridX;
            gridShift.Y = ty - gridY;

            gridMarker = CreateGridGizmo();
            gridTarget = CreateGridGizmo();

            TranslateTransform tt = new TranslateTransform();
            tt.X = gridX - 3.5;
            tt.Y = gridY - 3.5;
            gridMarker.RenderTransform = tt;

            TranslateTransform tt1 = new TranslateTransform();
            tt1.X = (int)tx - 3.5;
            tt1.Y = (int)ty - 3.5;
            gridTarget.RenderTransform = tt1;

            editor.workspace.Children.Add(gridMarker);
            editor.workspace.Children.Add(gridTarget);

            double[] dash = { 10, 5 };
            gridLine = new System.Windows.Shapes.Line();
            gridLine.X1 = gridX;
            gridLine.Y1 = gridY;
            gridLine.X2 = (int)tx;
            gridLine.Y2 = (int)ty;
            gridLine.Stroke = Brushes.Black;
            gridLine.StrokeDashArray = dash;

            editor.workspace.Children.Add(gridLine);
        }

        public void UpdateGridControl(double tx, double ty)
        {
            TranslateTransform tt1 = new TranslateTransform();
            tt1.X = (int)tx - 3.5;
            tt1.Y = (int)ty - 3.5;
            gridTarget.RenderTransform = tt1;
            gridLine.X2 = (int)tx;
            gridLine.Y2 = (int)ty;
        }
    }

    public class SelectionGizmo : Control, IGizmo
    {
        protected Editor editor;
        protected Canvas content;

        public double x;
        public double y;
        public double w;
        public double h;


        bool dragging = false;
        Point startDrag;
        Point endDrag;
        System.Windows.Shapes.Rectangle selectionRect;

        public SelectionGizmo(Editor editor)
        {
            this.editor = editor;

            System.IO.Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Gizmo.xaml");
            content = this.InitializeFromXaml(new System.IO.StreamReader(s).ReadToEnd()) as Canvas;

            double[] dash = { 10, 5 };
            selectionRect = new Rectangle();
            selectionRect.Fill = Brushes.FromColor(Color.FromRgb(0xbb, 0xbb, 0xbb));
            selectionRect.Stroke = Brushes.Black;
            selectionRect.StrokeDashArray = dash;
            selectionRect.Opacity = 0.6;
            selectionRect.Visibility = Visibility.Visible;
            content.Children.Add(selectionRect);
            editor.workspace.Children.Add(this);
        }

        virtual public void Update() 
        { 
            double lx = startDrag.X < endDrag.X ? lx = startDrag.X : lx = endDrag.X;
            double rx = startDrag.X > endDrag.X ? rx = startDrag.X : rx = endDrag.X;
            double ly = startDrag.Y < endDrag.Y ? ly = startDrag.Y : ly = endDrag.Y;
            double ry = startDrag.Y > endDrag.Y ? ry = startDrag.Y : ry = endDrag.Y;

            TranslateTransform tt = new TranslateTransform();
            x = tt.X = lx;
            y = tt.Y = ly;
            selectionRect.RenderTransform = tt;
            w = selectionRect.Width = rx - lx;
            h = selectionRect.Height = ry - ly;
        }

        virtual public bool HitTest(MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            return p.X >= ((TranslateTransform)content.RenderTransform).X && p.X <= ((TranslateTransform)content.RenderTransform).X + content.Width &&
                p.Y >= ((TranslateTransform)content.RenderTransform).Y && p.Y <= ((TranslateTransform)content.RenderTransform).Y + content.Height;
        }

        public void HandleMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            endDrag = editor.GetWorldPos(e);
            Update();
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (dragging == false) return;
            endDrag = editor.GetWorldPos(e);
            Update();
        }

        public void HandleMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.dragging = true;
            endDrag = startDrag = editor.GetWorldPos(e);
            Update();
        }

        public void Destroy()
        {
            editor.workspace.Children.Remove(this);
        }
    }

    public class MoveGizmo : EditGizmo
    {
        public MoveGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
        }
    }

    public class RectangleMoveGizmo : MoveGizmo
    {
        RectangleSurface target;
        double tcx;
        double tcy;

        public RectangleMoveGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (RectangleSurface)editable;
            Update();
        }

        public override void Update()
        {
            AbstractSurface tile = (AbstractSurface)target;

            content.Width = tile.Verts[0].X - tile.Verts[2].X + 2 * 10;
            content.Height = tile.Verts[0].Y - tile.Verts[1].Y + 2 * 10;

            TranslateTransform tt = new TranslateTransform();
            tt.X = -10;
            tt.Y = -10;
            content.RenderTransform = tt;
        }

        override public void StartDrag() 
        {
            base.StartDrag();
            tcx = target.Center.X;
            tcy = target.Center.Y;

            InitGridControl(tcx, tcy);
        }

        override public void DragAction(Point shift) 
        {
            double nctx = tcx + shift.X;
            double ncty = tcy + shift.Y;
            
            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                double w2 = target.Width / 2;
                double h2 = target.Height / 2;
                nctx = AlignToGridX(nctx - w2) + w2;
                ncty = AlignToGridY(ncty - h2) + h2;
            }

            target.Init(nctx, ncty, target.Width, target.Height);
            
            UpdateGridControl(nctx, ncty);
        }
    }

    public class RectangleResizeTLGizmo : EditGizmo
    {
        RectangleSurface target;
        double lx;
        double ty;
        double rx;
        double by;

        public RectangleResizeTLGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (RectangleSurface)editable;
            Update();
        }

        override public void StartDrag()
        {
            base.StartDrag();
            lx = target.Center.X - target.Width / 2;
            ty = target.Center.Y - target.Height / 2;
            rx = target.Center.X + target.Width / 2;
            by = target.Center.Y + target.Height / 2;

            InitGridControl(lx, ty);
        }

        public override void Update()
        {
            AbstractSurface tile = (AbstractSurface)target;

            content.Width = 10;
            content.Height = 10;

            TranslateTransform tt = new TranslateTransform();
            tt.X = -5;
            tt.Y = -5;
            content.RenderTransform = tt;
        }

        override public void DragAction(Point shift)
        {
            double ntlx = lx + shift.X;
            double ntly = ty + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                ntlx = AlignToGridX(ntlx);
                ntly = AlignToGridY(ntly);
            }

            double ncx = (ntlx + rx) / 2;
            double ncy = (ntly + by) / 2;
            double nw = Math.Abs(rx - ntlx);
            double nh = Math.Abs(by - ntly);

            target.Init(ncx, ncy, nw, nh);

            UpdateGridControl(ntlx, ntly);
        }
    }

    public class RectangleResizeTRGizmo : EditGizmo
    {
        RectangleSurface target;
        double lx;
        double ty;
        double rx;
        double by;

        public RectangleResizeTRGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (RectangleSurface)editable;
            Update();
        }

        override public void StartDrag()
        {
            base.StartDrag();
            lx = target.Center.X - target.Width / 2;
            ty = target.Center.Y - target.Height / 2;
            rx = target.Center.X + target.Width / 2;
            by = target.Center.Y + target.Height / 2;

            InitGridControl(rx, ty);
        }

        public override void Update()
        {

            AbstractSurface tile = (AbstractSurface)target;

            content.Width = 10;
            content.Height = 10;

            double w = tile.Verts[0].X - tile.Verts[2].X;
            double h = tile.Verts[0].Y - tile.Verts[1].Y;

            TranslateTransform tt = new TranslateTransform();
            tt.X = w-5;
            tt.Y = -5;
            content.RenderTransform = tt;
        }

        override public void DragAction(Point shift)
        {
            double ntrx = rx + shift.X;
            double ntry = ty + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                ntrx = AlignToGridX(ntrx);
                ntry = AlignToGridY(ntry);
            }

            double ncx = (ntrx + lx) / 2;
            double ncy = (ntry + by) / 2;
            double nw = Math.Abs(lx - ntrx);
            double nh = Math.Abs(by - ntry);

            target.Init(ncx, ncy, nw, nh);

            UpdateGridControl(ntrx, ntry);
        }
    }

    public class RectangleResizeBRGizmo : EditGizmo
    {
        RectangleSurface target;
        double lx;
        double ty;
        double rx;
        double by;

        public RectangleResizeBRGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (RectangleSurface)editable;
            Update();
        }

        override public void StartDrag()
        {
            base.StartDrag();
            lx = target.Center.X - target.Width / 2;
            ty = target.Center.Y - target.Height / 2;
            rx = target.Center.X + target.Width / 2;
            by = target.Center.Y + target.Height / 2;

            InitGridControl(rx, by);
        }

        public override void Update()
        {

            AbstractSurface tile = (AbstractSurface)target;

            content.Width = 10;
            content.Height = 10;

            double w = tile.Verts[0].X - tile.Verts[2].X;
            double h = tile.Verts[0].Y - tile.Verts[1].Y;

            TranslateTransform tt = new TranslateTransform();
            tt.X = w-5;
            tt.Y = h-5;
            content.RenderTransform = tt;
        }

        override public void DragAction(Point shift)
        {
            double nbrx = rx + shift.X;
            double nbry = by + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                nbrx = AlignToGridX(nbrx);
                nbry = AlignToGridY(nbry);
            }

            double ncx = (nbrx + lx) / 2;
            double ncy = (nbry + ty) / 2;
            double nw = Math.Abs(lx - nbrx);
            double nh = Math.Abs(ty - nbry);

            target.Init(ncx, ncy, nw, nh);

            UpdateGridControl(nbrx, nbry);
        }
    }

    public class RectangleResizeBLGizmo : EditGizmo
    {
        RectangleSurface target;
        double lx;
        double ty;
        double rx;
        double by;

        public RectangleResizeBLGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (RectangleSurface)editable;
            Update();
        }

        override public void StartDrag()
        {
            base.StartDrag();
            lx = target.Center.X - target.Width / 2;
            ty = target.Center.Y - target.Height / 2;
            rx = target.Center.X + target.Width / 2;
            by = target.Center.Y + target.Height / 2;

            InitGridControl(lx, by);
        }

        public override void Update()
        {

            AbstractSurface tile = (AbstractSurface)target;

            content.Width = 10;
            content.Height = 10;

            double w = tile.Verts[0].X - tile.Verts[2].X;
            double h = tile.Verts[0].Y - tile.Verts[1].Y;

            TranslateTransform tt = new TranslateTransform();
            tt.X = -5;
            tt.Y = h-5;
            content.RenderTransform = tt;
        }

        override public void DragAction(Point shift)
        {
            double nblx = lx + shift.X;
            double nbly = by + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                nblx = AlignToGridX(nblx);
                nbly = AlignToGridY(nbly);
            }

            double ncx = (nblx + rx) / 2;
            double ncy = (nbly + ty) / 2;
            double nw = Math.Abs(rx - nblx);
            double nh = Math.Abs(ty - nbly);

            target.Init(ncx, ncy, nw, nh);

            UpdateGridControl(nblx, nbly);
        }
    }

    public class CircleMoveGizmo : MoveGizmo
    {
        CircleSurface target;
        double tcx;
        double tcy;

        public CircleMoveGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (CircleSurface)editable;
            Update();
        }

        public override void Update()
        {
            AbstractSurface tile = (AbstractSurface)target;

            content.Width = target.Radius * 2 + 2 * 15;
            content.Height = target.Radius * 2 + 2 * 15;

            TranslateTransform tt = new TranslateTransform();
            tt.X = -15;
            tt.Y = -15;
            content.RenderTransform = tt;
        }

        override public void StartDrag()
        {
            base.StartDrag();
            tcx = target.Center.X;
            tcy = target.Center.Y;

            InitGridControl(tcx, tcy);
        }

        override public void DragAction(Point shift)
        {
            double ntcx = tcx + shift.X;
            double ntcy = tcy + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                ntcx = AlignToGridX(ntcx);
                ntcy = AlignToGridY(ntcy);
            }

            target.Init(ntcx, ntcy, target.Radius);

            UpdateGridControl(ntcx, ntcy);
        }
    }

    public class CircleRadiusGizmo : EditGizmo
    {
        CircleSurface target;
        double tr;

        public CircleRadiusGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (CircleSurface)editable;
            Update();
        }

        public override void Update()
        {
            content.Width = 10;
            content.Height = 10;

            TranslateTransform tt = new TranslateTransform();
            tt.X = target.Radius-5;
            tt.Y = -5;
            content.RenderTransform = tt;
        }

        override public void StartDrag()
        {
            base.StartDrag();
            tr = target.Radius;
        }

        override public void DragAction(Point shift)
        {
            double nr = tr - shift.Y;
            if (nr < 10) nr = 10;
            target.Init(target.Center.X, target.Center.Y, nr);
        }
    }

    public class LineSurfaceMoveGizmo : MoveGizmo
    {
        LineSurface target;
        Vector tp1;
        Vector tp2;

        public LineSurfaceMoveGizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (LineSurface)editable;
            Update();
        }

        public override void Update()
        {
            AbstractSurface tile = (AbstractSurface)target;

            content.Width = Math.Abs(target.P2.X - target.P1.X) + 2 * 10;
            content.Height = Math.Abs(target.P2.Y - target.P1.Y) + 2 * 10;

            double lx = target.P1.X < target.P2.X ? target.P1.X : target.P2.X;
            double ly = target.P1.Y < target.P2.Y ? target.P1.Y : target.P2.Y;

            TranslateTransform tt = new TranslateTransform();
            tt.X = lx - 10;
            tt.Y = ly - 10;
            content.RenderTransform = tt;
        }

        override public void StartDrag()
        {
            base.StartDrag();
            tp1 = target.OP1;
            tp2 = target.OP2;

            InitGridControl((tp1.X + tp2.X) / 2, (tp1.Y + tp2.Y) / 2);
        }

        override public void DragAction(Point shift)
        {
            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                shift.X = AlignToGridX(shift.X);
                shift.Y = AlignToGridY(shift.Y);
            }

            double ntp1x = tp1.X + shift.X;
            double ntp1y = tp1.Y + shift.Y;
            double ntp2x = tp2.X + shift.X;
            double ntp2y = tp2.Y + shift.Y;

            target.Init(ntp1x, ntp1y, ntp2x, ntp2y);

            UpdateGridControl((ntp1x+ntp2x)/2, (ntp1y+ntp2y)/2);
        }
    }

    public class LineSurfaceH1Gizmo : EditGizmo
    {
        LineSurface target;
        Vector tp1;

        public LineSurfaceH1Gizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (LineSurface)editable;
            Update();
        }

        override public void StartDrag()
        {
            base.StartDrag();
            tp1 = target.OP1;
            InitGridControl(tp1.X, tp1.Y);
        }

        public override void Update()
        {

            AbstractSurface tile = (AbstractSurface)target;

            content.Width = 10;
            content.Height = 10;

            TranslateTransform tt = new TranslateTransform();
            tt.X = target.P1.X - 5;
            tt.Y = target.P1.Y - 5;
            content.RenderTransform = tt;
        }

        override public void DragAction(Point shift)
        {
            double nx = tp1.X + shift.X;
            double ny = tp1.Y + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                nx = AlignToGridX(nx);
                ny = AlignToGridY(ny);
            }

            target.Init(nx, ny, target.OP2.X, target.OP2.Y);

            UpdateGridControl(nx, ny);
        }
    }

    public class LineSurfaceH2Gizmo : EditGizmo
    {
        LineSurface target;
        Vector tp2;

        public LineSurfaceH2Gizmo(Editor parent, Visual visual)
            : base(parent, visual)
        {
            target = (LineSurface)editable;
            Update();
        }

        override public void StartDrag()
        {
            base.StartDrag();
            tp2 = target.OP2;
            InitGridControl(tp2.X, tp2.Y);
        }

        public override void Update()
        {
            AbstractSurface tile = (AbstractSurface)target;

            content.Width = 10;
            content.Height = 10;

            TranslateTransform tt = new TranslateTransform();
            tt.X = target.P2.X - 5;
            tt.Y = target.P2.Y - 5;
            content.RenderTransform = tt;
        }

        override public void DragAction(Point shift)
        {
            double nx = tp2.X + shift.X;
            double ny = tp2.Y + shift.Y;

            // if grid enabled, align to grid
            if (editor.GridEnabled)
            {
                nx = AlignToGridX(nx);
                ny = AlignToGridY(ny);
            }

            target.Init(target.OP1.X, target.OP1.Y, nx, ny);

            UpdateGridControl(nx, ny);
        }
    }
}

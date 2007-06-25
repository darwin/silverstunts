using System;
namespace SilverStunts
{
    public interface IGizmo
    {
        void Destroy();
        bool HitTest(System.Windows.Input.MouseEventArgs e);
        void HandleMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e);
        void HandleMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e);
        void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e);
        void Update();
    }
}

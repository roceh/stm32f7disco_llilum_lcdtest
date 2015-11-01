namespace Managed.UI
{
    public class Button : Control
    {
        public string Text { get; set; }

        public Button(Application application) : base(application)
        { }

        public override void Draw()
        {
            base.Draw();

            Application.Display.DrawRectangle(AbsoluteLeft, AbsoluteTop, Width, Height, 0xFF000000);
            Application.Display.DrawCenteredString(Text, AbsoluteLeft, AbsoluteTop, Width, Height, Application.SystemFont);
        }
    }
}

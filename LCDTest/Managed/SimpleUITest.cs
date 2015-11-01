using Managed.UI;

namespace Managed
{
    /// <summary>
    /// Test for simple ui
    /// </summary>
    public class SimpleUITest
    {
        public void Run()
        {
            Application application = new Application();
            application.ShowDebug = true;
            Panel panel = new Panel(application) { Left = 0, Top = 0, Width = 480, Height = 272 };

            Label label = new Label(application) { Left = 10, Top = 30, Width = 200, Height = 32, Text = "Hello World" };
            panel.Add(label);

            Button button = new Button(application) { Left = 10, Top = 100, Width = 200, Height = 100, Text = "Touch" };
            button.TouchStart += (s, e) => { label.Text = "Touched"; };
            panel.Add(button);

            ListView listview = new ListView(application) { Left = 239, Top = 0, Width = 240, Height = 272, RowHeight = 30 };

            for (int i = 0; i < 1000; i++)
            {
                listview.Items.Add("ListView Item " + i.ToString());
            }

            panel.Add(listview);

            application.Run(panel);
        }
    }
}

namespace ParBoil.Utility;

public partial class OptionBox : Form
{
    public OptionBox(string bodyText = "")
    {
        InitializeComponent();

        if (bodyText != "")
        {
            Body.Text = bodyText;
            ClientSize = new Size(ClientSize.Width, Body.Height + 24);
        }
        else
        {
            Controls.Remove(Body);
            ClientSize = new Size(ClientSize.Width, 12);
        }
    }

    private string SelectedOption = "";
    private List<Button> Options = new List<Button>();
    private List<Panel> Panels = new List<Panel>();

    internal void AddOptions(params string[] labels)
    {
        var panel = new Panel()
        {
            Height = 24,
            Width = 0,
        };

        int maxWidth = Options.Count > 0 ? ClientSize.Width : 0;
        int x = 12;

        foreach (string label in labels)
        {
            var option = new Button();
            option.Text = label;
            option.Width = TextRenderer.MeasureText(label, Font).Width + 12;
            option.Location = new Point(x, 0);
            x += option.Width + 12;
            panel.Width = x;
            option.Click += delegate
            {
                SelectedOption = option.Text;
                this.Close();
            };
            Options.Add(option);
            panel.Controls.Add(option);
        }

        Controls.Add(panel);

        if (maxWidth < panel.Width)
        {
            maxWidth = panel.Width;
            panel.Location = new Point(0, ClientSize.Height);

            foreach (var oldPanel in Panels)
            {
                oldPanel.Location = new Point((maxWidth - oldPanel.Width) / 2, oldPanel.Location.Y);
            }
        }

        else
        {
            panel.Location = new Point((maxWidth - panel.Width) / 2, ClientSize.Height);
        }

        Panels.Add(panel);


        ClientSize = new Size(maxWidth, ClientSize.Height + 35);
    }

    internal string ShowOptions()
    {
        ShowDialog();

        return SelectedOption;
    }
}

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

    private int SelectedOption = -1;
    private Dictionary<string, int> OptionLookup = new Dictionary<string, int>();
    private Button[] Options;
    private List<Panel> Rows = new List<Panel>();

    internal void AddOptions(params string[] labels)
    {
        var panel = new Panel()
        {
            Height = 24,
            Width = 0,
        };

        int index;
        if (Options == null)
        {
            index = 0;
            Options = new Button[labels.Length];
        }
        else
        {
            index = Options.Length;

            var temp = new Button[Options.Length + labels.Length];
            Options.CopyTo(temp, 0);
            Options = temp;
        }

        int maxWidth = Options.Length == labels.Length ? ClientSize.Width : 0;
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
                SelectedOption = OptionLookup[option.Text];
                this.Close();
            };

            OptionLookup.Add(label, index);
            Options[index++] = option;
            panel.Controls.Add(option);
        }

        Controls.Add(panel);

        if (maxWidth < panel.Width)
        {
            maxWidth = panel.Width;
            panel.Location = new Point(0, ClientSize.Height);

            foreach (var oldPanel in Rows)
            {
                oldPanel.Location = new Point((maxWidth - oldPanel.Width) / 2, oldPanel.Location.Y);
            }
        }

        else
        {
            panel.Location = new Point((maxWidth - panel.Width) / 2, ClientSize.Height);
        }

        Rows.Add(panel);


        ClientSize = new Size(maxWidth, ClientSize.Height + 35);
    }

    internal int ShowOptions()
    {
        ShowDialog();

        return SelectedOption;
    }

    internal string GetOptionAt(int index) => Options[index].Text;

    internal void SetAcceptOption(string accept) => AcceptButton = Options[OptionLookup[accept]];
    internal void SetCancelOption(string cancel) => CancelButton = Options[OptionLookup[cancel]];
    internal void SetAcceptOption(uint option) => AcceptButton = Options[option];
    internal void SetCancelOption(uint option) => CancelButton = Options[option];
}

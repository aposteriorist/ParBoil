namespace ParBoil.Utility;

partial class OptionBox
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Body = new Label();
        SuspendLayout();
        // 
        // Body
        // 
        Body.AutoSize = true;
        Body.Location = new Point(12, 9);
        Body.Name = "Body";
        Body.Size = new Size(69, 15);
        Body.TabIndex = 0;
        Body.Text = "Default Text";
        // 
        // OptionBox
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(284, 57);
        Controls.Add(Body);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "OptionBox";
        ShowIcon = false;
        SizeGripStyle = SizeGripStyle.Hide;
        Text = "Options";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label Body;
}
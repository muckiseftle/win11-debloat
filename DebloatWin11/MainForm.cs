using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DebloatWin11
{
    public partial class MainForm : Form
    {
        private Debloater _debloater;

        public MainForm()
        {
            InitializeComponent();
            _debloater = new Debloater(Log);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            Task.Run(() =>
            {
                _debloater.RunAll();
                Invoke(new Action(() => startButton.Enabled = true));
            });
        }

        private void InitializeComponent()
        {
            this.startButton = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(12, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(148, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(12, 41);
            this.logBox.Multiline = true;
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logBox.Size = new System.Drawing.Size(760, 407);
            this.logBox.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.startButton);
            this.Name = "MainForm";
            this.Text = "Windows 11 Debloater";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TextBox logBox;

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }
            logBox.AppendText(message + Environment.NewLine);
        }
    }
}

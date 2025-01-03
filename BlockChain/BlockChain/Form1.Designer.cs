namespace BlockChain
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            connectbutton = new Button();
            destinationport = new TextBox();
            chainbox = new RichTextBox();
            minebox = new RichTextBox();
            minebutton = new Button();
            serverbox = new TextBox();
            username = new TextBox();
            SuspendLayout();
            // 
            // connectbutton
            // 
            connectbutton.Location = new Point(165, 27);
            connectbutton.Name = "connectbutton";
            connectbutton.Size = new Size(75, 23);
            connectbutton.TabIndex = 0;
            connectbutton.Text = "Connect";
            connectbutton.UseVisualStyleBackColor = true;
            connectbutton.Click += connectbutton_Click;
            // 
            // destinationport
            // 
            destinationport.Location = new Point(45, 27);
            destinationport.Name = "destinationport";
            destinationport.Size = new Size(100, 23);
            destinationport.TabIndex = 1;
            // 
            // chainbox
            // 
            chainbox.Location = new Point(45, 74);
            chainbox.Name = "chainbox";
            chainbox.Size = new Size(345, 342);
            chainbox.TabIndex = 2;
            chainbox.Text = "";
            // 
            // minebox
            // 
            minebox.Location = new Point(411, 74);
            minebox.Name = "minebox";
            minebox.Size = new Size(331, 342);
            minebox.TabIndex = 3;
            minebox.Text = "";
            // 
            // minebutton
            // 
            minebutton.Location = new Point(526, 27);
            minebutton.Name = "minebutton";
            minebutton.Size = new Size(75, 23);
            minebutton.TabIndex = 4;
            minebutton.Text = "Mine";
            minebutton.UseVisualStyleBackColor = true;
            minebutton.Click += minebutton_Click;
            // 
            // serverbox
            // 
            serverbox.Location = new Point(411, 27);
            serverbox.Name = "serverbox";
            serverbox.Size = new Size(100, 23);
            serverbox.TabIndex = 5;
            // 
            // username
            // 
            username.Location = new Point(622, 28);
            username.Name = "username";
            username.Size = new Size(100, 23);
            username.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(username);
            Controls.Add(serverbox);
            Controls.Add(minebutton);
            Controls.Add(minebox);
            Controls.Add(chainbox);
            Controls.Add(destinationport);
            Controls.Add(connectbutton);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button connectbutton;
        private TextBox destinationport;
        private RichTextBox chainbox;
        private Button minebutton;
        private TextBox serverbox;
        private RichTextBox minebox;
        private TextBox username;
    }
}

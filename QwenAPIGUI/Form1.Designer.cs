namespace QwenAPIGUI
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
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            groupBox1 = new GroupBox();
            webapi = new Button();
            passwordInput = new TextBox();
            emailInput = new TextBox();
            button1 = new Button();
            groupBox2 = new GroupBox();
            bxList = new ListBox();
            newChatBtn = new Button();
            refreshBtn = new Button();
            chatList = new ListBox();
            userInput = new TextBox();
            sendMessageBtn = new Button();
            chatHistory = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(0, 12);
            webView21.Margin = new Padding(4, 3, 4, 3);
            webView21.Name = "webView21";
            webView21.Size = new Size(362, 601);
            webView21.Source = new Uri("https://chat.qwen.ai/auth", UriKind.Absolute);
            webView21.TabIndex = 0;
            webView21.ZoomFactor = 1D;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(webapi);
            groupBox1.Controls.Add(passwordInput);
            groupBox1.Controls.Add(emailInput);
            groupBox1.Controls.Add(button1);
            groupBox1.Location = new Point(693, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(383, 126);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Auth";
            // 
            // webapi
            // 
            webapi.Location = new Point(275, 83);
            webapi.Name = "webapi";
            webapi.Size = new Size(88, 29);
            webapi.TabIndex = 7;
            webapi.Tag = "Start WebAPI service";
            webapi.Text = "WebAPI";
            webapi.UseVisualStyleBackColor = true;
            webapi.Click += webapi_Click;
            // 
            // passwordInput
            // 
            passwordInput.Location = new Point(82, 53);
            passwordInput.Margin = new Padding(4, 3, 4, 3);
            passwordInput.Name = "passwordInput";
            passwordInput.PasswordChar = '*';
            passwordInput.Size = new Size(222, 24);
            passwordInput.TabIndex = 6;
            passwordInput.UseSystemPasswordChar = true;
            // 
            // emailInput
            // 
            emailInput.Location = new Point(82, 23);
            emailInput.Margin = new Padding(4, 3, 4, 3);
            emailInput.Name = "emailInput";
            emailInput.Size = new Size(222, 24);
            emailInput.TabIndex = 5;
            // 
            // button1
            // 
            button1.Location = new Point(137, 83);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(115, 29);
            button1.TabIndex = 4;
            button1.Text = "Login";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(bxList);
            groupBox2.Controls.Add(newChatBtn);
            groupBox2.Controls.Add(refreshBtn);
            groupBox2.Controls.Add(chatList);
            groupBox2.Location = new Point(369, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(318, 601);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "Chat Sessions";
            // 
            // bxList
            // 
            bxList.FormattingEnabled = true;
            bxList.Location = new Point(6, 528);
            bxList.Name = "bxList";
            bxList.Size = new Size(306, 58);
            bxList.TabIndex = 9;
            // 
            // newChatBtn
            // 
            newChatBtn.Location = new Point(196, 33);
            newChatBtn.Margin = new Padding(4, 3, 4, 3);
            newChatBtn.Name = "newChatBtn";
            newChatBtn.Size = new Size(115, 29);
            newChatBtn.TabIndex = 8;
            newChatBtn.Text = "New";
            newChatBtn.UseVisualStyleBackColor = true;
            newChatBtn.Click += newChatBtn_Click;
            // 
            // refreshBtn
            // 
            refreshBtn.Location = new Point(7, 33);
            refreshBtn.Margin = new Padding(4, 3, 4, 3);
            refreshBtn.Name = "refreshBtn";
            refreshBtn.Size = new Size(115, 29);
            refreshBtn.TabIndex = 7;
            refreshBtn.Text = "Refresh";
            refreshBtn.UseVisualStyleBackColor = true;
            refreshBtn.Click += refreshBtn_Click;
            // 
            // chatList
            // 
            chatList.FormattingEnabled = true;
            chatList.Location = new Point(6, 68);
            chatList.Name = "chatList";
            chatList.Size = new Size(306, 454);
            chatList.TabIndex = 6;
            chatList.SelectedIndexChanged += chatList_SelectedIndexChanged;
            // 
            // userInput
            // 
            userInput.Location = new Point(693, 450);
            userInput.MaxLength = 999999;
            userInput.Multiline = true;
            userInput.Name = "userInput";
            userInput.Size = new Size(383, 163);
            userInput.TabIndex = 9;
            userInput.TextChanged += userInput_TextChanged;
            // 
            // sendMessageBtn
            // 
            sendMessageBtn.Enabled = false;
            sendMessageBtn.Location = new Point(991, 580);
            sendMessageBtn.Name = "sendMessageBtn";
            sendMessageBtn.Size = new Size(75, 26);
            sendMessageBtn.TabIndex = 10;
            sendMessageBtn.Text = "Send";
            sendMessageBtn.UseVisualStyleBackColor = true;
            sendMessageBtn.Click += sendMessageBtn_Click;
            // 
            // chatHistory
            // 
            chatHistory.AutoScroll = true;
            chatHistory.Location = new Point(693, 145);
            chatHistory.Name = "chatHistory";
            chatHistory.Size = new Size(382, 299);
            chatHistory.TabIndex = 11;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1087, 618);
            Controls.Add(chatHistory);
            Controls.Add(sendMessageBtn);
            Controls.Add(userInput);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(webView21);
            Font = new Font("Cascadia Code", 10.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private GroupBox groupBox1;
        private TextBox passwordInput;
        private TextBox emailInput;
        private Button button1;
        private GroupBox groupBox2;
        private Button refreshBtn;
        private ListBox chatList;
        private Button newChatBtn;
        private ListBox bxList;
        private TextBox userInput;
        private Button sendMessageBtn;
        private FlowLayoutPanel chatHistory;
        private Button webapi;
    }
}

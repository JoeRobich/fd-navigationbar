namespace NavigationBar
{
    partial class NavigationBar
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.memberComboBox = new System.Windows.Forms.ComboBox();
            this.classComboBox = new System.Windows.Forms.ComboBox();
            this.tablePanel = new System.Windows.Forms.TableLayoutPanel();
            this.bottomBorder = new System.Windows.Forms.Panel();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.tablePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // memberComboBox
            // 
            this.memberComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.memberComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.memberComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.memberComboBox.FormattingEnabled = true;
            this.memberComboBox.Location = new System.Drawing.Point(195, 3);
            this.memberComboBox.MaxDropDownItems = 25;
            this.memberComboBox.Name = "memberComboBox";
            this.memberComboBox.Size = new System.Drawing.Size(186, 21);
            this.memberComboBox.Sorted = true;
            this.memberComboBox.TabIndex = 0;
            this.memberComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox_DrawItem);
            this.memberComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // classComboBox
            // 
            this.classComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.classComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.classComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.classComboBox.FormattingEnabled = true;
            this.classComboBox.Location = new System.Drawing.Point(3, 3);
            this.classComboBox.MaxDropDownItems = 25;
            this.classComboBox.Name = "classComboBox";
            this.classComboBox.Size = new System.Drawing.Size(186, 21);
            this.classComboBox.Sorted = true;
            this.classComboBox.TabIndex = 1;
            this.classComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox_DrawItem);
            this.classComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // tablePanel
            // 
            this.tablePanel.ColumnCount = 2;
            this.tablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tablePanel.Controls.Add(this.memberComboBox, 1, 0);
            this.tablePanel.Controls.Add(this.classComboBox, 0, 0);
            this.tablePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.tablePanel.Location = new System.Drawing.Point(0, 0);
            this.tablePanel.Name = "tablePanel";
            this.tablePanel.RowCount = 1;
            this.tablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tablePanel.Size = new System.Drawing.Size(384, 27);
            this.tablePanel.TabIndex = 2;
            // 
            // bottomBorder
            // 
            this.bottomBorder.BackColor = System.Drawing.Color.DimGray;
            this.bottomBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomBorder.Location = new System.Drawing.Point(0, 30);
            this.bottomBorder.Name = "bottomBorder";
            this.bottomBorder.Size = new System.Drawing.Size(384, 1);
            this.bottomBorder.TabIndex = 3;
            // 
            // updateTimer
            // 
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // OutlineBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bottomBorder);
            this.Controls.Add(this.tablePanel);
            this.Name = "OutlineBar";
            this.Size = new System.Drawing.Size(384, 31);
            this.tablePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox memberComboBox;
        private System.Windows.Forms.ComboBox classComboBox;
        private System.Windows.Forms.TableLayoutPanel tablePanel;
        private System.Windows.Forms.Panel bottomBorder;
        private System.Windows.Forms.Timer updateTimer;
    }
}

//namespace ZooMenuApp.Forms
//{
//    partial class FormDietPlanDetails
//    {
//        private System.ComponentModel.IContainer components = null;
//        private System.Windows.Forms.Panel panelTop;
//        private System.Windows.Forms.ComboBox cmbDietPlans;
//        private System.Windows.Forms.Button btnRefreshPlans;
//        private System.Windows.Forms.DataGridView dgvDetails;
//        private System.Windows.Forms.Button btnReload;
//        private System.Windows.Forms.Button btnAdd;
//        private System.Windows.Forms.Button btnDelete;
//        private System.Windows.Forms.Button btnCalculate;
//        private System.Windows.Forms.Label lblStatus;
//        private System.Windows.Forms.Label lblInfo;
//        private System.Windows.Forms.Label lblDietPlan;

//        /// <summary>
//        /// Очищення ресурсів
//        /// </summary>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code
//        private void InitializeComponent()
//        {
//            this.panelTop = new System.Windows.Forms.Panel();
//            this.lblDietPlan = new System.Windows.Forms.Label();
//            this.cmbDietPlans = new System.Windows.Forms.ComboBox();
//            this.btnRefreshPlans = new System.Windows.Forms.Button();
//            this.dgvDetails = new System.Windows.Forms.DataGridView();
//            this.btnReload = new System.Windows.Forms.Button();
//            this.btnAdd = new System.Windows.Forms.Button();
//            this.btnDelete = new System.Windows.Forms.Button();
//            this.btnCalculate = new System.Windows.Forms.Button();
//            this.lblStatus = new System.Windows.Forms.Label();
//            this.lblInfo = new System.Windows.Forms.Label();
//            this.panelTop.SuspendLayout();
//            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).BeginInit();
//            this.SuspendLayout();
//            // 
//            // panelTop
//            // 
//            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
//            this.panelTop.Controls.Add(this.lblDietPlan);
//            this.panelTop.Controls.Add(this.cmbDietPlans);
//            this.panelTop.Controls.Add(this.btnRefreshPlans);
//            this.panelTop.Location = new System.Drawing.Point(10, 10);
//            this.panelTop.Name = "panelTop";
//            this.panelTop.Size = new System.Drawing.Size(1260, 60);
//            this.panelTop.TabIndex = 0;
//            // 
//            // lblDietPlan
//            // 
//            this.lblDietPlan.AutoSize = true;
//            this.lblDietPlan.Location = new System.Drawing.Point(10, 20);
//            this.lblDietPlan.Name = "lblDietPlan";
//            this.lblDietPlan.Size = new System.Drawing.Size(64, 20);
//            this.lblDietPlan.TabIndex = 0;
//            this.lblDietPlan.Text = "Раціон:";
//            // 
//            // cmbDietPlans
//            // 
//            this.cmbDietPlans.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
//            this.cmbDietPlans.FormattingEnabled = true;
//            this.cmbDietPlans.Location = new System.Drawing.Point(100, 15);
//            this.cmbDietPlans.Name = "cmbDietPlans";
//            this.cmbDietPlans.Size = new System.Drawing.Size(400, 28);
//            this.cmbDietPlans.TabIndex = 1;
//            // 
//            // btnRefreshPlans
//            // 
//            this.btnRefreshPlans.Location = new System.Drawing.Point(510, 15);
//            this.btnRefreshPlans.Name = "btnRefreshPlans";
//            this.btnRefreshPlans.Size = new System.Drawing.Size(40, 28);
//            this.btnRefreshPlans.TabIndex = 2;
//            this.btnRefreshPlans.Text = "🔄";
//            this.btnRefreshPlans.UseVisualStyleBackColor = true;
//            // 
//            // dgvDetails
//            // 
//            this.dgvDetails.AllowUserToAddRows = false;
//            this.dgvDetails.AllowUserToDeleteRows = true;
//            this.dgvDetails.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
//            this.dgvDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
//            this.dgvDetails.Location = new System.Drawing.Point(10, 120);
//            this.dgvDetails.Name = "dgvDetails";
//            this.dgvDetails.RowHeadersWidth = 51;
//            this.dgvDetails.RowTemplate.Height = 29;
//            this.dgvDetails.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
//            this.dgvDetails.Size = new System.Drawing.Size(1260, 480);
//            this.dgvDetails.TabIndex = 3;
//            // 
//            // btnReload
//            // 
//            this.btnReload.Location = new System.Drawing.Point(10, 80);
//            this.btnReload.Name = "btnReload";
//            this.btnReload.Size = new System.Drawing.Size(140, 30);
//            this.btnReload.TabIndex = 4;
//            this.btnReload.Text = "Оновити";
//            this.btnReload.UseVisualStyleBackColor = true;
//            // 
//            // btnAdd
//            // 
//            this.btnAdd.Location = new System.Drawing.Point(160, 80);
//            this.btnAdd.Name = "btnAdd";
//            this.btnAdd.Size = new System.Drawing.Size(140, 30);
//            this.btnAdd.TabIndex = 5;
//            this.btnAdd.Text = "Додати корм";
//            this.btnAdd.UseVisualStyleBackColor = true;
//            // 
//            // btnDelete
//            // 
//            this.btnDelete.Location = new System.Drawing.Point(310, 80);
//            this.btnDelete.Name = "btnDelete";
//            this.btnDelete.Size = new System.Drawing.Size(140, 30);
//            this.btnDelete.TabIndex = 6;
//            this.btnDelete.Text = "Видалити";
//            this.btnDelete.UseVisualStyleBackColor = true;
//            // 
//            // btnCalculate
//            // 
//            this.btnCalculate.Location = new System.Drawing.Point(460, 80);
//            this.btnCalculate.Name = "btnCalculate";
//            this.btnCalculate.Size = new System.Drawing.Size(180, 30);
//            this.btnCalculate.TabIndex = 7;
//            this.btnCalculate.Text = "Розрахувати калорії";
//            this.btnCalculate.UseVisualStyleBackColor = true;
//            // 
//            // lblStatus
//            // 
//            this.lblStatus.Location = new System.Drawing.Point(10, 610);
//            this.lblStatus.Name = "lblStatus";
//            this.lblStatus.Size = new System.Drawing.Size(1260, 20);
//            this.lblStatus.TabIndex = 8;
//            this.lblStatus.Text = "Готово";
//            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            // 
//            // lblInfo
//            // 
//            this.lblInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
//            this.lblInfo.ForeColor = System.Drawing.Color.DarkGreen;
//            this.lblInfo.Location = new System.Drawing.Point(650, 80);
//            this.lblInfo.Name = "lblInfo";
//            this.lblInfo.Size = new System.Drawing.Size(600, 30);
//            this.lblInfo.TabIndex = 9;
//            this.lblInfo.Text = "Виберіть раціон";
//            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            // 
//            // FormDietPlanDetails
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(1300, 650);
//            this.Controls.Add(this.lblInfo);
//            this.Controls.Add(this.lblStatus);
//            this.Controls.Add(this.btnCalculate);
//            this.Controls.Add(this.btnDelete);
//            this.Controls.Add(this.btnAdd);
//            this.Controls.Add(this.btnReload);
//            this.Controls.Add(this.dgvDetails);
//            this.Controls.Add(this.panelTop);
//            this.Name = "FormDietPlanDetails";
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
//            this.Text = "Склад раціону";
//            this.panelTop.ResumeLayout(false);
//            this.panelTop.PerformLayout();
//            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).EndInit();
//            this.ResumeLayout(false);

//        }
//        #endregion
//    }
//}

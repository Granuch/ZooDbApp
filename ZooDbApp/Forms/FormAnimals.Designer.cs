//namespace ZooMenuApp.Forms
//{
//    partial class FormAnimals
//    {
//        private System.ComponentModel.IContainer components = null;
//        private System.Windows.Forms.DataGridView dgvAnimals;
//        private System.Windows.Forms.Button btnRefresh;
//        private System.Windows.Forms.Button btnSave;
//        private System.Windows.Forms.Button btnAdd;
//        private System.Windows.Forms.Button btnDelete;
//        private System.Windows.Forms.Button btnSearch;
//        private System.Windows.Forms.Label lblStatus;

//        /// <summary>
//        /// Очистка ресурсів.
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
//            this.dgvAnimals = new System.Windows.Forms.DataGridView();
//            this.btnRefresh = new System.Windows.Forms.Button();
//            this.btnSave = new System.Windows.Forms.Button();
//            this.btnAdd = new System.Windows.Forms.Button();
//            this.btnDelete = new System.Windows.Forms.Button();
//            this.btnSearch = new System.Windows.Forms.Button();
//            this.lblStatus = new System.Windows.Forms.Label();
//            ((System.ComponentModel.ISupportInitialize)(this.dgvAnimals)).BeginInit();
//            this.SuspendLayout();
//            // 
//            // dgvAnimals
//            // 
//            this.dgvAnimals.AllowUserToAddRows = true;
//            this.dgvAnimals.AllowUserToDeleteRows = true;
//            this.dgvAnimals.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
//            this.dgvAnimals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
//            this.dgvAnimals.Location = new System.Drawing.Point(10, 50);
//            this.dgvAnimals.MultiSelect = true;
//            this.dgvAnimals.Name = "dgvAnimals";
//            this.dgvAnimals.RowHeadersWidth = 51;
//            this.dgvAnimals.RowTemplate.Height = 29;
//            this.dgvAnimals.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
//            this.dgvAnimals.Size = new System.Drawing.Size(1360, 550);
//            this.dgvAnimals.TabIndex = 0;
//            // 
//            // btnRefresh
//            // 
//            this.btnRefresh.Location = new System.Drawing.Point(10, 10);
//            this.btnRefresh.Name = "btnRefresh";
//            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
//            this.btnRefresh.TabIndex = 1;
//            this.btnRefresh.Text = "Оновити";
//            this.btnRefresh.UseVisualStyleBackColor = true;
//            this.btnRefresh.Click += new System.EventHandler(this.LoadData);
//            // 
//            // btnSave
//            // 
//            this.btnSave.Location = new System.Drawing.Point(120, 10);
//            this.btnSave.Name = "btnSave";
//            this.btnSave.Size = new System.Drawing.Size(100, 30);
//            this.btnSave.TabIndex = 2;
//            this.btnSave.Text = "Зберегти";
//            this.btnSave.UseVisualStyleBackColor = true;
//            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
//            // 
//            // btnAdd
//            // 
//            this.btnAdd.Location = new System.Drawing.Point(230, 10);
//            this.btnAdd.Name = "btnAdd";
//            this.btnAdd.Size = new System.Drawing.Size(100, 30);
//            this.btnAdd.TabIndex = 3;
//            this.btnAdd.Text = "Додати";
//            this.btnAdd.UseVisualStyleBackColor = true;
//            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
//            // 
//            // btnDelete
//            // 
//            this.btnDelete.Location = new System.Drawing.Point(340, 10);
//            this.btnDelete.Name = "btnDelete";
//            this.btnDelete.Size = new System.Drawing.Size(100, 30);
//            this.btnDelete.TabIndex = 4;
//            this.btnDelete.Text = "Видалити";
//            this.btnDelete.UseVisualStyleBackColor = true;
//            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
//            // 
//            // btnSearch
//            // 
//            this.btnSearch.Location = new System.Drawing.Point(450, 10);
//            this.btnSearch.Name = "btnSearch";
//            this.btnSearch.Size = new System.Drawing.Size(100, 30);
//            this.btnSearch.TabIndex = 5;
//            this.btnSearch.Text = "Пошук";
//            this.btnSearch.UseVisualStyleBackColor = true;
//            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
//            // 
//            // lblStatus
//            // 
//            this.lblStatus.Location = new System.Drawing.Point(10, 610);
//            this.lblStatus.Name = "lblStatus";
//            this.lblStatus.Size = new System.Drawing.Size(1360, 20);
//            this.lblStatus.TabIndex = 6;
//            this.lblStatus.Text = "Готово";
//            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            // 
//            // FormAnimals
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(1400, 700);
//            this.Controls.Add(this.lblStatus);
//            this.Controls.Add(this.btnSearch);
//            this.Controls.Add(this.btnDelete);
//            this.Controls.Add(this.btnAdd);
//            this.Controls.Add(this.btnSave);
//            this.Controls.Add(this.btnRefresh);
//            this.Controls.Add(this.dgvAnimals);
//            this.Name = "FormAnimals";
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
//            this.Text = "Тварини";
//            ((System.ComponentModel.ISupportInitialize)(this.dgvAnimals)).EndInit();
//            this.ResumeLayout(false);
//        }

//        #endregion
//    }
//}

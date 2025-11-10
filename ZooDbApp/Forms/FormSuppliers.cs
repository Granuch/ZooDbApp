using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormSuppliers : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormSuppliers()
        {
            InitializeComponent();
            this.Text = "Постачальники";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeControls();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }

        private void InitializeControls()
        {
            DataGridView dgv = new DataGridView
            {
                Name = "dgvSuppliers",
                Location = new Point(10, 50),
                Size = new Size(1160, 550),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgv);

            var buttons = new[]
            {
                ("Оновити", (EventHandler)((s, e) => LoadData())),
                ("Зберегти", BtnSave_Click),
                ("Додати", BtnAdd_Click),
                ("Видалити", BtnDelete_Click)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 110, 10),
                    Size = new Size(100, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 610),
                Size = new Size(1160, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSuppliers", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = "SELECT * FROM Suppliers ORDER BY CompanyName";
                dataAdapter = new SqlDataAdapter(query, connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["SupplierID"].HeaderText = "ID";
                    dgv.Columns["CompanyName"].HeaderText = "Назва компанії";
                    dgv.Columns["ContactPerson"].HeaderText = "Контактна особа";
                    dgv.Columns["Phone"].HeaderText = "Телефон";
                    dgv.Columns["Email"].HeaderText = "Email";
                    dgv.Columns["Address"].HeaderText = "Адреса";
                    dgv.Columns["SupplierType"].HeaderText = "Тип постачальника";
                    dgv.Columns["PaymentTerms"].HeaderText = "Умови оплати";
                    dgv.Columns["IsActive"].HeaderText = "Активний";
                    dgv.Columns["CreatedDate"].HeaderText = "Дата створення";

                    dgv.Columns["SupplierID"].ReadOnly = true;
                    dgv.Columns["CreatedDate"].ReadOnly = true;
                    dgv.Columns["SupplierID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["SupplierID"].Width = 50;
                }

                UpdateStatus($"Завантажено записів: {dataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSuppliers", false)[0] as DataGridView;
                dgv.EndEdit();

                dataAdapter.Update(dataTable);

                UpdateStatus("Дані успішно збережено!");
                MessageBox.Show("Дані успішно збережено!", "Успіх",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow newRow = dataTable.NewRow();
                newRow["CompanyName"] = "Нова компанія";
                newRow["ContactPerson"] = "Контактна особа";
                newRow["Phone"] = "+380000000000";
                newRow["Email"] = "company@example.com";
                newRow["Address"] = "Адреса компанії";
                newRow["SupplierType"] = "Універсальний";
                newRow["PaymentTerms"] = "Оплата при отриманні";
                newRow["IsActive"] = true;
                newRow["CreatedDate"] = DateTime.Now;

                dataTable.Rows.Add(newRow);
                UpdateStatus("Додано новий запис. Не забудьте зберегти!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSuppliers", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "Ви впевнені, що хочете видалити вибрані записи?",
                        "Підтвердження",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        foreach (DataGridViewRow row in dgv.SelectedRows)
                        {
                            if (!row.IsNewRow)
                            {
                                dgv.Rows.Remove(row);
                            }
                        }
                        UpdateStatus("Записи видалено. Не забудьте зберегти!");
                    }
                }
                else
                {
                    MessageBox.Show("Виберіть рядки для видалення!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message)
        {
            Label lblStatus = this.Controls.Find("lblStatus", false)[0] as Label;
            if (lblStatus != null)
            {
                lblStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
            }
        }
    }
}
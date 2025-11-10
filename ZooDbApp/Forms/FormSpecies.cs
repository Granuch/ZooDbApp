using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormSpecies : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormSpecies()
        {
            InitializeComponent();
            this.Text = "Види тварин";
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
                Name = "dgvSpecies",
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
                DataGridView dgv = this.Controls.Find("dgvSpecies", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = "SELECT * FROM Species ORDER BY SpeciesName";
                dataAdapter = new SqlDataAdapter(query, connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["SpeciesID"].HeaderText = "ID";
                    dgv.Columns["SpeciesName"].HeaderText = "Назва виду";
                    dgv.Columns["ScientificName"].HeaderText = "Наукова назва";
                    dgv.Columns["DietType"].HeaderText = "Тип живлення";
                    dgv.Columns["Category"].HeaderText = "Категорія";
                    dgv.Columns["AverageWeight"].HeaderText = "Середня вага (кг)";
                    dgv.Columns["Description"].HeaderText = "Опис";
                    dgv.Columns["CreatedDate"].HeaderText = "Дата створення";

                    dgv.Columns["SpeciesID"].ReadOnly = true;
                    dgv.Columns["CreatedDate"].ReadOnly = true;
                    dgv.Columns["SpeciesID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["SpeciesID"].Width = 50;
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
                DataGridView dgv = this.Controls.Find("dgvSpecies", false)[0] as DataGridView;
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
                newRow["SpeciesName"] = "Новий вид";
                newRow["DietType"] = "Всеїдні";
                newRow["Category"] = "Ссавці";
                newRow["AverageWeight"] = 50.0;
                newRow["Description"] = "Опис виду";
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
                DataGridView dgv = this.Controls.Find("dgvSpecies", false)[0] as DataGridView;

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
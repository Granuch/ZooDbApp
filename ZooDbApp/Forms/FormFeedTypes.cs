using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormFeedTypes : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormFeedTypes()
        {
            InitializeComponent();
            this.Text = "Типи кормів";
            this.Size = new Size(1400, 700);
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
                Name = "dgvFeedTypes",
                Location = new Point(10, 50),
                Size = new Size(1360, 550),
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
                ("Видалити", BtnDelete_Click),
                ("Фільтр", BtnFilter_Click)
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
                Size = new Size(1360, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvFeedTypes", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = "SELECT * FROM FeedTypes ORDER BY FeedCategory, FeedName";
                dataAdapter = new SqlDataAdapter(query, connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["FeedID"].HeaderText = "ID";
                    dgv.Columns["FeedName"].HeaderText = "Назва корму";
                    dgv.Columns["FeedCategory"].HeaderText = "Категорія";
                    dgv.Columns["ProteinPercent"].HeaderText = "Білки (%)";
                    dgv.Columns["FatPercent"].HeaderText = "Жири (%)";
                    dgv.Columns["CarbohydratesPercent"].HeaderText = "Вуглеводи (%)";
                    dgv.Columns["CaloriesPer100g"].HeaderText = "Калорії/100г";
                    dgv.Columns["MeasurementUnit"].HeaderText = "Одиниця виміру";
                    dgv.Columns["PricePerUnit"].HeaderText = "Ціна за од.";
                    dgv.Columns["StorageConditions"].HeaderText = "Умови зберігання";
                    dgv.Columns["ShelfLifeDays"].HeaderText = "Термін придатності (днів)";
                    dgv.Columns["CreatedDate"].HeaderText = "Дата створення";

                    dgv.Columns["FeedID"].ReadOnly = true;
                    dgv.Columns["CreatedDate"].ReadOnly = true;
                    dgv.Columns["FeedID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["FeedID"].Width = 50;
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
                DataGridView dgv = this.Controls.Find("dgvFeedTypes", false)[0] as DataGridView;
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
                newRow["FeedName"] = "Новий корм";
                newRow["FeedCategory"] = "Інше";
                newRow["ProteinPercent"] = 10.0;
                newRow["FatPercent"] = 5.0;
                newRow["CarbohydratesPercent"] = 20.0;
                newRow["CaloriesPer100g"] = 200;
                newRow["MeasurementUnit"] = "кг";
                newRow["PricePerUnit"] = 50.0;
                newRow["ShelfLifeDays"] = 30;
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
                DataGridView dgv = this.Controls.Find("dgvFeedTypes", false)[0] as DataGridView;

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

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            Form filterForm = new Form
            {
                Text = "Фільтр за категорією",
                Size = new Size(300, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            ComboBox cmbCategory = new ComboBox
            {
                Location = new Point(20, 20),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.AddRange(new[] { "Всі", "М'ясо", "Риба", "Фрукти", "Овочі", "Зерно", "Комбікорм", "Комахи", "Вітаміни", "Інше" });
            cmbCategory.SelectedIndex = 0;
            filterForm.Controls.Add(cmbCategory);

            Button btnApply = new Button
            {
                Text = "Застосувати",
                Location = new Point(90, 60),
                Size = new Size(100, 30)
            };
            btnApply.Click += (s, e) =>
            {
                try
                {
                    DataView dv = dataTable.DefaultView;
                    if (cmbCategory.SelectedIndex == 0)
                    {
                        dv.RowFilter = "";
                    }
                    else
                    {
                        dv.RowFilter = $"FeedCategory = '{cmbCategory.SelectedItem}'";
                    }
                    UpdateStatus($"Знайдено записів: {dv.Count}");
                    filterForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка фільтрації: {ex.Message}", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            filterForm.Controls.Add(btnApply);

            filterForm.ShowDialog();
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
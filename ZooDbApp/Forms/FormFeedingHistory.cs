using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormFeedingHistory : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormFeedingHistory()
        {
            InitializeComponent();
            this.Text = "Історія годування";
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
            // Панель фільтрів
            Panel panelFilter = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1360, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblFrom = new Label { Text = "Від:", Location = new Point(10, 20), Size = new Size(40, 20) };
            panelFilter.Controls.Add(lblFrom);

            DateTimePicker dtpFrom = new DateTimePicker
            {
                Name = "dtpFrom",
                Location = new Point(55, 17),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-7)
            };
            panelFilter.Controls.Add(dtpFrom);

            Label lblTo = new Label { Text = "До:", Location = new Point(200, 20), Size = new Size(30, 20) };
            panelFilter.Controls.Add(lblTo);

            DateTimePicker dtpTo = new DateTimePicker
            {
                Name = "dtpTo",
                Location = new Point(235, 17),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short
            };
            panelFilter.Controls.Add(dtpTo);

            Button btnFilter = new Button
            {
                Text = "Фільтрувати",
                Location = new Point(380, 17),
                Size = new Size(100, 25)
            };
            btnFilter.Click += (s, e) => LoadData();
            panelFilter.Controls.Add(btnFilter);

            this.Controls.Add(panelFilter);

            // DataGridView
            DataGridView dgv = new DataGridView
            {
                Name = "dgvHistory",
                Location = new Point(10, 120),
                Size = new Size(1360, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            this.Controls.Add(dgv);

            // Кнопки
            var buttons = new[]
            {
                ("Оновити", (EventHandler)((s, e) => LoadData())),
                ("Статистика", BtnStatistics_Click),
                ("Експорт", BtnExport_Click)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 130, 80),
                    Size = new Size(120, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 630),
                Size = new Size(1360, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvHistory", false)[0] as DataGridView;
                DateTimePicker dtpFrom = this.Controls.Find("dtpFrom", true)[0] as DateTimePicker;
                DateTimePicker dtpTo = this.Controls.Find("dtpTo", true)[0] as DateTimePicker;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                    SELECT fh.HistoryID, a.AnimalName, s.SpeciesName, f.FeedName, f.FeedCategory,
                           fh.FeedingDate, fh.QuantityGiven, fh.AnimalReaction,
                           emp.FullName AS EmployeeName, fh.Notes
                    FROM FeedingHistory fh
                    LEFT JOIN Animals a ON fh.AnimalID = a.AnimalID
                    LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                    LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
                    LEFT JOIN Employees emp ON fh.EmployeeID = emp.EmployeeID
                    WHERE CAST(fh.FeedingDate AS DATE) BETWEEN @From AND @To
                    ORDER BY fh.FeedingDate DESC";

                dataAdapter = new SqlDataAdapter(query, connection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@From", dtpFrom.Value.Date);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@To", dtpTo.Value.Date);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["HistoryID"].HeaderText = "ID";
                    dgv.Columns["AnimalName"].HeaderText = "Тварина";
                    dgv.Columns["SpeciesName"].HeaderText = "Вид";
                    dgv.Columns["FeedName"].HeaderText = "Корм";
                    dgv.Columns["FeedCategory"].HeaderText = "Категорія";
                    dgv.Columns["FeedingDate"].HeaderText = "Дата і час";
                    dgv.Columns["QuantityGiven"].HeaderText = "Кількість (г)";
                    dgv.Columns["AnimalReaction"].HeaderText = "Реакція";
                    dgv.Columns["EmployeeName"].HeaderText = "Годівничий";
                    dgv.Columns["Notes"].HeaderText = "Примітки";

                    dgv.Columns["HistoryID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["HistoryID"].Width = 50;
                }

                UpdateStatus($"Завантажено записів: {dataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStatistics_Click(object sender, EventArgs e)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                MessageBox.Show("Немає даних для статистики!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DateTimePicker dtpFrom = this.Controls.Find("dtpFrom", true)[0] as DateTimePicker;
                DateTimePicker dtpTo = this.Controls.Find("dtpTo", true)[0] as DateTimePicker;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            COUNT(*) AS TotalFeedings,
                            COUNT(DISTINCT a.AnimalID) AS AnimalsCount,
                            SUM(fh.QuantityGiven) AS TotalFoodKg,
                            COUNT(CASE WHEN fh.AnimalReaction = N'Відмова' THEN 1 END) AS Refusals
                        FROM FeedingHistory fh
                        LEFT JOIN Animals a ON fh.AnimalID = a.AnimalID
                        WHERE CAST(fh.FeedingDate AS DATE) BETWEEN @From AND @To";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@From", dtpFrom.Value.Date);
                        cmd.Parameters.AddWithValue("@To", dtpTo.Value.Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalFeedings = reader.GetInt32(0);
                                int animalsCount = reader.GetInt32(1);
                                decimal totalFood = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                                int refusals = reader.GetInt32(3);

                                string message = $"Статистика за період {dtpFrom.Value:dd.MM.yyyy} - {dtpTo.Value:dd.MM.yyyy}:\n\n" +
                                               $"Всього годувань: {totalFeedings}\n" +
                                               $"Годовано тварин: {animalsCount}\n" +
                                               $"Всього корму видано: {totalFood / 1000:F2} кг\n" +
                                               $"Відмов від їжі: {refusals}\n" +
                                               $"Середня кількість на годування: {(totalFeedings > 0 ? totalFood / totalFeedings : 0):F0} г";

                                MessageBox.Show(message, "Статистика годування",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка розрахунку статистики: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvHistory", false)[0] as DataGridView;

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Немає даних для експорту!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"Історія_годування_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        List<string> headers = new List<string>();
                        foreach (DataGridViewColumn col in dgv.Columns)
                        {
                            headers.Add(col.HeaderText);
                        }
                        writer.WriteLine(string.Join(";", headers));

                        // Дані
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                List<string> values = new List<string>();
                                foreach (DataGridViewColumn col in dgv.Columns)
                                {
                                    string value = row.Cells[col.Index].Value?.ToString() ?? "";
                                    values.Add(value.Replace(";", ","));
                                }
                                writer.WriteLine(string.Join(";", values));
                            }
                        }
                    }
                    MessageBox.Show("Дані успішно експортовано!", "Успіх",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка експорту: {ex.Message}", "Помилка",
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
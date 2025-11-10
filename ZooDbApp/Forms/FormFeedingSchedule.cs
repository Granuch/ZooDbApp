using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormFeedingSchedule : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormFeedingSchedule()
        {
            InitializeComponent();
            this.Text = "Графік годування";
            this.Size = new Size(1400, 750);
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

            Label lblDate = new Label
            {
                Text = "Дата:",
                Location = new Point(10, 20),
                Size = new Size(50, 20)
            };
            panelFilter.Controls.Add(lblDate);

            DateTimePicker dtpDate = new DateTimePicker
            {
                Name = "dtpDate",
                Location = new Point(70, 17),
                Size = new Size(150, 25),
                Format = DateTimePickerFormat.Short
            };
            dtpDate.ValueChanged += (s, e) => LoadData();
            panelFilter.Controls.Add(dtpDate);

            ComboBox cmbStatus = new ComboBox
            {
                Name = "cmbStatus",
                Location = new Point(240, 17),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] { "Всі", "Заплановано", "Виконано", "Пропущено", "Частково" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            panelFilter.Controls.Add(cmbStatus);

            Button btnToday = new Button
            {
                Text = "Сьогодні",
                Location = new Point(400, 17),
                Size = new Size(100, 25)
            };
            btnToday.Click += (s, e) => {
                dtpDate.Value = DateTime.Today;
                LoadData();
            };
            panelFilter.Controls.Add(btnToday);

            Button btnGenerate = new Button
            {
                Text = "Генерувати графік",
                Location = new Point(510, 17),
                Size = new Size(150, 25),
                BackColor = Color.LightGreen
            };
            btnGenerate.Click += BtnGenerate_Click;
            panelFilter.Controls.Add(btnGenerate);

            this.Controls.Add(panelFilter);

            // DataGridView
            DataGridView dgv = new DataGridView
            {
                Name = "dgvSchedule",
                Location = new Point(10, 120),
                Size = new Size(1360, 530),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgv);

            // Кнопки управління
            var buttons = new[]
            {
                ("Оновити", (EventHandler)((s, e) => LoadData())),
                ("Відмітити виконано", BtnMarkComplete_Click),
                ("Відмітити пропущено", BtnMarkMissed_Click),
                ("Видалити", BtnDelete_Click),
                ("Експорт в Excel", BtnExport_Click)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 160, 80),
                    Size = new Size(150, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            // Статус
            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 660),
                Size = new Size(1360, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);

            // Інформаційна панель
            Label lblInfo = new Label
            {
                Name = "lblInfo",
                Text = "",
                Location = new Point(820, 80),
                Size = new Size(550, 30),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblInfo);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSchedule", false)[0] as DataGridView;
                DateTimePicker dtpDate = this.Controls.Find("dtpDate", true)[0] as DateTimePicker;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                    SELECT fs.ScheduleID, a.AnimalName, s.SpeciesName, e.EnclosureName,
                           f.FeedName, f.FeedCategory, fs.ScheduledDate, fs.ScheduledTime,
                           fs.PlannedQuantity, fs.ActualQuantity, fs.FeedingStatus,
                           fs.ActualFeedingTime, emp.FullName AS EmployeeName, fs.Notes
                    FROM FeedingSchedule fs
                    LEFT JOIN Animals a ON fs.AnimalID = a.AnimalID
                    LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                    LEFT JOIN Enclosures e ON a.EnclosureID = e.EnclosureID
                    LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                    LEFT JOIN Employees emp ON fs.EmployeeID = emp.EmployeeID
                    WHERE CAST(fs.ScheduledDate AS DATE) = @Date
                    ORDER BY fs.ScheduledTime, a.AnimalName";

                dataAdapter = new SqlDataAdapter(query, connection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@Date", dtpDate.Value.Date);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["ScheduleID"].HeaderText = "ID";
                    dgv.Columns["AnimalName"].HeaderText = "Тварина";
                    dgv.Columns["SpeciesName"].HeaderText = "Вид";
                    dgv.Columns["EnclosureName"].HeaderText = "Вольєр";
                    dgv.Columns["FeedName"].HeaderText = "Корм";
                    dgv.Columns["FeedCategory"].HeaderText = "Категорія";
                    dgv.Columns["ScheduledDate"].HeaderText = "Дата";
                    dgv.Columns["ScheduledTime"].HeaderText = "Час";
                    dgv.Columns["PlannedQuantity"].HeaderText = "Плановано (г)";
                    dgv.Columns["ActualQuantity"].HeaderText = "Фактично (г)";
                    dgv.Columns["FeedingStatus"].HeaderText = "Статус";
                    dgv.Columns["ActualFeedingTime"].HeaderText = "Фактичний час";
                    dgv.Columns["EmployeeName"].HeaderText = "Відповідальний";
                    dgv.Columns["Notes"].HeaderText = "Примітки"; dgv.Columns["ScheduleID"].ReadOnly = true;
                    dgv.Columns["AnimalName"].ReadOnly = true;
                    dgv.Columns["SpeciesName"].ReadOnly = true;
                    dgv.Columns["EnclosureName"].ReadOnly = true;
                    dgv.Columns["FeedName"].ReadOnly = true;
                    dgv.Columns["FeedCategory"].ReadOnly = true;
                    dgv.Columns["EmployeeName"].ReadOnly = true;

                    dgv.Columns["ScheduleID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["ScheduleID"].Width = 50;

                    // Кольорове виділення за статусом
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.Cells["FeedingStatus"].Value != null)
                        {
                            string status = row.Cells["FeedingStatus"].Value.ToString();
                            switch (status)
                            {
                                case "Виконано":
                                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                                    break;
                                case "Пропущено":
                                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                                    break;
                                case "Частково":
                                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                                    break;
                            }
                        }
                    }
                }

                UpdateInfoPanel();
                UpdateStatus($"Завантажено записів: {dataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            try
            {
                ComboBox cmbStatus = this.Controls.Find("cmbStatus", true)[0] as ComboBox;

                if (dataTable != null)
                {
                    DataView dv = dataTable.DefaultView;
                    if (cmbStatus.SelectedIndex == 0)
                    {
                        dv.RowFilter = "";
                    }
                    else
                    {
                        dv.RowFilter = $"FeedingStatus = '{cmbStatus.SelectedItem}'";
                    }
                    UpdateStatus($"Показано записів: {dv.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка фільтрації: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateInfoPanel()
        {
            try
            {
                Label lblInfo = this.Controls.Find("lblInfo", false)[0] as Label;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    int total = dataTable.Rows.Count;
                    int completed = dataTable.Select("FeedingStatus = 'Виконано'").Length;
                    int pending = dataTable.Select("FeedingStatus = 'Заплановано'").Length;
                    int missed = dataTable.Select("FeedingStatus = 'Пропущено'").Length;

                    decimal completionRate = total > 0 ? (decimal)completed / total * 100 : 0;

                    lblInfo.Text = $"Всього: {total} | Виконано: {completed} | Заплановано: {pending} | Пропущено: {missed} | Виконання: {completionRate:F1}%";
                }
            }
            catch { }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                DateTimePicker dtpDate = this.Controls.Find("dtpDate", true)[0] as DateTimePicker;
                DateTime selectedDate = dtpDate.Value.Date;

                DialogResult result = MessageBox.Show(
                    $"Згенерувати графік годування на {selectedDate:dd.MM.yyyy}?\n\n" +
                    "Це створить записи годування для всіх активних тварин на основі їх раціонів.",
                    "Підтвердження",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // Перевірка чи вже є графік на цю дату
                        string checkQuery = "SELECT COUNT(*) FROM FeedingSchedule WHERE CAST(ScheduledDate AS DATE) = @Date";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@Date", selectedDate);
                            int existingCount = (int)checkCmd.ExecuteScalar();

                            if (existingCount > 0)
                            {
                                DialogResult overwrite = MessageBox.Show(
                                    $"На цю дату вже є {existingCount} записів.\nВидалити їх і створити нові?",
                                    "Увага",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (overwrite == DialogResult.Yes)
                                {
                                    string deleteQuery = "DELETE FROM FeedingSchedule WHERE CAST(ScheduledDate AS DATE) = @Date";
                                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        deleteCmd.Parameters.AddWithValue("@Date", selectedDate);
                                        deleteCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }

                        // Генерація графіку
                        string generateQuery = @"
                        INSERT INTO FeedingSchedule 
                            (AnimalID, FeedID, EmployeeID, ScheduledDate, ScheduledTime, PlannedQuantity, 
                             FeedingStatus, CreatedDate)
                        SELECT 
                            a.AnimalID,
                            dpd.FeedID,
                            (SELECT TOP 1 EmployeeID FROM Employees WHERE Position = N'Годівничий' AND IsActive = 1 ORDER BY NEWID()),
                            @Date,
                            dpd.FeedingTime,
                            dpd.QuantityGrams,
                            N'Заплановано',
                            GETDATE()
                        FROM Animals a
                        INNER JOIN Species s ON a.SpeciesID = s.SpeciesID
                        INNER JOIN DietPlans dp ON s.SpeciesID = dp.SpeciesID
                        INNER JOIN DietPlanDetails dpd ON dp.DietPlanID = dpd.DietPlanID
                        WHERE a.IsActive = 1 AND dp.IsActive = 1
                        ORDER BY dpd.FeedingTime, a.AnimalName";

                        using (SqlCommand generateCmd = new SqlCommand(generateQuery, conn))
                        {
                            generateCmd.Parameters.AddWithValue("@Date", selectedDate);
                            int rowsInserted = generateCmd.ExecuteNonQuery();

                            MessageBox.Show($"Графік успішно згенеровано!\nСтворено записів: {rowsInserted}",
                                "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка генерації графіку: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMarkComplete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSchedule", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        foreach (DataGridViewRow row in dgv.SelectedRows)
                        {
                            if (!row.IsNewRow && row.Cells["ScheduleID"].Value != null)
                            {
                                int scheduleId = Convert.ToInt32(row.Cells["ScheduleID"].Value);
                                decimal plannedQty = Convert.ToDecimal(row.Cells["PlannedQuantity"].Value);

                                string updateQuery = @"
                                UPDATE FeedingSchedule 
                                SET FeedingStatus = N'Виконано',
                                    ActualQuantity = @ActualQuantity,
                                    ActualFeedingTime = GETDATE()
                                WHERE ScheduleID = @ScheduleID";

                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                                    cmd.Parameters.AddWithValue("@ActualQuantity", plannedQty);
                                    cmd.ExecuteNonQuery();
                                }

                                // Додаємо запис в історію
                                int animalId = Convert.ToInt32(row.Cells["AnimalName"].Tag ??
                                    GetAnimalIdByName(conn, row.Cells["AnimalName"].Value.ToString()));
                                int feedId = GetFeedIdByName(conn, row.Cells["FeedName"].Value.ToString());
                                int employeeId = GetEmployeeIdByName(conn, row.Cells["EmployeeName"].Value.ToString());

                                string historyQuery = @"
                                INSERT INTO FeedingHistory 
                                    (AnimalID, FeedID, EmployeeID, FeedingDate, QuantityGiven, AnimalReaction, CreatedDate)
                                VALUES 
                                    (@AnimalID, @FeedID, @EmployeeID, GETDATE(), @Quantity, N'Нормальна', GETDATE())";

                                using (SqlCommand histCmd = new SqlCommand(historyQuery, conn))
                                {
                                    histCmd.Parameters.AddWithValue("@AnimalID", animalId);
                                    histCmd.Parameters.AddWithValue("@FeedID", feedId);
                                    histCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                                    histCmd.Parameters.AddWithValue("@Quantity", plannedQty);
                                    histCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    UpdateStatus("Годування відмічено як виконані!");
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Виберіть записи для відмітки!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMarkMissed_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSchedule", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        foreach (DataGridViewRow row in dgv.SelectedRows)
                        {
                            if (!row.IsNewRow && row.Cells["ScheduleID"].Value != null)
                            {
                                int scheduleId = Convert.ToInt32(row.Cells["ScheduleID"].Value);

                                string updateQuery = @"
                                UPDATE FeedingSchedule 
                                SET FeedingStatus = N'Пропущено',
                                    ActualFeedingTime = GETDATE()
                                WHERE ScheduleID = @ScheduleID";

                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    UpdateStatus("Годування відмічено як пропущені!");
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Виберіть записи для відмітки!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSchedule", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "Ви впевнені, що хочете видалити вибрані записи?",
                        "Підтвердження",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            foreach (DataGridViewRow row in dgv.SelectedRows)
                            {
                                if (!row.IsNewRow && row.Cells["ScheduleID"].Value != null)
                                {
                                    int scheduleId = Convert.ToInt32(row.Cells["ScheduleID"].Value);
                                    string deleteQuery = "DELETE FROM FeedingSchedule WHERE ScheduleID = @ScheduleID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@ScheduleID", scheduleId);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        UpdateStatus("Записи видалено!");
                        LoadData();
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

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvSchedule", false)[0] as DataGridView;

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Немає даних для експорту!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt",
                    DefaultExt = "csv",
                    FileName = $"Графік_годування_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        List<string> headers = new List<string>();
                        foreach (DataGridViewColumn col in dgv.Columns)
                        {
                            if (col.Visible)
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
                                    if (col.Visible)
                                    {
                                        string value = row.Cells[col.Index].Value?.ToString() ?? "";
                                        values.Add(value.Replace(";", ","));
                                    }
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

        private int GetAnimalIdByName(SqlConnection conn, string animalName)
        {
            string query = "SELECT AnimalID FROM Animals WHERE AnimalName = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", animalName);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetFeedIdByName(SqlConnection conn, string feedName)
        {
            string query = "SELECT FeedID FROM FeedTypes WHERE FeedName = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", feedName);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetEmployeeIdByName(SqlConnection conn, string employeeName)
        {
            string query = "SELECT EmployeeID FROM Employees WHERE FullName = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", employeeName);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
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
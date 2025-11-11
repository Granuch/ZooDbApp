using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormQueries : Form
    {
        private string connectionString =
                            ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private TabControl tabControl;
        private DataGridView dataGridViewResults;
        private Label lblRecordCount;

        public FormQueries()
        {
            InitializeComponent();
            this.Text = "SQL Запити та Звіти";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeControls();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }

        private void InitializeControls()
        {
            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(1360, 400)
            };
            this.Controls.Add(tabControl);

            tabControl.TabPages.Add(CreateAnimalsTab());
            tabControl.TabPages.Add(CreateFeedingTab());
            tabControl.TabPages.Add(CreateStockTab());
            tabControl.TabPages.Add(CreateStatisticsTab());
            tabControl.TabPages.Add(CreateReportsTab());
            tabControl.TabPages.Add(CreateCustomTab());

            dataGridViewResults = new DataGridView
            {
                Location = new Point(10, 420),
                Size = new Size(1360, 320),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dataGridViewResults);

            lblRecordCount = new Label
            {
                Text = "Знайдено записів: 0",
                Location = new Point(10, 750),
                Size = new Size(500, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblRecordCount);

            Button btnExport = new Button
            {
                Text = "Експорт в CSV",
                Location = new Point(1230, 745),
                Size = new Size(140, 30)
            };
            btnExport.Click += BtnExport_Click;
            this.Controls.Add(btnExport);
        }

        private TabPage CreateAnimalsTab()
        {
            TabPage tab = new TabPage("Тварини");

            int y = 20;

            Button btn1 = new Button
            {
                Text = "Всі тварини з їх видами",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn1.Click += (s, e) => ExecuteQuery(@"
                SELECT a.AnimalName AS Тварина, s.SpeciesName AS Вид, s.DietType AS ТипЖивлення,
                       e.EnclosureName AS Вольєр, a.Weight AS Вага, a.HealthStatus AS Здоровя
                FROM Animals a
                LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                LEFT JOIN Enclosures e ON a.EnclosureID = e.EnclosureID
                WHERE a.IsActive = 1
                ORDER BY s.SpeciesName, a.AnimalName");
            tab.Controls.Add(btn1);
            y += 50;

            Button btn2 = new Button
            {
                Text = "Тварини за вольєрами",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn2.Click += (s, e) => ExecuteQuery(@"
                SELECT e.EnclosureName AS Вольєр, COUNT(a.AnimalID) AS КількістьТварин,
                       e.Capacity AS Місткість, e.CurrentOccupancy AS ПоточнаЗаповненість
                FROM Enclosures e
                LEFT JOIN Animals a ON e.EnclosureID = a.EnclosureID AND a.IsActive = 1
                GROUP BY e.EnclosureName, e.Capacity, e.CurrentOccupancy
                ORDER BY КількістьТварин DESC");
            tab.Controls.Add(btn2);
            y += 50;

            Button btn3 = new Button
            {
                Text = "Тварини зі спеціальною дієтою",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn3.Click += (s, e) => ExecuteQuery(@"
                SELECT a.AnimalName AS Тварина, s.SpeciesName AS Вид,
                       a.SpecialDietNotes AS ПриміткиПроДієту, a.HealthStatus AS Стан
                FROM Animals a
                LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                WHERE a.SpecialDiet = 1 AND a.IsActive = 1
                ORDER BY s.SpeciesName");
            tab.Controls.Add(btn3);
            y += 50;

            Button btn4 = new Button
            {
                Text = "Розподіл тварин за типом живлення",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn4.Click += (s, e) => ExecuteQuery(@"
                SELECT s.DietType AS ТипЖивлення, COUNT(a.AnimalID) AS Кількість,
                       STRING_AGG(s.SpeciesName, ', ') AS Види
                FROM Species s
                LEFT JOIN Animals a ON s.SpeciesID = a.SpeciesID AND a.IsActive = 1
                GROUP BY s.DietType
                ORDER BY Кількість DESC");
            tab.Controls.Add(btn4);
            y += 50;

            Button btn5 = new Button
            {
                Text = "Середня вага тварин за видами",
                Location = new Point(270, 20),
                Size = new Size(250, 40)
            };
            btn5.Click += (s, e) => ExecuteQuery(@"
                SELECT s.SpeciesName AS Вид, COUNT(a.AnimalID) AS Кількість,
                       AVG(a.Weight) AS СередняВага, MIN(a.Weight) AS МінВага, MAX(a.Weight) AS МаксВага
                FROM Animals a
                LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                WHERE a.IsActive = 1 AND a.Weight IS NOT NULL
                GROUP BY s.SpeciesName
                ORDER BY СередняВага DESC");
            tab.Controls.Add(btn5);

            return tab;
        }

        private TabPage CreateFeedingTab()
        {
            TabPage tab = new TabPage("Годування");

            int y = 20;

            Button btn1 = new Button
            {
                Text = "Графік годування на сьогодні",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn1.Click += (s, e) => ExecuteQuery(@"
                SELECT a.AnimalName AS Тварина, f.FeedName AS Корм,
                       fs.ScheduledTime AS Час, fs.PlannedQuantity AS Кількість,
                       fs.FeedingStatus AS Статус, emp.FullName AS Годівничий
                FROM FeedingSchedule fs
                LEFT JOIN Animals a ON fs.AnimalID = a.AnimalID
                LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                LEFT JOIN Employees emp ON fs.EmployeeID = emp.EmployeeID
                WHERE CAST(fs.ScheduledDate AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY fs.ScheduledTime, a.AnimalName");
            tab.Controls.Add(btn1);
            y += 50;

            Button btn2 = new Button
            {
                Text = "Історія годування за тиждень",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn2.Click += (s, e) => ExecuteQuery(@"
                SELECT TOP 100
                       a.AnimalName AS Тварина, f.FeedName AS Корм,
                       fh.FeedingDate AS ДатаЧас, fh.QuantityGiven AS Кількість,
                       fh.AnimalReaction AS Реакція, emp.FullName AS Годівничий
                FROM FeedingHistory fh
                LEFT JOIN Animals a ON fh.AnimalID = a.AnimalID
                LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
                LEFT JOIN Employees emp ON fh.EmployeeID = emp.EmployeeID
                WHERE fh.FeedingDate >= DATEADD(day, -7, GETDATE())
                ORDER BY fh.FeedingDate DESC");
            tab.Controls.Add(btn2);
            y += 50;

            Button btn3 = new Button
            {
                Text = "Раціони та їх склад",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn3.Click += (s, e) => ExecuteQuery(@"
                SELECT s.SpeciesName AS Вид, dp.PlanName AS НазваРаціону,
                       dp.SeasonType AS Сезон, f.FeedName AS Корм,
                       dpd.QuantityGrams AS КількістьГ, dpd.FeedingTime AS Час
                FROM DietPlans dp
                LEFT JOIN Species s ON dp.SpeciesID = s.SpeciesID
                LEFT JOIN DietPlanDetails dpd ON dp.DietPlanID = dpd.DietPlanID
                LEFT JOIN FeedTypes f ON dpd.FeedID = f.FeedID
                WHERE dp.IsActive = 1
                ORDER BY s.SpeciesName, dp.PlanName, dpd.FeedingTime");
            tab.Controls.Add(btn3);
            y += 50;

            Button btn4 = new Button
            {
                Text = "Найпопулярніші корми",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn4.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedName AS Корм, f.FeedCategory AS Категорія,
                       COUNT(fh.HistoryID) AS РазівВикористано,
                       SUM(fh.QuantityGiven) AS ЗагальнаКількістьГ
                FROM FeedingHistory fh
                LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
                WHERE fh.FeedingDate >= DATEADD(month, -1, GETDATE())
                GROUP BY f.FeedName, f.FeedCategory
                ORDER BY РазівВикористано DESC");
            tab.Controls.Add(btn4);
            y += 50;

            Button btn5 = new Button
            {
                Text = "Відмови від їжі",
                Location = new Point(270, 20),
                Size = new Size(250, 40)
            };
            btn5.Click += (s, e) => ExecuteQuery(@"
                SELECT a.AnimalName AS Тварина, s.SpeciesName AS Вид,
                       f.FeedName AS Корм, COUNT(*) AS КількістьВідмов
                FROM FeedingHistory fh
                LEFT JOIN Animals a ON fh.AnimalID = a.AnimalID
                LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
                WHERE fh.AnimalReaction = N'Відмова'
                GROUP BY a.AnimalName, s.SpeciesName, f.FeedName
                ORDER BY КількістьВідмов DESC");
            tab.Controls.Add(btn5);

            return tab;
        }

        private TabPage CreateStockTab()
        {
            TabPage tab = new TabPage("Склад");

            int y = 20;

            Button btn1 = new Button
            {
                Text = "Поточні залишки",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn1.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedName AS Корм, f.FeedCategory AS Категорія,
                       SUM(fs.Quantity) AS Залишок, f.MeasurementUnit AS Одиниця,
                       MIN(fs.ExpiryDate) AS НайближчийТермін
                FROM FeedStock fs
                LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                WHERE fs.Quantity > 0
                GROUP BY f.FeedName, f.FeedCategory, f.MeasurementUnit
                ORDER BY f.FeedCategory, f.FeedName");
            tab.Controls.Add(btn1);
            y += 50;

            Button btn2 = new Button
            {
                Text = "Прострочені корми",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn2.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedName AS Корм, fs.Quantity AS Кількість,
                       fs.ExpiryDate AS ТермінПридатності,
                       DATEDIFF(day, fs.ExpiryDate, GETDATE()) AS ДнівПрострочено,
                       s.CompanyName AS Постачальник
                FROM FeedStock fs
                LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                LEFT JOIN Suppliers s ON fs.SupplierID = s.SupplierID
                WHERE fs.ExpiryDate < GETDATE() AND fs.Quantity > 0
                ORDER BY fs.ExpiryDate");
            tab.Controls.Add(btn2);
            y += 50;

            Button btn3 = new Button
            {
                Text = "Корми з терміном < 7 днів",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn3.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedName AS Корм, fs.Quantity AS Кількість,
                       fs.ExpiryDate AS ТермінПридатності,
                       DATEDIFF(day, GETDATE(), fs.ExpiryDate) AS ДнівЗалишилось,
                       s.CompanyName AS Постачальник
                FROM FeedStock fs
                LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                LEFT JOIN Suppliers s ON fs.SupplierID = s.SupplierID
                WHERE DATEDIFF(day, GETDATE(), fs.ExpiryDate) BETWEEN 0 AND 7
                      AND fs.Quantity > 0
                ORDER BY fs.ExpiryDate");
            tab.Controls.Add(btn3);
            y += 50;

            Button btn4 = new Button
            {
                Text = "Загальна вартість запасів",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn4.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedCategory AS Категорія,
                       COUNT(DISTINCT f.FeedID) AS КількістьВидів,
                       SUM(fs.Quantity) AS ЗагальнаКількість,
                       SUM(fs.TotalCost) AS ЗагальнаВартість
                FROM FeedStock fs
                LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                WHERE fs.Quantity > 0
                GROUP BY f.FeedCategory
                ORDER BY ЗагальнаВартість DESC");
            tab.Controls.Add(btn4);
            y += 50;

            Button btn5 = new Button
            {
                Text = "Постачальники та обсяги",
                Location = new Point(270, 20),
                Size = new Size(250, 40)
            };
            btn5.Click += (s, e) => ExecuteQuery(@"
                SELECT s.CompanyName AS Постачальник,
                       COUNT(DISTINCT fs.FeedID) AS ВидівКормів,
                       SUM(fs.Quantity) AS ЗагальнаКількість,
                       SUM(fs.TotalCost) AS ЗагальнаВартість
                FROM FeedStock fs
                LEFT JOIN Suppliers s ON fs.SupplierID = s.SupplierID
                GROUP BY s.CompanyName
                ORDER BY ЗагальнаВартість DESC");
            tab.Controls.Add(btn5);

            return tab;
        }

        private TabPage CreateStatisticsTab()
        {
            TabPage tab = new TabPage("Статистика");

            int y = 20;

            Button btn1 = new Button
            {
                Text = "Загальна статистика системи",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn1.Click += (s, e) => ExecuteQuery(@"
                SELECT 
                    'Тварини' AS Категорія,
                    COUNT(*) AS Кількість
                FROM Animals WHERE IsActive = 1
                UNION ALL
                SELECT 'Види тварин', COUNT(*) FROM Species
                UNION ALL
                SELECT 'Вольєри', COUNT(*) FROM Enclosures
                UNION ALL
                SELECT 'Типи кормів', COUNT(*) FROM FeedTypes
                UNION ALL
                SELECT 'Раціони', COUNT(*) FROM DietPlans WHERE IsActive = 1
                UNION ALL
                SELECT 'Працівники', COUNT(*) FROM Employees WHERE IsActive = 1
                UNION ALL
                SELECT 'Постачальники', COUNT(*) FROM Suppliers WHERE IsActive = 1");
            tab.Controls.Add(btn1);
            y += 50;

            Button btn2 = new Button
            {
                Text = "Статистика годувань за місяць",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn2.Click += (s, e) => ExecuteQuery(@"
                SELECT 
                    CAST(fh.FeedingDate AS DATE) AS Дата,
                    COUNT(*) AS ВсьогоГодувань,
                    COUNT(DISTINCT fh.AnimalID) AS ТваринГодовано,
                    SUM(fh.QuantityGiven) / 1000.0 AS КормуВиданоКг,
                    COUNT(CASE WHEN fh.AnimalReaction = N'Відмова' THEN 1 END) AS Відмов
                FROM FeedingHistory fh
                WHERE fh.FeedingDate >= DATEADD(month, -1, GETDATE())
                GROUP BY CAST(fh.FeedingDate AS DATE)
                ORDER BY Дата DESC");
            tab.Controls.Add(btn2);
            y += 50;

            Button btn3 = new Button
            {
                Text = "Топ-10 найактивніших годівничих",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn3.Click += (s, e) => ExecuteQuery(@"
                SELECT TOP 10
                    emp.FullName AS Годівничий,
                    COUNT(fh.HistoryID) AS ВиконаноГодувань,
                    SUM(fh.QuantityGiven) / 1000.0 AS ВиданоКормуКг
                FROM FeedingHistory fh
                LEFT JOIN Employees emp ON fh.EmployeeID = emp.EmployeeID
                WHERE fh.FeedingDate >= DATEADD(month, -1, GETDATE())
                GROUP BY emp.FullName
                ORDER BY ВиконаноГодувань DESC");
            tab.Controls.Add(btn3);
            y += 50;

            Button btn4 = new Button
            {
                Text = "Споживання кормів за категоріями",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn4.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedCategory AS Категорія,
                       COUNT(DISTINCT fh.AnimalID) AS ТваринСпоживають,
                       SUM(fh.QuantityGiven) / 1000.0 AS ВсьогоСпожитоКг,
                       AVG(fh.QuantityGiven) AS СереднєНаГодуванняГ
                FROM FeedingHistory fh
                LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
                WHERE fh.FeedingDate >= DATEADD(month, -1, GETDATE())
                GROUP BY f.FeedCategory
                ORDER BY ВсьогоСпожитоКг DESC");
            tab.Controls.Add(btn4);
            y += 50;

            Button btn5 = new Button
            {
                Text = "Витрати на корми за місяць",
                Location = new Point(270, 20),
                Size = new Size(250, 40)
            };
            btn5.Click += (s, e) => ExecuteQuery(@"
                SELECT 
                    f.FeedCategory AS Категорія,
                    SUM(fh.QuantityGiven) / 1000.0 AS СпожитоКг,
                    f.PricePerUnit AS ЦінаЗаКг,
                    SUM(fh.QuantityGiven / 1000.0 * f.PricePerUnit) AS ПриблизнаВартість
                FROM FeedingHistory fh
                LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
                WHERE fh.FeedingDate >= DATEADD(month, -1, GETDATE())
                GROUP BY f.FeedCategory, f.PricePerUnit
                ORDER BY ПриблизнаВартість DESC");
            tab.Controls.Add(btn5);

            return tab;
        }

        private TabPage CreateReportsTab()
        {
            TabPage tab = new TabPage("Звіти");

            int y = 20;

            Button btn1 = new Button
            {
                Text = "Денне меню (сьогодні)",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn1.Click += (s, e) => ExecuteQuery(@"
                SELECT f.FeedCategory AS Категорія, f.FeedName AS Корм,
                       SUM(fs.PlannedQuantity) / 1000.0 AS ПотрібноКг,
                       f.MeasurementUnit AS Одиниця
                FROM FeedingSchedule fs
                LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                WHERE CAST(fs.ScheduledDate AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY f.FeedCategory, f.FeedName, f.MeasurementUnit
ORDER BY f.FeedCategory, f.FeedName");
            tab.Controls.Add(btn1);
            y += 50;
            Button btn2 = new Button
            {
                Text = "Вартість утримання за видами",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn2.Click += (s, e) => ExecuteQuery(@"
            SELECT s.SpeciesName AS Вид,
                   COUNT(DISTINCT a.AnimalID) AS КількістьТварин,
                   SUM(fh.QuantityGiven) / 1000.0 AS СпожитоКгЗаМісяць,
                   SUM(fh.QuantityGiven / 1000.0 * f.PricePerUnit) AS ВартістьЗаМісяць,
                   SUM(fh.QuantityGiven / 1000.0 * f.PricePerUnit) / COUNT(DISTINCT a.AnimalID) AS ВартістьНаТварину
            FROM Animals a
            LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
            LEFT JOIN FeedingHistory fh ON a.AnimalID = fh.AnimalID 
                AND fh.FeedingDate >= DATEADD(month, -1, GETDATE())
            LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
            WHERE a.IsActive = 1
            GROUP BY s.SpeciesName
            ORDER BY ВартістьЗаМісяць DESC");
            tab.Controls.Add(btn2);
            y += 50;

            Button btn3 = new Button
            {
                Text = "Звіт про виконання графіку",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn3.Click += (s, e) => ExecuteQuery(@"
            SELECT 
                CAST(fs.ScheduledDate AS DATE) AS Дата,
                COUNT(*) AS Заплановано,
                SUM(CASE WHEN fs.FeedingStatus = N'Виконано' THEN 1 ELSE 0 END) AS Виконано,
                SUM(CASE WHEN fs.FeedingStatus = N'Пропущено' THEN 1 ELSE 0 END) AS Пропущено,
                SUM(CASE WHEN fs.FeedingStatus = N'Заплановано' THEN 1 ELSE 0 END) AS Очікується,
                CAST(SUM(CASE WHEN fs.FeedingStatus = N'Виконано' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS ВідсотокВиконання
            FROM FeedingSchedule fs
            WHERE fs.ScheduledDate >= DATEADD(day, -7, GETDATE())
            GROUP BY CAST(fs.ScheduledDate AS DATE)
            ORDER BY Дата DESC");
            tab.Controls.Add(btn3);
            y += 50;

            Button btn4 = new Button
            {
                Text = "Аналіз раціонів",
                Location = new Point(10, y),
                Size = new Size(250, 40)
            };
            btn4.Click += (s, e) => ExecuteQuery(@"
            SELECT s.SpeciesName AS Вид, dp.PlanName AS Раціон,
                   dp.SeasonType AS Сезон, dp.DailyCalories AS КалорійНаДень,
                   dp.FeedingsPerDay AS ГодуваньНаДень,
                   COUNT(dpd.DetailID) AS КомпонентівУРаціоні,
                   SUM(dpd.QuantityGrams) / 1000.0 AS ЗагальнаВагаКг
            FROM DietPlans dp
            LEFT JOIN Species s ON dp.SpeciesID = s.SpeciesID
            LEFT JOIN DietPlanDetails dpd ON dp.DietPlanID = dpd.DietPlanID
            WHERE dp.IsActive = 1
            GROUP BY s.SpeciesName, dp.PlanName, dp.SeasonType, dp.DailyCalories, dp.FeedingsPerDay
            ORDER BY s.SpeciesName");
            tab.Controls.Add(btn4);
            y += 50;

            Button btn5 = new Button
            {
                Text = "Топ-10 найдорожчих у утриманні",
                Location = new Point(270, 20),
                Size = new Size(250, 40)
            };
            btn5.Click += (s, e) => ExecuteQuery(@"
            SELECT TOP 10
                a.AnimalName AS Тварина,
                s.SpeciesName AS Вид,
                SUM(fh.QuantityGiven) / 1000.0 AS СпожитоКгЗаМісяць,
                SUM(fh.QuantityGiven / 1000.0 * f.PricePerUnit) AS ВартістьУтриманняЗаМісяць
            FROM Animals a
            LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
            LEFT JOIN FeedingHistory fh ON a.AnimalID = fh.AnimalID 
                AND fh.FeedingDate >= DATEADD(month, -1, GETDATE())
            LEFT JOIN FeedTypes f ON fh.FeedID = f.FeedID
            WHERE a.IsActive = 1
            GROUP BY a.AnimalName, s.SpeciesName
            ORDER BY ВартістьУтриманняЗаМісяць DESC");
            tab.Controls.Add(btn5);

            return tab;
        }

        private TabPage CreateCustomTab()
        {
            TabPage tab = new TabPage("Власний SQL");

            Label lblInfo = new Label
            {
                Text = "Введіть SQL запит:",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            tab.Controls.Add(lblInfo);

            TextBox txtQuery = new TextBox
            {
                Name = "txtCustomQuery",
                Location = new Point(10, 35),
                Size = new Size(500, 180),  // Зменшено ширину
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10),
                Text = "SELECT * FROM Animals WHERE IsActive = 1"
            };
            tab.Controls.Add(txtQuery);

            Button btnExecute = new Button
            {
                Text = "Виконати запит",
                Location = new Point(10, 225),  // Зміщено вниз
                Size = new Size(150, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnExecute.Click += (s, e) =>
            {
                TextBox txt = tab.Controls.Find("txtCustomQuery", false)[0] as TextBox;
                if (!string.IsNullOrWhiteSpace(txt.Text))
                {
                    ExecuteQuery(txt.Text);
                }
                else
                {
                    MessageBox.Show("Введіть SQL запит!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            tab.Controls.Add(btnExecute);

            Button btnClear = new Button
            {
                Text = "Очистити",
                Location = new Point(170, 225),  // Зміщено вниз
                Size = new Size(100, 35),
                Font = new Font("Arial", 9)
            };
            btnClear.Click += (s, e) =>
            {
                TextBox txt = tab.Controls.Find("txtCustomQuery", false)[0] as TextBox;
                txt.Clear();
                txt.Text = "SELECT * FROM Animals WHERE IsActive = 1";
            };
            tab.Controls.Add(btnClear);

            Button btnFormat = new Button
            {
                Text = "Форматувати",
                Location = new Point(280, 225),
                Size = new Size(110, 35),
                Font = new Font("Arial", 9)
            };
            btnFormat.Click += (s, e) =>
            {
                TextBox txt = tab.Controls.Find("txtCustomQuery", false)[0] as TextBox;
                // Простий форматер SQL
                string formatted = txt.Text
                    .Replace(" FROM ", "\nFROM ")
                    .Replace(" WHERE ", "\nWHERE ")
                    .Replace(" JOIN ", "\nJOIN ")
                    .Replace(" GROUP BY ", "\nGROUP BY ")
                    .Replace(" ORDER BY ", "\nORDER BY ");
                txt.Text = formatted;
            };
            tab.Controls.Add(btnFormat);

            // Приклади запитів - зміщено праворуч
            Panel panelExamples = new Panel
            {
                Location = new Point(520, 10),  // Зміщено праворуч і вгору
                Size = new Size(820, 480),  // Збільшено висоту
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                BackColor = Color.WhiteSmoke
            };

            Label lblExamples = new Label
            {
                Text = "📋 Приклади запитів (клікніть для використання):",
                Location = new Point(10, 10),
                Size = new Size(790, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            panelExamples.Controls.Add(lblExamples);

            string[] examples = new[]
            {
        "-- Всі активні тварини\nSELECT AnimalName, SpeciesName, Gender, Weight\nFROM Animals a\nJOIN Species s ON a.SpeciesID = s.SpeciesID\nWHERE IsActive = 1",

        "-- Тварини певного виду\nSELECT a.AnimalName, a.BirthDate, e.EnclosureName\nFROM Animals a\nJOIN Species s ON a.SpeciesID = s.SpeciesID\nJOIN Enclosures e ON a.EnclosureID = e.EnclosureID\nWHERE s.SpeciesName = 'Лев'",

        "-- Корми дорожче 100 грн\nSELECT FeedName, FeedCategory, PricePerUnit, CaloriesPer100g\nFROM FeedTypes\nWHERE PricePerUnit > 100\nORDER BY PricePerUnit DESC",

        "-- Кількість тварин за вольєрами\nSELECT e.EnclosureName, COUNT(a.AnimalID) AS [Кількість тварин],\n       e.Capacity AS [Місткість], e.CurrentOccupancy AS [Заповненість]\nFROM Enclosures e\nLEFT JOIN Animals a ON e.EnclosureID = a.EnclosureID\nGROUP BY e.EnclosureName, e.Capacity, e.CurrentOccupancy",

        "-- Годування за останні 7 днів\nSELECT a.AnimalName, f.FeedName, fh.FeedingDate,\n       fh.QuantityGiven, fh.AnimalReaction\nFROM FeedingHistory fh\nJOIN Animals a ON fh.AnimalID = a.AnimalID\nJOIN FeedTypes f ON fh.FeedID = f.FeedID\nWHERE fh.FeedingDate >= DATEADD(day, -7, GETDATE())\nORDER BY fh.FeedingDate DESC",

        "-- Раціони з калоріями\nSELECT dp.PlanName, s.SpeciesName, dp.SeasonType,\n       SUM(dpd.QuantityGrams) AS [Загальна вага, г],\n       SUM(dpd.QuantityGrams * f.CaloriesPer100g / 100) AS [Калорії]\nFROM DietPlans dp\nJOIN Species s ON dp.SpeciesID = s.SpeciesID\nJOIN DietPlanDetails dpd ON dp.DietPlanID = dpd.DietPlanID\nJOIN FeedTypes f ON dpd.FeedID = f.FeedID\nGROUP BY dp.PlanName, s.SpeciesName, dp.SeasonType",

        "-- Прострочені корми на складі\nSELECT f.FeedName, fs.Quantity, fs.ExpiryDate,\n       DATEDIFF(day, GETDATE(), fs.ExpiryDate) AS [Днів до закінчення],\n       s.CompanyName AS [Постачальник]\nFROM FeedStock fs\nJOIN FeedTypes f ON fs.FeedID = f.FeedID\nJOIN Suppliers s ON fs.SupplierID = s.SupplierID\nWHERE fs.ExpiryDate <= DATEADD(day, 7, GETDATE())\nORDER BY fs.ExpiryDate",

        "-- Статистика по працівникам\nSELECT e.FullName, e.Position,\n       COUNT(fh.HistoryID) AS [Кількість годувань],\n       SUM(fh.QuantityGiven) AS [Всього корму, г]\nFROM Employees e\nLEFT JOIN FeedingHistory fh ON e.EmployeeID = fh.EmployeeID\nWHERE e.IsActive = 1\nGROUP BY e.FullName, e.Position\nORDER BY COUNT(fh.HistoryID) DESC",

        "-- Топ-5 найдорожчих замовлень\nSELECT TOP 5 fo.OrderID, s.CompanyName, fo.OrderDate,\n       fo.TotalAmount, fo.OrderStatus, fo.PaymentStatus\nFROM FeedOrders fo\nJOIN Suppliers s ON fo.SupplierID = s.SupplierID\nORDER BY fo.TotalAmount DESC"
    };

            int exY = 45;
            int exampleNum = 1;
            foreach (string example in examples)
            {
                Panel examplePanel = new Panel
                {
                    Location = new Point(10, exY),
                    Size = new Size(780, 90),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };

                Label lblNum = new Label
                {
                    Text = $"Приклад {exampleNum}:",
                    Location = new Point(5, 5),
                    Size = new Size(770, 15),
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    ForeColor = Color.DarkGreen
                };
                examplePanel.Controls.Add(lblNum);

                TextBox txtExample = new TextBox
                {
                    Text = example,
                    Location = new Point(5, 23),
                    Size = new Size(770, 62),
                    Multiline = true,
                    ReadOnly = true,
                    BackColor = Color.Ivory,
                    Font = new Font("Consolas", 8),
                    BorderStyle = BorderStyle.None,
                    Cursor = Cursors.Hand
                };
                txtExample.Click += (s, e) =>
                {
                    TextBox source = s as TextBox;
                    TextBox target = tab.Controls.Find("txtCustomQuery", false)[0] as TextBox;
                    target.Text = source.Text;
                    target.Focus();
                };
                examplePanel.Controls.Add(txtExample);

                // Клік по всій панелі також копіює запит
                examplePanel.Click += (s, e) =>
                {
                    TextBox target = tab.Controls.Find("txtCustomQuery", false)[0] as TextBox;
                    target.Text = txtExample.Text;
                    target.Focus();
                };

                panelExamples.Controls.Add(examplePanel);
                exY += 100;
                exampleNum++;
            }

            tab.Controls.Add(panelExamples);

            // Додаємо підказку знизу
            Label lblHint = new Label
            {
                Text = "💡 Підказка: Використовуйте SELECT, FROM, WHERE, JOIN, GROUP BY, ORDER BY для створення запитів",
                Location = new Point(10, 270),
                Size = new Size(500, 40),
                Font = new Font("Arial", 8),
                ForeColor = Color.DarkSlateGray
            };
            tab.Controls.Add(lblHint);

            return tab;
        }

        private void ExecuteQuery(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridViewResults.DataSource = dt;
                    lblRecordCount.Text = $"✓ Знайдено записів: {dt.Rows.Count}";
                    lblRecordCount.ForeColor = Color.Green;
                    lblRecordCount.Font = new Font("Arial", 9, FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка виконання запиту:\n\n{ex.Message}\n\nПеревірте синтаксис SQL запиту.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblRecordCount.Text = "✗ Помилка виконання запиту!";
                lblRecordCount.ForeColor = Color.Red;
                lblRecordCount.Font = new Font("Arial", 9, FontStyle.Bold);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewResults.DataSource == null || dataGridViewResults.Rows.Count == 0)
                {
                    MessageBox.Show("Немає даних для експорту!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "csv",
                    FileName = $"Звіт_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        List<string> headers = new List<string>();
                        foreach (DataGridViewColumn col in dataGridViewResults.Columns)
                        {
                            if (col.Visible)
                                headers.Add($"\"{col.HeaderText}\"");
                        }
                        writer.WriteLine(string.Join(";", headers));

                        // Дані
                        foreach (DataGridViewRow row in dataGridViewResults.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                List<string> values = new List<string>();
                                foreach (DataGridViewColumn col in dataGridViewResults.Columns)
                                {
                                    if (col.Visible)
                                    {
                                        string value = row.Cells[col.Index].Value?.ToString() ?? "";
                                        // Екрануємо лапки та спецсимволи
                                        value = value.Replace("\"", "\"\"");
                                        values.Add($"\"{value}\"");
                                    }
                                }
                                writer.WriteLine(string.Join(";", values));
                            }
                        }
                    }

                    DialogResult openFile = MessageBox.Show(
                        "Дані успішно експортовано!\n\nВідкрити файл?",
                        "Успіх",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (openFile == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка експорту: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
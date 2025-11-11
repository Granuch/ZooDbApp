using System.Data;
using System.Data.SqlClient;
using ZooMenuApp.Forms;

//private string connectionString = @"Data Source=DESKTOP-MH1GJKG;Initial Catalog=ZooMenuDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";


namespace ZooMenuApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Text = "Система управління зоопарком - Меню тварин";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeMenu();
            InitializeDashboard();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }

        private void InitializeMenu()
        {
            MenuStrip menuStrip = new MenuStrip();

            // Меню "Довідники"
            ToolStripMenuItem dictionariesMenu = new ToolStripMenuItem("📚 Довідники");

            dictionariesMenu.DropDownItems.Add(CreateMenuItem("Види тварин",
                (s, e) => OpenForm(new FormSpecies())));
            dictionariesMenu.DropDownItems.Add(CreateMenuItem("Тварини",
                (s, e) => OpenForm(new FormAnimals())));
            dictionariesMenu.DropDownItems.Add(CreateMenuItem("Вольєри",
                (s, e) => OpenForm(new FormEnclosures())));
            dictionariesMenu.DropDownItems.Add(CreateMenuItem("Типи кормів",
                (s, e) => OpenForm(new FormFeedTypes())));
            dictionariesMenu.DropDownItems.Add(CreateMenuItem("Працівники",
                (s, e) => OpenForm(new FormEmployees())));
            dictionariesMenu.DropDownItems.Add(CreateMenuItem("Постачальники",
                (s, e) => OpenForm(new FormSuppliers())));

            menuStrip.Items.Add(dictionariesMenu);

            // Меню "Раціони"
            ToolStripMenuItem dietsMenu = new ToolStripMenuItem("🍽️ Раціони");

            dietsMenu.DropDownItems.Add(CreateMenuItem("Плани раціонів",
                (s, e) => OpenForm(new FormDietPlans())));
            dietsMenu.DropDownItems.Add(CreateMenuItem("Склад раціонів",
                (s, e) => OpenForm(new FormDietPlanDetails())));

            menuStrip.Items.Add(dietsMenu);

            // Меню "Годування"
            ToolStripMenuItem feedingMenu = new ToolStripMenuItem("🍖 Годування");

            feedingMenu.DropDownItems.Add(CreateMenuItem("Графік годування",
                (s, e) => OpenForm(new FormFeedingSchedule())));
            feedingMenu.DropDownItems.Add(CreateMenuItem("Історія годування",
                (s, e) => OpenForm(new FormFeedingHistory())));

            menuStrip.Items.Add(feedingMenu);

            // Меню "Склад"
            ToolStripMenuItem warehouseMenu = new ToolStripMenuItem("📦 Склад");

            warehouseMenu.DropDownItems.Add(CreateMenuItem("Запаси кормів",
                (s, e) => OpenForm(new FormFeedStock())));
            warehouseMenu.DropDownItems.Add(CreateMenuItem("Замовлення",
                (s, e) => OpenForm(new FormFeedOrders())));

            menuStrip.Items.Add(warehouseMenu);

            // Меню "Звіти та Запити"
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("📊 Звіти та Запити");

            reportsMenu.DropDownItems.Add(CreateMenuItem("SQL Запити",
                (s, e) => OpenForm(new FormQueries())));

            menuStrip.Items.Add(reportsMenu);

            // Меню "Про програму"
            ToolStripMenuItem aboutMenu = new ToolStripMenuItem("ℹ️ Про програму");
            aboutMenu.Click += (s, e) => MessageBox.Show(
                "Система управління зоопарком\nМеню тварин v1.0\n\n" +
                "Розроблено для автоматизації процесів\nгодування тварин у зоологічному парку",
                "Про програму", MessageBoxButtons.OK, MessageBoxIcon.Information);
            menuStrip.Items.Add(aboutMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private ToolStripMenuItem CreateMenuItem(string text, EventHandler clickHandler)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += clickHandler;
            return item;
        }

        private void OpenForm(Form form)
        {
            form.Show();
        }

        private void InitializeDashboard()
        {
            Panel dashboardPanel = new Panel
            {
                Location = new Point(50, 80),
                Size = new Size(1100, 550),
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "🦁 Система управління зоопарком 🦁",
                Font = new Font("Arial", 26, FontStyle.Bold),
                Location = new Point(200, 30),
                Size = new Size(700, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkGreen
            };
            dashboardPanel.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Меню тварин та управління годуванням",
                Font = new Font("Arial", 16),
                Location = new Point(300, 85),
                Size = new Size(500, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkSlateGray
            };
            dashboardPanel.Controls.Add(lblSubtitle);

            // Швидкі кнопки доступу
            int buttonY = 150;
            var quickButtons = new[]
            {
                ("🦁 Тварини", typeof(FormAnimals)),
                ("🏠 Вольєри", typeof(FormEnclosures)),
                ("🍖 Корми", typeof(FormFeedTypes)),
                ("📅 Графік", typeof(FormFeedingSchedule)),
                ("📦 Склад", typeof(FormFeedStock)),
                ("📊 Запити", typeof(FormQueries))
            };

            int col = 0;
            foreach (var (text, formType) in quickButtons)
            {
                Button btn = new Button
                {
                    Text = text,
                    Size = new Size(180, 100),
                    Location = new Point(150 + (col % 3) * 250, buttonY + (col / 3) * 130),
                    Font = new Font("Arial", 13, FontStyle.Bold),
                    BackColor = Color.LightSkyBlue,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.DodgerBlue;

                btn.Click += (s, e) => {
                    var form = (Form)Activator.CreateInstance(formType);
                    OpenForm(form);
                };

                dashboardPanel.Controls.Add(btn);
                col++;
            }

            this.Controls.Add(dashboardPanel);
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NHB3.ApiConnect;

namespace NHB3
{
    public partial class Home : Form
    {
        ApiConnect ac;
        string currency = "TBTC";
        bool botRunning = false;
        JArray orders;
        JObject market;

        System.Threading.Timer timer;

        // Modern UI colors
        private Color PrimaryColor = Color.FromArgb(41, 128, 185);      // Blue
        private Color PrimaryDarkColor = Color.FromArgb(30, 96, 139);   // Darker Blue
        private Color AccentColor = Color.FromArgb(39, 174, 96);        // Green
        private Color DangerColor = Color.FromArgb(231, 76, 60);        // Red
        private Color BackgroundColor = Color.FromArgb(236, 240, 241);  // Light Gray
        private Color CardColor = Color.White;
        private Color TextPrimaryColor = Color.FromArgb(44, 62, 80);    // Dark Gray
        private Color TextSecondaryColor = Color.FromArgb(127, 140, 141); // Medium Gray

        public Home()
        {
            InitializeComponent();
            ApplyModernStyling();

            ac = new ApiConnect();

            ApiSettings saved = ac.readSettings();

            if (saved.OrganizationID != null) {
                ac.setup(saved);

                if (saved.Enviorment == 1) {
                    currency = "BTC";
                }
                ac.currency = currency;
                refreshBalance();
                refreshOrders(false);
                ac.getPools(true);

                timer = new System.Threading.Timer(
                    e => runBot(),
                    null,
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(60));
            }

            UpdateMetricsCards();
        }

        private void ApplyModernStyling()
        {
            // Form styling
            this.BackColor = BackgroundColor;
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);

            // Menu strip modern styling
            menuStrip1.BackColor = PrimaryColor;
            menuStrip1.ForeColor = Color.White;
            menuStrip1.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            menuStrip1.Renderer = new ModernMenuRenderer();

            // Status strip styling
            statusStrip1.BackColor = PrimaryDarkColor;
            statusStrip1.ForeColor = Color.White;
            statusStrip1.Font = new Font("Segoe UI", 9F);

            // DataGridView modern styling
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.BackgroundColor = CardColor;
            dataGridView1.GridColor = Color.FromArgb(230, 230, 230);
            dataGridView1.DefaultCellStyle.BackColor = CardColor;
            dataGridView1.DefaultCellStyle.ForeColor = TextPrimaryColor;
            dataGridView1.DefaultCellStyle.SelectionBackColor = PrimaryColor;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = PrimaryDarkColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimaryDarkColor;
            dataGridView1.ColumnHeadersHeight = 35;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }

        private void UpdateMetricsCards()
        {
            // Update metrics panel values if they exist
            if (lblTotalOrders != null && orders != null)
            {
                lblTotalOrders.Text = orders.Count.ToString();
            }

            if (lblActiveRigs != null && orders != null)
            {
                int activeRigs = 0;
                foreach (JObject order in orders)
                {
                    activeRigs += int.Parse("" + order["rigsCount"]);
                }
                lblActiveRigs.Text = activeRigs.ToString();
            }

            if (lblTotalSpeed != null && orders != null)
            {
                decimal totalSpeed = 0;
                foreach (JObject order in orders)
                {
                    totalSpeed += decimal.Parse("" + order["acceptedCurrentSpeed"], CultureInfo.InvariantCulture);
                }
                lblTotalSpeed.Text = totalSpeed.ToString("F2");
            }
        }

        private void api_Click(object sender, EventArgs e)
        {
            ApiForm af = new ApiForm(ac);
            af.FormBorderStyle = FormBorderStyle.FixedSingle;
            af.ShowDialog();
        }

        private void pools_Click(object sender, EventArgs e)
        {
            PoolsForm pf = new PoolsForm(ac);
            pf.FormBorderStyle = FormBorderStyle.FixedSingle;
            pf.ShowDialog();
        }

        private void botToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BotForm bf = new BotForm();
            bf.FormBorderStyle = FormBorderStyle.FixedSingle;
            bf.ShowDialog();
        }

        private void arbitrageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchArbitrageDashboard();
        }

        private void newOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OrderForm of = new OrderForm(ac);
            of.FormBorderStyle = FormBorderStyle.FixedSingle;
            of.FormClosed += new FormClosedEventHandler(f_FormClosed); //refresh orders
            of.ShowDialog();
        }

        private void ordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshOrders(false);
        }

        private void balanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshBalance();
        }

        private void refreshBalance() {
            if (ac.connected)
            {
                JObject balance = ac.getBalance(currency);
                if (balance != null)
                {
                    string balanceText = "Balance: " + balance["available"] + " " + currency;
                    this.toolStripStatusLabel2.Text = balanceText;

                    // Update balance card if it exists
                    if (lblBalance != null)
                    {
                        lblBalance.Text = balance["available"] + " " + currency;
                    }
                }
            } else {
                this.toolStripStatusLabel2.Text = "Balance: N/A " + currency;
                if (lblBalance != null)
                {
                    lblBalance.Text = "N/A";
                }
            }
        }

        private void refreshOrders(bool fromThread)
        {
            if (ac.connected)
            {
                orders = ac.getOrders();

                //filter out data
                JArray cleanOrders = new JArray();
                foreach (JObject order in orders)
                {
                    JObject cleanOrder = new JObject();
                    cleanOrder.Add("id", ""+order["id"]);
                    cleanOrder.Add("market", "" + order["market"]);
                    cleanOrder.Add("pool", "" + order["pool"]["name"]);
                    cleanOrder.Add("type", (""+order["type"]["code"]).Equals("STANDARD") ? "standard" : "fixed");
                    cleanOrder.Add("algorithm", "" + order["algorithm"]["algorithm"]);
                    cleanOrder.Add("amount", "" + order["amount"]);
                    cleanOrder.Add("payedAmount", "" + order["payedAmount"]);
                    cleanOrder.Add("availableAmount", "" + order["availableAmount"]);

                    float payed = float.Parse("" + order["payedAmount"], CultureInfo.InvariantCulture);
                    float available = float.Parse("" + order["availableAmount"], CultureInfo.InvariantCulture);
                    float spent_factor = payed / available * 100;

                    cleanOrder.Add("spentPercent", "" + spent_factor.ToString("0.00")+ "%");
                    cleanOrder.Add("limit", "" + order["limit"]);
                    cleanOrder.Add("price", "" + order["price"]);

                    cleanOrder.Add("rigsCount", "" + order["rigsCount"]);
                    cleanOrder.Add("acceptedCurrentSpeed", "" + order["acceptedCurrentSpeed"]);
                    cleanOrders.Add(cleanOrder);
                }

                if (fromThread)
                {
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        dataGridView1.DataSource = cleanOrders;
                        UpdateMetricsCards();
                    });
                } else
                {
                    dataGridView1.DataSource = cleanOrders;
                    UpdateMetricsCards();
                }

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dataGridView1.AllowUserToOrderColumns = true;
                dataGridView1.AllowUserToResizeColumns = true;
            }
        }

        private void refreshMarket() {
            if (ac.connected)
            {
                market = new JObject(); //flush
                //market = ac.getMarket();
            }
        }

        private void autoPilotOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Bot: Stopped";
            toolStripStatusLabel1.ForeColor = DangerColor;
            botRunning = false;

            if (lblBotStatus != null)
            {
                lblBotStatus.Text = "Stopped";
                lblBotStatus.ForeColor = DangerColor;
            }
        }

        private void autoPilotONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Bot: Idle";
            toolStripStatusLabel1.ForeColor = AccentColor;
            botRunning = true;
            runBot();

            if (lblBotStatus != null)
            {
                lblBotStatus.Text = "Running";
                lblBotStatus.ForeColor = AccentColor;
            }
        }

        private void editSelectedOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected) == 1)
            {
                OrderForm of = new OrderForm(ac);
                of.FormBorderStyle = FormBorderStyle.FixedSingle;
                of.setEditMode((JObject)orders[dataGridView1.SelectedRows[0].Index]);
                of.FormClosed += new FormClosedEventHandler(f_FormClosed); //refresh orders
                of.ShowDialog();
            }
        }

        private void f_FormClosed(object sender, FormClosedEventArgs e)
        {
            refreshOrders(false);
        }

        private void runBot() {
            if (!botRunning) {
                return;
            }

            //read needed data
            String fileName = Path.Combine(Directory.GetCurrentDirectory(), "bot.json");
            if (!File.Exists(fileName))
            {
                return;
            }

            toolStripStatusLabel1.Text = "Bot: Working";
            toolStripStatusLabel1.ForeColor = Color.Orange;

            BotSettings saved = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(@fileName));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("bot iteration tasks {0} {1} {2}", saved.reffilOrder, saved.lowerPrice, saved.increasePrice);

            Control.CheckForIllegalCrossThreadCalls = false;
            refreshOrders(true);

            if (saved.lowerPrice || saved.increasePrice) {
                refreshMarket();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("orders to process: {0}", orders.Count);

            //do refill??
            if (saved.reffilOrder) {
                foreach (JObject order in orders)
                {
                    float payed = float.Parse("" + order["payedAmount"], CultureInfo.InvariantCulture);
                    float available = float.Parse("" + order["availableAmount"], CultureInfo.InvariantCulture);
                    float spent_factor = payed/available*100;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("?refill?; order {0}, payed {1}, available {2}, percent {3}", order["id"], payed, available, spent_factor.ToString("0.00"));

                    if (spent_factor > 90)
                    {
                        JObject algo = ac.getAlgo(""+order["algorithm"]["algorithm"]);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("===> refill order for {0}", algo["minimalOrderAmount"]);
                        ac.refillOrder(""+order["id"], ""+algo["minimalOrderAmount"]);
                    }
                }
            }

            //do speed adjust??
            if (saved.lowerPrice || saved.increasePrice) {
                foreach (JObject order in orders) {
                    string order_type = "" + order["type"]["code"];
                    if (order_type.Equals("STANDARD"))
                    {
                        JObject algo = ac.getAlgo("" + order["algorithm"]["algorithm"]);
                        float order_speed = float.Parse("" + order["acceptedCurrentSpeed"], CultureInfo.InvariantCulture);
                        float rigs_count = float.Parse("" + order["rigsCount"], CultureInfo.InvariantCulture);
                        float order_price = float.Parse("" + order["price"], CultureInfo.InvariantCulture);
                        float price_step_down = float.Parse("" + algo["priceDownStep"], CultureInfo.InvariantCulture);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("?adjust price?; order {0}, speed {1}, rigs {2}, price {3}, step_down {4}", order["id"], order_speed, rigs_count, order_price, price_step_down);

                        if (saved.increasePrice && (order_speed == 0 || rigs_count == 0)) {
                            float new_price = (float)Math.Round(order_price + (price_step_down * -1), 4);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("===> price up order to {0}", new_price);
                            ac.updateOrder("" + order["algorithm"]["algorithm"], "" + order["id"], new_price.ToString(new CultureInfo("en-US")), "" + order["limit"]);
                        } else if (saved.lowerPrice && (order_speed > 0 || rigs_count > 0)) {
                            Dictionary<string, float> market = getOrderPriceRangesForAlgoAndMarket("" + order["algorithm"]["algorithm"], "" + order["market"]);
                            var list = market.Keys.ToList();
                            list.Sort();

                            int idx = 0;
                            foreach (var key in list)
                            {
                                float curr_tier_price = float.Parse(key, CultureInfo.InvariantCulture);
                                if (key.Equals(""+order_price)) {
                                    break;
                                }
                                idx++;
                            }

                            if (idx > 1) {
                                float new_price = (float)Math.Round(order_price + price_step_down, 4);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("===> price down order to {0}", new_price);
                                ac.updateOrder("" + order["algorithm"]["algorithm"], "" + order["id"], new_price.ToString(new CultureInfo("en-US")), "" + order["limit"]);
                            }
                        }
                    }
                }
            }
            toolStripStatusLabel1.Text = "Bot: Idle";
            toolStripStatusLabel1.ForeColor = AccentColor;
        }

        private Dictionary<string, float> getOrderPriceRangesForAlgoAndMarket(string oa, string om)
        {
            var prices = new Dictionary<string, float>();

            //simple cache
            if (market[oa] == null)
            {
                market[oa] = ac.getMarketForAlgo(oa);
            }

            foreach (JObject order in market[oa])
            {
                string order_type   = "" + order["type"];
                string order_algo   = "" + order["algorithm"];
                string order_market = "" + order["market"];
                float order_speed   = float.Parse("" + order["acceptedCurrentSpeed"], CultureInfo.InvariantCulture);
                string order_price  = "" + order["price"];

                if (order_type.Equals("STANDARD") && order_algo.Equals(oa) && order_market.Equals(om) && order_speed > 0) {
                    if (prices.ContainsKey(order_price))
                    {
                        prices[order_price] = prices[order_price] + order_speed;
                    }
                    else
                    {
                        prices[order_price] = order_speed;
                    }
                }
            }
            return prices;
        }
    }

    // Modern menu renderer for flat design
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(52, 152, 219)), e.Item.ContentRectangle);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(52, 152, 219);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(52, 152, 219);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(52, 152, 219);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(30, 96, 139);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(30, 96, 139);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color ImageMarginGradientBegin => Color.FromArgb(41, 128, 185);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(41, 128, 185);
        public override Color ImageMarginGradientEnd => Color.FromArgb(41, 128, 185);
    }
}

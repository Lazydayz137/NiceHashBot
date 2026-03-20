using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NHB3.Core.Models;
using NHB3.Core.Services;
using NHB3.MiningDutch;
using NHB3.MiningRigRentals;
using NHB3.NiceHash;
using NHB3.Profitability;

namespace NHB3
{
    /// <summary>
    /// Dashboard for displaying real-time arbitrage opportunities
    /// </summary>
    public partial class ArbitrageDashboard : Form
    {
        private readonly RealArbitrageCalculator _arbitrageCalculator;
        private readonly AutoExecutionEngine _autoExecutor;
        private readonly ArbitrageTracker _tracker;
        private System.Windows.Forms.Timer _refreshTimer;
        private CancellationTokenSource _cancellationTokenSource;

        private DataGridView dgvOpportunities;
        private Button btnScan;
        private Button btnRefresh;
        private Button btnAutoExecute;
        private CheckBox chkAutoMode;
        private NumericUpDown numMinMargin;
        private ComboBox cmbProfitType;
        private Label lblStatus;
        private TabControl tabControl;
        private DataGridView dgvHistory;
        private Label lblStats;

        public ArbitrageDashboard(
            RealArbitrageCalculator arbitrageCalculator,
            AutoExecutionEngine autoExecutor,
            ArbitrageTracker tracker)
        {
            _arbitrageCalculator = arbitrageCalculator ?? throw new ArgumentNullException(nameof(arbitrageCalculator));
            _autoExecutor = autoExecutor;
            _tracker = tracker;
            _cancellationTokenSource = new CancellationTokenSource();

            InitializeComponent();
            SetupRefreshTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "Arbitrage Opportunities - Real-Time Profitability";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Tab 1: Opportunities
            var tabOpportunities = new TabPage("Live Opportunities");
            SetupOpportunitiesTab(tabOpportunities);
            tabControl.TabPages.Add(tabOpportunities);

            // Tab 2: History & Stats
            var tabHistory = new TabPage("History & Performance");
            SetupHistoryTab(tabHistory);
            tabControl.TabPages.Add(tabHistory);

            this.Controls.Add(tabControl);
        }

        private void SetupOpportunitiesTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };

            // Controls row 1
            var lblMinMargin = new Label { Text = "Min Margin %:", Location = new Point(10, 15), AutoSize = true };
            numMinMargin = new NumericUpDown
            {
                Location = new Point(110, 12),
                Width = 80,
                Minimum = 0,
                Maximum = 100,
                Value = 5,
                DecimalPlaces = 1
            };

            var lblProfitType = new Label { Text = "Metric:", Location = new Point(210, 15), AutoSize = true };
            cmbProfitType = new ComboBox
            {
                Location = new Point(270, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProfitType.Items.AddRange(new object[] { "Current", "Estimate 24h", "Actual 24h" });
            cmbProfitType.SelectedIndex = 2; // Default to Actual 24h

            btnScan = new Button
            {
                Text = "Scan All Algorithms",
                Location = new Point(410, 10),
                Width = 150,
                Height = 30
            };
            btnScan.Click += async (s, e) => await ScanForOpportunitiesAsync();

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(570, 10),
                Width = 100,
                Height = 30
            };
            btnRefresh.Click += async (s, e) => await RefreshOpportunitiesAsync();

            panel.Controls.AddRange(new Control[] { lblMinMargin, numMinMargin, lblProfitType, cmbProfitType, btnScan, btnRefresh });

            // Controls row 2
            chkAutoMode = new CheckBox
            {
                Text = "Auto-Execute Mode (>15% margin)",
                Location = new Point(10, 50),
                Width = 250
            };
            chkAutoMode.CheckedChanged += OnAutoModeChanged;

            btnAutoExecute = new Button
            {
                Text = "Execute Selected",
                Location = new Point(270, 47),
                Width = 120,
                Height = 25,
                Enabled = false
            };
            btnAutoExecute.Click += async (s, e) => await ExecuteSelectedOpportunityAsync();

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(410, 50),
                AutoSize = true,
                ForeColor = Color.Green
            };

            panel.Controls.AddRange(new Control[] { chkAutoMode, btnAutoExecute, lblStatus });

            // DataGridView for opportunities
            dgvOpportunities = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            dgvOpportunities.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "Algorithm", Name = "Algorithm", FillWeight = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Type", Name = "Type", FillWeight = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "Margin %", Name = "Margin", FillWeight = 70 },
                new DataGridViewTextBoxColumn { HeaderText = "Profit BTC/day", Name = "Profit", FillWeight = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Buy Cost", Name = "BuyCost", FillWeight = 90 },
                new DataGridViewTextBoxColumn { HeaderText = "Revenue", Name = "Revenue", FillWeight = 90 },
                new DataGridViewTextBoxColumn { HeaderText = "Hashrate", Name = "Hashrate", FillWeight = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Notes", Name = "Notes", FillWeight = 200 }
            });

            dgvOpportunities.SelectionChanged += OnOpportunitySelected;
            dgvOpportunities.CellFormatting += OnCellFormatting;

            tab.Controls.Add(dgvOpportunities);
            tab.Controls.Add(panel);
        }

        private void SetupHistoryTab(TabPage tab)
        {
            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            dgvHistory.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "Timestamp", Name = "Timestamp", FillWeight = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "Algorithm", Name = "Algorithm", FillWeight = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Type", Name = "Type", FillWeight = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Expected Margin", Name = "ExpectedMargin", FillWeight = 90 },
                new DataGridViewTextBoxColumn { HeaderText = "Actual Margin", Name = "ActualMargin", FillWeight = 90 },
                new DataGridViewTextBoxColumn { HeaderText = "Expected Profit", Name = "ExpectedProfit", FillWeight = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Actual Profit", Name = "ActualProfit", FillWeight = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 80 }
            });

            lblStats = new Label
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10),
                Text = "Loading statistics..."
            };

            var btnRefreshHistory = new Button
            {
                Text = "Refresh History",
                Dock = DockStyle.Bottom,
                Height = 35
            };
            btnRefreshHistory.Click += (s, e) => RefreshHistory();

            tab.Controls.Add(dgvHistory);
            tab.Controls.Add(lblStats);
            tab.Controls.Add(btnRefreshHistory);
        }

        private void SetupRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 300000 // 5 minutes
            };
            _refreshTimer.Tick += async (s, e) => await AutoRefreshAsync();
        }

        private async Task ScanForOpportunitiesAsync()
        {
            try
            {
                SetStatus("Scanning all algorithms...", Color.Blue);
                btnScan.Enabled = false;

                var minMargin = (decimal)numMinMargin.Value;
                var profitType = GetSelectedProfitType();

                var opportunities = await _arbitrageCalculator.FindAllOpportunitiesAsync(
                    defaultHashrate: 1000m,
                    minProfitMargin: minMargin,
                    profitType: profitType,
                    cancellationToken: _cancellationTokenSource.Token
                );

                DisplayOpportunities(opportunities);
                SetStatus($"Found {opportunities.Count} opportunities (>{minMargin}% margin)", Color.Green);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to scan for opportunities");
                SetStatus($"Error: {ex.Message}", Color.Red);
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private async Task RefreshOpportunitiesAsync()
        {
            // Re-scan with current settings
            await ScanForOpportunitiesAsync();
        }

        private async Task AutoRefreshAsync()
        {
            if (chkAutoMode.Checked)
            {
                await RefreshOpportunitiesAsync();
            }
        }

        private void DisplayOpportunities(List<ArbitrageOpportunity> opportunities)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayOpportunities(opportunities)));
                return;
            }

            dgvOpportunities.Rows.Clear();

            foreach (var opp in opportunities.OrderByDescending(o => o.ProfitMargin))
            {
                var row = new DataGridViewRow();
                row.CreateCells(dgvOpportunities,
                    opp.Algorithm,
                    opp.Type,
                    opp.ProfitMargin.ToString("F2"),
                    opp.NetProfit.ToString("F8"),
                    opp.BuyCost.ToString("F8"),
                    opp.SellRevenue.ToString("F8"),
                    opp.RecommendedHashrate.ToString("F2"),
                    TruncateNotes(opp.Notes)
                );
                row.Tag = opp; // Store full opportunity
                dgvOpportunities.Rows.Add(row);
            }
        }

        private void OnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvOpportunities.Rows[e.RowIndex];
            if (row.Cells["Margin"].Value != null &&
                decimal.TryParse(row.Cells["Margin"].Value.ToString(), out var margin))
            {

                // Color code by margin
                if (margin > 15)
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                else if (margin > 10)
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                else if (margin > 5)
                    row.DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void OnOpportunitySelected(object sender, EventArgs e)
        {
            btnAutoExecute.Enabled = dgvOpportunities.SelectedRows.Count > 0;
        }

        private async void OnAutoModeChanged(object sender, EventArgs e)
        {
            if (chkAutoMode.Checked)
            {
                _refreshTimer.Start();
                SetStatus("Auto-execute mode ACTIVE - scanning every 5 minutes", Color.Orange);
            }
            else
            {
                _refreshTimer.Stop();
                SetStatus("Auto-execute mode OFF", Color.Green);
            }
        }

        private async Task ExecuteSelectedOpportunityAsync()
        {
            if (dgvOpportunities.SelectedRows.Count == 0) return;

            var opportunity = dgvOpportunities.SelectedRows[0].Tag as ArbitrageOpportunity;
            if (opportunity == null) return;

            var result = MessageBox.Show(
                $"Execute arbitrage?\n\n{opportunity.Type}\n{opportunity.Algorithm}\nMargin: {opportunity.ProfitMargin:F2}%\nProfit: {opportunity.NetProfit:F8} BTC/day",
                "Confirm Execution",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                await ExecuteOpportunityAsync(opportunity);
            }
        }

        private async Task ExecuteOpportunityAsync(ArbitrageOpportunity opportunity)
        {
            try
            {
                SetStatus($"Executing {opportunity.Type} for {opportunity.Algorithm}...", Color.Blue);

                var execution = await _autoExecutor.ExecuteAsync(opportunity, _cancellationTokenSource.Token);

                if (execution.Success)
                {
                    SetStatus($"✅ Executed successfully: {execution.Message}", Color.Green);
                    _tracker?.LogExecution(execution);
                    MessageBox.Show($"Success!\n\n{execution.Message}", "Execution Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    SetStatus($"❌ Execution failed: {execution.ErrorMessage}", Color.Red);
                    MessageBox.Show($"Failed!\n\n{execution.ErrorMessage}", "Execution Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex, "Failed to execute opportunity");
                SetStatus($"Error: {ex.Message}", Color.Red);
            }
        }

        private void RefreshHistory()
        {
            if (_tracker == null) return;

            var executions = _tracker.GetRecentExecutions(100);
            dgvHistory.Rows.Clear();

            foreach (var exec in executions.OrderByDescending(e => e.Timestamp))
            {
                dgvHistory.Rows.Add(
                    exec.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    exec.Algorithm,
                    exec.Type,
                    exec.ExpectedMargin.ToString("F2") + "%",
                    exec.ActualMargin?.ToString("F2") + "%" ?? "N/A",
                    exec.ExpectedProfit.ToString("F8"),
                    exec.ActualProfit?.ToString("F8") ?? "N/A",
                    exec.Status
                );
            }

            // Update stats
            var stats = _tracker.GetStatistics();
            lblStats.Text = $@"Performance Statistics:
Total Executions: {stats.TotalExecutions}
Successful: {stats.SuccessfulExecutions} ({stats.SuccessRate:F1}%)
Total Expected Profit: {stats.TotalExpectedProfit:F8} BTC
Total Actual Profit: {stats.TotalActualProfit:F8} BTC
Average Margin: {stats.AverageMargin:F2}%
Best Trade: {stats.BestTrade?.Algorithm} ({stats.BestTrade?.ActualMargin:F2}%)";
        }

        private void SetStatus(string message, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetStatus(message, color)));
                return;
            }

            lblStatus.Text = message;
            lblStatus.ForeColor = color;
            Logger.Instance.Info($"[ArbitrageDashboard] {message}");
        }

        private ProfitabilityType GetSelectedProfitType()
        {
            return cmbProfitType.SelectedIndex switch
            {
                0 => ProfitabilityType.Current,
                1 => ProfitabilityType.Estimate24h,
                2 => ProfitabilityType.Actual24h,
                _ => ProfitabilityType.Actual24h
            };
        }

        private string TruncateNotes(string notes)
        {
            if (string.IsNullOrEmpty(notes)) return "";
            var firstLine = notes.Split('\n')[0];
            return firstLine.Length > 80 ? firstLine.Substring(0, 77) + "..." : firstLine;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _cancellationTokenSource?.Cancel();
            base.OnFormClosing(e);
        }
    }
}

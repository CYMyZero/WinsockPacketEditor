using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WPELibrary.Lib;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;

namespace WPELibrary
{
    public partial class Socket_FilterForm : Form
    {
        private Socket_FilterInfo sfiSelect;
        private int LoadAllCount = 0;

        private TabPage tpPattern;
        private DataGridView dgvPattern;

        private Form tipForm;
        private Label tipLabel;

        private const string TIP_SEARCH_MODE =
            "顺序模式（默认）：特征按表格里的顺序在包中依次出现，第 2 个特征必须排在第 1 个后面。\r\n" +
            "无序模式：每个特征独立在包中搜索，顺序任意（第 2 个特征可以在第 1 个前面）。\r\n" +
            "举例：特征 10 08、20 05 两个：\r\n" +
            "　顺序模式：要求 10 08 在 20 05 前面出现才算命中。\r\n" +
            "　无序模式：20 05 在 10 08 前面也能命中。";

        private const string TIP_PATTERN =
            "特征：十六进制字节，空格分隔（如 10 08）。\r\n" +
            "输入时可不加空格，编辑完自动格式化（10081F → 10 08 1F）。\r\n" +
            "每行一个特征，全部特征命中才执行（顺序模式按顺序、无序模式任意）。";

        private const string TIP_MODIFY =
            "【替换】偏移=值，逗号分隔多条。偏移 0=特征第1字节、1=第2字节、-1=前1格、2=后1格。留空=不修改。\r\n" +
            "　案例：特征 10 08 改成 01 10 08 02 05，规则写 -1=01,2=02,3=05（长度不变，强制替换前后字节）。\r\n" +
            "【换包】在第一个特征行的修改规则填完整新数据包字节（如 01 02 03 04），命中特征后整包替换；第一行为空则不换包。\r\n" +
            "【拦截】无需填修改规则，特征命中即拦截。";

        #region//窗体加载

        public Socket_FilterForm(Socket_FilterInfo sfi)
        {
            try
            {
                MultiLanguage.SetDefaultLanguage(MultiLanguage.DefaultLanguage);
                InitializeComponent();                

                this.InitPatternUI();

                if (sfi != null)
                {
                    this.sfiSelect = sfi;

                    this.InitFrom();
                    this.InitDGV();

                    if (!this.bgwFilterInfo.IsBusy)
                    {
                        this.bFilterButton_Save.Enabled = false;
                        this.tcFilterInfo.Enabled = false;
                        this.lFilterInfo.Visible = true;

                        this.bgwFilterInfo.RunWorkerAsync();
                    }                    
                }
                else
                {
                    Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_28));
                    this.Close();
                }                                
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//初始化

        private void InitPatternUI()
        {
            try
            {
                this.gbFilterModifyFrom.MouseEnter += (s, e) => ShowTip(TIP_SEARCH_MODE);
                this.gbFilterModifyFrom.MouseLeave += (s, e) => HideTip();

                this.tpPattern = new TabPage();
                this.tpPattern.Text = "特征搜索";
                this.tpPattern.UseVisualStyleBackColor = true;

                this.dgvPattern = new DataGridView();
                this.dgvPattern.Dock = DockStyle.Fill;
                this.dgvPattern.AutoGenerateColumns = false;
                this.dgvPattern.AllowUserToAddRows = true;
                this.dgvPattern.AllowUserToDeleteRows = true;
                this.dgvPattern.RowHeadersVisible = false;
                this.dgvPattern.BackgroundColor = SystemColors.Window;
                this.dgvPattern.DefaultCellStyle.BackColor = Color.LightYellow;
                this.dgvPattern.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.dgvPattern.RowTemplate.Height = 30;
                this.dgvPattern.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvPattern.CellEndEdit += new DataGridViewCellEventHandler(this.dgvPattern_CellEndEdit);
                this.dgvPattern.CellMouseEnter += new DataGridViewCellEventHandler(this.dgvPattern_CellMouseEnter);
                this.dgvPattern.CellMouseLeave += new DataGridViewCellEventHandler(this.dgvPattern_CellMouseLeave);

                DataGridViewTextBoxColumn colPattern = new DataGridViewTextBoxColumn();
                colPattern.HeaderText = "特征（十六进制，如 10 08）";
                colPattern.Name = "colPattern";
                colPattern.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                colPattern.FillWeight = 50;
                this.dgvPattern.Columns.Add(colPattern);

                DataGridViewTextBoxColumn colModify = new DataGridViewTextBoxColumn();
                colModify.HeaderText = "修改规则（偏移=值，如 -1=01,2=05）";
                colModify.Name = "colModify";
                colModify.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                colModify.FillWeight = 50;
                this.dgvPattern.Columns.Add(colModify);

                ContextMenuStrip cmsPattern = new ContextMenuStrip();

                ToolStripMenuItem miAdd = new ToolStripMenuItem("添加特征");
                miAdd.Click += new EventHandler(this.bAddPattern_Click);
                cmsPattern.Items.Add(miAdd);

                ToolStripMenuItem miRemove = new ToolStripMenuItem("删除选中");
                miRemove.Click += new EventHandler(this.bRemovePattern_Click);
                cmsPattern.Items.Add(miRemove);

                cmsPattern.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem miUp = new ToolStripMenuItem("向上移动");
                miUp.Click += new EventHandler(this.bMoveUpPattern_Click);
                cmsPattern.Items.Add(miUp);

                ToolStripMenuItem miDown = new ToolStripMenuItem("向下移动");
                miDown.Click += new EventHandler(this.bMoveDownPattern_Click);
                cmsPattern.Items.Add(miDown);

                ToolStripMenuItem miTop = new ToolStripMenuItem("置顶");
                miTop.Click += new EventHandler(this.bMoveTopPattern_Click);
                cmsPattern.Items.Add(miTop);

                this.dgvPattern.ContextMenuStrip = cmsPattern;

                this.tpPattern.Controls.Add(this.dgvPattern);

                this.tcFilterInfo.Controls.Add(this.tpPattern);
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void dgvPattern_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == this.dgvPattern.Columns["colPattern"].Index && e.RowIndex >= 0)
                {
                    object value = this.dgvPattern.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    string input = value == null ? string.Empty : value.ToString();
                    string formatted = FormatHexPattern(input);
                    this.dgvPattern.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = formatted;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void dgvPattern_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == this.dgvPattern.Columns["colPattern"].Index)
                {
                    ShowTip(TIP_PATTERN);
                }
                else if (e.ColumnIndex == this.dgvPattern.Columns["colModify"].Index)
                {
                    ShowTip(TIP_MODIFY);
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void dgvPattern_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            HideTip();
        }

        private void EnsureTipForm()
        {
            if (tipForm == null)
            {
                tipForm = new TipForm();
                tipForm.FormBorderStyle = FormBorderStyle.None;
                tipForm.ShowInTaskbar = false;
                tipForm.StartPosition = FormStartPosition.Manual;
                tipForm.BackColor = System.Drawing.Color.FromArgb(255, 255, 225);
                tipForm.TopMost = true;
                tipForm.Owner = this;

                tipLabel = new Label();
                tipLabel.AutoSize = true;
                tipLabel.MaximumSize = new Size(560, 0);
                tipLabel.Padding = new Padding(10);
                tipLabel.ForeColor = SystemColors.InfoText;
                tipForm.Controls.Add(tipLabel);
            }
        }

        private void ShowTip(string text)
        {
            try
            {
                EnsureTipForm();
                tipLabel.Text = text;

                Size preferred = tipLabel.PreferredSize;
                tipForm.ClientSize = new Size(preferred.Width + 20, preferred.Height + 20);

                Point cursor = Cursor.Position;
                int x = cursor.X + 16;
                int y = cursor.Y + 16;

                Rectangle screen = Screen.FromPoint(cursor).WorkingArea;
                if (x + tipForm.Width > screen.Right) x = cursor.X - tipForm.Width - 16;
                if (y + tipForm.Height > screen.Bottom) y = cursor.Y - tipForm.Height - 16;

                tipForm.Location = new Point(x, y);
                tipForm.Show();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void HideTip()
        {
            try
            {
                if (tipForm != null && tipForm.Visible)
                {
                    tipForm.Hide();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private string FormatHexPattern(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            System.Text.StringBuilder hex = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                {
                    hex.Append(c);
                }
            }

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            for (int i = 0; i < hex.Length; i += 2)
            {
                if (i > 0)
                    result.Append(' ');

                int remain = hex.Length - i;
                result.Append(hex.ToString(i, remain >= 2 ? 2 : remain));
            }

            return result.ToString().ToUpper();
        }

        private void InitFrom()
        {
            try
            {
                this.Text = string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_16), sfiSelect.FName);
                this.lFilterInfo.Text = string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_175), "0");
                             
                switch (sfiSelect.FMode)
                {
                    case Socket_Cache.Filter.FilterMode.Normal:
                        this.rbFilterMode_Normal.Checked = true;
                        break;

                    case Socket_Cache.Filter.FilterMode.Advanced:
                    case Socket_Cache.Filter.FilterMode.Pattern:
                        this.rbFilterMode_Advanced.Checked = true;
                        break;
                }
                this.FilterModeChange();

                switch (sfiSelect.FAction)
                {
                    case Socket_Cache.Filter.FilterAction.Replace:
                        this.rbFilterAction_Replace.Checked = true;
                        break;

                    case Socket_Cache.Filter.FilterAction.Intercept:
                        this.rbFilterAction_Intercept.Checked = true;
                        break;

                    case Socket_Cache.Filter.FilterAction.Change:
                        this.rbFilterAction_Change.Checked = true;
                        break;

                    case Socket_Cache.Filter.FilterAction.NoModify_Display:
                        this.rbFilterAction_NoModify_Display.Checked = true;
                        break;

                    case Socket_Cache.Filter.FilterAction.NoModify_NoDisplay:
                        this.rbFilterAction_NoModify_NoDisplay.Checked = true;
                        break;                    
                }            

                switch (sfiSelect.FStartFrom)
                {
                    case Socket_Cache.Filter.FilterStartFrom.Head:
                        this.rbFilterModifyFrom_Head.Checked = true;
                        break;

                    case Socket_Cache.Filter.FilterStartFrom.Position:
                        this.rbFilterModifyFrom_Position.Checked = true;
                        break;
                }
                this.FilterModifyFromChange();

                this.cbFilterAction_Execute.Checked = sfiSelect.IsExecute;
                this.FilterAction_ExecuteChange();

                switch (sfiSelect.FEType)
                { 
                    case Socket_Cache.Filter.FilterExecuteType.Send:
                        this.cbbFilterAction_ExecuteType.SelectedIndex = 0;
                        break;

                    case Socket_Cache.Filter.FilterExecuteType.Robot:
                        this.cbbFilterAction_ExecuteType.SelectedIndex = 1;
                        break;
                }
                this.FilterAction_ExecuteTypeChanged();

                this.cbFilter_AppointHeader.Checked = sfiSelect.AppointHeader;
                this.txtFilter_HeaderContent.Text = sfiSelect.HeaderContent;
                this.FilterAppointHeaderChange();

                this.cbFilter_AppointSocket.Checked = sfiSelect.AppointSocket;
                this.nudFilter_SocketContent.Value = sfiSelect.SocketContent;
                this.FilterAppointSocketChange();

                this.cbFilter_AppointLength.Checked = sfiSelect.AppointLength;
                if (!string.IsNullOrEmpty(sfiSelect.LengthContent))
                {
                    if (sfiSelect.LengthContent.Contains("-"))
                    {
                        string[] sLengthContent = sfiSelect.LengthContent.Split('-');

                        if (int.TryParse(sLengthContent[0], out int iLenFrom))
                        {
                            this.nudFilter_LengthContent_From.Value = iLenFrom;
                        }

                        if (int.TryParse(sLengthContent[1], out int iLenTo))
                        {
                            this.nudFilter_LengthContent_To.Value = iLenTo;
                        }
                    }
                    else
                    {
                        if (int.TryParse(sfiSelect.LengthContent, out int iLength))
                        {
                            this.nudFilter_LengthContent_From.Value = iLength;
                            this.nudFilter_LengthContent_To.Value = iLength;
                        }
                    }                    
                }                
                this.FilterAppointLengthChange();

                this.cbFilter_AppointPort.Checked = sfiSelect.AppointPort;
                this.nudFilter_PortContent.Value = sfiSelect.PortContent;
                this.FilterAppointPortChange();

                this.cbProgressionContinuous.Checked = sfiSelect.IsProgressionContinuous;
                this.nudProgressionStep.Value = sfiSelect.ProgressionStep;
                this.cbProgressionCarry.Checked = sfiSelect.IsProgressionCarry;
                this.nudProgressionCarry.Value = sfiSelect.ProgressionCarryNumber;
                this.ProgressionCarryChange();

                this.txtFilterName.Text = sfiSelect.FName;
                this.cbFilterFunction_Send.Checked = sfiSelect.FFunction.Send;
                this.cbFilterFunction_SendTo.Checked = sfiSelect.FFunction.SendTo;
                this.cbFilterFunction_Recv.Checked = sfiSelect.FFunction.Recv;
                this.cbFilterFunction_RecvFrom.Checked = sfiSelect.FFunction.RecvFrom;
                this.cbFilterFunction_WSASend.Checked = sfiSelect.FFunction.WSASend;
                this.cbFilterFunction_WSASendTo.Checked = sfiSelect.FFunction.WSASendTo;
                this.cbFilterFunction_WSARecv.Checked = sfiSelect.FFunction.WSARecv;
                this.cbFilterFunction_WSARecvFrom.Checked = sfiSelect.FFunction.WSARecvFrom;                
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitDGV()
        {
            try
            {
                dgvFilterNormal.AutoGenerateColumns = false;
                dgvFilterNormal.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgvFilterNormal, true, null);

                dgvFilterAdvanced_Search.AutoGenerateColumns = false;
                dgvFilterAdvanced_Search.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgvFilterAdvanced_Search, true, null);

                dgvFilterAdvanced_Modify_FromHead.AutoGenerateColumns = false;
                dgvFilterAdvanced_Modify_FromHead.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgvFilterAdvanced_Modify_FromHead, true, null);

                dgvFilterAdvanced_Modify_FromPosition.AutoGenerateColumns = false;
                dgvFilterAdvanced_Modify_FromPosition.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgvFilterAdvanced_Modify_FromPosition, true, null);

                this.InitDGV_Normal();
                this.InitDGV_Advanced_Search();
                this.InitDGV_Advanced_Modify_Head();
                this.InitDGV_Advanced_Modify_Position();
                this.InitDGV_Normal_ByAdvance();

                this.dgvFilterAdvanced_Search.Height = this.tlpFilterAdvanced.Height / 2;
                this.dgvFilterAdvanced_Modify_FromHead.Height = this.tlpFilterAdvanced.Height / 2;
                this.dgvFilterAdvanced_Modify_FromPosition.Height = this.tlpFilterAdvanced.Height / 2;                

                if (dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells["col000"] != null)
                {
                    dgvFilterAdvanced_Modify_FromPosition.FirstDisplayedCell = dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells["col000"];
                }                

                this.InitProgressionPosition();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }        

        private void InitDGV_Normal()
        {
            try
            {
                if (this.dgvFilterNormal.Columns.Count > 0)
                {
                    this.dgvFilterNormal.Columns.Clear();
                }

                for (int i = 0; i < Socket_Cache.Filter.FilterSize_MaxLen; i++)
                {
                    DataGridViewTextBoxColumn dgv = Socket_Operation.InitDGVColumn(i + 1, Color.RoyalBlue, Color.LightYellow);
                    dgvFilterNormal.Columns.Add(dgv);
                    dgv.Width = dgv.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true) + 5;
                }

                if (dgvFilterNormal.Rows.Count == 0)
                {
                    dgvFilterNormal.Rows.Add();
                    dgvFilterNormal.Rows.Add();
                }                
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitDGV_Normal_ByAdvance()
        {
            try
            {
                int iWidth = this.dgvFilterAdvanced_Search.Columns[0].Width;

                for (int i = 0; i < this.dgvFilterNormal.Columns.Count; i++)
                {
                    this.dgvFilterNormal.Columns[i].Width = iWidth;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitDGV_Advanced_Search()
        {
            try
            {
                if (this.dgvFilterAdvanced_Search.Columns.Count > 0)
                {
                    this.dgvFilterAdvanced_Search.Columns.Clear();
                }                

                for (int i = 0; i < Socket_Cache.Filter.FilterSize_MaxLen; i++)
                {
                    DataGridViewTextBoxColumn dgv = Socket_Operation.InitDGVColumn(i + 1, Color.RoyalBlue, Color.LightYellow);
                    dgvFilterAdvanced_Search.Columns.Add(dgv);
                    dgv.Width = dgv.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true) + 5;
                }

                if (dgvFilterAdvanced_Search.Rows.Count == 0)
                {
                    dgvFilterAdvanced_Search.Rows.Add();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitDGV_Advanced_Modify_Head()
        {
            try
            {
                if (this.dgvFilterAdvanced_Modify_FromHead.Columns.Count > 0)
                {
                    this.dgvFilterAdvanced_Modify_FromHead.Columns.Clear();
                }                                

                for (int i = 0; i < Socket_Cache.Filter.FilterSize_MaxLen; i++)
                {
                    DataGridViewTextBoxColumn dgv = Socket_Operation.InitDGVColumn(i + 1, Color.RoyalBlue, Color.Yellow);
                    dgvFilterAdvanced_Modify_FromHead.Columns.Add(dgv);
                    dgv.Width = dgv.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true) + 5;
                }

                if (dgvFilterAdvanced_Modify_FromHead.Rows.Count == 0)
                {
                    dgvFilterAdvanced_Modify_FromHead.Rows.Add();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }        

        private void InitDGV_Advanced_Modify_Position()
        {
            try
            {
                if (this.dgvFilterAdvanced_Modify_FromPosition.Columns.Count > 0)
                {
                    this.dgvFilterAdvanced_Modify_FromPosition.Columns.Clear();
                }

                int iSize = Socket_Cache.Filter.FilterSize_MaxLen;

                for (int i = -iSize; i < iSize; i++)
                {
                    DataGridViewTextBoxColumn dgv = Socket_Operation.InitDGVColumn(i, Color.RoyalBlue, Color.Yellow);
                    dgvFilterAdvanced_Modify_FromPosition.Columns.Add(dgv);
                    dgv.Width = dgv.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true) + 5;
                }

                if (dgvFilterAdvanced_Modify_FromPosition.Rows.Count == 0)
                {
                    dgvFilterAdvanced_Modify_FromPosition.Rows.Add();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitProgressionPosition()
        {
            try
            {
                if (!string.IsNullOrEmpty(sfiSelect.ProgressionPosition))
                {
                    string[] slProgressionPosition = sfiSelect.ProgressionPosition.Split(',');

                    foreach (string sPosition in slProgressionPosition)
                    {
                        if (!string.IsNullOrEmpty(sPosition))
                        {
                            if (int.TryParse(sPosition, out int iIndex))
                            {
                                switch (sfiSelect.FMode)
                                {
                                    case Socket_Cache.Filter.FilterMode.Normal:

                                        if (dgvFilterNormal.Rows.Count == 2 && dgvFilterNormal.Columns.Count > iIndex)
                                        {
                                            this.dgvFilterNormal.Rows[1].Cells[iIndex].Style.BackColor = Color.DarkRed;
                                        }                                        

                                        break;

                                    case Socket_Cache.Filter.FilterMode.Advanced:

                                        switch (sfiSelect.FStartFrom)
                                        {
                                            case Socket_Cache.Filter.FilterStartFrom.Head:

                                                if (dgvFilterAdvanced_Modify_FromHead.Rows.Count == 1 && dgvFilterAdvanced_Modify_FromHead.Columns.Count > iIndex)
                                                {
                                                    this.dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[iIndex].Style.BackColor = Color.DarkRed;
                                                }                                                

                                                break;

                                            case Socket_Cache.Filter.FilterStartFrom.Position:

                                                iIndex += Socket_Cache.Filter.FilterSize_MaxLen;

                                                if (dgvFilterAdvanced_Modify_FromPosition.Rows.Count == 1 && dgvFilterAdvanced_Modify_FromPosition.Columns.Count > iIndex)
                                                {                                                    
                                                    this.dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[iIndex].Style.BackColor = Color.DarkRed;
                                                }                                                

                                                break;
                                        }

                                        break;
                                }                                
                            }
                        }
                    }                    
                }                
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }        

        #endregion

        #region//滤镜动作-执行

        private void cbFilterAction_Execute_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterAction_ExecuteChange();
        }      

        private void FilterAction_ExecuteChange()
        {
            this.cbbFilterAction_Execute.Enabled = this.cbbFilterAction_ExecuteType.Enabled = cbFilterAction_Execute.Checked;            
        }

        private void cbbFilterAction_ExecuteType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.FilterAction_ExecuteTypeChanged();
        }

        private void FilterAction_ExecuteTypeChanged()
        {
            try
            {
                if (this.cbbFilterAction_ExecuteType.SelectedIndex == 0)
                {
                    this.InitSendInfo();
                }
                else
                {
                    this.InitRobotInfo();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitSendInfo()
        {
            try
            {
                if (Socket_Cache.SendList.lstSend.Count > 0)
                {
                    cbbFilterAction_Execute.DataSource = Socket_Cache.SendList.lstSend;
                    cbbFilterAction_Execute.DisplayMember = "SName";
                    cbbFilterAction_Execute.ValueMember = "SID";

                    this.cbbFilterAction_Execute.SelectedValue = sfiSelect.SID;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void InitRobotInfo()
        {
            try
            {
                if (Socket_Cache.RobotList.lstRobot.Count > 0)
                {
                    cbbFilterAction_Execute.DataSource = Socket_Cache.RobotList.lstRobot;
                    cbbFilterAction_Execute.DisplayMember = "RName";
                    cbbFilterAction_Execute.ValueMember = "RID";

                    this.cbbFilterAction_Execute.SelectedValue = sfiSelect.RID;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//模式切换

        private void rbFilterMode_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterModeChange();
        }

        private void FilterModeChange()
        {
            try
            {
                if (rbFilterMode_Normal.Checked)
                {
                    this.tpNormal.Parent = this.tcFilterInfo;
                    this.tpAdvanced.Parent = null;
                    this.tpPattern.Parent = null;

                    this.gbFilterModifyFrom.Enabled = false;
                    this.gbProgression.Enabled = true;
                }
                else if (rbFilterMode_Advanced.Checked)
                {
                    this.tpNormal.Parent = null;
                    this.tpAdvanced.Parent = null;
                    this.tpPattern.Parent = this.tcFilterInfo;

                    this.gbFilterModifyFrom.Enabled = true;
                    this.gbProgression.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//修改起始于切换

        private void rbFilterModifyFrom_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterModifyFromChange();
        }

        private void FilterModifyFromChange()
        {
            try
            {
                if (rbFilterModifyFrom_Head.Checked)
                {
                    this.dgvFilterAdvanced_Modify_FromHead.Visible = true;
                    this.dgvFilterAdvanced_Modify_FromPosition.Visible = false;
                }
                else if (rbFilterModifyFrom_Position.Checked)
                {
                    this.dgvFilterAdvanced_Modify_FromHead.Visible = false;
                    this.dgvFilterAdvanced_Modify_FromPosition.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//指定类型

        #region//指定包头

        private void cbFilter_AppointHeader_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterAppointHeaderChange();
        }

        private void FilterAppointHeaderChange()
        {
            this.txtFilter_HeaderContent.Enabled = this.cbFilter_AppointHeader.Checked;
        }

        private void txtFilter_HeaderContent_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!Socket_Operation.CheckTextInput_IsHex(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//指定套接字

        private void cbFilter_AppointSocket_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterAppointSocketChange();
        }

        private void FilterAppointSocketChange()
        {
            this.nudFilter_SocketContent.Enabled = this.cbFilter_AppointSocket.Checked;
        }

        #endregion

        #region//指定长度

        private void cbFilter_AppointLength_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterAppointLengthChange();
        }

        private void FilterAppointLengthChange()
        {
            this.nudFilter_LengthContent_From.Enabled = this.nudFilter_LengthContent_To.Enabled = this.cbFilter_AppointLength.Checked;
        }

        #endregion

        #region//指定端口

        private void cbFilter_AppointPort_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterAppointPortChange();
        }

        private void FilterAppointPortChange()
        {
            this.nudFilter_PortContent.Enabled = this.cbFilter_AppointPort.Checked;
        }

        #endregion

        #endregion

        #region//递进

        private void cbProgressionCarry_CheckedChanged(object sender, EventArgs e)
        {
            this.ProgressionCarryChange();
        }

        private void ProgressionCarryChange()
        {
            try
            {
                this.nudProgressionCarry.Enabled = this.cbProgressionCarry.Checked;
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//显示滤镜内容（异步）

        private void bgwFilterInfo_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            this.ShowFilterInfo();
        }

        private void bgwFilterInfo_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.lFilterInfo.Text = string.Format(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_175), e.ProgressPercentage.ToString());
        }

        private void bgwFilterInfo_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.lFilterInfo.Visible = false;
            this.tcFilterInfo.Enabled = true;
            this.bFilterButton_Save.Enabled = true;
        }

        private void ShowFilterInfo()
        {
            try
            {
                if (sfiSelect.FMode == Socket_Cache.Filter.FilterMode.Pattern)
                {
                    this.LoadPatternFromString();
                    return;
                }

                if (!string.IsNullOrEmpty(sfiSelect.FSearch))
                {
                    string[] sSearchAll = sfiSelect.FSearch.Split(',');

                    foreach (string s in sSearchAll)
                    {
                        if (int.TryParse(s.Split('|')[0], out int iIndex))
                        {
                            string sValue = s.Split('|')[1];

                            if (this.dgvFilterNormal.Rows.Count == 2)
                            {
                                if (iIndex < this.dgvFilterNormal.Rows[0].Cells.Count)
                                {
                                    this.dgvFilterNormal.Rows[0].Cells[iIndex].Value = sValue;
                                }
                            }

                            if (this.dgvFilterAdvanced_Search.Rows.Count == 1)
                            {
                                if (iIndex < this.dgvFilterAdvanced_Search.Rows[0].Cells.Count)
                                {
                                    this.dgvFilterAdvanced_Search.Rows[0].Cells[iIndex].Value = sValue;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(sfiSelect.FModify))
                {
                    string[] sModifyAll = sfiSelect.FModify.Split(',');
                    this.LoadAllCount = sModifyAll.Length;

                    int LoadCount = 0;
                    foreach (string s in sModifyAll)
                    {
                        if (int.TryParse(s.Split('|')[0], out int iIndex))
                        {
                            string sValue = s.Split('|')[1];

                            switch (sfiSelect.FMode)
                            {
                                case Socket_Cache.Filter.FilterMode.Normal:

                                    if (this.dgvFilterNormal.Rows.Count == 2)
                                    {
                                        if (iIndex < this.dgvFilterNormal.Rows[1].Cells.Count)
                                        {
                                            this.dgvFilterNormal.Rows[1].Cells[iIndex].Value = sValue;
                                        }
                                    }

                                    break;

                                case Socket_Cache.Filter.FilterMode.Advanced:

                                    switch (sfiSelect.FStartFrom)
                                    {
                                        case Socket_Cache.Filter.FilterStartFrom.Head:

                                            if (this.dgvFilterAdvanced_Modify_FromHead.Rows.Count == 1)
                                            {
                                                if (iIndex < this.dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells.Count)
                                                {
                                                    this.dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[iIndex].Value = sValue;
                                                }
                                            }

                                            break;

                                        case Socket_Cache.Filter.FilterStartFrom.Position:

                                            if (this.dgvFilterAdvanced_Modify_FromPosition.Rows.Count == 1)
                                            {
                                                iIndex += Socket_Cache.Filter.FilterSize_MaxLen;

                                                if (iIndex < this.dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells.Count)
                                                {                                                    
                                                    this.dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[iIndex].Value = sValue;

                                                    LoadCount++;
                                                    this.bgwFilterInfo.ReportProgress(LoadCount * 100 / LoadAllCount);
                                                }
                                            }

                                            break;
                                    }

                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LoadPatternFromString()
        {
            try
            {
                this.dgvPattern.Rows.Clear();

                List<string> searches = new List<string>();
                if (!string.IsNullOrEmpty(sfiSelect.FSearch))
                {
                    searches.AddRange(sfiSelect.FSearch.Split(';'));
                }

                List<string> modifies = new List<string>();
                if (!string.IsNullOrEmpty(sfiSelect.FModify))
                {
                    modifies.AddRange(sfiSelect.FModify.Split(';'));
                }

                for (int i = 0; i < searches.Count; i++)
                {
                    string sPattern = searches[i];
                    if (string.IsNullOrEmpty(sPattern))
                    {
                        continue;
                    }

                    int rowIdx = this.dgvPattern.Rows.Add();
                    this.dgvPattern.Rows[rowIdx].Cells["colPattern"].Value = sPattern;

                    if (i < modifies.Count)
                    {
                        this.dgvPattern.Rows[rowIdx].Cells["colModify"].Value = modifies[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void bAddPattern_Click(object sender, EventArgs e)
        {
            try
            {
                this.dgvPattern.Rows.Add();
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void bRemovePattern_Click(object sender, EventArgs e)
        {
            try
            {
                List<DataGridViewRow> rowsToRemove = new List<DataGridViewRow>();

                foreach (DataGridViewRow row in this.dgvPattern.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        rowsToRemove.Add(row);
                    }
                }

                if (rowsToRemove.Count == 0 &&
                    this.dgvPattern.CurrentRow != null &&
                    !this.dgvPattern.CurrentRow.IsNewRow)
                {
                    rowsToRemove.Add(this.dgvPattern.CurrentRow);
                }

                foreach (DataGridViewRow row in rowsToRemove)
                {
                    this.dgvPattern.Rows.Remove(row);
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void bMoveUpPattern_Click(object sender, EventArgs e)
        {
            MovePatternRow(-1);
        }

        private void bMoveDownPattern_Click(object sender, EventArgs e)
        {
            MovePatternRow(1);
        }

        private void bMoveTopPattern_Click(object sender, EventArgs e)
        {
            MovePatternRowToTop();
        }

        private void MovePatternRow(int offset)
        {
            try
            {
                if (this.dgvPattern.CurrentRow == null || this.dgvPattern.CurrentRow.IsNewRow)
                    return;

                int index = this.dgvPattern.CurrentRow.Index;
                int target = index + offset;

                int lastDataIndex = this.dgvPattern.Rows.Count - 1;
                if (lastDataIndex >= 0 && this.dgvPattern.Rows[lastDataIndex].IsNewRow)
                    lastDataIndex--;

                if (target < 0 || target > lastDataIndex)
                    return;

                SwapPatternRows(index, target);

                this.dgvPattern.ClearSelection();
                this.dgvPattern.Rows[target].Selected = true;
                this.dgvPattern.CurrentCell = this.dgvPattern.Rows[target].Cells["colPattern"];
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void MovePatternRowToTop()
        {
            try
            {
                if (this.dgvPattern.CurrentRow == null || this.dgvPattern.CurrentRow.IsNewRow)
                    return;

                int index = this.dgvPattern.CurrentRow.Index;
                if (index == 0)
                    return;

                DataGridViewRow row = this.dgvPattern.Rows[index];
                object pattern = row.Cells["colPattern"].Value;
                object modify = row.Cells["colModify"].Value;

                this.dgvPattern.Rows.RemoveAt(index);
                this.dgvPattern.Rows.Insert(0, pattern, modify);
                int newIndex = 0;

                this.dgvPattern.ClearSelection();
                this.dgvPattern.Rows[newIndex].Selected = true;
                this.dgvPattern.CurrentCell = this.dgvPattern.Rows[newIndex].Cells["colPattern"];
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SwapPatternRows(int indexA, int indexB)
        {
            DataGridViewRow rowA = this.dgvPattern.Rows[indexA];
            DataGridViewRow rowB = this.dgvPattern.Rows[indexB];

            object patternA = rowA.Cells["colPattern"].Value;
            object modifyA = rowA.Cells["colModify"].Value;

            rowA.Cells["colPattern"].Value = rowB.Cells["colPattern"].Value;
            rowA.Cells["colModify"].Value = rowB.Cells["colModify"].Value;
            rowB.Cells["colPattern"].Value = patternA;
            rowB.Cells["colModify"].Value = modifyA;
        }

        private void DGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.Value != null)
                {
                    e.Value = e.Value.ToString().ToUpper();
                    e.FormattingApplied = true;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion        

        #region//多选单元格（拖拽框选 + Shift点击）

        private DataGridViewCell _selectStartCell = null;
        private bool _isSelecting = false;

        private void DGV_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    DataGridView dgv = sender as DataGridView;
                    if (dgv == null) return;

                    if (Control.ModifierKeys == Keys.Shift && _selectStartCell != null)
                    {
                        // Shift+点击：从起始单元格到当前单元格框选
                        SelectCellRange(dgv, _selectStartCell, dgv[e.ColumnIndex, e.RowIndex]);
                    }
                    else
                    {
                        // 普通点击：记录起始单元格，开始拖拽选择
                        _selectStartCell = dgv[e.ColumnIndex, e.RowIndex];
                        _isSelecting = true;
                        dgv.ClearSelection();
                        _selectStartCell.Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DGV_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (_isSelecting && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    DataGridView dgv = sender as DataGridView;
                    if (dgv == null) return;

                    DataGridViewCell endCell = dgv[e.ColumnIndex, e.RowIndex];
                    SelectCellRange(dgv, _selectStartCell, endCell);
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void DGV_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isSelecting = false;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void SelectCellRange(DataGridView dgv, DataGridViewCell startCell, DataGridViewCell endCell)
        {
            try
            {
                if (startCell == null || endCell == null) return;

                int minRow = Math.Min(startCell.RowIndex, endCell.RowIndex);
                int maxRow = Math.Max(startCell.RowIndex, endCell.RowIndex);
                int minCol = Math.Min(startCell.ColumnIndex, endCell.ColumnIndex);
                int maxCol = Math.Max(startCell.ColumnIndex, endCell.ColumnIndex);

                dgv.ClearSelection();
                for (int r = minRow; r <= maxRow; r++)
                {
                    for (int c = minCol; c <= maxCol; c++)
                    {
                        if (r >= 0 && r < dgv.RowCount && c >= 0 && c < dgv.ColumnCount)
                        {
                            dgv[c, r].Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//粘贴数据（异步）

        private void dgvFilterNormal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                string sClipboardText = Clipboard.GetText().Trim();
                this.PastePacketData(dgvFilterNormal, sClipboardText);
            }
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                this.DeleteSelectedCells(dgvFilterNormal);
            }
        }

        private void dgvFilterAdvanced_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                string sClipboardText = Clipboard.GetText().Trim();
                this.PastePacketData(dgvFilterAdvanced_Search, sClipboardText);
            }
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                this.DeleteSelectedCells(dgvFilterAdvanced_Search);
            }
        }

        private void dgvFilterAdvanced_Modify_FromHead_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                string sClipboardText = Clipboard.GetText().Trim();
                this.PastePacketData(dgvFilterAdvanced_Modify_FromHead, sClipboardText);
            }
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                this.DeleteSelectedCells(dgvFilterAdvanced_Modify_FromHead);
            }
        }

        private void dgvFilterAdvanced_Modify_FromPosition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                string sClipboardText = Clipboard.GetText().Trim();
                this.PastePacketData(dgvFilterAdvanced_Modify_FromPosition, sClipboardText);
            }
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                this.DeleteSelectedCells(dgvFilterAdvanced_Modify_FromPosition);
            }
        }

        private void DeleteSelectedCells(DataGridView dgv)
        {
            try
            {
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    if (cell.RowIndex >= 0 && cell.ColumnIndex >= 0)
                    {
                        cell.Value = DBNull.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private async void PastePacketData(DataGridView dgv, string sData)
        {
            this.bFilterButton_Save.Enabled = false;

            await Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(sData) && Socket_Operation.IsHexString(sData))
                    {
                        string[] DataCells = sData.Split(' ');

                        int iRow = dgv.CurrentCell.RowIndex;
                        int iCol = dgv.CurrentCell.ColumnIndex;

                        for (int i = 0; i < DataCells.Length; i++)
                        {
                            if (iCol + i < dgv.ColumnCount)
                            {
                                dgv[iCol + i, iRow].Value = Convert.ChangeType(DataCells[i].ToUpper(), dgv[iCol + i, iRow].ValueType);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_42));
                    }
                }
                catch (Exception ex)
                {
                    Socket_Operation.DoLog(nameof(PastePacketData), ex.Message);
                }
            });

            this.bFilterButton_Save.Enabled = true;
        }

        #endregion

        #region//滤镜设置合法性检测

        public bool CheckFilterIsValid()
        {
            bool bReturn = true;

            try
            {
                string sCheckValue = string.Empty;

                //滤镜名称
                sCheckValue = this.txtFilterName.Text.Trim();
                if (string.IsNullOrEmpty(sCheckValue))
                {
                    Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_19));
                    return false;
                }

                //普通滤镜
                for (int i = 0; i < this.dgvFilterNormal.Columns.Count; i++)
                {
                    if (dgvFilterNormal.Rows[0].Cells[i].Value != null)
                    {
                        sCheckValue = dgvFilterNormal.Rows[0].Cells[i].Value.ToString().Trim();
                        if (!Socket_Operation.IsValidFilterString(sCheckValue))
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_83));
                            return false;
                        }
                    }

                    if (dgvFilterNormal.Rows[1].Cells[i].Value != null)
                    {
                        sCheckValue = dgvFilterNormal.Rows[1].Cells[i].Value.ToString().Trim();
                        if (!Socket_Operation.IsValidFilterString(sCheckValue))
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_83));
                            return false;
                        }
                    }
                }

                //高级滤镜（搜索）
                for (int i = 0; i < this.dgvFilterAdvanced_Search.Columns.Count; i++)
                {
                    if (dgvFilterAdvanced_Search.Rows[0].Cells[i].Value != null)
                    {
                        sCheckValue = dgvFilterAdvanced_Search.Rows[0].Cells[i].Value.ToString().Trim();
                        if (!Socket_Operation.IsValidFilterString(sCheckValue))
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_83));
                            return false;
                        }
                    }
                }

                //高级滤镜（修改 - 从头开始）
                for (int i = 0; i < this.dgvFilterAdvanced_Modify_FromHead.Columns.Count; i++)
                {
                    if (dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[i].Value != null)
                    {
                        sCheckValue = dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[i].Value.ToString().Trim();
                        if (!Socket_Operation.IsValidFilterString(sCheckValue))
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_83));
                            return false;
                        }
                    }
                }

                //高级滤镜（修改 - 自发现有连锁的位置）
                for (int i = 0; i < this.dgvFilterAdvanced_Modify_FromPosition.Columns.Count; i++)
                {
                    if (dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[i].Value != null)
                    {
                        sCheckValue = dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[i].Value.ToString().Trim();
                        if (!Socket_Operation.IsValidFilterString(sCheckValue))
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_83));
                            return false;
                        }
                    }
                }

                //换包（数据完整度检测）
                if (this.rbFilterAction_Change.Checked)
                {
                    int iMaxIndex = 0;

                    //普通滤镜
                    if (this.rbFilterMode_Normal.Checked)
                    {
                        for (int i = 0; i < this.dgvFilterNormal.Columns.Count; i++)
                        {
                            if (dgvFilterNormal.Rows[1].Cells[i].Value != null)
                            {
                                sCheckValue = dgvFilterNormal.Rows[1].Cells[i].Value.ToString().Trim();
                                if (!string.IsNullOrEmpty(sCheckValue))
                                {
                                    iMaxIndex = i;
                                }
                            }
                        }

                        if (iMaxIndex == 0)
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_174));
                            return false;
                        }

                        for (int i = 0; i < iMaxIndex; i++)
                        {
                            if (dgvFilterNormal.Rows[1].Cells[i].Value == null)
                            {
                                Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_174));
                                return false;
                            }
                        }
                    }

                    //特征搜索滤镜（高级，换包只取第一个特征行的包）
                    if (this.rbFilterMode_Advanced.Checked)
                    {
                        bool hasPacket = false;

                        foreach (DataGridViewRow row in this.dgvPattern.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string sPattern = row.Cells["colPattern"].Value == null ? string.Empty : row.Cells["colPattern"].Value.ToString().Trim();
                            if (string.IsNullOrEmpty(sPattern))
                            {
                                continue;
                            }

                            string sModify = row.Cells["colModify"].Value == null ? string.Empty : row.Cells["colModify"].Value.ToString().Trim();
                            hasPacket = !string.IsNullOrEmpty(sModify);
                            break;
                        }

                        if (!hasPacket)
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_174));
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }

            return bReturn;
        }

        #endregion

        #region//关闭按钮

        private void bFilterButton_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region//保存按钮       

        private void bFilterButton_Save_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.CheckFilterIsValid())
                {
                    string sFName_New = this.txtFilterName.Text.Trim();
                    string sHeaderContent_New = string.Empty;
                    string sLengthContent_New = string.Empty;
                    decimal dSocketContent_New = 0;                    
                    decimal dPortContent_New = 0;
                    decimal dProgressionStep_New = 1;
                    decimal dProgressionCarryNumber_New = 1;
                    int iProgressionCount_New = 0;                    
                    bool bIsExecute_New, bIsProgressionContinuous_New, bIsProgressionCarry_New;
                    bool bAppointHeader_New, bAppointSocket_New, bAppointLength_New, bAppointPort_New;
                    StringBuilder sbProgression = new StringBuilder();
                    StringBuilder sbSearch = new StringBuilder();
                    StringBuilder sbModify = new StringBuilder();

                    Socket_Cache.Filter.FilterMode FilterMode_New;
                    Socket_Cache.Filter.FilterAction FilterAction_New;
                    Socket_Cache.Filter.FilterExecuteType FilterExecuteType_New;
                    Guid SID_New = Guid.Empty;
                    Guid RID_New = Guid.Empty;
                    Socket_Cache.Filter.FilterFunction FilterFunction_New;
                    Socket_Cache.Filter.FilterStartFrom FilterStartFrom_New;

                    bIsExecute_New = this.cbFilterAction_Execute.Checked;
                    bAppointHeader_New = this.cbFilter_AppointHeader.Checked;
                    bAppointSocket_New = this.cbFilter_AppointSocket.Checked;
                    bAppointLength_New = this.cbFilter_AppointLength.Checked;
                    bAppointPort_New = this.cbFilter_AppointPort.Checked;
                    bIsProgressionContinuous_New = this.cbProgressionContinuous.Checked;
                    bIsProgressionCarry_New = this.cbProgressionCarry.Checked;

                    sHeaderContent_New = this.txtFilter_HeaderContent.Text.Trim();
                    sLengthContent_New = this.nudFilter_LengthContent_From.Value.ToString() + "-" + this.nudFilter_LengthContent_To.Value.ToString();
                    dSocketContent_New = this.nudFilter_SocketContent.Value;                    
                    dPortContent_New = this.nudFilter_PortContent.Value;
                    dProgressionStep_New = this.nudProgressionStep.Value;
                    dProgressionCarryNumber_New = this.nudProgressionCarry.Value;

                    if (rbFilterMode_Normal.Checked)
                    {
                        FilterMode_New = Socket_Cache.Filter.FilterMode.Normal;
                    }
                    else if (rbFilterMode_Advanced.Checked)
                    {
                        FilterMode_New = Socket_Cache.Filter.FilterMode.Pattern;
                    }
                    else
                    {
                        FilterMode_New = Socket_Cache.Filter.FilterMode.Normal;
                    }

                    if (rbFilterAction_Replace.Checked)
                    {
                        FilterAction_New = Socket_Cache.Filter.FilterAction.Replace;
                    }
                    else if (rbFilterAction_Intercept.Checked)
                    {
                        FilterAction_New = Socket_Cache.Filter.FilterAction.Intercept;
                    }
                    else if (rbFilterAction_Change.Checked)
                    {
                        FilterAction_New = Socket_Cache.Filter.FilterAction.Change;
                    }
                    else if (rbFilterAction_NoModify_Display.Checked)
                    {
                        FilterAction_New = Socket_Cache.Filter.FilterAction.NoModify_Display;
                    }
                    else if (rbFilterAction_NoModify_NoDisplay.Checked)
                    {
                        FilterAction_New = Socket_Cache.Filter.FilterAction.NoModify_NoDisplay;
                    }
                    else
                    {
                        FilterAction_New = Socket_Cache.Filter.FilterAction.NoModify_Display;
                    }

                    if (cbFilterAction_Execute.Checked)
                    {
                        if (this.cbbFilterAction_ExecuteType.SelectedIndex == 0)
                        {
                            FilterExecuteType_New = Socket_Cache.Filter.FilterExecuteType.Send;

                            if (cbbFilterAction_Execute.SelectedValue != null)
                            {
                                SID_New = (Guid)cbbFilterAction_Execute.SelectedValue;
                            }
                        }
                        else if (this.cbbFilterAction_ExecuteType.SelectedIndex == 1)
                        {
                            FilterExecuteType_New = Socket_Cache.Filter.FilterExecuteType.Robot;

                            if (cbbFilterAction_Execute.SelectedValue != null)
                            {
                                RID_New = (Guid)cbbFilterAction_Execute.SelectedValue;
                            }
                        }
                        else
                        {
                            FilterExecuteType_New = new Socket_Cache.Filter.FilterExecuteType();
                        }
                    }
                    else
                    {
                        FilterExecuteType_New = new Socket_Cache.Filter.FilterExecuteType();
                    }

                    FilterFunction_New.Send = this.cbFilterFunction_Send.Checked;
                    FilterFunction_New.SendTo = this.cbFilterFunction_SendTo.Checked;
                    FilterFunction_New.Recv = this.cbFilterFunction_Recv.Checked;
                    FilterFunction_New.RecvFrom = this.cbFilterFunction_RecvFrom.Checked;
                    FilterFunction_New.WSASend = this.cbFilterFunction_WSASend.Checked;
                    FilterFunction_New.WSASendTo = this.cbFilterFunction_WSASendTo.Checked;
                    FilterFunction_New.WSARecv = this.cbFilterFunction_WSARecv.Checked;
                    FilterFunction_New.WSARecvFrom = this.cbFilterFunction_WSARecvFrom.Checked;

                    if (rbFilterModifyFrom_Head.Checked)
                    {
                        FilterStartFrom_New = Socket_Cache.Filter.FilterStartFrom.Head;
                    }
                    else
                    {
                        FilterStartFrom_New = Socket_Cache.Filter.FilterStartFrom.Position;
                    }                    

                    switch (FilterMode_New)
                    {
                        case Socket_Cache.Filter.FilterMode.Normal:

                            for (int i = 0; i < this.dgvFilterNormal.Columns.Count; i++)
                            {
                                if (dgvFilterNormal.Rows[1].Cells[i].Style.BackColor == Color.DarkRed)
                                {
                                    sbProgression.Append(i).Append(",");
                                }

                                if (dgvFilterNormal.Rows[0].Cells[i].Value != null)
                                {
                                    string sSearchValue = dgvFilterNormal.Rows[0].Cells[i].Value.ToString().Trim();

                                    if (!String.IsNullOrEmpty(sSearchValue))
                                    {
                                        sbSearch.Append(i).Append("|").Append(sSearchValue).Append(",");
                                    }
                                }

                                if (dgvFilterNormal.Rows[1].Cells[i].Value != null)
                                {
                                    string sModifyValue = dgvFilterNormal.Rows[1].Cells[i].Value.ToString().Trim();

                                    if (!String.IsNullOrEmpty(sModifyValue))
                                    {
                                        sbModify.Append(i).Append("|").Append(sModifyValue).Append(",");
                                    }
                                }
                            }

                            break;

                        case Socket_Cache.Filter.FilterMode.Advanced:

                            for (int i = 0; i < this.dgvFilterAdvanced_Search.Columns.Count; i++)
                            {
                                string sValue = string.Empty;

                                if (dgvFilterAdvanced_Search.Rows[0].Cells[i].Value != null)
                                {
                                    sValue = dgvFilterAdvanced_Search.Rows[0].Cells[i].Value.ToString().Trim();
                                }

                                if (!String.IsNullOrEmpty(sValue))
                                {
                                    sbSearch.Append(i).Append("|").Append(sValue).Append(",");
                                }
                            }

                            switch (FilterStartFrom_New)
                            {
                                case Socket_Cache.Filter.FilterStartFrom.Head:

                                    for (int i = 0; i < this.dgvFilterAdvanced_Modify_FromHead.Columns.Count; i++)
                                    {
                                        if (dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[i].Style.BackColor == Color.DarkRed)
                                        {
                                            sbProgression.Append(i).Append(",");
                                        }

                                        if (dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[i].Value != null)
                                        {
                                            string sValue = dgvFilterAdvanced_Modify_FromHead.Rows[0].Cells[i].Value.ToString().Trim();

                                            if (!String.IsNullOrEmpty(sValue))
                                            {
                                                sbModify.Append(i).Append("|").Append(sValue).Append(",");
                                            }
                                        }
                                    }

                                    break;

                                case Socket_Cache.Filter.FilterStartFrom.Position:

                                    for (int i = 0; i < this.dgvFilterAdvanced_Modify_FromPosition.Columns.Count; i++)
                                    {
                                        int iIndex = int.Parse(dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[i].OwningColumn.HeaderText);

                                        if (dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[i].Style.BackColor == Color.DarkRed)
                                        {                                            
                                            sbProgression.Append(iIndex).Append(",");
                                        }

                                        if (dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[i].Value != null)
                                        {
                                            string sValue = dgvFilterAdvanced_Modify_FromPosition.Rows[0].Cells[i].Value.ToString().Trim();

                                            if (!String.IsNullOrEmpty(sValue))
                                            {
                                                sbModify.Append(iIndex).Append("|").Append(sValue).Append(",");
                                            }
                                        }
                                    }

                                    break;
                            }

                            break;

                        case Socket_Cache.Filter.FilterMode.Pattern:
                            {
                                bool isFirst = true;

                                foreach (DataGridViewRow row in this.dgvPattern.Rows)
                                {
                                    if (row.IsNewRow)
                                    {
                                        continue;
                                    }

                                    string sPattern = row.Cells["colPattern"].Value == null ? string.Empty : row.Cells["colPattern"].Value.ToString().Trim();
                                    string sPatternModify = row.Cells["colModify"].Value == null ? string.Empty : row.Cells["colModify"].Value.ToString().Trim();

                                    if (string.IsNullOrEmpty(sPattern))
                                    {
                                        continue;
                                    }

                                    if (!isFirst)
                                    {
                                        sbSearch.Append(";");
                                        sbModify.Append(";");
                                    }

                                    sbSearch.Append(sPattern);
                                    sbModify.Append(sPatternModify);
                                    isFirst = false;
                                }
                            }

                            break;
                    }

                    string sProgression_New = sbProgression.ToString().TrimEnd(',');
                    string sSearch_New = sbSearch.ToString().TrimEnd(',');
                    string sModify_New = sbModify.ToString().TrimEnd(',');

                    Socket_Cache.Filter.UpdateFilter(
                        sfiSelect,
                        sFName_New,
                        bAppointHeader_New,
                        sHeaderContent_New,
                        bAppointSocket_New,
                        dSocketContent_New,
                        bAppointLength_New,
                        sLengthContent_New,
                        bAppointPort_New,
                        dPortContent_New,
                        FilterMode_New,
                        FilterAction_New,
                        bIsExecute_New,
                        FilterExecuteType_New,
                        SID_New,
                        RID_New,
                        FilterFunction_New,
                        FilterStartFrom_New,
                        bIsProgressionContinuous_New,
                        dProgressionStep_New,
                        bIsProgressionCarry_New,
                        dProgressionCarryNumber_New,
                        sProgression_New,
                        iProgressionCount_New,
                        sSearch_New,
                        sModify_New);

                    this.Close();
                }                
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion        

        #region//右键菜单

        private void cmsDGV_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string sDGVName = ((ContextMenuStrip)sender).SourceControl.Name;
            string sItemText = e.ClickedItem.Name;

            cmsDGV.Close();

            try
            {
                string sCellText = string.Empty;

                DataGridView dgv = new DataGridView();

                switch (sDGVName)
                {
                    case "dgvFilterNormal":
                        dgv = this.dgvFilterNormal;
                        break;

                    case "dgvFilterAdvanced_Search":
                        dgv = this.dgvFilterAdvanced_Search;
                        break;

                    case "dgvFilterAdvanced_Modify_FromHead":
                        dgv = this.dgvFilterAdvanced_Modify_FromHead;
                        break;

                    case "dgvFilterAdvanced_Modify_FromPosition":
                        dgv = this.dgvFilterAdvanced_Modify_FromPosition;
                        break;
                }

                switch (sItemText)
                {
                    case "cmsDGV_Copy":

                        if (dgv.CurrentCell.Value != null)
                        {
                            sCellText = dgv.CurrentCell.Value.ToString();
                            Clipboard.SetText(sCellText);
                        }
                        
                        break;

                    case "cmsDGV_Cut":

                        if (dgv.CurrentCell.Value != null)
                        {
                            sCellText = dgv.CurrentCell.Value.ToString();
                            Clipboard.SetText(sCellText);
                            dgv.CurrentCell.Value = null;
                        }
                        
                        break;

                    case "cmsDGV_Paste":

                        string sClipboardText = Clipboard.GetText().Trim();
                        this.PastePacketData(dgv, sClipboardText);

                        break;

                    case "cmsDGV_Delete":

                        this.DeleteSelectedCells(dgv);

                        break;

                    case "cmsDGV_Progression_Enable":

                        if (dgv.Name.Equals("dgvFilterAdvanced_Search"))
                        {
                            Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_73));
                            break;
                        }

                        if (dgv.Name.Equals("dgvFilterNormal"))
                        {
                            int iRowIndex = dgv.CurrentCell.RowIndex;

                            if (iRowIndex == 0)
                            {
                                Socket_Operation.ShowMessageBox(MultiLanguage.GetDefaultLanguage(MultiLanguage.MutiLan_73));
                                break;
                            }
                        }

                        dgv.CurrentCell.Style.BackColor = Color.DarkRed;
                        dgv.CurrentCell.Selected = false;

                        break;

                    case "cmsDGV_Progression_Disable":

                        dgv.CurrentCell.Style.BackColor = dgv.Rows[0].DefaultCellStyle.BackColor;
                        dgv.CurrentCell.Selected = false;

                        break;
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        private class TipForm : Form
        {
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                    cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                    return cp;
                }
            }
        }
    }
}

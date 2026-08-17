using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using WPELibrary;
using WPELibrary.Lib;

namespace WinsockPacketEditor
{
    public class MainForm : Form
    {
        private Socket_Form socketForm;
        private SocketProxy_Form socketProxyForm;

        private Panel pnlNav;
        private Button btnCollapse;
        private Button btnCapture;
        private Button btnProxy;
        private Panel pnlHost;
        private bool navCollapsed = false;

        private NotifyIcon niMain;
        private ContextMenuStrip cmsIcon;
        private ToolStripMenuItem cmsIcon_Show;
        private ToolStripMenuItem cmsIcon_Exit;

        private readonly Color NavBackColor = Color.FromArgb(45, 50, 58);
        private readonly Color NavActiveColor = Color.FromArgb(55, 100, 200);
        private readonly Color NavTextColor = Color.FromArgb(230, 232, 235);

        #region//窗体

        public MainForm()
        {
            try
            {
                Socket_Cache.System.LoadSystemConfig_FromDB();
                MultiLanguage.SetDefaultLanguage(Socket_Cache.System.DefaultLanguage);

                BuildUI();
                BuildTray();
                InitForms();
                SwitchView(proxy: true);
                SetNavCollapsed(true);
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion

        #region//构建界面

        private void BuildUI()
        {
            this.Text = Socket_Cache.System.WPE + " - " + Socket_Operation.AssemblyVersion;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1600, 900);
            this.MinimumSize = new Size(960, 640);
            this.KeyPreview = true;

            try
            {
                using (Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath))
                {
                    if (icon != null)
                    {
                        this.Icon = (Icon)icon.Clone();
                    }
                }
            }
            catch
            {
            }

            pnlHost = new Panel();
            pnlHost.Dock = DockStyle.Fill;
            pnlHost.BackColor = SystemColors.Control;

            pnlNav = new Panel();
            pnlNav.Dock = DockStyle.Left;
            pnlNav.Width = 160;
            pnlNav.BackColor = NavBackColor;
            pnlNav.Padding = new Padding(0, 12, 0, 0);

            btnCollapse = CreateNavButton("«");
            btnCollapse.Dock = DockStyle.Top;
            btnCollapse.Height = 32;
            btnCollapse.TextAlign = ContentAlignment.MiddleCenter;
            btnCollapse.Padding = new Padding(0, 0, 0, 0);
            btnCollapse.Click += (s, e) => ToggleNav();

            btnCapture = CreateNavButton("抓包");
            btnCapture.Dock = DockStyle.Top;
            btnCapture.Click += (s, e) => SwitchView(proxy: false);

            btnProxy = CreateNavButton("代理");
            btnProxy.Dock = DockStyle.Top;
            btnProxy.Click += (s, e) => SwitchView(proxy: true);

            pnlNav.Controls.Add(btnCollapse);
            pnlNav.Controls.Add(btnCapture);
            pnlNav.Controls.Add(btnProxy);

            this.Controls.Add(pnlHost);
            this.Controls.Add(pnlNav);
        }

        private Button CreateNavButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Height = 52;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 78, 88);
            btn.ForeColor = NavTextColor;
            btn.BackColor = NavBackColor;
            btn.Font = new Font("微软雅黑", 11F, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(22, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        #endregion

        #region//托盘

        private void BuildTray()
        {
            cmsIcon_Show = new ToolStripMenuItem();
            cmsIcon_Show.Name = "cmsIcon_Show";
            cmsIcon_Show.Text = "打开";

            ToolStripSeparator separator = new ToolStripSeparator();
            separator.Name = "tssIcon1";

            cmsIcon_Exit = new ToolStripMenuItem();
            cmsIcon_Exit.Name = "cmsIcon_Exit";
            cmsIcon_Exit.Text = "退出";

            cmsIcon = new ContextMenuStrip();
            cmsIcon.Name = "cmsIcon";
            cmsIcon.Items.AddRange(new ToolStripItem[] { cmsIcon_Show, separator, cmsIcon_Exit });
            cmsIcon.ItemClicked += new ToolStripItemClickedEventHandler(cmsIcon_ItemClicked);

            niMain = new NotifyIcon();
            niMain.ContextMenuStrip = cmsIcon;
            niMain.Icon = this.Icon;
            niMain.Text = Socket_Cache.System.WPE;
            niMain.Visible = true;
            niMain.MouseDoubleClick += new MouseEventHandler(niMain_MouseDoubleClick);
        }

        private void niMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowMainForm();
            }
        }

        private void cmsIcon_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            cmsIcon.Close();

            switch (e.ClickedItem.Name)
            {
                case "cmsIcon_Show":
                    ShowMainForm();
                    break;

                case "cmsIcon_Exit":
                    this.Close();
                    break;
            }
        }

        private void ShowMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        #endregion

        #region//嵌入子窗口

        private void InitForms()
        {
            socketForm = new Socket_Form();
            socketForm.OnFormClosingAction = null;
            socketForm.SetEmbeddedMode();
            EmbedForm(socketForm);

            socketProxyForm = new SocketProxy_Form(socketForm);
            socketProxyForm.SetEmbeddedMode();
            EmbedForm(socketProxyForm);
        }

        private void EmbedForm(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.ShowInTaskbar = false;
            pnlHost.Controls.Add(form);
            form.Show();
        }

        #endregion

        #region//切换视图

        private void SwitchView(bool proxy)
        {
            if (socketForm == null || socketProxyForm == null)
            {
                return;
            }

            if (proxy)
            {
                socketForm.Hide();
                socketProxyForm.Show();
                socketProxyForm.BringToFront();
            }
            else
            {
                socketProxyForm.Hide();
                socketForm.Show();
                socketForm.BringToFront();
            }

            btnProxy.BackColor = proxy ? NavActiveColor : NavBackColor;
            btnCapture.BackColor = proxy ? NavBackColor : NavActiveColor;
        }

        private void SetNavCollapsed(bool collapsed)
        {
            navCollapsed = collapsed;
            pnlNav.Width = navCollapsed ? 40 : 160;
            btnCollapse.Text = navCollapsed ? "»" : "«";

            btnCapture.Text = navCollapsed ? "抓" : "抓包";
            btnProxy.Text = navCollapsed ? "代" : "代理";

            ContentAlignment align = navCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
            Padding pad = navCollapsed ? new Padding(0, 0, 0, 0) : new Padding(22, 0, 0, 0);

            btnCapture.TextAlign = align;
            btnProxy.TextAlign = align;
            btnCapture.Padding = pad;
            btnProxy.Padding = pad;
        }

        private void ToggleNav()
        {
            SetNavCollapsed(!navCollapsed);
        }

        #endregion

        #region//窗体事件

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                if (socketForm != null && !socketForm.IsDisposed)
                {
                    socketForm.SaveConfigs();
                }

                if (socketProxyForm != null && !socketProxyForm.IsDisposed)
                {
                    socketProxyForm.Shutdown();
                }

                if (niMain != null)
                {
                    niMain.Visible = false;
                    niMain.Dispose();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            try
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                Socket_Operation.DoLog_Proxy(MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        #endregion
    }
}

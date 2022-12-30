namespace Pinokio.IKDT
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.LoadBtn = new DevExpress.XtraBars.BarButtonItem();
            this.RunBtn = new DevExpress.XtraBars.BarButtonItem();
            this.PathStringEdit = new DevExpress.XtraBars.BarEditItem();
            this.ServerOnOffBtn = new DevExpress.XtraBars.BarToggleSwitchItem();
            this.SimTimeEditItem = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.SimSpeedEditItem = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEdit3 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.IKPageGroup = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.ribbonPageGroup3 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.barEditItem1 = new DevExpress.XtraBars.BarEditItem();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControl1
            // 
            this.ribbonControl1.EmptyAreaImageOptions.ImagePadding = new System.Windows.Forms.Padding(35, 32, 35, 32);
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl1.ExpandCollapseItem,
            this.ribbonControl1.SearchEditItem,
            this.LoadBtn,
            this.RunBtn,
            this.PathStringEdit,
            this.ServerOnOffBtn,
            this.SimTimeEditItem,
            this.SimSpeedEditItem});
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 16;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.OptionsMenuMinWidth = 385;
            this.ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPage1});
            this.ribbonControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemTextEdit1,
            this.repositoryItemTextEdit2,
            this.repositoryItemTextEdit3});
            this.ribbonControl1.Size = new System.Drawing.Size(1002, 160);
            // 
            // LoadBtn
            // 
            this.LoadBtn.Caption = "Load";
            this.LoadBtn.Id = 1;
            this.LoadBtn.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("LoadBtn.ImageOptions.Image")));
            this.LoadBtn.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("LoadBtn.ImageOptions.LargeImage")));
            this.LoadBtn.Name = "LoadBtn";
            // 
            // RunBtn
            // 
            this.RunBtn.Caption = "Run";
            this.RunBtn.Id = 2;
            this.RunBtn.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RunBtn.ImageOptions.Image")));
            this.RunBtn.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("RunBtn.ImageOptions.LargeImage")));
            this.RunBtn.Name = "RunBtn";
            // 
            // PathStringEdit
            // 
            this.PathStringEdit.Edit = null;
            this.PathStringEdit.Id = 9;
            this.PathStringEdit.Name = "PathStringEdit";
            // 
            // ServerOnOffBtn
            // 
            this.ServerOnOffBtn.Caption = "Server On / Off";
            this.ServerOnOffBtn.Id = 12;
            this.ServerOnOffBtn.Name = "ServerOnOffBtn";
            // 
            // SimTimeEditItem
            // 
            this.SimTimeEditItem.Edit = this.repositoryItemTextEdit2;
            this.SimTimeEditItem.EditWidth = 100;
            this.SimTimeEditItem.Id = 14;
            this.SimTimeEditItem.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SimTimeEditItem.ImageOptions.Image")));
            this.SimTimeEditItem.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("SimTimeEditItem.ImageOptions.LargeImage")));
            this.SimTimeEditItem.Name = "SimTimeEditItem";
            // 
            // repositoryItemTextEdit2
            // 
            this.repositoryItemTextEdit2.AutoHeight = false;
            this.repositoryItemTextEdit2.Name = "repositoryItemTextEdit2";
            // 
            // SimSpeedEditItem
            // 
            this.SimSpeedEditItem.Caption = "배속 :     ";
            this.SimSpeedEditItem.Edit = this.repositoryItemTextEdit3;
            this.SimSpeedEditItem.Id = 15;
            this.SimSpeedEditItem.Name = "SimSpeedEditItem";
            // 
            // repositoryItemTextEdit3
            // 
            this.repositoryItemTextEdit3.AutoHeight = false;
            this.repositoryItemTextEdit3.Name = "repositoryItemTextEdit3";
            // 
            // ribbonPage1
            // 
            this.ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.IKPageGroup,
            this.ribbonPageGroup2,
            this.ribbonPageGroup1});
            this.ribbonPage1.Name = "ribbonPage1";
            this.ribbonPage1.Text = "Simulation";
            // 
            // IKPageGroup
            // 
            this.IKPageGroup.ItemLinks.Add(this.LoadBtn);
            this.IKPageGroup.ItemLinks.Add(this.RunBtn);
            this.IKPageGroup.Name = "IKPageGroup";
            this.IKPageGroup.Text = "Model";
            // 
            // ribbonPageGroup2
            // 
            this.ribbonPageGroup2.ItemLinks.Add(this.SimTimeEditItem);
            this.ribbonPageGroup2.ItemLinks.Add(this.SimSpeedEditItem);
            this.ribbonPageGroup2.Name = "ribbonPageGroup2";
            this.ribbonPageGroup2.Text = "Simulation";
            // 
            // ribbonPageGroup1
            // 
            this.ribbonPageGroup1.ItemLinks.Add(this.ServerOnOffBtn);
            this.ribbonPageGroup1.Name = "ribbonPageGroup1";
            this.ribbonPageGroup1.Text = "Sever";
            // 
            // repositoryItemTextEdit1
            // 
            this.repositoryItemTextEdit1.AutoHeight = false;
            this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
            // 
            // ribbonPageGroup3
            // 
            this.ribbonPageGroup3.Name = "ribbonPageGroup3";
            this.ribbonPageGroup3.Text = "Simulation";
            // 
            // barEditItem1
            // 
            this.barEditItem1.Edit = null;
            this.barEditItem1.EditWidth = 100;
            this.barEditItem1.Id = 7;
            this.barEditItem1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("barEditItem1.ImageOptions.Image")));
            this.barEditItem1.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barEditItem1.ImageOptions.LargeImage")));
            this.barEditItem1.Name = "barEditItem1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 599);
            this.Controls.Add(this.ribbonControl1);
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.Ribbon = this.ribbonControl1;
            this.Text = "IK TECH DT";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEdit1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.BarButtonItem LoadBtn;
        private DevExpress.XtraBars.BarButtonItem RunBtn;
        private DevExpress.XtraBars.BarEditItem PathStringEdit;
        private DevExpress.XtraBars.BarToggleSwitchItem ServerOnOffBtn;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup IKPageGroup;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup2;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.BarEditItem SimTimeEditItem2;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup3;
        private DevExpress.XtraBars.BarEditItem barEditItem1;
        private DevExpress.XtraBars.BarEditItem SimTimeEditItem;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit2;
        private DevExpress.XtraBars.BarEditItem SimSpeedEditItem;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEdit3;
    }
}


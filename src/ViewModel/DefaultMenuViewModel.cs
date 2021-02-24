using Atomus.Control;
using Atomus.Diagnostics;
using Atomus.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Atomus.Windows.Controls.Menu.Controllers;
using Atomus.Windows.Controls.Menu.Models;
using System.Threading.Tasks;

namespace Atomus.Windows.Controls.Menu.ViewModel
{
    public class DefaultMenuViewModel : Atomus.MVVM.ViewModel
    {
        #region Declare
        private ImageBrush refreshBackgroundImage;
        private ImageBrush expendAllBackgroundImage;
        private ImageBrush collapseAllBackgroundImage;

        private bool isEnabledControl;

        private ObservableCollection<TreeViewItem> menuData;

        private decimal menuID;
        private decimal parentMenuID;

        #endregion

        #region Property
        public ICore Core { get; set; }

        public ImageBrush RefreshBackgroundImage
        {
            get  {  return this.refreshBackgroundImage;   }
            set
            {
                if (this.refreshBackgroundImage != value)
                {
                    this.refreshBackgroundImage = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public ImageBrush ExpendAllBackgroundImage
        {
            get   {   return this.expendAllBackgroundImage;    }
            set
            {
                if (this.expendAllBackgroundImage != value)
                {
                    this.expendAllBackgroundImage = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public ImageBrush CollapseAllBackgroundImage
        {
            get { return this.collapseAllBackgroundImage; }
            set
            {
                if (this.collapseAllBackgroundImage != value)
                {
                    this.collapseAllBackgroundImage = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<System.Windows.Controls.TreeViewItem> MenuData
        {
            get { return this.menuData; }
            set
            {
                if (this.menuData != value)
                {
                    this.menuData = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public decimal MenuID
        {
            get
            {
                return this.menuID;
            }
            set
            {
                if (this.menuID != value)
                {
                    this.menuID = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public decimal ParentMenuID
        {
            get
            {
                return this.parentMenuID;
            }
            set
            {
                if (this.parentMenuID != value)
                {
                    this.parentMenuID = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public class MenuItem
        {
            public decimal MENU_ID;
            public decimal PARENT_MENU_ID;
            public string NAME;
            public string DESCRIPTION;
            //public string IMAGE_URL1;
            //public string IMAGE_URL2;
            //public string IMAGE_URL3;
            //public string IMAGE_URL4;
            public ImageSource ImageSource1;
            public ImageSource ImageSource2;
            public ImageSource ImageSource3;
            public ImageSource ImageSource4;
            public decimal ASSEMBLY_ID;
            public string NAMESPACE;
            public bool VISIBLE_ONE;
        }

        public bool IsEnabledControl
        {
            get
            {
                return this.isEnabledControl;
            }
            set
            {
                if (this.isEnabledControl != value)
                {
                    this.isEnabledControl = value;
                    this.NotifyPropertyChanged();
                }
            }
        }


        public ICommand SearchCommand { get; set; }
        public ICommand ExpandAllCommand { get; set; }
        public ICommand CollapseAllCommand { get; set; }
        #endregion

        #region INIT
        public DefaultMenuViewModel()
        {
            this.menuID = -1;
            this.ParentMenuID = -1;

            this.SearchCommand = new MVVM.DelegateCommand(() => { this.SearchProcess(this.menuID, this.ParentMenuID); }
                                                            , () => { return this.isEnabledControl; });
            this.ExpandAllCommand = new MVVM.DelegateCommand(() => { this.ExpandAllProcess(); }
                                                            , () => { return this.isEnabledControl; });
            this.CollapseAllCommand = new MVVM.DelegateCommand(() => { this.CollapseAllProcess(); }
                                                            , () => { return this.isEnabledControl; });
        }
        public DefaultMenuViewModel(ICore core) : this()
        {
            this.Core = core;

            try
            {
                this.isEnabledControl = true;

                this.GetBackgroundImage();
            }
            catch (Exception ex)
            {
                DiagnosticsTool.MyTrace(ex);
            }

        }
        #endregion

        #region IO
        private async void GetBackgroundImage()
        {
            try
            {
                this.refreshBackgroundImage = new ImageBrush(await this.Core.GetAttributeMediaWebImage("RefreshImage"));
                if (this.refreshBackgroundImage != null)
                    this.NotifyPropertyChanged("RefreshBackgroundImage");

                this.expendAllBackgroundImage = new ImageBrush(await this.Core.GetAttributeMediaWebImage("ExpendAllImage"));
                if (this.expendAllBackgroundImage != null)
                    this.NotifyPropertyChanged("ExpendAllBackgroundImage");

                this.collapseAllBackgroundImage = new ImageBrush(await this.Core.GetAttributeMediaWebImage("CollapseAllImage"));
                if (this.collapseAllBackgroundImage != null)
                {
                    this.NotifyPropertyChanged("CollapseAllBackgroundImage");

                    this.SearchCommand.Execute(null);
                }
            }
            catch (Exception exception)
            {
                DiagnosticsTool.MyTrace(exception);
            }
        }


        private async void SearchProcess(decimal MENU_ID, decimal PARENT_MENU_ID)
        {
            Service.IResponse result;

            try
            {
                this.IsEnabledControl = false;
                (this.SearchCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();

                result = await this.Core.SearchAsync(new DefaultMenuSearchModel()
                {
                    START_MENU_ID = MENU_ID,
                    ONLY_PARENT_MENU_ID = PARENT_MENU_ID
                });

                if (result.Status == Service.Status.OK)
                {
                    this.MenuData = null;
                    if (result.DataSet != null && result.DataSet.Tables.Count > 0)
                        this.MenuData = await this.SetTree(result.DataSet.Tables[1]);
                }
                else
                    this.WindowsMessageBoxShow(Application.Current.Windows[0], result.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                DiagnosticsTool.MyTrace(ex);
                //this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
            }
            finally
            {
                this.IsEnabledControl = true;
                (this.SearchCommand as Atomus.MVVM.DelegateCommand).RaiseCanExecuteChanged();
            }
        }

        public DataTable GetSearchTopMenu()
        {
            Service.IResponse result;

            try
            {
                result = this.Core.Search(new DefaultMenuSearchModel()
                {
                    START_MENU_ID = 0,
                    ONLY_PARENT_MENU_ID = 0
                });

                if (result.Status == Service.Status.OK && result.DataSet != null && result.DataSet.Tables.Count > 0)
                    return result.DataSet.Tables[1];
                else
                    return null;
            }
            catch (Exception ex)
            {
                DiagnosticsTool.MyTrace(ex);
                //this.WindowsMessageBoxShow(Application.Current.Windows[0], ex);
                return null;
            }
            finally
            {
            }
        }
        private async Task<ObservableCollection<TreeViewItem>> SetTree(DataTable dataTable)
        {
            ObservableCollection<TreeViewItem> trees;
            TreeViewItem treeViewItem;
            MenuItem menuItem;
            StackPanel stackPanel;
            Image image;
            ImageSource imageSourceFolder;
            ImageSource imageSourceAssemblies;
            Label label;
            bool showNamespace;
            bool showAssemblyID;
            bool isAdd;
            string rootImagesUrlPath;

            trees = new ObservableCollection<TreeViewItem>();

            imageSourceFolder = new ImageBrush(await this.Core.GetAttributeMediaWebImage("FolderImage")).ImageSource;
            imageSourceAssemblies = new ImageBrush(await this.Core.GetAttributeMediaWebImage("AssembliesImage")).ImageSource;

            showNamespace = this.Core.GetAttribute("ShowNamespace.RESPONSIBILITY_ID").Split(',').Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString());
            showAssemblyID = this.Core.GetAttribute("ShowAssemblyID.RESPONSIBILITY_ID").Split(',').Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString());

            rootImagesUrlPath = Factory.FactoryConfig.GetAttribute("Atomus", "RootImagesUrlPath");

            foreach (DataRow dataRow in dataTable.Rows)
            {
                menuItem = new MenuItem()
                {
                    MENU_ID = (decimal)dataRow["MENU_ID"],
                    PARENT_MENU_ID = (decimal)dataRow["PARENT_MENU_ID"],
                    NAME = (string)dataRow["NAME"],
                    DESCRIPTION = (string)dataRow["DESCRIPTION"],

                    ImageSource1 = dataRow["IMAGE_URL1"] != DBNull.Value && (string)dataRow["IMAGE_URL1"] != "" ?
                        (((string)dataRow["IMAGE_URL1"]).Contains("http") ?
                            new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri((string)dataRow["IMAGE_URL1"]))).ImageSource
                            : new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri(string.Format("{0}{1}", (string)dataRow["IMAGE_URL1"], rootImagesUrlPath)))).ImageSource)
                        : null,

                    ImageSource2 = dataRow["IMAGE_URL2"] != DBNull.Value && (string)dataRow["IMAGE_URL2"] != "" ?
                        (((string)dataRow["IMAGE_URL2"]).Contains("http") ?
                            new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri((string)dataRow["IMAGE_URL2"]))).ImageSource
                            : new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri(string.Format("{0}{1}", (string)dataRow["IMAGE_URL2"], rootImagesUrlPath)))).ImageSource)
                        : null,

                    ImageSource3 = dataRow["IMAGE_URL3"] != DBNull.Value && (string)dataRow["IMAGE_URL3"] != "" ?
                        (((string)dataRow["IMAGE_URL3"]).Contains("http") ?
                            new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri((string)dataRow["IMAGE_URL3"]))).ImageSource
                            : new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri(string.Format("{0}{1}", (string)dataRow["IMAGE_URL3"], rootImagesUrlPath)))).ImageSource)
                        : null,

                    ImageSource4 = dataRow["IMAGE_URL4"] != DBNull.Value && (string)dataRow["IMAGE_URL4"] != "" ?
                        (((string)dataRow["IMAGE_URL4"]).Contains("http") ?
                            new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri((string)dataRow["IMAGE_URL4"]))).ImageSource
                            : new ImageBrush(await this.Core.GetAttributeMediaWebImage(new Uri(string.Format("{0}{1}", (string)dataRow["IMAGE_URL4"], rootImagesUrlPath)))).ImageSource)
                        : null,

                    ASSEMBLY_ID = dataRow["ASSEMBLY_ID"] == DBNull.Value ? -1 : (decimal)dataRow["ASSEMBLY_ID"],
                    NAMESPACE = dataRow["NAMESPACE"].ToString(),
                    VISIBLE_ONE = dataRow["VISIBLE_ONE"] != DBNull.Value && (string)dataRow["VISIBLE_ONE"] != "N"
                };

                stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };

                if (menuItem.ImageSource1 != null)
                    image = new Image() { Source = menuItem.ImageSource1 };
                else
                {
                    if (menuItem.ASSEMBLY_ID > 0)
                        image = new Image() { Source = imageSourceAssemblies };
                    else
                        image = new Image() { Source = imageSourceFolder };
                }
                stackPanel.Children.Add(image);

                label = new Label() { Content = showAssemblyID ? string.Format("{0} ({1}.{2})", menuItem.NAME.Translate(), menuItem.MENU_ID, menuItem.ASSEMBLY_ID) : menuItem.NAME.Translate() };
                label.ToolTip = showNamespace ? string.Format("{0} {1}", menuItem.DESCRIPTION.Translate(), menuItem.NAMESPACE) : menuItem.DESCRIPTION.Translate();
                label.VerticalAlignment = VerticalAlignment.Center;
                label.Tag = menuItem;
                stackPanel.Children.Add(label);


                treeViewItem = new TreeViewItem() { Header = stackPanel, Tag = menuItem };

                if (menuItem.ASSEMBLY_ID > 0)
                    label.MouseDoubleClick += Label_MouseDoubleClick;

                if (trees.Count > 0)
                {
                    isAdd = false;

                    foreach (TreeViewItem treeViewItemTmp in trees)
                    {
                        if ((treeViewItemTmp.Tag as MenuItem).MENU_ID == menuItem.PARENT_MENU_ID)
                        {
                            treeViewItemTmp.Items.Add(treeViewItem);
                            isAdd = true;
                        }
                        else
                        {
                            var a = treeViewItemTmp.Items.OfType<TreeViewItem>().ToList().Where(x => (x.Tag as MenuItem).MENU_ID == menuItem.PARENT_MENU_ID);

                            if (a.Count() == 1)
                            {
                                a.First().Items.Add(treeViewItem);
                                isAdd = true;
                            }
                        }
                    }

                    if (!isAdd)
                        trees.Add(treeViewItem);
                }
                else
                    trees.Add(treeViewItem);
            }

            return trees;
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label label;
            MenuItem menuItem;

            label = (sender as Label);

            if (label.Tag != null && label.Tag is MenuItem)
            {
                menuItem = (label.Tag as MenuItem);
                (this.Core as IAction).ControlAction(this, "Menu.OpenControl", new object[] { menuItem.MENU_ID, menuItem.ASSEMBLY_ID, menuItem.VISIBLE_ONE });
            }
        }

        private void ExpandAllProcess()
        {
            (this.Core as IAction).ControlAction(this, "ExpandAllNodes", true);
        }
        private void CollapseAllProcess()
        {
            (this.Core as IAction).ControlAction(this, "CollapseAllNodes", false);
        }
        #endregion

        #region ETC
        #endregion
    }
}
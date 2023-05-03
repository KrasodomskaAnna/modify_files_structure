using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using Path = System.IO.Path;

namespace lab_2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        public MainWindow()
        {
            InitializeComponent();
        }

        private static class Extensions
        {
            public static string getRAHS(FileAttributes attributes)
            {
                string rahs = "";
                rahs += attributes.HasFlag(FileAttributes.ReadOnly) ? "r" : "-";
                rahs += attributes.HasFlag(FileAttributes.Archive) ? "a" : "-";
                rahs += attributes.HasFlag(FileAttributes.Hidden) ? "h" : "-";
                rahs += attributes.HasFlag(FileAttributes.System) ? "s" : "-";
                return rahs;
            }
			
            public static FileAttributes remoteAttribute(FileAttributes attributes, FileAttributes attToRemove)
            {
                return attributes & ~attToRemove;
            }
			
			public static void deleteAttribute(TreeViewItem selected)
			{
				FileAttributes attr = File.GetAttributes(selected.Tag.ToString());
				if((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					attr = Extensions.remoteAttribute(attr, FileAttributes.ReadOnly);
					File.SetAttributes(selected.Tag.ToString(), attr);
				}
			}
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                Description = "Wybierz folder aby otworzyć",
                ShowNewFolderButton = true,
            };
            dlg.ShowDialog();

            string path = dlg.SelectedPath;
            if(path.Length <= 0)
                return;

            treeview.Items.Clear();
            
            this.rootPath = path;
            // tworzenie  drzewa
            treeview.Items.Add(createTree(new DirectoryInfo(path)));
        }

        private void OnExitClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private TreeViewItem createTree(DirectoryInfo dirInfo)
        {
            FileInfo[] files = dirInfo.GetFiles();
            DirectoryInfo[] directoryInfos = dirInfo.GetDirectories();

            var dirMen = new System.Windows.Controls.ContextMenu();
            var createD = new System.Windows.Controls.MenuItem { Header = "Create" };
            var deleteD = new System.Windows.Controls.MenuItem { Header = "Delete" };
            createD.Click += createDirectory;
            deleteD.Click += deleteDirectory;
            dirMen.Items.Add(createD);
            dirMen.Items.Add(deleteD);
            var treeViewItem = new TreeViewItem
            {
                Header = dirInfo.Name,
                Tag = dirInfo.FullName,
                ContextMenu = dirMen,
            };

            var fileMen = new System.Windows.Controls.ContextMenu();
            var createF = new System.Windows.Controls.MenuItem { Header = "Open" };
            var deleteF = new System.Windows.Controls.MenuItem { Header = "Delete" };
            createF.Click += openFile;
            deleteF.Click += deleteFile;
            fileMen.Items.Add(createF);
            fileMen.Items.Add(deleteF);
            foreach (FileInfo file in files)
            {
                var item = new TreeViewItem
                {
                    Header = file.Name,
                    Tag = file.FullName,
                    ContextMenu = fileMen,
                };

                treeViewItem.Items.Add(item);
            }

            foreach (DirectoryInfo directoryInfo in directoryInfos)
            {
                treeViewItem.Items.Add(createTree(directoryInfo));
            }

            return treeViewItem;
        }

        private void openFile(object sender, RoutedEventArgs e)
        {
            if (treeview.SelectedItem == null) return;

            var selected = treeview.SelectedItem as TreeViewItem;
            string fileContent = File.ReadAllText(selected.Tag.ToString());
            fileContentBox.Text = (fileContent.Length > 0) ? fileContent : $"Uwaga! {selected.Header} jest pusty!";
        }

        private void deleteFile(object sender, RoutedEventArgs e)
        {
            if (treeview.SelectedItem == null) return;

            var selected = treeview.SelectedItem as TreeViewItem;
            Extensions.deleteAttribute(selected);

            // usuwanie z kompa
            File.Delete(selected.Tag.ToString());

            // usuwanie z widoku
            TreeViewItem parent = selected.Parent as TreeViewItem;
            if (parent != null)
                parent.Items.Remove(selected);
            else
                treeview.Items.Clear();
        }

        private void deleteDirectory(object sender, RoutedEventArgs e)
        {
            var selected = treeview.SelectedItem as TreeViewItem;
            if (selected == null) return;
            Extensions.deleteAttribute(selected);

            // usuwanie z kompa
            Directory.Delete(selected.Tag.ToString(), true);

            // usuwanie z widoku
            TreeViewItem parent = selected.Parent as TreeViewItem;
            if (parent != null)
                parent.Items.Remove(selected);
            else
                treeview.Items.Clear();
        }

        private void createDirectory(object sender, RoutedEventArgs e) {
            if (treeview.SelectedItem == null) return;

            var selected = treeview.SelectedItem as TreeViewItem;
			var dir = new DirectoryInfo(selected.Tag.ToString());
            if (dir == null) return;

            CreatorDialog dlg = new CreatorDialog();
            var result = dlg.ShowDialog();
			
			if (result == false) return;
			
			var path = Path.Combine(dir.FullName, dlg.name.Text);
			if (File.Exists(path)) return;
            if (dlg.file_t.IsChecked == true)
            {
                System.IO.File.Create(path);
            }
            else
            {
                System.IO.Directory.CreateDirectory(path);
            }
			
			FileAttributes attr = 0;
            if(dlg.readOnly.IsChecked == true) attr |= FileAttributes.ReadOnly;
            if(dlg.archive.IsChecked == true) attr |= FileAttributes.Archive;
            if(dlg.hidden.IsChecked == true) attr |= FileAttributes.Hidden;
            if(dlg.system.IsChecked == true) attr |= FileAttributes.System;
			File.SetAttributes(path, attr);

            treeview.Items.Clear();
            treeview.Items.Add(createTree(new DirectoryInfo(this.rootPath)));
        }

        private void ShowRAHSInfo(object sender, MouseButtonEventArgs e)
        {
            if (treeview.SelectedItem == null)
            {
                rahsInfo.Text = "";
                return;
            }
            TreeViewItem selectedItem = (TreeViewItem)treeview.SelectedItem;
            rahsInfo.Text = Extensions.getRAHS(File.GetAttributes(selectedItem.Tag.ToString()));
        }

    }
}

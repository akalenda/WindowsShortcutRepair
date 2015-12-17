using Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WindowsShortcutRepair
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFD = new OpenFileDialog();
        private FolderBrowserDialog folderBD = new FolderBrowserDialog();
        private BindingList<ShortcutTransform> shortcutsLoaded;
        private HashSet<String> shortcutMembership;

        public MainWindow()
        {
            InitializeComponent();
            resetList();
            shortcutDataGrid.DataContext = shortcutsLoaded;
            openFD.Multiselect = true;
            openFD.Filter = "Shortcuts (*.lnk)|*.LNK" 
                + "|All files (*.*)|*.*";
            openFD.Title = "Select shortcuts";
        }

        /* **************************** GUI event functions *************************************/
        private void loadShortcutsBttn_Click(object sender, RoutedEventArgs e)
        {
            if (openFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                foreach (String filename in openFD.FileNames)
                    addToSetUsing(filename);
            doPostupdateChanges();
        }

        private void loadDirectoryBttn_Click(object sender, RoutedEventArgs e)
        {
            if (folderBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stack<String> pendingDirs = new Stack<String>();
                pendingDirs.Push(folderBD.SelectedPath);
                while (pendingDirs.Count > 0)
                {
                    String currentDir = pendingDirs.Pop();
                    foreach (String dirname in Directory.EnumerateDirectories(currentDir))
                        pendingDirs.Push(dirname);
                    foreach (String filename in Directory.EnumerateFiles(currentDir))
                        addToSetUsing(filename);
                }
            }
            doPostupdateChanges();
        }

        private void clearListBttn_Click(object sender, RoutedEventArgs e)
        {
            resetList();
        }

        private void regexTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshList();
        }

        private void regexButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ShortcutTransform shortcut in shortcutsLoaded)
                shortcut.applyAndSave();
            refreshList();
        }

        /* *************************** Helpers ************************************************/
        private void resetList()
        {
            shortcutsLoaded = null;
            shortcutsLoaded = new BindingList<ShortcutTransform>();
            shortcutMembership = new HashSet<String>();
            doPostupdateChanges();
        }

        private void refreshList()
        {
            shortcutDataGrid.DataContext = null;
            shortcutDataGrid.DataContext = shortcutsLoaded;
            doPostupdateChanges();
        }

        private void doPostupdateChanges()
        {
            numSelectedLabel.Content = shortcutsLoaded.Count + " items selected";
        }

        private void addToSetUsing(String filename)
        {
            if (shortcutMembership.Contains(filename))
                return;
            try {
                shortcutsLoaded.Add(createLnkFrom(filename));
                shortcutMembership.Add(filename);
            } catch(NotImplementedException swallowed) { /*occurs when something that's not a link is selected*/}
        }

        /* *********************************** .lnk file logic ***********************************/
        public ShortcutTransform createLnkFrom(String filename)
        {
            Shell shell = new Shell32.Shell();
            String shortcutFullPath = System.IO.Path.GetFullPath(filename);
            Folder folder = shell.NameSpace(System.IO.Path.GetDirectoryName(shortcutFullPath));
            FolderItem folderItem = folder.Items().Item(System.IO.Path.GetFileName(shortcutFullPath));
            ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink; /*throws NotImplementedException when item is not a link*/
            return new ShortcutTransform(link, this);
        }

        public class ShortcutTransform
        {
            private ShellLinkObject _link;
            private MainWindow _parent;

            public ShortcutTransform(ShellLinkObject toBeTransformed, MainWindow windowRef)
            {
                _link = toBeTransformed;
                _parent = windowRef;
            }
            public String OldWorkingDirectory
            {
                get { return _link.WorkingDirectory; }
            }
            public String NewWorkingDirectory
            {
                get { return regexHelper(OldWorkingDirectory); }
            }
            public String OldPath
            {
                get { return _link.Path; }
            }
            public String NewPath
            {
                get { return regexHelper(OldPath); }
            }
            public void applyAndSave()
            {
                _link.Path = NewPath;
                _link.WorkingDirectory = NewWorkingDirectory;
                _link.Save();
            }
            private String regexHelper(String toModify)
            {
                try
                {
                    return Regex.Replace(toModify, _parent.regexSearchTextbox.Text, _parent.regexReplaceTextbox.Text);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
        }
    }
}

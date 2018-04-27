using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Explorer_WinForms_L4_v2
{
    public partial class Form1 : Form
    {
        TreeNode root;
        string listViewPathAtTheMoment;
        List<List<TreeNode>> allPath;
        int amountOfStepsBack = 0;

        public Form1()
        {
            InitializeComponent();
            root = new TreeNode("This PC");
            allPath = new List<List<TreeNode>>();
            button1.Enabled = false;
            button2.Enabled = false;
            textBox2.Visible = false;
            // Add images
            imageList1.Images.Add(Image.FromFile(@"D:\\Visual Studio\\Visual Studio C# Projects\\Explorer_WinForms_L4_v2\\Explorer_WinForms_L4_v2\\folderIcon.png"));
            imageList1.Images.Add(Image.FromFile(@"D:\\Visual Studio\\Visual Studio C# Projects\\Explorer_WinForms_L4_v2\\Explorer_WinForms_L4_v2\\openFolderIcon.png"));
            imageList1.Images.Add(Image.FromFile(@"D:\\Visual Studio\\Visual Studio C# Projects\\Explorer_WinForms_L4_v2\\Explorer_WinForms_L4_v2\\fileIcon.png"));
            treeView1.ImageList = imageList1;
            treeView1.ImageIndex = 0;
            // Set top dataGridView
            for (int i = 0; i < 5;i++)
            {
                dataGridView1.Columns.Add("", "");
                dataGridView1.Columns[i].Width = 92;
            }
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.ReadOnly = true;
            // Add image of folder
            DataGridViewImageCell DGic = new DataGridViewImageCell();
            DGic.Value = Image.FromFile(@"D:\\Visual Studio\\Visual Studio C# Projects\\Explorer_WinForms_L4_v2\\Explorer_WinForms_L4_v2\\folderIcon.png");
            dataGridView1[0, 0] = DGic;
            dataGridView1.Columns[0].Width = 24;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Creating root
            treeView1.Nodes.Add(root);
            var disks =  DriveInfo.GetDrives();

            foreach(DriveInfo di in disks)
            {
                if(di.IsReady)
                    root.Nodes.Add(di.Name);
            }
            root.ImageIndex = 1;
            treeView1.Nodes[0].Expand();

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            button1.Enabled = true;
            // Clear listView
            listView1.Items.Clear();
            string path = null;
            List<TreeNode> currentPath = new List<TreeNode>();
            currentPath.Clear();
            for (int i = 1; i < 5; i++)
            {
                dataGridView1[i, 0].Value = "";
            }
            // Check '\\'
            if (CheckSlash(e.Node.Text))
            {
                path = e.Node.Text;
                dataGridView1[1, 0].Value = e.Node.Text;
                GetPathToRoot(e.Node, currentPath);
            }
            else
            {
                // Get root and create full path
                GetPathToRoot(e.Node, currentPath);
                path = GetCurrentPath(currentPath);
            }
            while(amountOfStepsBack != 0)
            {
                allPath.RemoveAt(allPath.Count - 1);
                amountOfStepsBack--;
            }
            button2.Enabled = false;
            allPath.Add(currentPath);
            // Set top Path
            listViewPathAtTheMoment = path;

            SetListViewItems(path);

            if (e.Node.Nodes.Count == 0)
            {
                e.Node.ImageIndex = 1;
            }

            dataGridView1.Visible = true;
            textBox2.Visible = false;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(listView1.SelectedItems == null || listView1.SelectedItems.Count == 0)
            {
                return;
            }
            if (listView1.SelectedItems[0].SubItems[2].Text != "Folder")
            {
                return;
            }
            while (amountOfStepsBack != 0)
            {
                allPath.RemoveAt(allPath.Count - 1);
                amountOfStepsBack--;
            }
            button2.Enabled = false;
            List<TreeNode> currentPath = new List<TreeNode>(allPath[allPath.Count - 1]);
            button1.Enabled = true;
            currentPath.Insert(0, new TreeNode(listView1.SelectedItems[0].Text));
            allPath.Add(currentPath);
            string path = GetCurrentPath(currentPath);
            listView1.Items.Clear();
            SetListViewItems(path);
            dataGridView1.Visible = true;
            textBox2.Visible = false;

        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            int amount_of_nodes = e.Node.Nodes.Count;
            if (amount_of_nodes == 0)
                return;

            string path = null, folderName = null;
            IEnumerable<string> folders;
            List<TreeNode> tmp = new List<TreeNode>();
            int j = 0;
            for (int i = 0; i < amount_of_nodes; i++)
            {
                path = "";
                tmp.Clear();
                if (CheckSlash(e.Node.Nodes[i].Text))
                {
                    path = e.Node.Nodes[i].Text;
                }
                else
                {
                    GetPathToRoot(e.Node.Nodes[i], tmp);
                    for (int k = tmp.Count - 1; k >= 0; k--)
                    {
                        path += tmp[k].Text;
                        if (tmp.Count == 2 || k == 0 || k == tmp.Count - 1)
                        {
                            continue;
                        }
                        path += "\\";
                    }
                }

                try
                {
                    folders = Directory.GetDirectories(path)
                    .Where(d => !new DirectoryInfo(d).Attributes.HasFlag(FileAttributes.System | FileAttributes.Hidden));
                }
                catch
                {
                    continue;
                }
                foreach (string di in folders)
                {
                    folderName = GetNameFromPath(di);
                    e.Node.Nodes[i].Nodes.Add(folderName);
                }
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 1;
        }

        public static string ReverseXor(string s)
        {
            char[] charArray = s.ToCharArray();
            int len = s.Length - 1;

            for (int i = 0; i < len; i++, len--)
            {
                charArray[i] ^= charArray[len];
                charArray[len] ^= charArray[i];
                charArray[i] ^= charArray[len];
            }

            return new string(charArray);
        }

        public static bool CheckSlash(string s)
        {
            foreach(char c in s)
            {
                if (c == '\\')
                    return true;
            }
            return false;
        }

        private void GetPathToRoot(TreeNode node, List<TreeNode> path)
        {
            if (node.Text == "This PC") return; // previous node was the root.
            else
            {
                path.Add(node);
                GetPathToRoot(node.Parent, path);
            }
        }

        private string GetNameFromPath(string s)
        {
            string reversedPath, name = null;
            int j;
            reversedPath = ReverseXor(s);
            if (reversedPath[0] == '\\')
            {

            }
            else
            {
                j = 0;
                while (reversedPath[j] != '\\')
                {
                    name += reversedPath[j];
                    j++;
                }
            }
            return ReverseXor(name);
        }

        private bool CheckDot(string s)
        {
            foreach(char c in s)
            {
                if (c == '.')
                    return true;
            }
            return false;
        }

        private static long GetDirectorySize(string p)
        {
            // 1
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*");

            // 2
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4
            // Return total size
            return b;
        }

        private void SetListViewItems(string path)
        {
            if (path == null)
                return;
            ListViewItem item;
            string folderName;
            // Get string [] with directories
            var dir = Directory.GetDirectories(path)
                .Where(d => !new DirectoryInfo(d).Attributes.HasFlag(FileAttributes.System | FileAttributes.Hidden));
            // Add it to the list
            foreach (string di in dir)
            {
                folderName = GetNameFromPath(di);
                item = new ListViewItem(folderName);
                item.SubItems.Add(File.GetLastWriteTime(di).ToString());
                item.SubItems.Add("Folder");
                item.ImageIndex = 0;
                item.SubItems.Add("");
                //item.SubItems.Add((GetDirectorySize(di) / 1024).ToString() + "KB");
                listView1.Items.Add(item);

            }
            // Same shit with files
            var files = Directory.GetFiles(path);
            foreach (string di in files)
            {
                folderName = GetNameFromPath(di);
                item = new ListViewItem(folderName);
                item.SubItems.Add(File.GetLastWriteTime(di).ToString());
                item.SubItems.Add(Path.GetExtension(di));
                item.ImageIndex = 2;
                FileInfo fileInfo = new FileInfo(di);
                double size = (Math.Round((double)fileInfo.Length / 1024));
                if (size == 0)
                    size = 1;
                item.SubItems.Add(size.ToString() + " KB");
                listView1.Items.Add(item);
            }
        }

        private string GetCurrentPath(List<TreeNode> treePath)
        {
            string path = null;
            int j = 1;
            bool flag = false;
            if (treePath.Count > 4)
            {
                for (int i = treePath.Count - 1; i >= 0; i--)
                {
                    if (i == 3)
                        flag = true;
                    if (flag)
                    {
                        dataGridView1[j, 0].Value = treePath[i].Text;
                        j++;
                    }
                    path += treePath[i].Text;
                    if (treePath.Count == 2 || i == 0 || i == treePath.Count - 1)
                    {
                        continue;
                    }
                    path += "\\";
                }
            }
            else
            {
                for (int i = treePath.Count - 1; i >= 0; i--)
                {
                    dataGridView1[j, 0].Value = treePath[i].Text;
                    path += treePath[i].Text;
                    j++;
                    if (treePath.Count == 2 || i == 0 || i == treePath.Count - 1)
                    {
                        continue;
                    }
                    path += "\\";

                }
            }
            return path;
        }

        private List<TreeNode> GetListTreeNodeFromPath(string path)
        {
            List<TreeNode> res = new List<TreeNode>();
            string tmp ="";
            for(int i =0; i < path.Length; i++)
            {
                if(i == 2)
                {
                    tmp += '\\';
                    res.Add(new TreeNode(tmp));
                    tmp = "";
                    continue;
                }
                if(path[i] == '\\' || i == path.Length - 1)
                {
                    if(i == path.Length - 1)
                        tmp += path[i];
                    res.Add(new TreeNode(tmp));
                    tmp = "";
                    continue;
                }
                tmp += path[i];
            }
            res.Reverse();
            return res;
        }

        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (allPath.Count == 1 || allPath.Count == 0)
                return;
            button2.Enabled = true;
            for (int i = 1; i < 5; i++)
            {
                dataGridView1[i, 0].Value = "";
            }
            amountOfStepsBack++;
            string path = GetCurrentPath(allPath[allPath.Count - 1 - amountOfStepsBack]);
            listView1.Items.Clear();
            SetListViewItems(path);
            if (allPath.Count - 1 - amountOfStepsBack == 0)
                button1.Enabled = false;
            dataGridView1.Visible = true;
            textBox2.Visible = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            for (int i = 1; i < 5; i++)
            {
                dataGridView1[i, 0].Value = "";
            }
            amountOfStepsBack--;
            string path = GetCurrentPath(allPath[allPath.Count - 1 - amountOfStepsBack]);
            listView1.Items.Clear();
            SetListViewItems(path);
            if (amountOfStepsBack == 0)
                button2.Enabled = false;
            dataGridView1.Visible = true;
            textBox2.Visible = false;

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (allPath.Count == 0 || allPath == null || e.ColumnIndex == -1)
                return;
            List<TreeNode> currentPath = new List<TreeNode>(allPath[allPath.Count - 1 - amountOfStepsBack]);
            string path = null;
            if (e.ColumnIndex == 0 || e.ColumnIndex >= currentPath.Count)
            {
                path = GetCurrentPath(currentPath);
                dataGridView1.Visible = false;
                textBox2.Text = path;
                textBox2.Visible = true;
            }
            button1.Enabled = true;
            int count = currentPath.Count;
            for (int i = 0; i < count - e.ColumnIndex; i++)
            {
                currentPath.RemoveAt(0);
            }
            for (int i = 1; i < 5; i++)
            {
                dataGridView1[i, 0].Value = "";
            }

            allPath.Add(currentPath);
            path = GetCurrentPath(currentPath);
            listView1.Items.Clear();
            SetListViewItems(path);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyData == Keys.Enter)
            {
                var currentPath = GetListTreeNodeFromPath(textBox2.Text);
                string path = GetCurrentPath(currentPath);
                try
                {
                    var dir = Directory.GetDirectories(path);
                }
                catch
                {
                    MessageBox.Show("Wrong path!","ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    path = GetCurrentPath(allPath[allPath.Count - 1 - amountOfStepsBack]);
                    SetListViewItems(path);
                    return;
                }
                button2.Enabled = false;
                button1.Enabled = true;
                allPath.Add(currentPath);
                listView1.Items.Clear();
                SetListViewItems(path);
                textBox2.Visible = false;
                dataGridView1.Visible = true;
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (allPath.Count == 0 || allPath == null)
                    return;
                //    var search = Directory.GetFiles(path, textBox1.Text);
                string[] files = null;
                string pattern = ".*" + textBox1.Text + ".*[.]*.*";
                string path = GetCurrentPath(allPath[allPath.Count - 1 - amountOfStepsBack]);
                Regex reg = new Regex(pattern);
                try
                {
                    files = Directory.GetFiles(path, "*.*");
                }
                catch
                {

                }
                if(files.Length == 0 || files == null)
                {
                    MessageBox.Show("NO RESULTS!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string fileName;
                ListViewItem item;
                listView1.Items.Clear();
                foreach (string di in files)
                {

                    fileName = GetNameFromPath(di);
                    if(reg.IsMatch(fileName))
                    { 
                        item = new ListViewItem(fileName);
                        item.SubItems.Add(File.GetLastWriteTime(di).ToString());
                        item.SubItems.Add(Path.GetExtension(di));
                        item.ImageIndex = 2;
                        FileInfo fileInfo = new FileInfo(di);
                        double size = (Math.Round((double)fileInfo.Length / 1024));
                        if (size == 0)
                            size = 1;
                        item.SubItems.Add(size.ToString() + " KB");
                        listView1.Items.Add(item);
                    }
                }
            }
            //IEnumerable<string> files = null;
            //try { files = Directory.EnumerateFiles(path, pattern); }
            //catch { }

            //if (files != null)
            //{
            //    foreach (var file in files) yield return file;
            //}

            //IEnumerable<string> directories = null;
            //try { directories = Directory.EnumerateDirectories(path); }
            //catch { }

            //if (directories != null)
            //{
            //    foreach (var file in directories.SelectMany(d => EnumerateAllFiles(d, pattern)))
            //    {
            //        yield return file;
            //    }
            //}
        }
    }
}

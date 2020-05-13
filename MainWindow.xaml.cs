using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using Font;
using NETCore.Encrypt;

namespace MiniNotepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand CommandInsertDateTime = new RoutedCommand();
        public static RoutedCommand CommandNewWindow = new RoutedCommand();
        public static RoutedCommand CommandShowAbout = new RoutedCommand();
        public static RoutedCommand CommandShowHelp = new RoutedCommand();
        public static RoutedCommand CommandFeedBack = new RoutedCommand();
        public static RoutedCommand CommandChooseFont = new RoutedCommand();
        public static RoutedCommand CommandGoTo = new RoutedCommand();
        public static RoutedCommand CommandFindNext = new RoutedCommand();
        public static RoutedCommand CommandSetPassword = new RoutedCommand();


        private string findstr;
        private int findnow;


        private const string fjcode = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやよらりるれろわをぐげござじずぞだぢづでばびぶべぱぴぷぺぽん";
        private const string b64code = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
        private const string prefix = "先辈真言：";
        internal string Base64ToFakeJapanese(string b64)
        {
            string ret = string.Empty;
            ret += prefix;
            foreach (char c in b64) ret += fjcode[b64code.IndexOf(c)];
            return ret;
        }
        internal string FakeJapaneseToBase64(string fj)
        {
            string ret = string.Empty;
            fj = fj.Substring(prefix.Length);
            foreach (char c in fj) ret += b64code[fjcode.IndexOf(c)];
            return ret;
        }


        public MainWindow() : this(new FileContext()) { }
        public MainWindow(FileContext context)
        {
            InitializeComponent();
            DataContext = context;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) DoOpen(args[1]);
        }

        private void NewFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataContext = new FileContext();
        }

        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".txt";
            ofd.Filter = "文本文档|*.txt|黑 色 高 级 文 档|*.bad|所有文件|*.*";
            if (ofd.ShowDialog() == true)
            {
                DoOpen(ofd.FileName);
            }
        }

        internal void DoOpen(string path)
        {
            var ctx = new FileContext(path);
            var oldctx = DataContext as FileContext;
            try
            {
                DataContext = ctx;
                ctx.Status = FcStatus.Opening;
                if (ctx.Path.EndsWith(".bad"))
                {
                    CommandSetPassword.Execute(null, this);
                    ctx.Content = EncryptProvider.AESDecrypt(FakeJapaneseToBase64(File.ReadAllText(ctx.Path)), EncryptProvider.Md5(ctx.Password));
                    if (string.IsNullOrEmpty(ctx.Content))
                    {
                        MessageBox.Show(this, "解密失败，密码可能有错。");
                        ctx.Path = string.Empty;
                        DataContext = oldctx;
                        return;
                    }
                    ctx.isBADFile = true;
                }
                else
                {
                    ctx.isBADFile = false;
                    ctx.Content = File.ReadAllText(ctx.Path);
                }
                ctx.Status = FcStatus.Opened;
            }
            catch (UnauthorizedAccessException )
            {
                MessageBox.Show("程序无权访问该文件，请尝试使用管理员权限运行。");
                DataContext = oldctx;
            }
        }

        private void SaveFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            if (ctx.Path != string.Empty)
            {
                ctx.Status = FcStatus.Saving;
                if (ctx.isBADFile)
                {
                    if (string.IsNullOrEmpty(ctx.Content))
                    {
                        MessageBox.Show(this, "无法保存内容为空的 黑 色 高 级 文 档。");
                        return;
                    }
                    File.WriteAllText(ctx.Path, Base64ToFakeJapanese(EncryptProvider.AESEncrypt(ctx.Content, EncryptProvider.Md5(ctx.Password))));
                }
                else File.WriteAllText(ctx.Path, ctx.Content);
                ctx.Status = FcStatus.Opened;
            }
            else ApplicationCommands.SaveAs.Execute(null, this);
        }

        private void SaveFileAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.DefaultExt = "*.txt";
            sfd.Filter = "文本文档|*.txt|黑 色 高 级 文 档|*.bad";
            if (sfd.ShowDialog() == true)
            {
                var ctx = DataContext as FileContext;
                ctx.Path = sfd.FileName;
                ctx.Status = FcStatus.Saving;
                if (ctx.Path.EndsWith(".bad"))
                {
                    ctx.isBADFile = true;
                    CommandSetPassword.Execute(null, this);
                    if (string.IsNullOrEmpty(ctx.Content))
                    {
                        MessageBox.Show(this, "无法保存内容为空的 黑 色 高 级 文 档。");
                        return;
                    }
                    File.WriteAllText(ctx.Path, Base64ToFakeJapanese(EncryptProvider.AESEncrypt(ctx.Content, EncryptProvider.Md5(ctx.Password))));
                }
                else
                {
                    ctx.isBADFile = false;
                    File.WriteAllText(ctx.Path, ctx.Content);
                }
                ctx.Status = FcStatus.Opened;
            }
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ctx = DataContext as FileContext;
            if (ctx.Status == FcStatus.Modified)
            {
                MessageBoxResult result = MessageBox.Show("您想将更改保存到 " + (ctx.Path == string.Empty? "无标题" : ctx.Path) + " 吗？", "黑 色 高 级 记 事 本", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        ApplicationCommands.Save.Execute(null, this);
                        e.Cancel = ctx.Status == FcStatus.Modified;
                        break;
                    case MessageBoxResult.No:
                        e.Cancel = false;
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void InsertDateTime_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            ctx.Content += DateTime.Now.ToString();
        }

        private void tbContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            ctx.Status = FcStatus.Modified;
        }

        private void NewWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (new MainWindow()).Show();
        }

        private void ShowAbout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AboutWindow w = new AboutWindow();
            w.Owner = this;
            w.ShowDialog();
        }

        private void ShowHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/qzhsjz/BlackAdvancedNotepad");
        }

        private void FeedBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "mailto:qzhsjz@gmail.com");
        }

        private void GoTo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputDialog ind = new InputDialog("转到", "请输入行号：");
            ind.Owner = this;
            if (ind.ShowDialog() != true) return;
            try
            {
                tbContent.CaretIndex = tbContent.GetCharacterIndexFromLineIndex(Convert.ToInt32(ind.Result) - 1);
            }
            catch (ArgumentOutOfRangeException) { };
            tbContent.Focus();
        }

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            InputDialog ind = new InputDialog("查找", "请输入查找内容：", findstr);
            ind.Owner = this;
            if (ind.ShowDialog() != true || ctx.Content == null) return;
            findstr = ind.Result;
            findnow = ctx.Content.IndexOf(findstr);
            if (findnow < 0)
            {
                MessageBox.Show(this, "已完成对该文档的搜索。");
                return;
            }
            tbContent.Select(findnow, findstr.Length);
            tbContent.ScrollToLine(tbContent.GetLineIndexFromCharacterIndex(findnow));
            tbContent.Focus();
        }

        private void FindNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            findnow = ctx.Content.IndexOf(findstr, findnow + 1);
            if (findnow < 0)
            {
                MessageBox.Show(this, "已完成对该文档的搜索。");
                return;
            }
            tbContent.Select(findnow, findstr.Length);
            tbContent.ScrollToLine(tbContent.GetLineIndexFromCharacterIndex(findnow));
            tbContent.Focus();
        }

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string f, r;
            var ctx = DataContext as FileContext;
            InputDialog indf = new InputDialog("查找并替换", "请输入查找内容：", findstr);
            InputDialog indr = new InputDialog("查找并替换", "请输入替换内容：");
            indf.Owner = this;
            indr.Owner = this;
            if (indf.ShowDialog() != true) return;
            f = indf.Result;
            if (indr.ShowDialog() != true || ctx.Content == null) return;
            r = indr.Result;
            MessageBoxResult result = MessageBox.Show(this, "将会把 " + f + " 全部替换为 " + r + " ，是否执行？", "全部替换", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ctx.Content = ctx.Content.Replace(f, r);
                MessageBox.Show(this, "已完成对该文档的全部替换。");
                tbContent.Focus();
            }
        }

        private void MainWindow_OnPreviewDrop(object sender, DragEventArgs e)
        {
            try
            {
                DoOpen(((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString());
                e.Handled = true;
            }
            catch (NullReferenceException) { }
        }

        private void TbContent_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetFormats().Contains("FileDrop")) e.Handled = true;
        }

        private void SetPassword_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            InputDialog ind = new InputDialog("设置密码", "请输入密码（留空则使用默认密码）：");
            try { ind.Owner = this; }
            catch (System.InvalidOperationException) { ind.Title = "黑 色 高 级 记 事 本"; }
            if (ind.ShowDialog() != true || string.IsNullOrEmpty(ind.Result)) ctx.Password = "JeF8U9wHFOMfs2Y8";
            else ctx.Password = ind.Result;
            if (ctx.Status == FcStatus.Modified || ctx.Status == FcStatus.Opened) ApplicationCommands.Save.Execute(null, this);
        }

        private void ChooseFont_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ctx = DataContext as FileContext;
            FontDialog fd = new FontDialog();
            fd.Owner = this;
            if (fd.ShowDialog() == true)
            {
                ctx.ContentFontFamily = fd.ResultFontFamily;
                ctx.ContentFontSize = fd.ResultFontSize;
                ctx.ContentFontStyle = fd.ResultTypeFace.Style;
                ctx.ContentFontWeight = fd.ResultTypeFace.Weight;
            }
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog pDialog = new PrintDialog();
            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
            pDialog.UserPageRangeEnabled = true;

            bool? print = pDialog.ShowDialog();
            if (print == true)
            {
                XpsDocument xpsDocument = new XpsDocument("C:\\FixedDocumentSequence.xps", FileAccess.ReadWrite);
                FixedDocumentSequence fixedDocSeq = xpsDocument.GetFixedDocumentSequence();
                pDialog.PrintDocument(fixedDocSeq.DocumentPaginator, "打印任务");
            }
        }
    }
}

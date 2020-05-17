using Accessibility;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MiniNotepad
{

    public enum FcStatus
    {
        Opened, Modified, Opening, Saving,
    }

    [AddINotifyPropertyChangedInterface]
    public class FileContext
    {
        public string Password { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public FcStatus Status { get; set; }

        public int Row { get; set; } = 1;
        public int Column { get; set; } = 1;
        public string CaretPositionTxt => "行 " + Row.ToString() + " ，列 " + Column.ToString();

        public string MainStatusTxt => Status == FcStatus.Opening ? "正在打开……" :
            Status == FcStatus.Saving ? "正在保存……" :
            Status == FcStatus.Opened ? "就绪" :
            Status == FcStatus.Modified ? "就绪" :
            "状态错误";
        public string TitleTxt => ( Path != string.Empty ? Path : "无标题" ) + ( Status == FcStatus.Modified ? "*" : "" ) + (isBADFile ? " - 黑 色 高 级 记 事 本" : " - 记事本");
        
        public Visibility VsBADFile { get; set; } = Visibility.Collapsed;
        public Brush Contentbg { get; set; } = Brushes.White;
        public Brush Contentfg { get; set; } = Brushes.Black;
        public Brush Contentsel { get; set; } = SystemParameters.WindowGlassBrush;
        public Brush Contentbdr { get; set; } = SystemColors.ActiveBorderBrush;
        private bool _isBADFile = false;
        public bool isBADFile
        {
            get => _isBADFile;
            set
            {
                _isBADFile = value;
                VsBADFile = value ? Visibility.Visible : Visibility.Collapsed;
                Contentbg = value ? Brushes.Black : Brushes.White;
                Contentfg = value ? Brushes.White : Brushes.Black;
                Contentsel = value ? Brushes.White : SystemParameters.WindowGlassBrush;
                Contentbdr = value ? Brushes.White : SystemColors.ActiveBorderBrush;
            }
        }

        public string BADFileStatusTxt => isBADFile ? "黑 色 高 级 文 档" : "";

        public Visibility VsStatusBar { get; set; } = Visibility.Visible;
        public bool ShowStatusBar
        {
            get => VsStatusBar == Visibility.Visible;
            set => VsStatusBar = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public TextWrapping TwContent { get; set; } = TextWrapping.Wrap;
        public bool AutoWrap 
        {
            get => TwContent == TextWrapping.Wrap;
            set => TwContent = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public FontFamily ContentFontFamily { get; set; } = new FontFamily("Microsoft YaHei UI");
        public double ContentFontSize { get; set; } = 12;
        public FontStyle ContentFontStyle { get; set; } = FontStyles.Normal;
        public FontWeight ContentFontWeight { get; set; } = FontWeights.Normal;

        public FileContext()
        {
            Path = string.Empty;
        }
        public FileContext(string path)
        {
            Path = path;
        }

    }
}

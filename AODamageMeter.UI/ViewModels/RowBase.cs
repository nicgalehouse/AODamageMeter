using AODamageMeter.UI.Helpers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowBase : ViewModelBase
    {
        protected const string EmDash = NumberFormatter.EmDash;

        protected RowBase(DamageMeterViewModel damageMeterViewModel)
            => DamageMeterViewModel = damageMeterViewModel;

        protected DamageMeterViewModel DamageMeterViewModel { get; }

        // This damage meter isn't necessarily the same damage meter that created the stats for this row. Important
        // because it's this (most recent) damage meter that controls where the script file should be located.
        protected DamageMeter CurrentDamageMeter => DamageMeterViewModel.DamageMeter;

        public abstract string Title { get; }
        public abstract string LeftText { get; }
        public abstract string LeftTextToolTip { get; }

        protected int _displayIndex;
        public int DisplayIndex
        {
            get => _displayIndex;
            protected set => Set(ref _displayIndex, value);
        }

        protected string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            protected set => Set(ref _iconPath, value);
        }

        protected Color _color;
        public Color Color
        {
            get => _color;
            protected set => Set(ref _color, value);
        }

        protected double _percentWidth;
        public double PercentWidth
        {
            get => _percentWidth;
            protected set => Set(ref _percentWidth, value);
        }

        protected string _rightText;
        public string RightText
        {
            get => _rightText;
            protected set => Set(ref _rightText, value);
        }

        public abstract string RightTextToolTip { get; }

        public virtual void Update(int? displayIndex = null)
        {
            DisplayIndex = displayIndex ?? DisplayIndex;
            RaisePropertyChanged(nameof(LeftTextToolTip));
            RaisePropertyChanged(nameof(RightTextToolTip));
        }

        public void CopyAndScriptLeftTextTooltip()
            => CopyAndScript(LeftTextToolTip);

        public virtual bool TryCopyAndScriptProgressedRowsInfo()
            => false;

        public void CopyAndScriptRightTextTooltip()
            => CopyAndScript(RightTextToolTip);

        public string RowScriptText
            => $"{(DisplayIndex < 10 ? " " : "")}{DisplayIndex}. {RightText} {LeftText}";

        protected void CopyAndScript(string body)
        {
            Clipboard.SetText($"{Title}{Environment.NewLine}{Environment.NewLine}{body}");

            string scriptTitle = Title;
            string scriptBody = FormatForScript(body);
            int markupLength = "<a href=\"text://\"></a>".Length;
            int totalLength = scriptTitle.Length + scriptBody.Length + markupLength;
            int lengthOverTheLimit = Math.Max(totalLength - 1024, 0);
            string script = lengthOverTheLimit > 0
                ? $"<a href=\"text://{scriptBody.Substring(0, scriptBody.Length - lengthOverTheLimit - 3)}...\">{Title}</a>"
                : $"<a href=\"text://{scriptBody}\">{Title}</a>";

            if (VerifyScriptDirectoryExists())
            {
                try { File.WriteAllText(CurrentDamageMeter.GetScriptFilePath(), script); }
                catch { }
            }
        }

        private bool VerifyScriptDirectoryExists()
        {
            string scriptFolderPath = CurrentDamageMeter.GetScriptFolderPath();
            if (!Directory.Exists(scriptFolderPath))
            {
                try { Directory.CreateDirectory(scriptFolderPath); }
                catch { return false; }
            }
            return true;
        }

        private string FormatForScript(string text)
            => text.Replace(Environment.NewLine, "<br>")
            .Replace(EmDash, "--")
            .Replace("≤", "<=")
            .Replace("≥", ">=")
            .Replace("〈", "(")
            .Replace("〉", ")");
    }
}

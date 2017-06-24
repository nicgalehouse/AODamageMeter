using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowBase : ViewModelBase
    {
        protected const string EmDash = NumberFormatter.EmDash;
        protected const string EnDash = NumberFormatter.EnDash;
        protected const string EmDashPercent = NumberFormatter.EmDashPercent;

        protected RowBase(FightViewModel fightViewModel)
            => FightViewModel = fightViewModel;

        public FightViewModel FightViewModel { get; }
        public DamageMeter DamageMeter => FightViewModel.DamageMeter;
        public Fight Fight => FightViewModel.Fight;
        public Character Owner => FightViewModel.Owner;
        public FightCharacter FightOwner => FightViewModel.FightOwner;

        public abstract string Title { get; }
        public abstract string UnnumberedLeftText { get; }
        public abstract string LeftTextToolTip { get; }
        public abstract string RightTextToolTip { get; }
        public virtual bool SupportsRowNumbers => true;

        protected string _leftText;
        public string LeftText
        {
            get => _leftText;
            protected set => Set(ref _leftText, value);
        }

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

        public virtual void Update(int? displayIndex = null)
        {
            DisplayIndex = displayIndex ?? DisplayIndex;
            LeftText = Settings.Default.ShowRowNumbers && SupportsRowNumbers
                ? $"{DisplayIndex}. {UnnumberedLeftText}"
                : UnnumberedLeftText;
            RaisePropertyChanged(nameof(LeftTextToolTip));
            RaisePropertyChanged(nameof(RightTextToolTip));
        }

        public void CopyAndScriptLeftTextTooltip()
            => CopyAndScript(LeftTextToolTip);

        public virtual bool TryCopyAndScriptProgressedRowsInfo()
            => false;

        protected bool CopyAndScriptProgressedRowsInfo(IEnumerable<MainRowBase> progressedRows)
        {
            var body = new StringBuilder();
            foreach (var row in progressedRows.OrderBy(r => r.DisplayIndex))
            {
                body.AppendLine($"{(row.DisplayIndex < 10 ? " " : "")}{row.DisplayIndex}. {row.RightText} {row.UnnumberedLeftText}");
            }

            CopyAndScript(
$@"{Title}

{body}");

            return true;
        }

        public void CopyAndScriptRightTextTooltip()
            => CopyAndScript(RightTextToolTip);

        protected void CopyAndScript(string text)
        {
            Clipboard.SetText(text);

            string scriptTitle = FormatForScript(Title);
            string scriptBody = FormatForScript(text);
            int markupLength = "<a href=\"text://\"></a>".Length;
            int totalLength = scriptTitle.Length + scriptBody.Length + markupLength;
            int lengthOverTheLimit = Math.Max(totalLength - 1024, 0);
            string script = lengthOverTheLimit > 0
                ? $"<a href=\"text://{scriptBody.Substring(0, scriptBody.Length - lengthOverTheLimit - 3)}...\">{scriptTitle}</a>"
                : $"<a href=\"text://{scriptBody}\">{scriptTitle}</a>";

            if (VerifyScriptDirectoryExists())
            {
                try { File.WriteAllText(DamageMeter.GetScriptFilePath(), script); }
                catch { }
            }
        }

        private bool VerifyScriptDirectoryExists()
        {
            string scriptFolderPath = DamageMeter.GetScriptFolderPath();
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
            .Replace(EnDash, "-")
            .Replace("≤", "<=")
            .Replace("≥", ">=")
            .Replace("〈", "(")
            .Replace("〉", ")");
    }
}

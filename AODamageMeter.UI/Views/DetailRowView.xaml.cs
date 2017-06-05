using System.Windows.Controls;

namespace AODamageMeter.UI.Views
{
    public partial class DetailRowView : UserControl
    {
        public DetailRowView()
        {
            InitializeComponent();

            // Matters when font properties change. These two text blocks share the same height, and name's
            // text block has no width-only false positives, so pivot off of its event.
            LeftTextBlock.SizeChanged += (_, e) => 
            {
                if (!e.HeightChanged) return;

                Canvas.SetTop(LeftTextBlock, (18 - LeftTextBlock.ActualHeight) / 2);
                Canvas.SetTop(RightTextBlock, (18 - LeftTextBlock.ActualHeight) / 2);
            };
        }
    }
}

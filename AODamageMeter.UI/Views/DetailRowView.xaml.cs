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
            NameTextBlock.SizeChanged += (_, e) => 
            {
                if (!e.HeightChanged) return;

                Canvas.SetTop(NameTextBlock, (18 - NameTextBlock.ActualHeight) / 2);
                Canvas.SetTop(RightTextBlock, (18 - NameTextBlock.ActualHeight) / 2);
            };
        }
    }
}

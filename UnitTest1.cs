using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace TestFileExplorer
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            int result_value = 3;

            await Task.Delay(100);

            var menu = mainWindow.GetVisualDescendants().OfType<MenuItem>().First(m => m.Name == "menuitem_view");
            var menuitem = menu.GetLogicalChildren().OfType<CheckBox>().First(mi => mi.Name == "navigation_panel");            

            var main_panel = mainWindow.GetVisualDescendants().OfType<Grid>().First(mp => mp.Name == "main_panel");

            menuitem.IsChecked = true; // кликнуть. так ничего не изменится

            await Task.Delay(100);

            var widthOfMainPanel = main_panel.GetValue(Grid.ColumnSpanProperty);

            Assert.True(widthOfMainPanel.Equals(result_value));
        }       
    }
}
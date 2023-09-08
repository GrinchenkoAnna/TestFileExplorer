using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using FileExplorer.ViewModels;
using static FileExplorer.ViewModels.DirectoryItemViewModel;
using FileExplorer.Views;
using DynamicData;
using System.Collections.ObjectModel;
using System.Drawing.Text;

namespace TestFileExplorer
{
    public class UnitTest1
    {
        public ISynchronizationHelper? synchronizationHelper;

        [Fact]
        public async void OpenDirectory()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            await Task.Delay(100);

            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //какая-нибудь папка из диска C
            var folder = new DirectoryViewModel
                (
                Path.GetFullPath
                    (
                        Directory.GetDirectories("C://").First().ToString()
                    )
                );         

            //функция открытия папки и добавления элементов в DirectoriesAndFiles
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //подсчет элементов в директории
            int count = Directory.GetDirectories(folder.FullName).Length
                + Directory.GetFiles(folder.FullName).Length;

            //количество элементов в DirectoriesAndFiles
            int result = 0;

            //подсчет элементов в DirectoriesAndFiles
            foreach (var item in mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles)
            {
                result++;
            }

            await Task.Delay(50);     

            //количество элементво должно совпадать
            Assert.True(count == result, "кол-во добавленных эл-тов не совпадает с кол-вом существующих");       
        }       
    }
}
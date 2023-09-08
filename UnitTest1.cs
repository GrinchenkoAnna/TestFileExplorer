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

            //�����-������ ����� �� ����� C
            var folder = new DirectoryViewModel
                (
                Path.GetFullPath
                    (
                        Directory.GetDirectories("C://").First().ToString()
                    )
                );         

            //������� �������� ����� � ���������� ��������� � DirectoriesAndFiles
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //������� ��������� � ����������
            int count = Directory.GetDirectories(folder.FullName).Length
                + Directory.GetFiles(folder.FullName).Length;

            //���������� ��������� � DirectoriesAndFiles
            int result = 0;

            //������� ��������� � DirectoriesAndFiles
            foreach (var item in mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles)
            {
                result++;
            }

            await Task.Delay(50);     

            //���������� ��������� ������ ���������
            Assert.True(count == result, "���-�� ����������� ��-��� �� ��������� � ���-��� ������������");       
        }       
    }
}
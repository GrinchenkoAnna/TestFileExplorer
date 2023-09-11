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
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Security.Claims;

namespace TestFileExplorer
{
    public class Directories
    {
        public ISynchronizationHelper? synchronizationHelper;
        private readonly ITestOutputHelper output;

        public Directories(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async void Open()
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
                        Directory.GetDirectories(@"C:\").First()
                    )
                );
            output.WriteLine("Chosed folder: {0}", folder.FullName);

            //Записать пути в колекцию
            ObservableCollection<string> ItemsInDirectory = new ObservableCollection<string>();
            foreach (string dir in Directory.EnumerateDirectories(folder.FullName))
            {
                ItemsInDirectory.Add(dir);
                output.WriteLine("Item in system: {0}", dir);
            }
            foreach (string file in Directory.EnumerateFiles(folder.FullName))
            {
                ItemsInDirectory.Add(file);
                output.WriteLine("Item in system: {0}", file);
            }

            //функция открытия папки и добавления элементов в DirectoriesAndFiles
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //подсчет элементов в директории
            output.WriteLine("Directories in folder: {0}", Directory.GetDirectories(folder.FullName).Length);
            output.WriteLine("Files in folder: {0}", Directory.GetFiles(folder.FullName).Length);
            int count = Directory.GetDirectories(folder.FullName).Length
                + Directory.GetFiles(folder.FullName).Length;

            //количество элементов в DirectoriesAndFiles
            int result = 0;

            //пересечение двух коллекций
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine("Item in DAF: {0}", item.FullName);
            }
            var intersection = ItemsInDirectory.Intersect(directoriesAndFiles.Select(i => i.FullName)).ToList();
            foreach (var item in intersection) { result++; }

            output.WriteLine("Count = {0}", count);
            output.WriteLine("Result = {0}", result);

            await Task.Delay(50);

            //количество элементво должно совпадать
            Assert.True(count == result);
        }

        [Fact]
        public async void Copy()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            await Task.Delay(100);

            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\test_folder\test_file.txt");

            //выбор папки на диске C
            var folder = new DirectoryViewModel(@"C:\test_folder\test_file.txt");
            output.WriteLine("Chosed folder: {0}", folder.FullName);

            //копирование этой папки
            var buffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            foreach (var item in buffer)
            {
                buffer.Remove(item);
            }
            mainWindowViewModel.CurrentDirectoryItem.Copy(folder);

            //проверка: в буфере один элемент (тот, который был скопирован)  
            if (buffer.Contains(folder.FullName) && buffer.Count == 1)
            {
                output.WriteLine("Item in ItemBuffer: {0}", buffer.First().ToString());
                Assert.True(true);
                Directory.Delete(folder.FullName);
            }
            else
            {
                foreach (var item in buffer) 
                {
                    output.WriteLine("Wrong item in ItemBuffer: {0}", item);
                }
                Assert.True(false);
            }
        }
    }
}
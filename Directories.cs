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

            //кака€-нибудь папка из диска C
            var folder = new DirectoryViewModel
                (
                Path.GetFullPath
                    (
                        Directory.GetDirectories(@"C:\").First()
                    )
                );
            output.WriteLine($"Chosed folder: {folder.FullName}");

            //«аписать пути в колекцию
            ObservableCollection<string> ItemsInDirectory = new ObservableCollection<string>();
            foreach (string dir in Directory.EnumerateDirectories(folder.FullName))
            {
                ItemsInDirectory.Add(dir);
                output.WriteLine($"Item in system: {dir}");
            }
            foreach (string file in Directory.EnumerateFiles(folder.FullName))
            {
                ItemsInDirectory.Add(file);
                output.WriteLine($"Item in system: {file}");
            }

            //функци€ открыти€ папки и добавлени€ элементов в DirectoriesAndFiles
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //подсчет элементов в директории
            output.WriteLine($"Directories in folder: {Directory.GetDirectories(folder.FullName).Length}");
            output.WriteLine($"Files in folder: {Directory.GetFiles(folder.FullName).Length}");
            int count = Directory.GetDirectories(folder.FullName).Length
                + Directory.GetFiles(folder.FullName).Length;

            //количество элементов в DirectoriesAndFiles
            int result = 0;

            //пересечение двух коллекций
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF: {item.FullName}");
            }
            var intersection = ItemsInDirectory.Intersect(directoriesAndFiles.Select(i => i.FullName)).ToList();
            foreach (var item in intersection) { result++; }

            output.WriteLine($"Count = {count}");
            output.WriteLine($"Result = {result}");

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

            Directory.CreateDirectory(@"C:\test_folder\test_folder2");

            //выбор папки на диске C
            var folder = new DirectoryViewModel(@"C:\test_folder\test_folder2");
            output.WriteLine($"Chosed folder: {folder.FullName}");

            //копирование этой папки
            var buffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            foreach (var item in buffer)
            {
                buffer.Remove(item);
            }
            mainWindowViewModel.CurrentDirectoryItem.Copy(folder);

            await Task.Delay(50);

            //проверка: в буфере один элемент (тот, который был скопирован)  
            if (buffer.Contains(folder.FullName) && buffer.Count == 1)
            {
                output.WriteLine($"Item in ItemBuffer: {buffer.First()}");
                Assert.True(true);
                Directory.Delete(folder.FullName);
            }
            else
            {
                foreach (var item in buffer) 
                {
                    output.WriteLine($"Wrong item in ItemBuffer: {item}");
                }
                Assert.True(false);
            }
        }

        [Fact]
        public async void Paste()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            await Task.Delay(100);

            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\test_folder");
            //File.Create(@"C:\test_folder\test_file.txt");

            //запись папки в "буфер"
            var buffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            foreach (var item in buffer)
            {
                buffer.Remove(item);
            }
            buffer.Add(@"C:\test_folder");
            //buffer.Add(@"C:\test_folder\test_file.txt");
            foreach (var item in buffer)
            {
                output.WriteLine($"Item in buffer: {item}");
            }

            //выполнение вставки папки в выбранную директорию
            mainWindowViewModel.CurrentDirectoryItem.Paste(@"C:\");

            //открытие этой директории
            var destination_folder = new DirectoryViewModel(@"C:\");
            mainWindowViewModel.CurrentDirectoryItem.Open(destination_folder);

            //проверка: вставленна€ папка находитс€ в данной директории
            bool folder_success = false;
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_1: {item.FullName}");
                if (item.FullName == @"C:\test_folder Ч копи€")
                {
                    folder_success = true;                    
                }
            }     

            ////проверка: файл тоже переместилс€
            //bool file_success = false;
            //var test_folder = new DirectoryViewModel(@"C:\test_folder Ч копи€");
            //mainWindowViewModel.CurrentDirectoryItem.Open(test_folder);
            //foreach (var item in directoriesAndFiles)
            //{
            //    output.WriteLine($"Item in DAF_2: {item.FullName}");
            //    if (item.FullName == @"C:\test_folder Ч копи€\test_file.txt")
            //    {
            //        file_success = true;
            //    }
            //}

            //await Task.Delay(50);

            if (folder_success == true)
            {
                Directory.Delete(@"C:\test_folder", true);
                Directory.Delete(@"C:\test_folder Ч копи€", true);
                Assert.True(true);
            }

            //if (folder_success == true && file_success == true)
            //{
            //    Directory.Delete(@"C:\test_folder", true);
            //    Directory.Delete(@"C:\test_folder Ч копи€", true);
            //    Assert.True(true);
            //}
            //else
            //{
            //    if (folder_success == true && file_success == false)
            //    {
            //        Directory.Delete(@"C:\test_folder", true);
            //        Directory.Delete(@"C:\test_folder Ч копи€", true);
            //        Assert.False(true, "pasted folder, not file");

            //    }
            //    else if (folder_success == false && file_success == true)
            //    {
            //        Directory.Delete(@"C:\test_folder", true);
            //        Directory.Delete(@"C:\test_folder Ч копи€", true);
            //        Assert.False(true, "pasted file, not folder");
            //    }
            //    else
            //    {
            //        Directory.Delete(@"C:\test_folder", true);
            //        Directory.Delete(@"C:\test_folder Ч копи€", true);
            //        Assert.False(true, "hothing had been pasted");
            //    }
            //}
        }
    }
}
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
        public async void OpenTest()
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
            output.WriteLine($"Chosed folder: {folder.FullName}");

            //Записать пути в колекцию
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

            //функция открытия папки и добавления элементов в DirectoriesAndFiles
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
        public async void CopyTest()
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
        public async void CutTest()
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
            mainWindowViewModel.CurrentDirectoryItem.Cut(folder);

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
        public async void PasteTest()
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

            //проверка: вставленная папка находится в данной директории
            bool folder_success = false;
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_1: {item.FullName}");
                if (item.FullName == @"C:\test_folder — копия")
                {
                    folder_success = true;                    
                }
            }     

            ////проверка: файл тоже переместился
            //bool file_success = false;
            //var test_folder = new DirectoryViewModel(@"C:\test_folder — копия");
            //mainWindowViewModel.CurrentDirectoryItem.Open(test_folder);
            //foreach (var item in directoriesAndFiles)
            //{
            //    output.WriteLine($"Item in DAF_2: {item.FullName}");
            //    if (item.FullName == @"C:\test_folder — копия\test_file.txt")
            //    {
            //        file_success = true;
            //    }
            //}

            //await Task.Delay(50);

            if (folder_success == true)
            {
                Directory.Delete(@"C:\test_folder", true);
                Directory.Delete(@"C:\test_folder — копия", true);
                Assert.True(true);
            }

            //if (folder_success == true && file_success == true)
            //{
            //    Directory.Delete(@"C:\test_folder", true);
            //    Directory.Delete(@"C:\test_folder — копия", true);
            //    Assert.True(true);
            //}
            //else
            //{
            //    if (folder_success == true && file_success == false)
            //    {
            //        Directory.Delete(@"C:\test_folder", true);
            //        Directory.Delete(@"C:\test_folder — копия", true);
            //        Assert.False(true, "pasted folder, not file");

            //    }
            //    else if (folder_success == false && file_success == true)
            //    {
            //        Directory.Delete(@"C:\test_folder", true);
            //        Directory.Delete(@"C:\test_folder — копия", true);
            //        Assert.False(true, "pasted file, not folder");
            //    }
            //    else
            //    {
            //        Directory.Delete(@"C:\test_folder", true);
            //        Directory.Delete(@"C:\test_folder — копия", true);
            //        Assert.False(true, "hothing had been pasted");
            //    }
            //}
        }

        [Fact]
        public async void DeleteTest()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            await Task.Delay(100);

            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание папок и файлов на диске C
            Directory.CreateDirectory(@"C:\test_folder1");
            Directory.CreateDirectory(@"C:\test_folder2");
            var folder2 = new DirectoryViewModel(@"C:\test_folder2");
            //File.Create(@"C:\test_file1.txt");
            //File.Create(@"C:\test_file2.txt");
            //var file2 = new FileViewModel(@"C:\test_file2");
            //File.SetAttributes(@"C:\test_file1.txt", FileAttributes.Normal);
            //File.SetAttributes(@"C:\test_file2.txt", FileAttributes.Normal);

            output.WriteLine(@"Chosed folder: C:\test_folder1");
            output.WriteLine(@"Chosed folder: C:\test_folder2");
            //output.WriteLine(@"Chosed file: C:\test_file1.txt");
            //output.WriteLine(@"Chosed file: C:\test_file2.txt");
            output.WriteLine("");

            //открытие директории C:\
            var main_folder = new DirectoryViewModel(@"C:\");
            mainWindowViewModel.CurrentDirectoryItem.Open(main_folder);

            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");                
            }
            output.WriteLine("");

            //удаление папок и файлов
            mainWindowViewModel.CurrentDirectoryItem.Delete(@"C:\test_folder1");
            mainWindowViewModel.CurrentDirectoryItem.Delete(folder2.FullName);
            //mainWindowViewModel.CurrentDirectoryItem.Delete(@"C:\test_file1");
            //mainWindowViewModel.CurrentDirectoryItem.Delete(file2.FullName);

            //проверка: в DirectoriesAndFiles нет удаленных папок и файлов
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
                if (item.FullName == @"C:\test_folder1")
                {
                    Assert.True(false, "Из DAF не удалена test_folder1");
                }
                if (item.FullName == folder2.FullName)
                {
                    Assert.True(false, "Из DAF не удалена test_folder2");
                }
                //if (item.FullName == @"C:\test_file1")
                //{
                //    Assert.True(false, "Из DAF не удален test_file1.txt");
                //}
                //if (item.FullName == file2.FullName)
                //{
                //    Assert.True(false, "Из DAF не удален test_file2.txt");
                //}
            }

            //проверка: файлы и папки удалены из системы
            if (Directory.Exists(@"C:\test_folder1"))
            {
                Assert.True(false, "Не удалена test_folder1");
            }
            if (Directory.Exists(@"C:\test_folder2"))
            {
                Assert.True(false, "Не удалена test_folder2");
            }
            //if (Directory.Exists(@"C:\test_file1"))
            //{
            //    Assert.True(false, "Не удален test_file1.txt");
            //}
            //if (Directory.Exists(@"C:\test_file2"))
            //{
            //    Assert.True(false, "Не удален test_file2.txt");
            //}            

            Assert.True(true);
        }

        [Fact]
        public async void AddToQuickAccessTest()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            await Task.Delay(100);

            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание элементов, которые будут добавлены на панель быстрого доступа
            Directory.CreateDirectory(@"C:\test_folder");
            var folder = new DirectoryViewModel(@"C:\test_folder");
            //File.Create(@"C:\test_file.txt");
            //var file = new FileViewModel(@"C:\test_file.txt");

            //просмотр панели быстрого доступа до добавления элементов
            var quickAccessPanel = mainWindowViewModel.CurrentDirectoryItem.QuickAccessItems;
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_before: {item.FullName}");
            }
            output.WriteLine("");

            //добавление элементов на панель быстрого доступа
            mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(folder);
            //mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(file);

            //просмотр панели быстрого доступа после добавления элементов
            //bool file_success = false;
            bool folder_success = false;
            foreach (var item in quickAccessPanel)
            {
                if (item.FullName == @"C:\test_folder")
                {
                    folder_success = true;
                }
                //if (item.FullName == @"C:\test_file.txt")
                //{
                //    file_success = true;
                //}
                output.WriteLine($"Item in QAI_after: {item.FullName}");
            }

            //проверка
            if (folder_success == true)
            {
                mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
                Directory.Delete(@"C:\test_folder");
                Assert.True(true);
            }            
            else
            {
                Assert.True(false, "folder not added");
            }

            //проверка: оба элемента добавлены
            //if (folder_success == true && file_success == true)
            //{
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
            //    Directory.Delete(@"C:\test_folder");
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(file);
            //    File.Delete(@"C:\test_file.txt");
            //    Assert.True(true);
            //}
            //else if (folder_success == true && file_success == false)
            //{
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
            //    Directory.Delete(@"C:\test_folder");
            //    Assert.True(false, "folder added, file - not");
            //}
            //else if (folder_success == false && file_success == true)
            //{
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(file);
            //    File.Delete(@"C:\test_file.txt");
            //    Assert.True(false, "file added, folder - not");
            //}
            //else
            //{
            //    Assert.True(false, "not added not folder nor file");
            //}
        }

        [Fact]
        public async void RemoveFromQuickAccessTest()
        {
            var app = AvaloniaApp.GetApp();
            var mainWindow = AvaloniaApp.GetMainWindow();
            await Task.Delay(100);

            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание элементов, которые будут добавлены на панель быстрого доступа
            Directory.CreateDirectory(@"C:\test_folder");
            var folder = new DirectoryViewModel(@"C:\test_folder");
            //File.Create(@"C:\test_file.txt");
            //var file = new FileViewModel(@"C:\test_file.txt");

            //просмотр панели быстрого доступа до добавления элементов
            var quickAccessPanel = mainWindowViewModel.CurrentDirectoryItem.QuickAccessItems;
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_before: {item.FullName}");
            }
            output.WriteLine("");

            //просмотр панели быстрого доступа после добавления элементов
            mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(folder);
            //mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(file);
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_after_adding: {item.FullName}");
            }
            output.WriteLine("");

            //удаление добавленных элементов
            mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
            //mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(file);

            //просмотр панели быстрого доступа после удаленая добавленных элементов
            //bool file_fail = false;
            bool folder_fail = false;
            foreach (var item in quickAccessPanel)
            {
                if (item.FullName == @"C:\test_folder")
                {
                    folder_fail = true;
                }
                //if (item.FullName == @"C:\test_file.txt")
                //{
                //    file_fail = true;
                //}
                output.WriteLine($"Item in QAI_after_removing: {item.FullName}");
            }

            //проверка
            if (folder_fail == false)
            {
                Assert.True(true);
            }
            else
            {
                Assert.True(false, "folder not removed");
            }

            //проверка: оба элемента удалены
            //if (folder_fail == true && file_file == true)
            //{
            //    Assert.True(true);
            //}
            //else if (older_fail == false && file_fail == true)
            //{
            //    Assert.True(false, "folder removed, file - not");
            //}
            //else if (folder_fail == true && file_fail == false)
            //{
            //    Assert.True(false, "file removed, folder - not");
            //}
            //else
            //{
            //    Assert.True(false, "not removed not folder nor file");
            //}
        }
    }
}
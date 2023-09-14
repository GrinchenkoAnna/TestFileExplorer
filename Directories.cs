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
using System.IO;

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
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\copy_test_folder");

            //выбор папки на диске C
            var folder = new DirectoryViewModel(@"C:\copy_test_folder");
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
                Directory.Delete(folder.FullName, true);
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
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\cut_test_folder");

            //выбор папки на диске C
            var folder = new DirectoryViewModel(@"C:\cut_test_folder");
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
                Directory.Delete(folder.FullName, true);
                Assert.True(true);                
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
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\paste_test_folder");
            //File.Create(@"C:\paste_test_folder\paste_test_file.txt");

            //запись папки в "буфер"
            var buffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            foreach (var item in buffer)
            {
                buffer.Remove(item);
            }
            buffer.Add(@"C:\paste_test_folder");
            //buffer.Add(@"C:\paste_test_folder\paste_test_file.txt");
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
                if (item.FullName == @"C:\paste_test_folder — копия")
                {
                    folder_success = true;                    
                }
            }     

            ////проверка: файл тоже переместился
            //bool file_success = false;
            //var test_folder = new DirectoryViewModel(@"C:\paste_test_folder — копия");
            //mainWindowViewModel.CurrentDirectoryItem.Open(test_folder);
            //foreach (var item in directoriesAndFiles)
            //{
            //    output.WriteLine($"Item in DAF_2: {item.FullName}");
            //    if (item.FullName == @"C:\paste_test_folder — копия\paste_test_file.txt")
            //    {
            //        file_success = true;
            //    }
            //}

            //await Task.Delay(50);

            if (folder_success == true)
            {
                Directory.Delete(@"C:\paste_test_folder", true);
                Directory.Delete(@"C:\paste_test_folder — копия", true);
                Assert.True(true);
            }

            //if (folder_success == true && file_success == true)
            //{
            //    Directory.Delete(@"C:\paste_test_folder", true);
            //    Directory.Delete(@"C:\paste_test_folder — копия", true);
            //    Assert.True(true);
            //}
            //else
            //{
            //    if (folder_success == true && file_success == false)
            //    {
            //        Directory.Delete(@"C:\paste_test_folder", true);
            //        Directory.Delete(@"C:\paste_test_folder — копия", true);
            //        Assert.False(true, "pasted folder, not file");

            //    }
            //    else if (folder_success == false && file_success == true)
            //    {
            //        Directory.Delete(@"C:\paste_test_folder", true);
            //        Directory.Delete(@"C:\paste_test_folder — копия", true);
            //        Assert.False(true, "pasted file, not folder");
            //    }
            //    else
            //    {
            //        Directory.Delete(@"C:\paste_test_folder", true);
            //        Directory.Delete(@"C:\paste_test_folder — копия", true);
            //        Assert.False(true, "hothing had been pasted");
            //    }
            //}
        }

        [Fact]
        public async void DeleteTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание папок и файлов на диске C
            Directory.CreateDirectory(@"C:\delete_test_folder1");
            Directory.CreateDirectory(@"C:\delete_test_folder2");
            var folder2 = new DirectoryViewModel(@"C:\delete_test_folder2");
            //File.Create(@"C:\delete_test_file1.txt");
            //File.Create(@"C:\delete_test_file2.txt");
            //var file2 = new FileViewModel(@"C:\delete_test_file2");
            //File.SetAttributes(@"C:\delete_test_file1.txt", FileAttributes.Normal);
            //File.SetAttributes(@"C:\delete_test_file2.txt", FileAttributes.Normal);

            output.WriteLine(@"Chosed folder: C:\delete_test_folder1");
            output.WriteLine(@"Chosed folder: C:\delete_test_folder2");
            //output.WriteLine(@"Chosed file: C:\delete_test_file1.txt");
            //output.WriteLine(@"Chosed file: C:\delete_test_file2.txt");
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
            mainWindowViewModel.CurrentDirectoryItem.Delete(@"C:\delete_test_folder1");
            mainWindowViewModel.CurrentDirectoryItem.Delete(folder2.FullName);
            //mainWindowViewModel.CurrentDirectoryItem.Delete(@"C:\delete_test_file1");
            //mainWindowViewModel.CurrentDirectoryItem.Delete(file2.FullName);

            //проверка: в DirectoriesAndFiles нет удаленных папок и файлов
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
                if (item.FullName == @"C:\delete_test_folder1")
                {
                    Assert.True(false, "Из DAF не удалена delete_test_folder1");
                }
                if (item.FullName == folder2.FullName)
                {
                    Assert.True(false, "Из DAF не удалена delete_test_folder2");
                }
                //if (item.FullName == @"C:\delete_test_file1")
                //{
                //    Assert.True(false, "Из DAF не удален delete_test_file1.txt");
                //}
                //if (item.FullName == file2.FullName)
                //{
                //    Assert.True(false, "Из DAF не удален delete_test_file2.txt");
                //}
            }

            //проверка: файлы и папки удалены из системы
            if (Directory.Exists(@"C:\delete_test_folder1"))
            {
                Assert.True(false, "Не удалена delete_test_folder1");
            }
            if (Directory.Exists(@"C:\delete_test_folder2"))
            {
                Assert.True(false, "Не удалена delete_test_folder2");
            }
            //if (Directory.Exists(@"C:\delete_test_file1"))
            //{
            //    Assert.True(false, "Не удален delete_test_file1.txt");
            //}
            //if (Directory.Exists(@"C:\delete_test_file2"))
            //{
            //    Assert.True(false, "Не удален delete_test_file2.txt");
            //}            

            Assert.True(true);
        }

        [Fact]
        public async void AddToQuickAccessTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание элементов, которые будут добавлены на панель быстрого доступа
            Directory.CreateDirectory(@"C:\add_to_quick_access_test_folder");
            var folder = new DirectoryViewModel(@"C:\add_to_quick_access_test_folder");
            //File.Create(@"C:\add_to_quick_access_test_file.txt");
            //var file = new FileViewModel(@"C:\add_to_quick_access_test_file.txt");

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
                if (item.FullName == @"C:\add_to_quick_access_test_folder")
                {
                    folder_success = true;
                }
                //if (item.FullName == @"C:\add_to_quick_access_test_file.txt")
                //{
                //    file_success = true;
                //}
                output.WriteLine($"Item in QAI_after: {item.FullName}");
            }

            //проверка
            if (folder_success == true)
            {
                mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
                Directory.Delete(@"C:\add_to_quick_access_test_folder");
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
            //    Directory.Delete(@"C:\add_to_quick_access_test_folder");
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(file);
            //    File.Delete(@"C:\add_to_quick_access_test_file.txt");
            //    Assert.True(true);
            //}
            //else if (folder_success == true && file_success == false)
            //{
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
            //    Directory.Delete(@"C:\add_to_quick_access_test_folder");
            //    Assert.True(false, "folder added, file - not");
            //}
            //else if (folder_success == false && file_success == true)
            //{
            //    mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(file);
            //    File.Delete(@"C:\add_to_quick_access_test_file.txt");
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
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание элементов, которые будут добавлены на панель быстрого доступа
            Directory.CreateDirectory(@"C:\remove_from_quick_access_test_folder");
            var folder = new DirectoryViewModel(@"C:\remove_from_quick_access_test_folder");
            //File.Create(@"C:\remove_from_quick_access_test_file.txt");
            //var file = new FileViewModel(@"C:\remove_from_quick_access_test_file.txt");

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
                if (item.FullName == @"C:\remove_from_quick_access_test_folder")
                {
                    folder_fail = true;
                }
                //if (item.FullName == @"C:\remove_from_quick_access_test_file.txt")
                //{
                //    file_fail = true;
                //}
                output.WriteLine($"Item in QAI_after_removing: {item.FullName}");
            }

            //проверка
            if (folder_fail == false)
            {
                Directory.Delete(@"C:\remove_from_quick_access_test_folder", true);
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
            //else if (folder_fail == false && file_fail == true)
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

        [Fact]
        public async void AddToInformationPanelTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            // создание элемента, который будет добавлен на панель сведений
            var folder = new DirectoryViewModel(@"C:\add_to_information_panel_test_folder");
            var fake_folder = new DirectoryViewModel(@"C:\add_to_information_panel_fake_test_folder");

            //добавление элемента на панель сведений
            mainWindowViewModel.CurrentDirectoryItem.AddToInformation(fake_folder);

            //просмотр панели сведений до добавления элемента
            var informationPanel = mainWindowViewModel.CurrentDirectoryItem.InformationItems;
            foreach (var item in informationPanel)
            {
                output.WriteLine($"Item in IP_before: {item.FullName}");
            }

            //добавление элемента на панель сведений
            mainWindowViewModel.CurrentDirectoryItem.AddToInformation(folder);

            //просмотр панели сведений после добавления элемента
            bool folder_success = false;
            foreach (var item in informationPanel)
            {
                output.WriteLine($"Item in IP_after: {item.FullName}");
                if (item.FullName == @"C:\add_to_information_panel_test_folder")
                {
                    folder_success = true;
                }
                if (item.FullName == @"C:\add_to_information_panel_fake_test_folder")
                {
                    Assert.True(false, "the fake element remains in the collection");
                }
            }

            //проверка
            if (folder_success == true)
            {
                if (informationPanel.Count > 1)
                {
                    Assert.True(false, "in collection more than 1 element");
                }
                Assert.True(true);
            }
            else
            {
                Assert.True(false, "folder not added");
            }           
        }

        [Fact]
        public async void AddSortedItemsTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //создание нужных элементов
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\q");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\w");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\e");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\ea");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\r");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\t");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\y");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\ys");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\2");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\1");
            Directory.CreateDirectory(@"C:\add_sorted_items_test_folder\2f");

            //добавление их в DAF через функцию Open
            var folder = new DirectoryViewModel(@"C:\add_sorted_items_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //просмотр коллекции до сортировки
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //выполнение тестируемой функции
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\add_sorted_items_test_folder");
            var dirs = directoryInfo.EnumerateDirectories().OrderByDescending(d => d.Name);
            var files = directoryInfo.EnumerateFiles().OrderByDescending(f => f.Name);
            mainWindowViewModel.CurrentDirectoryItem.AddSortedItems(dirs, files);

            //просмотр коллекции после сортировки
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //результат
            if (directoriesAndFiles.First().Name == "ys"
                && directoriesAndFiles.Last().Name == "1")
            {
                Directory.Delete(@"C:\add_sorted_items_test_folder", true);
                Assert.True(true);
            }
            else
            {
                Assert.True(false);
            }            
        }


        [Fact]
        public async void SortByNameTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //указание нужного порядка сортировки
            MainWindow.asc = false;
            MainWindow.desc = true;            

            //создание нужных элементов
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\k");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\h");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\hg");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\ia");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\i");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\m");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\t");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\t5");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\8");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\0");
            Directory.CreateDirectory(@"C:\sort_by_name_test_folder\6j");

            //добавление их в DAF через функцию Open
            var folder = new DirectoryViewModel(@"C:\sort_by_name_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //просмотр коллекции до сортировки
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //выполнение тестируемой функции
            mainWindowViewModel.CurrentDirectoryItem.SortByName(folder.FullName);

            //просмотр коллекции после сортировки
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //результат
            if (directoriesAndFiles.First().Name == "t5"
                && directoriesAndFiles.Last().Name == "0")
            {
                Directory.Delete(@"C:\sort_by_name_test_folder", true);
                Assert.True(true);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async void SortByDateOfChangeTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //указание нужного порядка сортировки
            MainWindow.asc = true;
            MainWindow.desc = false;

            //создание нужных элементов
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder");
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder\Harry");
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder\Ron");
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder\Hermione");

            //добавление их в DAF через функцию Open
            var folder = new DirectoryViewModel(@"C:\sort_by_date_of_change_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //просмотр коллекции до сортировки
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //изменение элементов
            Directory.Move(@"C:\sort_by_date_of_change_test_folder\Ron",
                @"C:\sort_by_date_of_change_test_folder\Won-Won");
            Directory.Move(@"C:\sort_by_date_of_change_test_folder\Hermione",
                @"C:\sort_by_date_of_change_test_folder\HermioneGranger");
            Directory.Move(@"C:\sort_by_date_of_change_test_folder\Harry",
                @"C:\sort_by_date_of_change_test_folder\HarryPotter");            

            //выполнение тестируемой функции
            mainWindowViewModel.CurrentDirectoryItem.SortByDateOfChange(folder.FullName);

            //просмотр коллекции после сортировки
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //результат
            if (directoriesAndFiles.First().Name == "Won-Won"
                && directoriesAndFiles.Last().Name == "HarryPotter")
            {
                Directory.Delete(@"C:\sort_by_date_of_change_test_folder", true);
                Assert.True(true);
            }
            else
            {
                Directory.Delete(@"C:\sort_by_date_of_change_test_folder", true);
                Assert.True(false);
            }
        }
    }
}
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Xunit.Assert;

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

            //�����-������ ����� �� ����� C
            var folder = new DirectoryViewModel
                (
                Path.GetFullPath
                    (
                        Directory.GetDirectories(@"C:\").First()
                    )
                );
            output.WriteLine($"Chosed folder: {folder.FullName}");

            //�������� ���� � ��������
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

            //������� �������� ����� � ���������� ��������� � DirectoriesAndFiles
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //������� ��������� � ����������
            output.WriteLine($"Directories in folder: {Directory.GetDirectories(folder.FullName).Length}");
            output.WriteLine($"Files in folder: {Directory.GetFiles(folder.FullName).Length}");
            int count = Directory.GetDirectories(folder.FullName).Length
                + Directory.GetFiles(folder.FullName).Length;

            //���������� ��������� � DirectoriesAndFiles
            int result = 0;

            //����������� ���� ���������
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF: {item.FullName}");
            }
            var intersection1 = ItemsInDirectory.Intersect(directoriesAndFiles.Select(i => i.FullName)).ToList();
            
            foreach (var item in intersection1) { result++; }

            output.WriteLine($"Count = {count}");
            output.WriteLine($"Result = {result}");

            //���������� ��������� ������ ���������
            Assert.True(count == result);
        }

        [Fact]
        public async void CopyTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\copy_test_folder");

            //����� ����� �� ����� C
            var folder = new DirectoryViewModel(@"C:\copy_test_folder");
            output.WriteLine($"Chosed folder: {folder.FullName}");

            //����������� ���� �����
            var buffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            foreach (var item in buffer)
            {
                buffer.Remove(item);
            }
            mainWindowViewModel.CurrentDirectoryItem.Copy(folder);

            //��������: � ������ ���� ������� (���, ������� ��� ����������)  
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

            //����� ����� �� ����� C
            var folder = new DirectoryViewModel(@"C:\cut_test_folder");
            output.WriteLine($"Chosed folder: {folder.FullName}");

            //����������� ���� �����
            var buffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            foreach (var item in buffer)
            {
                buffer.Remove(item);
            }
            mainWindowViewModel.CurrentDirectoryItem.Cut(folder);

            //��������: � ������ ���� ������� (���, ������� ��� ����������)  
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
        public async void AddToItemBufferTest()
        {
            var mainWindowViewModel = new MainWindowViewModel(synchronizationHelper);

            //������� ��������� ��������
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Vladimir");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Moskow");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Novosibirsk");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Saint Petersburg");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Tomsk");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Chelyabinsk");
            Directory.CreateDirectory(@"C:\add_to_item_buffer_test_folder\Yakutsk");

            //���������� ����������� �������
            var itemBuffer = mainWindowViewModel.CurrentDirectoryItem.ItemBuffer;
            itemBuffer.Clear();
            mainWindowViewModel.CurrentDirectoryItem.AddToItemBuffer(@"C:\add_to_item_buffer_test_folder");

            //�������� ����������� ���������            
            int result = 0;
            foreach (var item in itemBuffer)
            {
                if (item == @"C:\add_to_item_buffer_test_folder") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Vladimir") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Moskow") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Novosibirsk") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Saint Petersburg") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Tomsk") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Chelyabinsk") { result++; }
                else if (item == @"C:\add_to_item_buffer_test_folder\Yakutsk") { result++; }
                else { result--; }
                output.WriteLine($"Item in ItemBuffer: {item}");
            }

            //�������� ��������� ��������
            Directory.Delete(@"C:\add_to_item_buffer_test_folder", true);

            //��������
            Assert.True(result == 8);
        }

        [Fact]
        public async void GetNameOfCopiedItem()
        {
            var mainWindowViewModel = new MainWindowViewModel(synchronizationHelper);

            //������� ��������� ��������
            Directory.CreateDirectory(@"C:\get_name_of_copied_item_test_folder");
            Directory.CreateDirectory(@"C:\get_name_of_copied_item_test_folder\folder");
            Directory.CreateDirectory(@"C:\get_name_of_copied_item_test_folder\folder � �����");
            Directory.CreateDirectory(@"C:\get_name_of_copied_item_test_folder\folder � ����� � �����");
            Directory.CreateDirectory(@"C:\get_name_of_copied_item_test_folder\folder1");

            //���������� ����������� �������
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\get_name_of_copied_item_test_folder");
            string result1 = mainWindowViewModel.CurrentDirectoryItem.GetNameOfCopiedItem(directoryInfo, "folder");
            output.WriteLine($"result1 = {result1}");
            string result2 = mainWindowViewModel.CurrentDirectoryItem.GetNameOfCopiedItem(directoryInfo, "folder1");
            output.WriteLine($"result2 = {result2}");
            string result3 = mainWindowViewModel.CurrentDirectoryItem.GetNameOfCopiedItem(directoryInfo, "folder2");
            output.WriteLine($"result3 = {result3}");

            //�������� ��������� ��������
            Directory.Delete(@"C:\get_name_of_copied_item_test_folder", true);

            //��������
            Assert.True(result1 == "folder � ����� � ����� � �����" 
                && result2 == "folder1 � �����"
                && result3 == "folder2");
        }

        [Fact]
        public async void PasteTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            Directory.CreateDirectory(@"C:\paste_test_folder");
            //File.Create(@"C:\paste_test_folder\paste_test_file.txt");

            //������ ����� � "�����"
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

            //���������� ������� ����� � ��������� ����������
            mainWindowViewModel.CurrentDirectoryItem.Paste(@"C:\");

            //�������� ���� ����������
            var destination_folder = new DirectoryViewModel(@"C:\");
            mainWindowViewModel.CurrentDirectoryItem.Open(destination_folder);

            //��������: ����������� ����� ��������� � ������ ����������
            bool folder_success = false;
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_1: {item.FullName}");
                if (item.FullName == @"C:\paste_test_folder � �����")
                {
                    folder_success = true;                    
                }
            }     

            ////��������: ���� ���� ������������
            //bool file_success = false;
            //var test_folder = new DirectoryViewModel(@"C:\paste_test_folder � �����");
            //mainWindowViewModel.CurrentDirectoryItem.Open(test_folder);
            //foreach (var item in directoriesAndFiles)
            //{
            //    output.WriteLine($"Item in DAF_2: {item.FullName}");
            //    if (item.FullName == @"C:\paste_test_folder � �����\paste_test_file.txt")
            //    {
            //        file_success = true;
            //    }
            //}

            if (folder_success == true)
            {
                Directory.Delete(@"C:\paste_test_folder", true);
                Directory.Delete(@"C:\paste_test_folder � �����", true);
                Assert.True(true);
            }

            //if (folder_success == true && file_success == true)
            //{
            //    Directory.Delete(@"C:\paste_test_folder", true);
            //    Directory.Delete(@"C:\paste_test_folder � �����", true);
            //    Assert.True(true);
            //}
            //else
            //{
            //    if (folder_success == true && file_success == false)
            //    {
            //        Directory.Delete(@"C:\paste_test_folder", true);
            //        Directory.Delete(@"C:\paste_test_folder � �����", true);
            //        Assert.False(true, "pasted folder, not file");

            //    }
            //    else if (folder_success == false && file_success == true)
            //    {
            //        Directory.Delete(@"C:\paste_test_folder", true);
            //        Directory.Delete(@"C:\paste_test_folder � �����", true);
            //        Assert.False(true, "pasted file, not folder");
            //    }
            //    else
            //    {
            //        Directory.Delete(@"C:\paste_test_folder", true);
            //        Directory.Delete(@"C:\paste_test_folder � �����", true);
            //        Assert.False(true, "hothing had been pasted");
            //    }
            //}
        }

        [Fact]
        public async void PasteSubItemsTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            Directory.CreateDirectory(@"C:\paste_subitems_test_folder1");
            Directory.CreateDirectory(@"C:\paste_subitems_test_folder1\123");
            Directory.CreateDirectory(@"C:\paste_subitems_test_folder1\456");
            Directory.CreateDirectory(@"C:\paste_subitems_test_folder1\789");
            Directory.CreateDirectory(@"C:\paste_subitems_test_folder1\000");
            Directory.CreateDirectory(@"C:\paste_subitems_test_folder2");

            //���������� ����������� �������
            mainWindowViewModel.CurrentDirectoryItem.ItemBuffer.Add(@"C:\paste_subitems_test_folder1");
            int result = 0;
           await mainWindowViewModel.CurrentDirectoryItem.PasteSubitems(@"C:\paste_subitems_test_folder1\123", @"C:\paste_subitems_test_folder2");
           await mainWindowViewModel.CurrentDirectoryItem.PasteSubitems(@"C:\paste_subitems_test_folder1\456", @"C:\paste_subitems_test_folder2");
           await mainWindowViewModel.CurrentDirectoryItem.PasteSubitems(@"C:\paste_subitems_test_folder1\789", @"C:\paste_subitems_test_folder2");
           await mainWindowViewModel.CurrentDirectoryItem.PasteSubitems(@"C:\paste_subitems_test_folder1\000", @"C:\paste_subitems_test_folder2");

            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\paste_subitems_test_folder2");
            var directories = directoryInfo.EnumerateDirectories();     
            if (!directories.Any())
            {
                output.WriteLine("test_folder2 is empty");
            }
            foreach (var directory in directories)
            {
                if (directory.Name == "123") { result++; }
                else if (directory.Name == "456") { result++; }
                else if (directory.Name == "789") { result++; }
                else if (directory.Name == "000") { result++; }
                else { result--; }
                output.WriteLine($"directory in test_folder2: {directory.Name}");
            }

            //�������� �������� ���������
            Directory.Delete(@"C:\paste_subitems_test_folder1", true);
            Directory.Delete(@"C:\paste_subitems_test_folder2", true);

            //��������
            output.WriteLine($"result = {result}");
            Assert.True(result == 4);
        }

        [Fact]
        public async void DeleteStrTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� ����� � ������ �� ����� C
            Directory.CreateDirectory(@"C:\delete_test_folder1");
            //File.Create(@"C:\delete_test_file1.txt");
            //File.Create(@"C:\delete_test_file2.txt");
            //var file2 = new FileViewModel(@"C:\delete_test_file2");
            //File.SetAttributes(@"C:\delete_test_file1.txt", FileAttributes.Normal);
            //File.SetAttributes(@"C:\delete_test_file2.txt", FileAttributes.Normal);

            output.WriteLine(@"Chosed folder: C:\delete_test_folder1");
            //output.WriteLine(@"Chosed file: C:\delete_test_file1.txt");
            //output.WriteLine(@"Chosed file: C:\delete_test_file2.txt");
            output.WriteLine("");

            //�������� ���������� C:\
            var main_folder = new DirectoryViewModel(@"C:\");
            mainWindowViewModel.CurrentDirectoryItem.Open(main_folder);

            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");                
            }
            output.WriteLine("");

            //�������� ����� � ������
            mainWindowViewModel.CurrentDirectoryItem.Delete(@"C:\delete_test_folder1");
            //mainWindowViewModel.CurrentDirectoryItem.Delete(@"C:\delete_test_file1");

            //��������: � DirectoriesAndFiles ��� ��������� ����� � ������
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
                if (item.FullName == @"C:\delete_test_folder1")
                {
                    Assert.True(false, "�� DAF �� ������� delete_test_folder1");
                }
                //if (item.FullName == @"C:\delete_test_file1")
                //{
                //    Assert.True(false, "�� DAF �� ������ delete_test_file1.txt");
                //}
                //if (item.FullName == file2.FullName)
                //{
                //    Assert.True(false, "�� DAF �� ������ delete_test_file2.txt");
                //}
            }

            //��������: ����� � ����� ������� �� �������
            if (Directory.Exists(@"C:\delete_test_folder1"))
            {
                Assert.True(false, "�� ������� delete_test_folder1");
            }
            //if (Directory.Exists(@"C:\delete_test_file1"))
            //{
            //    Assert.True(false, "�� ������ delete_test_file1.txt");
            //}
            //if (Directory.Exists(@"C:\delete_test_file2"))
            //{
            //    Assert.True(false, "�� ������ delete_test_file2.txt");
            //}            

            Assert.True(true);
        }
        
        [Fact]
        public async void DeleteObjTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� ����� � ������ �� ����� C
            Directory.CreateDirectory(@"C:\delete_test_folder2");
            var folder2 = new DirectoryViewModel(@"C:\delete_test_folder2");
            //File.Create(@"C:\delete_test_file1.txt");
            //File.Create(@"C:\delete_test_file2.txt");
            //var file2 = new FileViewModel(@"C:\delete_test_file2");
            //File.SetAttributes(@"C:\delete_test_file1.txt", FileAttributes.Normal);
            //File.SetAttributes(@"C:\delete_test_file2.txt", FileAttributes.Normal);

            output.WriteLine(@"Chosed folder: C:\delete_test_folder2");
            //output.WriteLine(@"Chosed file: C:\delete_test_file1.txt");
            //output.WriteLine(@"Chosed file: C:\delete_test_file2.txt");
            output.WriteLine("");

            //�������� ���������� C:\
            var main_folder = new DirectoryViewModel(@"C:\");
            mainWindowViewModel.CurrentDirectoryItem.Open(main_folder);

            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");                
            }
            output.WriteLine("");

            //�������� ����� � ������
            mainWindowViewModel.CurrentDirectoryItem.Delete(folder2.FullName);
            //mainWindowViewModel.CurrentDirectoryItem.Delete(file2.FullName);

            //��������: � DirectoriesAndFiles ��� ��������� ����� � ������
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
                if (item.FullName == folder2.FullName)
                {
                    Assert.True(false, "�� DAF �� ������� delete_test_folder2");
                }
                //if (item.FullName == @"C:\delete_test_file1")
                //{
                //    Assert.True(false, "�� DAF �� ������ delete_test_file1.txt");
                //}
                //if (item.FullName == file2.FullName)
                //{
                //    Assert.True(false, "�� DAF �� ������ delete_test_file2.txt");
                //}
            }

            //��������: ����� � ����� ������� �� �������
            if (Directory.Exists(@"C:\delete_test_folder2"))
            {
                Assert.True(false, "�� ������� delete_test_folder2");
            }
            //if (Directory.Exists(@"C:\delete_test_file1"))
            //{
            //    Assert.True(false, "�� ������ delete_test_file1.txt");
            //}
            //if (Directory.Exists(@"C:\delete_test_file2"))
            //{
            //    Assert.True(false, "�� ������ delete_test_file2.txt");
            //}            

            Assert.True(true);
        }

        [Fact]
        public async void AddToQuickAccessTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� ���������, ������� ����� ��������� �� ������ �������� �������
            Directory.CreateDirectory(@"C:\add_to_quick_access_test_folder");
            var folder = new DirectoryViewModel(@"C:\add_to_quick_access_test_folder");
            //File.Create(@"C:\add_to_quick_access_test_file.txt");
            //var file = new FileViewModel(@"C:\add_to_quick_access_test_file.txt");

            //�������� ������ �������� ������� �� ���������� ���������
            var quickAccessPanel = mainWindowViewModel.CurrentDirectoryItem.QuickAccessItems;
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_before: {item.FullName}");
            }
            output.WriteLine("");

            //���������� ��������� �� ������ �������� �������
            mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(folder);
            //mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(file);

            //�������� ������ �������� ������� ����� ���������� ���������
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

            //��������
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

            //��������: ��� �������� ���������
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

            //�������� ���������, ������� ����� ��������� �� ������ �������� �������
            Directory.CreateDirectory(@"C:\remove_from_quick_access_test_folder");
            var folder = new DirectoryViewModel(@"C:\remove_from_quick_access_test_folder");
            //File.Create(@"C:\remove_from_quick_access_test_file.txt");
            //var file = new FileViewModel(@"C:\remove_from_quick_access_test_file.txt");

            //�������� ������ �������� ������� �� ���������� ���������
            var quickAccessPanel = mainWindowViewModel.CurrentDirectoryItem.QuickAccessItems;
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_before: {item.FullName}");
            }
            output.WriteLine("");

            //�������� ������ �������� ������� ����� ���������� ���������
            mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(folder);
            //mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(file);
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_after_adding: {item.FullName}");
            }
            output.WriteLine("");

            //�������� ����������� ���������
            mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(folder);
            //mainWindowViewModel.CurrentDirectoryItem.RemoveFromQuickAccess(file);

            //�������� ������ �������� ������� ����� �������� ����������� ���������
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

            //��������
            if (folder_fail == false)
            {
                Directory.Delete(@"C:\remove_from_quick_access_test_folder", true);
                Assert.True(true);
            }
            else
            {
                Assert.True(false, "folder not removed");
            }

            //��������: ��� �������� �������
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
        public async void AddDefaultFoldersTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� ��������� "�� ���������", ������� ����� ��������� �� ������ �������� �������
            Directory.CreateDirectory(@"C:\add_default_folders_test_folder");
            var folder = new DirectoryViewModel(@"C:\add_default_folders_test_folder");


            //�������� ������ �������� ������� �� ���������� ���������
            var quickAccessPanel = mainWindowViewModel.CurrentDirectoryItem.QuickAccessItems;
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_before: {item.FullName}");
            }
            output.WriteLine("");

            mainWindowViewModel.CurrentDirectoryItem.AddDefaultFolders(folder.Name, "DefaultFolder_12345");

            //�������� ������ �������� ������� ����� ���������� ���������
            mainWindowViewModel.CurrentDirectoryItem.AddToQuickAccess(folder);
            bool folder_fail = true;
            foreach (var item in quickAccessPanel)
            {
                output.WriteLine($"Item in QAI_after_adding: {item.FullName}");
                if (item.FullName == folder.FullName) { folder_fail = false; }
            }
            output.WriteLine("");            

            Directory.Delete(@"C:\add_default_folders_test_folder", true);

            //��������
            if (folder_fail == false)
            {
                Assert.True(true);
            }
            else
            {
                Assert.True(false, "folder not added");
            }
        }

        [Fact]
        public async void AddToInformationPanelTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            // �������� ��������, ������� ����� �������� �� ������ ��������
            var folder = new DirectoryViewModel(@"C:\add_to_information_panel_test_folder");
            var fake_folder = new DirectoryViewModel(@"C:\add_to_information_panel_fake_test_folder");

            //���������� �������� �� ������ ��������
            mainWindowViewModel.CurrentDirectoryItem.AddToInformation(fake_folder);

            //�������� ������ �������� �� ���������� ��������
            var informationPanel = mainWindowViewModel.CurrentDirectoryItem.InformationItems;
            foreach (var item in informationPanel)
            {
                output.WriteLine($"Item in IP_before: {item.FullName}");
            }

            //���������� �������� �� ������ ��������
            mainWindowViewModel.CurrentDirectoryItem.AddToInformation(folder);

            //�������� ������ �������� ����� ���������� ��������
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

            //��������
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

            //�������� ������ ���������
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

            //���������� �� � DAF ����� ������� Open
            var folder = new DirectoryViewModel(@"C:\add_sorted_items_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //�������� ��������� �� ����������
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //���������� ����������� �������
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\add_sorted_items_test_folder");
            var dirs = directoryInfo.EnumerateDirectories().OrderByDescending(d => d.Name);
            var files = directoryInfo.EnumerateFiles().OrderByDescending(f => f.Name);
            mainWindowViewModel.CurrentDirectoryItem.AddSortedItems(dirs, files);

            //�������� ��������� ����� ����������
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //���������
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

            //�������� ������� ������� ����������
            MainWindow.asc = false;
            MainWindow.desc = true;            

            //�������� ������ ���������
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

            //���������� �� � DAF ����� ������� Open
            var folder = new DirectoryViewModel(@"C:\sort_by_name_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //�������� ��������� �� ����������
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //���������� ����������� �������
            mainWindowViewModel.CurrentDirectoryItem.SortByName(folder.FullName);

            //�������� ��������� ����� ����������
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //���������
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

            //�������� ������� ������� ����������
            MainWindow.asc = true;
            MainWindow.desc = false;

            //�������� ������ ���������
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder");
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder\Harry");
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder\Ron");
            Directory.CreateDirectory(@"C:\sort_by_date_of_change_test_folder\Hermione");

            //���������� �� � DAF ����� ������� Open
            var folder = new DirectoryViewModel(@"C:\sort_by_date_of_change_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //�������� ��������� �� ����������
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //��������� ���������
            Directory.SetLastWriteTime(@"C:\sort_by_date_of_change_test_folder\Harry", new DateTime(1980,7,31));
            Directory.SetLastWriteTime(@"C:\sort_by_date_of_change_test_folder\Ron", new DateTime(1980,3,1));
            Directory.SetLastWriteTime(@"C:\sort_by_date_of_change_test_folder\Hermione", new DateTime(1979,9,19));
                     

            //���������� ����������� �������
            mainWindowViewModel.CurrentDirectoryItem.SortByDateOfChange(folder.FullName);

            //�������� ��������� ����� ����������
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //���������
            if (directoriesAndFiles.First().Name == "Hermione"
                && directoriesAndFiles.Last().Name == "Harry")
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
        
        [Fact]
        public async void RefreshSortTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� ������� ������� ����������
            MainWindow.asc = true;
            MainWindow.desc = false;

            //�������� ������ ���������
            Directory.CreateDirectory(@"C:\refresh_sort_test_folder");
            Directory.CreateDirectory(@"C:\refresh_sort_test_folder\Ginny"); 
            Directory.CreateDirectory(@"C:\refresh_sort_test_folder\Neville"); 
            Directory.CreateDirectory(@"C:\refresh_sort_test_folder\Luna"); 

            //���������� �� � DAF ����� ������� Open
            var folder = new DirectoryViewModel(@"C:\refresh_sort_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //�������� ��������� �� ����������
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //��������� ���������
            Directory.SetLastWriteTime(@"C:\refresh_sort_test_folder\Ginny", new DateTime(1981,8,11));
            Directory.SetLastWriteTime(@"C:\refresh_sort_test_folder\Neville", new DateTime(1980,7,30));
            Directory.SetLastWriteTime(@"C:\refresh_sort_test_folder\Luna", new DateTime(1981,2,13));


            //���������� ����������� �������
            bool result1 = false;
            bool result2 = false;
            /*----------------------------------------------------------------------------------*/
            mainWindowViewModel.CurrentDirectoryItem.theLastSort = "dateOfChange"; //��������� ������� ���������� - �� ���� ���������
            output.WriteLine($"sort method: {mainWindowViewModel.CurrentDirectoryItem.theLastSort}");
            mainWindowViewModel.CurrentDirectoryItem.RefreshSort(@"C:\refresh_sort_test_folder");
            if (directoriesAndFiles.First().Name == "Neville" && directoriesAndFiles.Last().Name == "Ginny")
            {
                result1 = true;
            }            
            foreach (var item in directoriesAndFiles) //�������� ��������� ����� ����������
            {
                output.WriteLine($"Item in DAF_after1: {item.FullName}");
            }
            output.WriteLine("");
            /*----------------------------------------------------------------------------------*/
            mainWindowViewModel.CurrentDirectoryItem.theLastSort = "name"; //��������� ������ ���������� - �� �����
            output.WriteLine($"sort method: {mainWindowViewModel.CurrentDirectoryItem.theLastSort}");
            mainWindowViewModel.CurrentDirectoryItem.RefreshSort(@"C:\refresh_sort_test_folder");
            if (directoriesAndFiles.First().Name == "Ginny" && directoriesAndFiles.Last().Name == "Neville")
            {
                result2 = true;
            }            
            foreach (var item in directoriesAndFiles) //�������� ��������� ����� ����������
            {
                output.WriteLine($"Item in DAF_after2: {item.FullName}");
            }
            /*----------------------------------------------------------------------------------*/

            //���������
            if (result1 && result2)
            {
                Directory.Delete(@"C:\refresh_sort_test_folder", true);
                Assert.True(true);
            }
            else
            {
                Directory.Delete(@"C:\refresh_sort_test_folder", true);
                Assert.True(false);
            }
        }

        [Fact]
        public async void CreateNewFolderTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� ������ ���������
            Directory.CreateDirectory(@"C:\create_new_folder_test_folder");

            //���������� �� � DAF ����� ������� Open
            var folder = new DirectoryViewModel(@"C:\create_new_folder_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //�������� ��������� �� �������� �����
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            if (directoriesAndFiles.Count == 0)
            {
                output.WriteLine("There is no elements in DAF");
            }
            else
            {
                foreach (var item in directoriesAndFiles)
                {
                    output.WriteLine($"Item in DAF_before: {item.FullName}");
                }
            }            
            output.WriteLine("");

            //�������� ����� ����� � C:\create_new_folder_test_folder
            mainWindowViewModel.CurrentDirectoryItem.CreateNewFolder(@"C:\create_new_folder_test_folder");
            mainWindowViewModel.CurrentDirectoryItem.CreateNewFolder(@"C:\create_new_folder_test_folder");

            //���������� DAF ����� ������� Open
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);

            //�������� ��������� �� �������� �����
            bool newfolder_success = false;
            bool newfolder1_success = false;
            foreach (var item in directoriesAndFiles)
            {
                if (item.Name == "����� �����")
                {
                    newfolder_success = true;
                }
                if (item.Name == "����� �����(1)")
                {
                    newfolder1_success = true;
                }
                output.WriteLine($"Item in DAF_after: {item.FullName}");
            }

            //������� ����������
            Directory.Delete(@"C:\create_new_folder_test_folder", true);

            //���������
            if (newfolder_success && newfolder1_success)
            {
                Assert.True(true);
            }
            else if (newfolder_success && !newfolder1_success)
            {
                Assert.True(false, "not created ����� �����(2)");
            }
            else if (!newfolder_success && newfolder1_success)
            {
                Assert.True(false, "not created ����� �����");
            }
            else
            {
                Assert.True(false, "not created any folder");
            }
        }

        [Fact]
        public async void GetNameOfNewFolderTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            if (!Directory.Exists(@"C:\get_name_of_new_folder_test_folder"))
            {
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder");
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder\����� �����");
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder\����� �����(1)");
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder\����� �����(2)");
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder\����� ����� - 1");
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder\����� ����� (1)");
                Directory.CreateDirectory(@"C:\get_name_of_new_folder_test_folder\����� ����� (3)");
            }

            //���������� ������ ������� �� ���������� ��������
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\get_name_of_new_folder_test_folder");            
            string result1 = mainWindowViewModel.CurrentDirectoryItem.GetNameOfNewFolder(directoryInfo, "����� �����");
            string result2 = mainWindowViewModel.CurrentDirectoryItem.GetNameOfNewFolder(directoryInfo, "����� ����� - 1");
            string result3 = mainWindowViewModel.CurrentDirectoryItem.GetNameOfNewFolder(directoryInfo, "����� �����");
            output.WriteLine($"result1 = {result1},\nresult2 = {result2},\nresult3 = {result3}");

            //�������� �������� ���������
            Directory.Delete(@"C:\get_name_of_new_folder_test_folder", true);

            //�������� ������ �������
            string sample1 = "����� �����(1)";
            string sample2 = "����� ����� - 1(1)";
            string sample3 = "����� �����";
            Assert.True((sample1 == result1) && (sample2 == result2) && (sample3 == result3));            
        }

        [Fact]
        public async void OpenTreeTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //��������� ��� ���������, ������� ������ ���� � ������
            ObservableCollection<string> ItemsMustBeInTree = new ObservableCollection<string>();
            ObservableCollection<FileEntityViewModel> LogicalDrives = new ObservableCollection<FileEntityViewModel>();
            int count = 0;
            int result = 0;

            //���������� � DirectoriesAndFiles ��������� ����� ������� OpenDirectory ��� ���������� ���������
            mainWindowViewModel.CurrentDirectoryItem.OpenDirectory();
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            //������� ����������� ��������� ��� ���������� ������
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Must be in tree: {item.FullName}");
                ItemsMustBeInTree.Add(item.FullName);
                LogicalDrives.Add(item);
                count++;
            }
            //� ������ ����� (�������� ��������� ��� ������) ������������ ������� � ���������� ���������
            foreach (var logical_drive in LogicalDrives) 
            {
                mainWindowViewModel.CurrentDirectoryItem.Open(logical_drive);
                foreach (var item in directoriesAndFiles)
                {
                    output.WriteLine($"Must be in tree: {item.FullName}");
                    ItemsMustBeInTree.Add(item.FullName);
                    count++;
                }
            }
            output.WriteLine("");

            //����������� ���������� ������
            mainWindowViewModel.CurrentDirectoryItem.OpenTree();
            var tree = mainWindowViewModel.CurrentDirectoryItem.TreeItems;
            output.WriteLine($"items in tree: {tree.Count}");
            foreach (var root in tree)
            {
                output.WriteLine($"Is in tree (root): {root.FullName}");
                foreach (var i in root.Subfolders)
                {
                    output.WriteLine($"  Is in tree: {i.FullName}");
                }
            }
            output.WriteLine("");

            //��������� ����������� ���������
            //������� ������������ ���� ���������� �����            
            var intersection1 = ItemsMustBeInTree.Intersect<string>(tree.Select(i => i.FullName)).ToList();
            foreach (var item in intersection1) 
            {
                output.WriteLine($"Intersection (root): {item}");
                result++; 
            }
            //����� ������������ ������������� ���������� ������
            foreach (var item in tree)
            {
                var intersection2 = ItemsMustBeInTree.Intersect<string>(item.Subfolders.Select(i => i.FullName)).ToList();
                foreach (var i in intersection2)
                {
                    output.WriteLine($"Intersection: {i}");
                    result++;
                }
            }
            output.WriteLine("");

            output.WriteLine($"Count = {count}");
            output.WriteLine($"Result = {result}");

            //���������� ��������� ������ ���������
            Assert.True(count == result);
        }

        [Fact]
        public async void GetSubfoldersTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            if (!Directory.Exists(@"C:\get_subfolders_test_folder"))
            {
                Directory.CreateDirectory(@"C:\get_subfolders_test_folder");
                Directory.CreateDirectory(@"C:\get_subfolders_test_folder\folder_1");
                Directory.CreateDirectory(@"C:\get_subfolders_test_folder\folder_2");
                Directory.CreateDirectory(@"C:\get_subfolders_test_folder\folder_3");
                //File.Create(@"C:\get_subfolders_test_folder\file_1.txt");
                //File.Create(@"C:\get_subfolders_test_folder\file_2.txt");
                //File.Create(@"C:\get_subfolders_test_folder\file_3.c");
                //File.Create(@"C:\get_subfolders_test_folder\file_4.cpp");
                //File.Create(@"C:\get_subfolders_test_folder\file_5.cs");
            }

            //��������� ��� �������� ����������� ��������� �������
            ObservableCollection<FileEntityViewModel> collection = new ObservableCollection<FileEntityViewModel>();
            collection = mainWindowViewModel.CurrentDirectoryItem.GetSubfolders(@"C:\get_subfolders_test_folder");

            //�������� ��������� ������ ����������� ����� ������� Open
            var test_folder = new DirectoryViewModel(@"C:\get_subfolders_test_folder");            
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            mainWindowViewModel.CurrentDirectoryItem.Open(test_folder);            

            //���������� FullName �������� �������� ���������
            List<string> path_collection = new List<string>(); 
            foreach (var item in collection)
            {
                path_collection.Add(item.FullName);
            }

            //����� path_collection
            foreach (var item in path_collection)
            {
                output.WriteLine($"Subitem: {item}");
            }
            output.WriteLine("");
            
            //�������� �������� ���������
            Directory.Delete(@"C:\get_subfolders_test_folder", true);

            //��������
            var intersection = path_collection.Intersect<string>(directoriesAndFiles.Select(item => item.FullName)).ToList();
            foreach (var item in intersection)
            {
                output.WriteLine($"Intersection: {item}");
            }
            
            Assert.True(intersection.Count == 3);            
        }

        [Fact]
        public async void OnMoveBackTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            if (!Directory.Exists(@"C:\on_move_test_folder"))
            {
                Directory.CreateDirectory(@"C:\on_move_test_folder");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_2");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_3");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1\folder_11");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1\folder_12");
            }

            //�������� �������� ����� ��� �����
            var folder = new DirectoryViewModel(@"C:\on_move_test_folder");
            var folder_1 = new DirectoryViewModel(@"C:\on_move_test_folder\folder_1");
            
            //�������� �������: ���������������� �������� � ���������� ������ ���������
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);
            mainWindowViewModel.CurrentDirectoryItem.Open(folder_1);
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            output.WriteLine($"Current Directory: {mainWindowViewModel.CurrentDirectoryItem.FilePath}");
            mainWindowViewModel.CurrentDirectoryItem.OnMoveBack(folder_1.FullName);
            output.WriteLine($"Current Directory: {mainWindowViewModel.CurrentDirectoryItem.FilePath}");

            int result = 0;

            //��������: ����� �������� ���� � ������������ ��������� (������ �� ����� �������)
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF: {item.FullName}");
                if (item.FullName == @"C:\on_move_test_folder\folder_1") { result++; }
                if (item.FullName == @"C:\on_move_test_folder\folder_2") { result++; }
                if (item.FullName == @"C:\on_move_test_folder\folder_3") { result++; }
            }
            output.WriteLine($"Result = {result}");

            //�������� ��������� ��� ������ ���������
            Directory.Delete(@"C:\on_move_test_folder", true);

            //���������
            Assert.True(result == 3);
        }

        [Fact]
        public async void OnMoveForwardTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            if (!Directory.Exists(@"C:\on_move_test_folder"))
            {
                Directory.CreateDirectory(@"C:\on_move_test_folder");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_2");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_3");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1\folder_11");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1\folder_12");
            }

            //�������� �������� ����� ��� �����
            var folder = new DirectoryViewModel(@"C:\on_move_test_folder");
            var folder_1 = new DirectoryViewModel(@"C:\on_move_test_folder\folder_1");

            //�������� �������: ���������������� �������� � ���������� ������ ���������
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);
            mainWindowViewModel.CurrentDirectoryItem.Open(folder_1);
            mainWindowViewModel.CurrentDirectoryItem.OnMoveBack(folder_1.FullName);
            output.WriteLine($"Current Directory: {mainWindowViewModel.CurrentDirectoryItem.FilePath}");
            mainWindowViewModel.CurrentDirectoryItem.OnMoveForward(folder_1.FullName);
            output.WriteLine($"Current Directory: {mainWindowViewModel.CurrentDirectoryItem.FilePath}");

            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;

            int result = 0;

            //��������: ����� �������� ���� � ������������ ��������� (������ �� ����� �������)
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF: {item.FullName}");
                if (item.FullName == @"C:\on_move_test_folder\folder_1\folder_11") { result++; }
                if (item.FullName == @"C:\on_move_test_folder\folder_1\folder_12") { result++; }
            }
            output.WriteLine($"Result = {result}");

            //�������� ��������� ��� ������ ���������
            Directory.Delete(@"C:\on_move_test_folder", true);

            //���������
            Assert.True(result == 2);
        }

        [Fact]
        public async void OnMoveUpTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            if (!Directory.Exists(@"C:\on_move_test_folder"))
            {
                Directory.CreateDirectory(@"C:\on_move_test_folder");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_2");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_3");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1\folder_11");
                Directory.CreateDirectory(@"C:\on_move_test_folder\folder_1\folder_12");
            }

            //�������� �������� ����� ��� �����
            var folder = new DirectoryViewModel(@"C:\on_move_test_folder");
            var folder_1 = new DirectoryViewModel(@"C:\on_move_test_folder\folder_1");
            var folder_12 = new DirectoryViewModel(@"C:\on_move_test_folder\folder_1\folder_12");

            //�������� �������: ���������������� �������� � ���������� ������ ���������
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);
            mainWindowViewModel.CurrentDirectoryItem.Open(folder_1);
            mainWindowViewModel.CurrentDirectoryItem.Open(folder_12);
            output.WriteLine($"Current Directory: {mainWindowViewModel.CurrentDirectoryItem.FilePath}");
            mainWindowViewModel.CurrentDirectoryItem.OnMoveUp(folder_12.FullName);
            output.WriteLine($"Current Directory: {mainWindowViewModel.CurrentDirectoryItem.FilePath}");

            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;

            int result = 0;

            //��������: ����� �������� ���� � ������������ ��������� (������ �� ����� �������)
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF: {item.FullName}");
                if (item.FullName == @"C:\on_move_test_folder\folder_1\folder_11") { result++; }
                if (item.FullName == @"C:\on_move_test_folder\folder_1\folder_12") { result++; }
            }
            output.WriteLine($"Result = {result}");

            //�������� ��������� ��� ������ ���������
            Directory.Delete(@"C:\on_move_test_folder", true);

            //���������
            Assert.True(result == 2);
        }

        [Fact]
        public async void SearchTest()
        {
            var mainWindowViewModel = new FileExplorer.ViewModels.MainWindowViewModel(synchronizationHelper);

            //�������� �������� ���������
            if (!Directory.Exists(@"C:\search_test_folder"))
            {
                Directory.CreateDirectory(@"C:\search_test_folder");
                Directory.CreateDirectory(@"C:\search_test_folder\NancyDrew");
                Directory.CreateDirectory(@"C:\search_test_folder\BessMarvin");
                Directory.CreateDirectory(@"C:\search_test_folder\GeorgeFan");
                Directory.CreateDirectory(@"C:\search_test_folder\4 8 15 16 23 42");
                Directory.CreateDirectory(@"C:\search_test_folder\Ut6QIanQ");
            }            

            //�������� �������� ����� ��� �����
            var folder = new DirectoryViewModel(@"C:\search_test_folder");
            var folder_nancy = new DirectoryViewModel(@"C:\search_test_folder\NancyDrew");
            var folder_bess = new DirectoryViewModel(@"C:\search_test_folder\BessMarvin");
            var folder_jess = new DirectoryViewModel(@"C:\search_test_folder\GeorgeFan");
            var folder_lost = new DirectoryViewModel(@"C:\search_test_folder\4 8 15 16 23 42");
            var folder_password = new DirectoryViewModel(@"C:\search_test_folder\Ut6QIanQ");

            //�������� ����������, � ������� ����� ����������� �����
            //(���������� � DAF ��������� ����� ������� Open)
            mainWindowViewModel.CurrentDirectoryItem.Open(folder);
            var directoriesAndFiles = mainWindowViewModel.CurrentDirectoryItem.DirectoriesAndFiles;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_before: {item.FullName}");
            }
            output.WriteLine("");

            //����� ����������� ������� (��������� DAF ����� ���)
            mainWindowViewModel.CurrentDirectoryItem.Search("an");

            //��������: ����� �������� ���� � ������������ ��������� (������ �� ����� �������)
            int result = 0;
            foreach (var item in directoriesAndFiles)
            {
                output.WriteLine($"Item in DAF_after: {item.FullName}");
                if (item.FullName == @"C:\search_test_folder\NancyDrew") { result++; }
                if (item.FullName == @"C:\search_test_folder\GeorgeFan") { result++; }
                if (item.FullName == @"C:\search_test_folder\Ut6QIanQ") { result++; }
            }
            output.WriteLine("");
            output.WriteLine($"Result = {result}");

            //�������� ��������� ��� ������ ���������
            Directory.Delete(@"C:\search_test_folder", true);

            //���������
            Assert.True(result == 3);
        }
    }
}
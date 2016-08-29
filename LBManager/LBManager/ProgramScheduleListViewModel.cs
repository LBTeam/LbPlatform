using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LBManager
{
    public class ProgramScheduleListViewModel:BindableBase
    {
        private FileSystemWatcher _fileWatcher;
        public ProgramScheduleListViewModel()
        {
            FetchProgramSchedules();
            InitializeFileWatcher();

        }

        private void FetchProgramSchedules(string directoryPath)
        {
            DirectoryInfo folder = new DirectoryInfo(directoryPath);

            foreach (FileInfo fileInfo in folder.GetFiles("*.playprog"))
            {
                Console.WriteLine(file.FullName);
            }
        }

        private void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher();
            _fileWatcher.Path = "";
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _fileWatcher.Filter = "";
            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.EnableRaisingEvents = true;

            _fileWatcher.Created += _fileWatcher_Created;
            _fileWatcher.Changed += _fileWatcher_Changed;
            _fileWatcher.Renamed += _fileWatcher_Renamed;
            _fileWatcher.Deleted += _fileWatcher_Deleted;
        }

        private void _fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

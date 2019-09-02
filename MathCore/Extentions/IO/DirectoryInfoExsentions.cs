﻿using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using DST = System.Diagnostics.DebuggerStepThroughAttribute;
using System.Diagnostics.Contracts;
using MathCore.Annotations;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    /// <summary>Класс методов-расширений для объектов класса System.IO.DirectoryInfo</summary>
    public static class DirectoryInfoExsentions
    {  
        [NotNull]
        public static Process ShowInFileExplorer([NotNull] this FileSystemInfo dir) => Process.Start("explorer", $"/select,\"{dir.FullName}\"") ?? throw new InvalidOperationException();

        [NotNull]
        public static Process OpenInFileExplorer([NotNull] this DirectoryInfo dir) => Process.Start("explorer", dir.FullName) ?? throw new InvalidOperationException();

        [CanBeNull]
        public static string GetRelativePosition([NotNull] this DirectoryInfo current, [NotNull] DirectoryInfo other)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            if (other is null) throw new ArgumentNullException(nameof(other));
            return GetRelativePosition(current.FullName, other.FullName);
        }

        [CanBeNull]
        public static string GetRelativePosition([NotNull] string current, [NotNull] string other)
        {
            if (current is null) throw new ArgumentNullException(nameof(current));
            if (other is null) throw new ArgumentNullException(nameof(other));

            const StringComparison str_cmp = StringComparison.InvariantCultureIgnoreCase;
            return !string.Equals(Path.GetPathRoot(current), Path.GetPathRoot(other), str_cmp)
                ? null
                : current.StartsWith(other, str_cmp)
                    ? current.Remove(0, other.Length)
                    : other.StartsWith(current, str_cmp)
                        ? other.Remove(0, current.Length)
                        : null;
        }

        public static bool IsSubDirectoryOf([CanBeNull] this DirectoryInfo target, [CanBeNull] DirectoryInfo parent) => !(target is null || parent is null) && target.FullName.StartsWith(parent.FullName, StringComparison.InvariantCultureIgnoreCase);

        public static FileInfo CreateFileInfo(this DirectoryInfo directory, string FileName)
        {
            FileName = FileName.Replace(':', '.');
            return new FileInfo(Path.Combine(directory.FullName, FileName));
        }

        /// <summary>Определить число всех вложенных файлов</summary>
        /// <param name="Directory">Исследуемая дирректория</param>
        /// <returns>Число файлов во всех вложенных поддиректориях</returns>
        [DST]
        public static long GetFilesCount(this DirectoryInfo Directory)
        {
            Contract.Requires(Directory != null);
            Contract.Ensures(Contract.Result<long>() >= 0);

            var result = 0L;
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(Directory);
            do
            {
                var info = queue.Dequeue();
                result += info.GetFiles().Length;
                info.GetDirectories().Foreach(queue.Enqueue);
            } while(queue.Count != 0);
            return result;
        }

        /// <summary>Определить объём всех вложенных файлов включая поддиректории</summary>
        /// <param name="Directory">Исследуемая дирректория</param>
        /// <returns>Число байт всех вложенных файлов</returns>
        [DST]
        public static long GetSize(this DirectoryInfo Directory)
        {
            Contract.Requires(Directory != null);
            Contract.Ensures(Contract.Result<long>() >= 0);

            var result = 0L;
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(Directory);
            do
            {
                var info = queue.Dequeue();
                result += info.GetFiles().Sum(f => f.Length);
                info.GetDirectories().Foreach(queue.Enqueue);
            } while(queue.Count != 0);
            return result;
        }

        /// <summary>Определить число поддерикторий</summary>
        /// <param name="Directory">Исследуемая дирректория</param>
        /// <returns>Число элементов в дереве поддерикторий</returns>
        [DST]
        public static long GetSubdirectoriesCount(this DirectoryInfo Directory)
        {
            Contract.Requires(Directory != null);
            Contract.Ensures(Contract.Result<long>() >= 0);

            var num = 0L;
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(Directory);
            do
            {
                var directories = queue.Dequeue().GetDirectories();
                num += directories.Length;
                directories.Foreach(queue.Enqueue);
            } while(queue.Count != 0);
            return num;
        }

        /// <summary>Проверить - является ли дирректория пустой</summary>
        /// <param name="Directory">Проверяемая дирректория</param>
        /// <returns>Истина, если дирректория пуста</returns>
        [DST]
        public static bool IsEmpty(this DirectoryInfo Directory)
        {
            Contract.Requires(Directory != null);

            return Directory.GetDirectories().Length == 0 && (Directory.GetFiles().Length == 0);
        }

        /// <summary>Получить объект наблюдения за дирректорией</summary>
        /// <param name="directory">Наблюдаемая дирректория</param>
        /// <param name="filter">Фильтр файлов</param>
        /// <returns>Объект наблюдатель</returns>
        [DST]
        public static FileSystemWatcher GetWacher(this DirectoryInfo directory, string filter = null)
        {
            Contract.Requires(directory != null);
            Contract.Ensures(Contract.Result<FileSystemWatcher>() != null);

            return string.IsNullOrEmpty(filter)
                ? new FileSystemWatcher(directory.FullName)
                : new FileSystemWatcher(directory.FullName, filter);
        }

        [NotNull]
        public static FileSystemWatcher GetWatcher([NotNull] this DirectoryInfo directory, [CanBeNull] string filter, [CanBeNull] Action<FileSystemWatcher> initializer = null)
        {
            var watcher = string.IsNullOrWhiteSpace(filter)
                        ? new FileSystemWatcher(directory.FullName)
                        : new FileSystemWatcher(directory.FullName, filter);
            initializer?.Invoke(watcher);
            return watcher;
        }

        [NotNull]
        public static FileSystemWatcher GetWatcher([NotNull] this DirectoryInfo directory, [CanBeNull] Action<FileSystemWatcher> initializer = null) => directory.GetWatcher(null, initializer);

        public static bool ContainsFile([NotNull] this DirectoryInfo directory, [NotNull] string file) => File.Exists(Path.Combine(directory.FullName, file));

        public static bool ContainsFileMask([NotNull] this DirectoryInfo directory, [NotNull] string mask) => directory.EnumerateFiles(mask).Any();

        public static bool IsSubdirectoryOf([NotNull] this DirectoryInfo directory, [NotNull] DirectoryInfo parent) => directory.FullName.StartsWith(parent.FullName, StringComparison.InvariantCultureIgnoreCase);
        public static bool IsParentOf([NotNull] this DirectoryInfo parent, [NotNull] DirectoryInfo directory) => directory.IsSubdirectoryOf(parent);

        [NotNull, ItemNotNull] public static IEnumerable<FileInfo> FindFiles([NotNull] this DirectoryInfo dir, [NotNull] string mask) => dir.EnumerateDirectories().SelectMany(d => d.FindFiles(mask)).InsertBefore(dir.EnumerateFiles(mask));

        /// <summary>Получение поддиректории по заданному пути. Если поддиректория отсутствует, то создать новую</summary>
        /// <param name="ParentDirectory">Родительская директория</param>
        /// <param name="SubDirectoryPath">Относительный путь к поддиректории</param>
        /// <returns>Поддиректория</returns>
        [NotNull]
        public static DirectoryInfo SubDirectoryOrCreate([NotNull] this DirectoryInfo ParentDirectory, [NotNull] string SubDirectoryPath)
        {
            if (ParentDirectory is null) throw new ArgumentNullException(nameof(ParentDirectory));
            if (SubDirectoryPath is null) throw new ArgumentNullException(nameof(SubDirectoryPath));
            if (string.IsNullOrWhiteSpace(SubDirectoryPath)) throw new ArgumentException("Не указан путь дочернего каталога", nameof(SubDirectoryPath));

            var sub_dir_path = Path.Combine(ParentDirectory.FullName, SubDirectoryPath);
            var sub_dir = new DirectoryInfo(sub_dir_path);
            if (sub_dir.Exists) return sub_dir;
            sub_dir.Create();
            sub_dir.Refresh();
            return sub_dir;
        }

        /// <summary>Формирование информации о поддиректории, заданной своим именем, либо относительным путём</summary>
        /// <param name="Directory">Корнневая директория</param><param name="SubDirectoryPath">Путь к поддиректории</param>
        /// <exception cref="ArgumentNullException">Если указана пустая ссылка на <paramref name="Directory"/></exception>
        /// <exception cref="ArgumentNullException">Если указана пустая ссылка на <paramref name="SubDirectoryPath"/></exception>
        /// <returns>Информация о поддиректории</returns>
        [NotNull]
        public static DirectoryInfo SubDirectory([NotNull] this DirectoryInfo Directory, [NotNull] string SubDirectoryPath)
        {
            if (Directory is null) throw new ArgumentNullException(nameof(Directory));
            if (SubDirectoryPath is null) throw new ArgumentNullException(nameof(SubDirectoryPath));
            return string.IsNullOrEmpty(SubDirectoryPath) ? Directory : new DirectoryInfo(Path.Combine(Directory.FullName, SubDirectoryPath));
        }
    }
}
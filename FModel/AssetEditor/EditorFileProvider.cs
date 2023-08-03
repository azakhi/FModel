using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Versions;

namespace FModel.AssetEditor
{
    public class EditorFileProvider : DefaultFileProvider
    {
        public static readonly string LocalFilesDirectoryName = "<Local files..>";

        private DirectoryInfo _workingDirectory;
        private Dictionary<string, OsGameFile> _osFiles;

        public IReadOnlyDictionary<string, OsGameFile> OsFiles => _osFiles;

        public int FileCount { get; private set; }
        public long Length { get; private set; }

        public EditorFileProvider(string directory, SearchOption searchOption, bool isCaseInsensitive = false, VersionContainer? versions = null)
            : base(directory, searchOption, isCaseInsensitive, versions)
        {
            _workingDirectory = new DirectoryInfo(directory);
            _osFiles = new Dictionary<string, OsGameFile>();
        }

        public override GameFile this[string path] => _osFiles.TryGetValue(path, out var file) ? file : base[path];

        public override async Task<IPackage> LoadPackageAsync(GameFile file)
        {
            if (file is not OsGameFile || file.Extension != "uasset")
            {
                return await base.LoadPackageAsync(file);
            }

            if (!file.IsUE4Package)
                throw new ArgumentException("File must be a package to be a loaded as one", nameof(file));

            OsFiles.TryGetValue(file.PathWithoutExtension + ".uexp", out var uexpFile);
            OsFiles.TryGetValue(file.PathWithoutExtension + ".ubulk", out var ubulkFile);
            OsFiles.TryGetValue(file.PathWithoutExtension + ".uptnl", out var uptnlFile);
            var uassetTask = file.CreateReaderAsync();
            var uexpTask = uexpFile?.CreateReaderAsync();
            var ubulkTask = ubulkFile?.CreateReaderAsync();
            var uptnlTask = uptnlFile?.CreateReaderAsync();
            var uasset = await uassetTask;
            var uexp = uexpTask != null ? await uexpTask : null;
            var ubulk = ubulkTask != null ? await ubulkTask : null;
            var uptnl = uptnlTask != null ? await uptnlTask : null;

            return new Package(uasset, uexp, ubulk, uptnl, this, MappingsForGame, UseLazySerialization);
        }

        public bool TryGetUassetDuo(string fullPath, out OsGameFile uassetFile, out OsGameFile uexpFile)
        {
            uassetFile = null;
            uexpFile = null;

            if (this[fullPath] is not OsGameFile uassetOsFile)
            {
                return false;
            }

            if (uassetOsFile.Extension != "uasset")
            {
                return false;
            }

            var uexpPath = Path.ChangeExtension(fullPath, ".uexp");
            if (this[uexpPath] is not OsGameFile uaexpOsFile)
            {
                return false;
            }

            uassetFile = uassetOsFile;
            uexpFile = uaexpOsFile;
            return true;
        }

        public void LoadLocalFiles()
        {
            FileCount = 0;
            Length = 0;
            if (!_workingDirectory.Exists)
                throw new ArgumentException("Given directory must exist", nameof(_workingDirectory));

            string mountPoint = _workingDirectory.Name + "/";

            foreach (var file in _workingDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                if (file.Extension != ".uasset" && file.Extension != ".uexp" && file.Extension != ".ubulk" && file.Extension != ".uptnl")
                {
                    continue;
                }

                FileCount++;
                Length += file.Length;
                var osFile = new OsGameFile(_workingDirectory, file, mountPoint, Versions);
                _osFiles[osFile.Path] = osFile;
            }
        }
    }
}

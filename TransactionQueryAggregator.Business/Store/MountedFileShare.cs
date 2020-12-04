using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Store
{
    public class MountedFileShare : IFileShare
    {
        private readonly string _path;

        public MountedFileShare(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public IAsyncEnumerable<string> ListAsync(IPathFilter pathFilter, CancellationToken cancellationToken)
        {
            if (pathFilter == null) throw new ArgumentNullException(nameof(pathFilter));
            return RecurseDirectory(_path, pathFilter, cancellationToken);
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value must not be null or whitespace", nameof(path));
            return Task.FromResult(Directory.Exists(path) || File.Exists(path));
        }

        public Task<MemoryStream> DownloadAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value must not be null or whitespace", nameof(path));
            return InternalDownloadAsync(path, cancellationToken);
        }

        private async Task<MemoryStream> InternalDownloadAsync(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
                return null;

            var ms = new MemoryStream();
            
            using (var fs = File.OpenRead(path))
            {
                await fs.CopyToAsync(ms, (int) fs.Length, cancellationToken);
            }

            return ms;
        }
                
        private async IAsyncEnumerable<string> RecurseDirectory(
            string directory,
            IPathFilter pathFilter, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            var subDirectories = Directory.GetDirectories(directory);
            var subFiles = Directory.GetFiles(directory);

            foreach (var subDirectory in subDirectories)
            {
                var action = pathFilter.DecideAction(subDirectory.Replace(_path, ""));

                if (action == PathAction.Recurse)
                    await foreach (var subItem in RecurseDirectory(subDirectory, pathFilter, cancellationToken)) yield return subItem;

                if (action == PathAction.Collect)
                    yield return subDirectory;
            }

            foreach (var subFile in subFiles)
            {
                var action = pathFilter.DecideAction(subFile.Replace(_path, ""));

                if (action == PathAction.Collect)
                    yield return subFile;
            }
        }
    }
}
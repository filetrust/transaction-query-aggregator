using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Store
{
    public class AzureFileShare : IFileShare
    {
        private readonly ShareClient _shareClient;

        public AzureFileShare(ShareClient shareClient)
        {
            _shareClient = shareClient ?? throw new ArgumentNullException(nameof(shareClient));
        }

        public IAsyncEnumerable<string> ListAsync(IPathFilter pathFilter, CancellationToken cancellationToken)
        {
            if (pathFilter == null) throw new ArgumentNullException(nameof(pathFilter));
            return RecurseDirectory(_shareClient.GetRootDirectoryClient(), pathFilter, cancellationToken);
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value must not be null or whitespace", nameof(path));
            return InternalExistsAsync(path, cancellationToken);
        }

        public Task<MemoryStream> DownloadAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value must not be null or whitespace", nameof(path));
            return InternalDownloadAsync(path, cancellationToken);
        }

        private async Task<MemoryStream> InternalDownloadAsync(string path, CancellationToken cancellationToken)
        {
            var fileClient = _shareClient.GetRootDirectoryClient().GetFileClient(path);

            if (!await InternalExistsAsync(fileClient, cancellationToken))
                return null;

            var ms = new MemoryStream();
            var file = await fileClient.DownloadAsync(cancellationToken: cancellationToken);
            await file.Value.Content.CopyToAsync(ms, (int)file.Value.ContentLength, cancellationToken);
            return ms;
        }

        private async Task<bool> InternalExistsAsync(string path, CancellationToken cancellationToken)
        {
            if (Path.HasExtension(path))
                return await InternalExistsAsync(_shareClient.GetRootDirectoryClient().GetFileClient(path), cancellationToken);

            return await InternalExistsAsync(_shareClient.GetDirectoryClient(path), cancellationToken);
        }

        private static async Task<bool> InternalExistsAsync(ShareDirectoryClient client, CancellationToken token)
        {
            try
            {
                return await client.ExistsAsync(token);
            }
            catch (RequestFailedException rex)
            {
                if (rex.ErrorCode == ShareErrorCode.ParentNotFound)
                    return false;

                throw;
            }
        }

        private static async Task<bool> InternalExistsAsync(ShareFileClient client, CancellationToken token)
        {
            try
            {
                return await client.ExistsAsync(token);
            }
            catch (RequestFailedException rex)
            {
                if (rex.ErrorCode == ShareErrorCode.ParentNotFound)
                    return false;

                throw;
            }
        }
        
        private static async IAsyncEnumerable<string> RecurseDirectory(
            ShareDirectoryClient directory,
            IPathFilter pathFilter, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var directoryPath = directory.Path;
            await foreach (var item in directory.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken))
            {
                var nextPath = $"{directoryPath}{(directoryPath == "" ? "" : "/")}{item.Name}";
                var nextPathAction = pathFilter.DecideAction(nextPath);

                if (item.IsDirectory && nextPathAction == PathAction.Recurse)
                {
                    var subDirectory = directory.GetSubdirectoryClient(item.Name);

                    await foreach (var subItem in RecurseDirectory(subDirectory, pathFilter, cancellationToken)) yield return subItem;
                }
                else if (nextPathAction == PathAction.Collect)
                    yield return nextPath;
            }
        }
    }
}
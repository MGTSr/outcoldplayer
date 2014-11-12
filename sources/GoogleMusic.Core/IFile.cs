﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IFile
    {
        string Name { get; }

        string Path { get; }

        Task<ulong> GetSizeAsync();
        Task DeleteAsync();
        Task MoveAsync(IFolder destination);

        Task<Stream> OpenReadWriteAsync();
    }
}

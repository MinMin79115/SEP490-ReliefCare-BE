using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(
            Stream stream,
            string fileName,
            CancellationToken cancellationToken = default);

        Task DeleteImageAsync(
            string publicId,
            CancellationToken cancellationToken = default);
    }
}

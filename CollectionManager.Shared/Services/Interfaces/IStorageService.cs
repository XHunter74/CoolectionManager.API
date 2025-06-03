using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhunter74.CollectionManager.Shared.Services.Interfaces;

public interface IStorageService
{
    Task UploadFileAsync(Guid fileId, byte[] sources);
    Task DeleteFileAsync(Guid fileId);
}

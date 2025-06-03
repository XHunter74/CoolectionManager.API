using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhunter74.CollectionManager.Shared.Services.Interfaces;

public interface IStorageService
{
    Task<Guid> UploadFileAsync(byte[] sources);
    Task DeleteFileAsync(Guid fileId);
}

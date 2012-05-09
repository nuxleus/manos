using System.Collections.Generic;
using System.Text;
using Manos.Collections;

namespace Manos.Http
{
    public interface IHttpDataRecipient
    {
        Encoding ContentEncoding { get; set; }

        DataDictionary PostData { get; set; }

        Dictionary<string, UploadedFile> Files { get; }

        string PostBody { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Text;

namespace FastCache
{
   public class AnyFilter : Stream {
  
      Stream originalStream;
      private readonly Encoding _responseEncoding;

      public string _streamContent = "";
  
      public AnyFilter(Stream originalFilter, Encoding encoding) {
        originalStream = originalFilter;
        _responseEncoding = encoding;
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
          _streamContent += _responseEncoding.GetString(buffer);
          originalStream.Write(buffer, offset, count);
      }
    }
}
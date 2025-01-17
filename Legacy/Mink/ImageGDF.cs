//! \file       ImageGDF.cs
//! \date       2017 Dec 29
//! \brief      Obfuscated BMP file.
//
// Copyright (C) 2017 by morkt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//

using System.ComponentModel.Composition;
using System.IO;

// [000225][Mink] Wonpara Wars

namespace GameRes.Formats.Mink
{
    [Export(typeof(ImageFormat))]
    public class GdfFormat : MbImageFormat
    {
        public override string         Tag { get { return "GDF"; } }
        public override string Description { get { return "Mink obfuscated bitmap"; } }
        public override uint     Signature { get { return 0; } }
        public override bool      CanWrite { get { return false; } }
        public override ImageData ReadAndExport(IBinaryStream file, ImageMetaData info, Stream exportFile)
        {
            throw new System.NotImplementedException();
        }

        public override void Pack(Stream file, IBinaryStream inputFile, ImageData bitmap)
        {
            throw new System.NotImplementedException();
        }

        public override ImageMetaData ReadMetaData (IBinaryStream stream)
        {
            int c1 = stream.ReadByte();
            int c2 = stream.ReadByte();
            if ('G' != c1 || 'D' != c2)
                return null;
            using (var bmp = OpenAsBitmap (stream))
                return Bmp.ReadMetaData (bmp);
        }

        public override void Write (Stream file, ImageData image)
        {
            throw new System.NotImplementedException ("GdfFormat.Write not implemented");
        }
    }
}

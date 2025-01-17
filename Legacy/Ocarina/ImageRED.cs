//! \file       ImageRED.cs
//! \date       2018 Jul 08
//! \brief      Ocarina image format.
//
// Copyright (C) 2018 by morkt
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
using System.Windows.Media;

namespace GameRes.Formats.Ocarina
{
    [Export(typeof(ImageFormat))]
    public class RedFormat : ImageFormat
    {
        public override string         Tag { get { return "RED"; } }
        public override string Description { get { return "Ocarina image format"; } }
        public override uint     Signature { get { return 0x304552; } } // 'RE0'

        public override ImageMetaData ReadMetaData (IBinaryStream file)
        {
            return new ImageMetaData { Width = 800, Height = 600, BPP = 32 };
        }

        public override ImageData Read (IBinaryStream file, ImageMetaData info)
        {
            file.Position = 4;
            var pixels = new uint[info.Width * info.Height];
            int dst = 0;
            while (dst < pixels.Length && file.PeekByte() != -1)
            {
                uint px = file.ReadUInt32();
                if (px != 0)
                    pixels[dst++] = px;
                else
                    dst += file.ReadUInt8();
            }
            return ImageData.Create (info, PixelFormats.Bgra32, null, pixels);
        }

        public override void Write (Stream file, ImageData image)
        {
            throw new System.NotImplementedException ("RedFormat.Write not implemented");
        }

        public override ImageData ReadAndExport(IBinaryStream file, ImageMetaData info, Stream exportFile)
        {
            throw new System.NotImplementedException();
        }

        public override void Pack(Stream file, IBinaryStream inputFile, ImageData bitmap)
        {
            throw new System.NotImplementedException();
        }
    }
}

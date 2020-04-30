// This file is a part of BigGustave, a png load-and-save library.
//namespace BigGustave // make it internal
namespace TpacTool.IO
{
    internal class Palette
    {
        public byte[] Data { get; }

        public Palette(byte[] data)
        {
            Data = data;
        }

        public Pixel GetPixel(int index)
        {
            var start = index * 3;

            return new Pixel(Data[start], Data[start + 1], Data[start + 2], 255, false);
        }
    }
}
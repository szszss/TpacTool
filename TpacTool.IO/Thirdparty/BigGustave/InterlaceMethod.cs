// This file is a part of BigGustave, a png load-and-save library.
//namespace BigGustave // make it internal
namespace TpacTool.IO
{
	/// <summary>
	/// Indicates the transmission order of the image data.
	/// </summary>
	public enum InterlaceMethod : byte
    {
        /// <summary>
        /// No interlace.
        /// </summary>
        None = 0,
        /// <summary>
        /// Adam7 interlace.
        /// </summary>
        Adam7 = 1
    }
}
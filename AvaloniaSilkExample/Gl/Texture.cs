using Silk.NET.OpenGL;
using System;
using SkiaSharp;
using System.IO;

namespace Tutorial
{
    public class Texture : IDisposable
    {
        private uint _handle;
        private GL _gl;

        public unsafe Texture(GL gl, string path)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            // Load the image using SkiaSharp.
            using (var stream = File.OpenRead(path))
            {
                using (var bitmap = SKBitmap.Decode(stream))
                {
                    // Reserve enough memory from the GPU for the whole image.
                    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)bitmap.Width, (uint)bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

                    // Convert the bitmap to a format suitable for OpenGL.
                    var pixmap = bitmap.PeekPixels();

                    // SkiaSharp stores images in contiguous memory, so we can send the image in one go.
                    var dataPtr = pixmap.GetPixels();

                    // Load the actual image.
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)bitmap.Width, (uint)bitmap.Height, PixelFormat.Rgba, PixelType.UnsignedByte, dataPtr);
                }
            }

            SetParameters();

        }

        public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
        {
            //Saving the gl instance.
            _gl = gl;

            //Generating the opengl handle;
            _handle = _gl.GenTexture();
            Bind();

            //We want the ability to create a texture using data generated from code aswell.
            fixed (void* d = &data[0])
            {
                //Setting the data of a texture.
                _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                SetParameters();
            }
        }

        private void SetParameters()
        {
            //Setting some texture perameters so the texture behaves as expected.
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            //Generating mipmaps.
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            //When we bind a texture we can choose which textureslot we can bind it to.
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public void Dispose()
        {
            //In order to dispose we need to delete the opengl handle for the texure.
            _gl.DeleteTexture(_handle);
        }
    }
}

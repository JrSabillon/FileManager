using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;

namespace SyT_FileManager.AppCode
{
    public static class StaticHelpers
    {
        /// <summary>
        /// Funcion creada para unir 2 modelos, copia los valores que no son nulos del objeto fuente en el objeto resultante
        /// </summary>
        /// <typeparam name="T">Tipo de modelo a unir</typeparam>
        /// <param name="target">Modelo resultante</param>
        /// <param name="source">Modelo fuente</param>
        public static void MergeObjects<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }

        public static byte[] GenerateCodeBar(string code, int? width, int? height, int? size)
        {
            var codeBar = new byte[] { };
            width = width ?? CodeBar.Width.GetAttribute<IntegerAttribute>().value;
            height = height ?? CodeBar.Height.GetAttribute<IntegerAttribute>().value;
            size = size ?? CodeBar.Size.GetAttribute<IntegerAttribute>().value;

            if (string.IsNullOrEmpty(code))
                throw new NullReferenceException("Codigo de barras no puede estar vacio.");

            using (var stream = new MemoryStream())
            {
                var bitmap = new Bitmap(width.Value, height.Value);
                var graphic = Graphics.FromImage(bitmap);
                var font = new BarcodeHandler().LoadFont("E39", size.Value);
                var point = new Point();
                var brush = new SolidBrush(Color.Black);

                graphic.FillRectangle(new SolidBrush(Color.White), 0, 0, width.Value, height.Value);
                graphic.DrawString($"*{code.ToUpper()}*", font, brush, point);
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Seek(0, SeekOrigin.Begin);
                codeBar = stream.ToArray();
            }

            return codeBar;
        }

        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            return type.GetField(name).GetCustomAttributes(false).OfType<T>().SingleOrDefault();
        }
    }

    /// <summary>
    /// Obtener atributo 'Integer' de valores enumerables.
    /// </summary>
    public class IntegerAttribute : Attribute
    {
        public int value;
        internal IntegerAttribute(int value)
        {
            this.value = value;
        }
    }
}
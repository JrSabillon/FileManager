using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SyT_FileManager.AppCode
{
    public class CustomPdfPageEventHandler : PdfPageEventHelper
    {
        const float horizontalPosition = 0.9f;
        const float verticalPosition = 0.02f;

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var footerText = new Phrase($"{writer.CurrentPageNumber}", new Font { Size = 10 });

            float posX = writer.PageSize.Width * horizontalPosition;
            float posY = writer.PageSize.Height * verticalPosition;
            float rotation = 0;

            ColumnText.ShowTextAligned(writer.DirectContent, Element.PHRASE, footerText, posX, posY, rotation);
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            var projectDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Image imgHead = Image.GetInstance(projectDirectory + "/Content/imgs/background.png");
            imgHead.ScalePercent(30f);
            imgHead.Alignment = Image.RIGHT_ALIGN;

            document.Add(imgHead);
         
            base.OnStartPage(writer, document);
        }
    }
}
using BLL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace BLL.Service
{
	public class TesseractOcrService : ITesseractOcrService
	{
		private readonly string _tessdataPath;

		public TesseractOcrService(string tessdataPath)
		{
			_tessdataPath = tessdataPath;
		}

		public string ExtractText(string imagePath, string language = "eng")
		{
			using var engine = new TesseractEngine(_tessdataPath, language, EngineMode.Default);
			using var img = Pix.LoadFromFile(imagePath);
			using var page = engine.Process(img);

			return page.GetText();
		}
	}
}

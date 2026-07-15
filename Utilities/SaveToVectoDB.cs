using System.Text;
using Tesseract;
using UglyToad.PdfPig.Content;
using System.Data;
using System.IO;
using UglyToad.PdfPig;
using XAct;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;


namespace MPCRS.Utilities
{
	public class SaveToVectoDB
	{
		public static string ProccessDocumentsIntoVectorDb(string DocDbkeyJson)
		{
			var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments");
			DataTable table = new DataTable();
			table = MPGlobals.GetDataForDatalist($"[dbo].[DocumentsIntoVectorDb_SSP] @DocDBkeyjson = '{DocDbkeyJson}' ");
			int flag = 0;
			int DocExistCounter = 0;
			int SavedCounter = 0;
			for (int i = 0; i < table.Rows.Count; i++)
			{

				DateTime starttime = DateTime.Now;
				string startTimeStr = starttime.ToString("yyyy-MM-dd HH:mm:ss");
                DataTable dtflag = MPGlobals.GetDataForDatalist("Select [DataJson] from [dbo].[AppSettings] where [AppSettingType] = 'Flag_DocIntoVectorDB'");
				if (int.Parse(dtflag.Rows[0][0].ToString()) == 1)
				{
					string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + table.Rows[i]["File_Location"].ToString() + table.Rows[i]["System_File_Name"].ToString());

					if (File.Exists(path))
					{
						var ocrResult = ExtractImagesAndPerformOCR(path, directory, table.Rows[i]["System_File_Name"].ToString(), table.Rows[i]["File_Name"].ToString());
						//var result = "Saved Embeddings";
                        var result = saveEmbeddings(table.Rows[i]["File_Name"].ToString(), ocrResult, table.Rows[i]["Document_Dbkey"].ToString());
                        DateTime endtime = DateTime.Now;
						TimeSpan difference = endtime.Subtract(starttime);
						string customString = difference.ToString(@"hh\:mm\:ss");
						string endTimeStr = endtime.ToString("yyyy-MM-dd HH:mm:ss");
						if(result.Contains("Saved Embeddings"))
						{
							//MPGlobals.ExceSQLNonQuery($"Update [dbo].[Documentation] set [Status_In_VectorDB] = 1 ,[VectorExecutionTime] ='{customString}'  where [Document_Dbkey] = {table.Rows[i]["Document_Dbkey"].ToString()}");
							MPGlobals.ExceSQLNonQuery($"UPDATE [dbo].[Documentation]   SET [Status_In_VectorDB] = 1 ,[VectorExecutionStartTime] = CAST('{startTimeStr}' AS DATETIME),[VectorExecutionEndTime] = CAST('{endTimeStr}' AS DATETIME) WHERE[Document_Dbkey] ='{table.Rows[i]["Document_Dbkey"].ToString()}'");
							SavedCounter++;
						}

                        DocExistCounter++;
					}
				}

				//if (DocExistCounter >= 5)
				//{
				//	break;
				//}
			}
			return $"Saved {SavedCounter} documents to vectorDB among {DocExistCounter} documents";
		}

        public static List<string> ExtractImagesAndPerformOCR(string pdfPath, string outputDir, string SystemFilename, string filename)
		{
			var pagesText = new List<string>();
			var imagestatus = "";
			bool unsupportedImageDetected = false;
			try
			{

				var parts = SystemFilename.Split('.');
				string uniqueID = parts[0];
				using (PdfDocument document = PdfDocument.Open(pdfPath))
				{
					int pageNumber = 1;
					foreach (var page in document.GetPages())
					{
						var pageTextData = new StringBuilder();
						var images = page.GetImages().ToList();
						int imageIndex = 0;

						var fullText = page.Text;

						var textSegments = fullText.Split(new[] { "\n" }, StringSplitOptions.None);
                        
                        foreach (var textSegment in textSegments)
						{

							pageTextData.AppendLine(textSegment);

							while (imageIndex < images.Count)
							{
								string imgPath = System.IO.Path.Combine(outputDir, $"Image_Page_{pageNumber}_{imageIndex}.png");
                                imagestatus = SaveImageToDisk(images[imageIndex], imgPath);
								if(imagestatus == "Unsupported image format")
								{
                                    unsupportedImageDetected = true;
                                    break;
								}
								string ocrText = PerformOCR(imgPath);

								if (!string.IsNullOrWhiteSpace(ocrText))
								{
									pageTextData.AppendLine(ocrText);
								}
								System.IO.File.Delete(imgPath);

								imageIndex++;
							}
                            
                        }
                        if (unsupportedImageDetected) break; // Exit outer loop if an unsupported image was detected
                        if (pageTextData.Length == 0 && imageIndex == 0)
						{
							pagesText.Add("[Empty Page]");
						}
						else
						{
							pagesText.Add(pageTextData.ToString());
						}

						pageNumber++;
					}
				}
				if (pagesText.Any(s => string.IsNullOrWhiteSpace(s)) || unsupportedImageDetected)
				{

					pagesText = ExtractAllImagesAndPerformOCR(pdfPath);
					return pagesText;

				}
				else
				{
					return pagesText;
				}

				// return string.Join("\n", pagesText); // Returning combined extracted text for debugging

			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				pagesText.Add(ex.Message);
				return pagesText;
				//return ex.Message; // Consider logging the exception for better error handling
			}
		}

		public static List<string> ExtractAllImagesAndPerformOCR(string pdfPath)
		{
			var pagesText = new List<string>();

			using (var document = PdfiumViewer.PdfDocument.Load(pdfPath))
			{
				for (int i = 0; i < document.PageCount; i++)
				{
					using (var image = document.Render(i, 300, 300, true)) // 300 DPI
					{
						string imagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"page_{i + 1}.png");
						image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png); // Save the image

						// Perform OCR on the saved image
						string ocrText = PerformOCR(imagePath);
						pagesText.Add(ocrText);


					}
				}
			}

			return pagesText;
		}

		public static string SaveImageToDisk(IPdfImage image, string outputPath)
		{
			try
			{
                if (image.RawBytes.Count == 0)
                {
					return "image empty";
                    //throw new InvalidOperationException("Image data is empty or null.");
                }

				// image is extracted as png and saved as png

				//if (image.TryGetPng(out byte[] pngBytes))
				//{
				//	System.IO.File.WriteAllBytes(outputPath, pngBytes);
				//	//Console.WriteLine($"Image saved successfully as {outputPath}.");
				//	return "image saved";
				//}

				// image is extracted if its png then saved as jpeg

				if (image.TryGetPng(out byte[] pngBytes))
				{
					// Load the PNG bytes into an ImageSharp Image
					using (var ms = new MemoryStream(pngBytes))
					{
						var img = Image.Load(ms);

						// Save the image as JPEG
						img.Save(outputPath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
						return "Image saved as JPEG successfully.";
					}
				}	
				return "Unsupported image format";
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return ex.Message;
            }
		}
        


        //public static string CheckOCRPerformance()
        //{
        //	try
        //	{
        //		//var filePath = @"D:\tessaractocr\test1.png";
        //		var filePath = @"C:\Users\admin\Pictures\Screenshots\Screenshot 2024-09-18 193624.png";

        //		// Perform OCR
        //		var ocrData = PerformOCR(filePath);
        //		return "Saved Successfully";

        //	}
        //	catch (Exception ex)
        //	{
        //		ErrorHandler.LogException(ex);
        //		return ex.Message.ToString();
        //	}

        //}


        // Perform OCR on the extracted image
        public static string PerformOCR(string imagePath)
		{
			try
			{
				var directory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/tessdata");
                using (var engine = new TesseractEngine(directory, "eng", EngineMode.Default))
				{
					using (var img = Pix.LoadFromFile(imagePath))
					{
						using (var page = engine.Process(img))
						{
							return page.GetText();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during OCR: " + ex.Message);
				return "OCR failed";
			}
		}

		public static string saveEmbeddings(string fileName, List<string> pages, string docID)
		{
			string GenerateResponsePyPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ChatWithDoc/SaveEmbeddings.py");
			List<string> parameters = new List<string>();
			parameters.Add(fileName);
			parameters.Add(docID);
			parameters.Add(pages);
			var result = PythonInvoker.RunPythonScript(GenerateResponsePyPath, "Generate", parameters);
			return result.ToString();
		}
	}
}

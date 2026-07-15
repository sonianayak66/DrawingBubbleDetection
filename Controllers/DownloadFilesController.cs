 using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using System.Security.Claims;
using iTextSharp.text.pdf;
using iTextSharp.text;
using ImageMagick;
using XAct.Users;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO.Compression;
using System.Web.Helpers;
using MPCRS.ViewModels;
using Microsoft.EntityFrameworkCore;
using MPCRS;
using static MPCRS.Utilities.Constants;
using System.Net.Mail;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using MPCRS.Utilities;
using System.Drawing;
using Image = System.Drawing.Image;
using Font = System.Drawing.Font;
using Color = System.Drawing.Color;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using BitMiracle.LibTiff.Classic;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;



public class downloadFileData
{
    public string DisplayFolderName { get; set; }
    public string DisplayFileDesc { get; set; }
    public string DisplayFileName { get; set; }
    public string FileName { get; set; }
    public string FileLocation { get; set; }
    public string fileType { get; set; }
    public string SourceTable { get; set; }

}

public class DownloadFilesController : Controller
{
    private readonly DESI_STFE_PRODContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly MPDapperContext mPDapperContext;
    public DownloadFilesController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
    {
        _dbContext = context;
        _configuration = configuration;
        this.mPDapperContext = mPDapperContext;
    }

    //Download requests
    public IActionResult DownloadRequest(int fileKey, string SourceTable, bool withoutWatermark)
    {
		try
		{
			downloadFileData fileData = GetAttachmentFileInfo(fileKey, SourceTable, withoutWatermark);

			int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
			Audit_log audit_Log = new Audit_log();
			audit_Log.table_name = fileData.SourceTable;
			audit_Log.Primary_key = fileKey;
			audit_Log.Event_Description = fileData.DisplayFileDesc;
			audit_Log.Remarks = fileData.DisplayFolderName;
			audit_Log.Updated_By = userId;
			audit_Log.Updated_On = DateTime.Now;
			_dbContext.Add(audit_Log);
			_dbContext.SaveChanges();


			string Filename = fileData.FileName;
			string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileData.FileLocation + fileData.FileName);
			if (fileData.fileType != null && withoutWatermark == false)
			{
				if (fileData.fileType.ToLower().Contains(".pdf") && fileData.fileType.ToLower().EndsWith(".zip") == false)
				{
					return RedirectToAction("AddWatermark", new { path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileData.FileLocation), fileName = Filename, originalFileName = fileData.DisplayFileName });
				}
				else if ((fileData.fileType.ToLower().Contains(".tif") || fileData.fileType.ToLower().Contains(".tiff")) && fileData.fileType.ToLower().EndsWith(".zip") == false)
				{
					//return RedirectToAction("AddWatermarkToTiff", new { path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileData.FileLocation), fileName = Filename, originalFileName = fileData.DisplayFileName });
					return RedirectToAction("ViewDocument", new { docid = fileKey, sourceTable = fileData.SourceTable, download = true });

				}
			}
			if (fileData.fileType != null && withoutWatermark == true)
			{
				return RedirectToAction("DownloadFileWithoutWatermark", new { path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + fileData.FileLocation), fileName = Filename, originalFileName = fileData.DisplayFileName });
			}

			byte[] fileBytes = System.IO.File.ReadAllBytes(path);
			return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileData.DisplayFileName);
			//  return Json(GetAttachmentFileInfo(fileKey, SourceTable));
		}
		catch (Exception ex)
		{
            ErrorHandler.LogException(ex);
            return RedirectToAction("Index", "Error");
			
		}
	
    }

    public downloadFileData GetAttachmentFileInfo(int fileKey, string SourceTable, bool withoutWatermark)
    {
        downloadFileData fileData = new();
        string woWatermarkText = withoutWatermark ? " without Watermark " : "";
        if (SourceTable == "Documentation")
        {
            
            Documentation documentation = _dbContext.Documentations.Where(x => x.Document_Dbkey == fileKey).FirstOrDefault();
            if (documentation != null)
            {
                Documentation documentation2 = _dbContext.Documentations.Where(x => x.Document_Dbkey == documentation.Parent_id).FirstOrDefault();
                fileData.DisplayFolderName = "From Folder - " + documentation2.Refrence_Title;

                if (documentation.Description != null)
                {
                    fileData.DisplayFileDesc = $"Download File {woWatermarkText} - {documentation.Refrence_Title} ({documentation.Description})";
                }
                else
                {
                    fileData.DisplayFileDesc = $"Download File {woWatermarkText} - {documentation.Refrence_Title}";
                }
                fileData.DisplayFileName = documentation.File_Name;
                fileData.FileName = documentation.System_File_Name;
                fileData.FileLocation = documentation.File_Location;
                fileData.fileType = documentation.File_type;
                fileData.SourceTable = "Documentation";
            }
        }
        else if (SourceTable == "Attachment")
        {
            MPCRS.Models.Attachment attachment = _dbContext.Attachments.Where(x => x.Attachment_Db_Key == fileKey).FirstOrDefault();
            if (attachment != null)
            {
                fileData.DisplayFolderName = "From - MPL Documents ";
				fileData.DisplayFileDesc = $"Download File {woWatermarkText} - {attachment.Orginal_File_Name}";
                fileData.DisplayFileName = attachment.Orginal_File_Name;
                fileData.FileName = attachment.Attachment_FileName;
                fileData.FileLocation = attachment.Attachment_location;
                fileData.fileType = Path.GetExtension(attachment.Orginal_File_Name);
                fileData.SourceTable = "Attachment";
            }

        }

        return fileData;
    }

    // Add watermark to .pdf (Download)
    public IActionResult AddWatermark(string path, string fileName, string originalFileName)
    {
        WatermarkConfiguration watermarkConfiguration = _dbContext.WatermarkConfigurations.Where(x => x.ConfigurationFor == "Documents").FirstOrDefault();
        string extension = System.IO.Path.GetExtension(fileName);

        // Load pdf file
        var pdfBytes = System.IO.File.ReadAllBytes(Path.Combine(path, fileName));
        var oldFile = new iTextSharp.text.pdf.PdfReader(pdfBytes);

        string username = User.FindFirst(ClaimTypes.GivenName).Value;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        Response.ContentType = "application/octet-stream";
        originalFileName = originalFileName == "" ? fileName : originalFileName;
        Response.Headers.Add("Content-Disposition", "attachment; filename=" + originalFileName);

        using (var ms = new MemoryStream())
        {
            // Setup PdfStamper
            using (var stamper = new PdfStamper(oldFile, ms))
            {
                var font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                var brush = new PdfGState();
              //  brush.FillOpacity = 0.3f;
                brush.FillOpacity = (float)watermarkConfiguration.FontOpacity;

                // Iterate through the pages in the original file
                for (var i = 1; i <= oldFile.NumberOfPages; i++)
                {
                    var page = stamper.GetOverContent(i);
                    page.BeginText();
                    page.SetFontAndSize(font, (float)watermarkConfiguration.FontSize);
                    page.SetGState(brush);


                    var width = oldFile.GetPageSize(i).Width;
                    var height = oldFile.GetPageSize(i).Height;

                    // Calculate the position for the first text (30% from the top)
                    // Now only one in center
                    var x1 = width / 2;
                    //var y1 = height * 0.7f; // 30% from the top
                    var y1 = height / 2;
                    page.SetTextMatrix(1, 0, 0, 1, x1, y1);
                    page.ShowTextAligned(Element.ALIGN_CENTER, $"Downloaded by: {username}, Date: {timestamp}", x1, y1, (float)watermarkConfiguration.Rotation);


                    //// Calculate the position for the second text (60% from the top)
                    //var x2 = width / 2;
                    //var y2 = height * 0.4f; // 60% from the top
                    //page.SetTextMatrix(1, 0, 0, 1, x2, y2);
                    //page.ShowTextAligned(Element.ALIGN_CENTER, $"Downloaded by: {username}, Date: {timestamp}", x2, y2, 30);

                    page.EndText();
                }
            }

            // Create a copy of the MemoryStream
            var copy = new MemoryStream(ms.ToArray());

            // Write the copy of the MemoryStream directly to the response body
            copy.CopyTo(Response.Body);
        }

        return new EmptyResult();

    }

    //public IActionResult AddWatermarkToSinglePageTiff(string path, string fileName, string originalFileName)
    //{
    //    string extension = System.IO.Path.GetExtension(fileName);
    //    string inputFilePath = Path.Combine(path, fileName);
    //    string UserName = User.FindFirst(ClaimTypes.GivenName).Value;
    //    string text = $"Downloaded by: {UserName}, Date: {DateTime.Now}";
    //    string watermarkedFileName = Guid.NewGuid().ToString() + extension;
    //    string downloadfolder = "/Attachments/Downloads/";
    //    downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
    //    string watermarkedFilePath = Path.Combine(downloadfolder, watermarkedFileName);
    //    if (!Directory.Exists(downloadfolder))
    //    {
    //        Directory.CreateDirectory(downloadfolder);
    //    }
    //    originalFileName = ((originalFileName == "") ? fileName : originalFileName);


    //    using (MagickImageCollection images = new MagickImageCollection(inputFilePath))
    //    {
    //        foreach (MagickImage image in images)
    //        {
    //            //Get the dimensions of the image
    //            int width = image.Width;
    //            int height = image.Height;

    //            // Calculate the position to center the text
    //            int textX = width / 2;
    //            int textY = height / 2;
    //            // Annotate the text on the image	

    //            // Set the text properties
    //            image.Settings.FontPointsize = height * 0.05; // Increase text size to 36
    //            image.Settings.FillColor = new MagickColor(MagickColors.Gray) { A = (ushort)(Quantum.Max / 3) }; // Set text color to red with 50% opacity
    //            image.Settings.TextGravity = Gravity.Center; // Set text gravity to center
    //            image.Settings.TextInterlineSpacing = 10; // Set text interline spacing

    //            image.Annotate(text, new MagickGeometry(textX, (int)(height * 0.5), 20, 20), Gravity.Center, 30);
    //            image.Annotate(text, new MagickGeometry(textX, (int)(height * 0.66), 20, 20), Gravity.Center, 30);


    //            //Draw the text annotation on the image
    //            image.Draw(drawables);
    //        }

    //        // Write the modified images to the output file
    //        images.Write(watermarkedFilePath);
    //    }

    //    // Read the modified image into a byte array
    //    byte[] fileBytes = System.IO.File.ReadAllBytes(watermarkedFilePath);

    //    // Set the content type for TIFF files
    //    //string contentType = "image/tiff";
    //    string contentType = "application/octet-stream";

    //    // Return the modified image in the response body
    //    return File(fileBytes, contentType, originalFileName);

    //}



    [HttpGet]
    //Extract zip file and show pdf and .tif available in that .zip file
    public IActionResult ShowExtractedFiles(int fileKey)
    {
        MPCRS.Models.Attachment attachment = _dbContext.Attachments.Where(x => x.Attachment_Db_Key == fileKey).FirstOrDefault();
        if (attachment != null)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            Audit_log audit_Log = new Audit_log();
            audit_Log.table_name = "Attachment";
            audit_Log.Primary_key = fileKey;
            audit_Log.Event_Description = "Download File - " + attachment.Orginal_File_Name;
            audit_Log.Remarks = "From - MPL Documents ";
            audit_Log.Updated_By = userId;
            audit_Log.Updated_On = DateTime.Now;
            _dbContext.Add(audit_Log);
            _dbContext.SaveChanges();
            //JsonResult jsonResult = ExtractZipFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location), attachment.Attachment_FileName, attachment.Orginal_File_Name);
            var zipFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location + attachment.Attachment_FileName);

            //if (!System.IO.File.Exists(zipFilePath))
            //{
            //	return NotFound("File not found.");
            //}
            // Generate a new GUID for the folder name
            var newFolderName = Guid.NewGuid().ToString();
            string downloadfolder = "/Attachments/Downloads/" + newFolderName + "/";

            var newFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);

            // Create the new folder
            Directory.CreateDirectory(newFolderPath);

            // Extract the zip file to the new folder
            ZipFile.ExtractToDirectory(zipFilePath, newFolderPath);

            // Get all file names with their locations
            var extractedFiles = Directory.GetFiles(newFolderPath, "*", SearchOption.AllDirectories)
                .Select(file => new ExtractedFilesVM
                {
                    FileName = Path.GetFileName(file),
                    FilePath = Path.Combine(newFolderPath).Replace("\\", "/"),
                    docId = fileKey
				}).ToList();
            return PartialView(extractedFiles);
        }
        return PartialView();
    }

    //View & download Requests
    [HttpGet]
    public ActionResult ViewDocument(int docid,string sourceTable, bool download = false)
    {
        try
        {
			if (download == false)
			{

				downloadFileData fileData = GetAttachmentFileInfo(docid, sourceTable, download);
				int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				Audit_log audit_Log = new Audit_log();
				audit_Log.table_name = fileData.SourceTable;
				audit_Log.Primary_key = docid;
				fileData.DisplayFileDesc = fileData.DisplayFileDesc.Replace("Download", "View");
				audit_Log.Event_Description = fileData.DisplayFileDesc;
				audit_Log.Remarks = fileData.DisplayFolderName;
				audit_Log.Updated_By = userId;
				audit_Log.Updated_On = DateTime.Now;
				_dbContext.Add(audit_Log);
				_dbContext.SaveChanges();
			}

			if (sourceTable == "Documentation")
			{

				Documentation documentation = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == docid).FirstOrDefault();
				DocumentsViewModel documentsViewModel = new();
				documentsViewModel.Document_Dbkey = documentation.Document_Dbkey;
				documentsViewModel.File_type = documentation.File_type;

				if (documentation.File_type.ToLower().Contains(".pdf") && (documentation.File_type.ToLower().EndsWith(".zip") == false))
				{
					documentsViewModel.System_File_Name = AddWatermark_View(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + documentation.File_Location), documentation.System_File_Name, documentation.File_Name);
				}
				if ((documentation.File_type.ToLower().Contains(".tiff") || documentation.File_type.ToLower().Contains(".tif")) && (documentation.File_type.ToLower().EndsWith(".zip") == false))
				{
					documentsViewModel.TiffToPngConvertedLocation = AddWatermarkToTiff(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + documentation.File_Location), documentation.System_File_Name, documentation.File_Name);
					if (download)
					{
						//if(documentsViewModel.TiffToPngConvertedLocation.Count <= 1)
						//{
						//    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + documentation.File_Location);
						//    string FilePath = Path.Combine(path, documentation.System_File_Name);
						//    string original_FileName = documentation.File_Name;
						//    return RedirectToAction("AddWatermarkToSinglePageTif", new { inputFilePath = FilePath, originalFileName = original_FileName });
						//}
						List<string> pngFiles = new List<string>();
						string downloadfolder = "/Attachments/Downloads/";
						downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);

						foreach (var item in documentsViewModel.TiffToPngConvertedLocation)
						{
							pngFiles.Add(Path.Combine(downloadfolder, item.Item1).Replace("\\", "/"));

						}

						if (documentation.File_type.ToLower().Contains(".tiff") == false)
						{
							var tiffPath = ConvertToMultiPageTiff(pngFiles, ".tif");
							var fileName = Path.GetFileName(documentation.File_Name);
							var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
							var contentType = "application/octet-stream";
							if (!tiffPath.IsNullOrEmpty())
							{
								return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = tiffPath, originalFileName = fileName });
							}
							return File(fileBytes, contentType, fileName);
						}
						else
						{
							var tiffPath = ConvertToMultiPageTiff(pngFiles, "tiff");
							var fileName = Path.GetFileName(documentation.File_Name);
							var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
							var contentType = "application/octet-stream";
							if (!tiffPath.IsNullOrEmpty())
							{
								return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = tiffPath, originalFileName = fileName });
							}
							return File(fileBytes, contentType, fileName);
						}


					}

				}
				return PartialView(documentsViewModel);
			}
			else if (sourceTable == "Attachment")
			{
				MPCRS.Models.Attachment attachment = _dbContext.Attachments.AsNoTracking().Where(x => x.Attachment_Db_Key == docid).FirstOrDefault();
				DocumentsViewModel documentsViewModel = new();
				documentsViewModel.Document_Dbkey = attachment.Attachment_Db_Key;
				documentsViewModel.File_type = Path.GetExtension(attachment.Orginal_File_Name);

				if (documentsViewModel.File_type.ToLower().Contains(".pdf") && (documentsViewModel.File_type.ToLower().EndsWith(".zip") == false))
				{

					documentsViewModel.System_File_Name = AddWatermark_View(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location), attachment.Attachment_FileName, attachment.Orginal_File_Name);
				}
				else if ((documentsViewModel.File_type.ToLower().Contains(".tiff") || documentsViewModel.File_type.ToLower().Contains(".tif")) && (documentsViewModel.File_type.ToLower().EndsWith(".zip") == false))
				{
					documentsViewModel.TiffToPngConvertedLocation = AddWatermarkToTiff(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location), attachment.Attachment_FileName, attachment.Orginal_File_Name);

					try
					{
						if (download)
						{
							//if (documentsViewModel.TiffToPngConvertedLocation.Count <= 1)
							//{
							//    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location);
							//    string FilePath = Path.Combine(path, attachment.Attachment_FileName);
							//    string original_FileName = attachment.Orginal_File_Name;
							//    return RedirectToAction("AddWatermarkToSinglePageTif", new { inputFilePath = FilePath, originalFileName = original_FileName });
							//}
							List<string> pngFiles = new List<string>();
							string Extension = Path.GetExtension(attachment.Orginal_File_Name);
							string downloadfolder = "/Attachments/Downloads/";
							downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);

							foreach (var item in documentsViewModel.TiffToPngConvertedLocation)
							{
								pngFiles.Add(Path.Combine(downloadfolder, item.Item1).Replace("\\", "/"));

							}

							if (Extension.ToLower().Contains(".tiff") == false)
							{
								var tiffPath = ConvertToMultiPageTiff(pngFiles, ".tif");
								var fileName = Path.GetFileName(attachment.Orginal_File_Name);
								var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
								var contentType = "application/octet-stream";
								if (!tiffPath.IsNullOrEmpty())
								{
									return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = tiffPath, originalFileName = fileName });
								}
								return File(fileBytes, contentType, fileName);
							}
							else
							{
								var tiffPath = ConvertToMultiPageTiff(pngFiles, "tiff");
								var fileName = Path.GetFileName(attachment.Orginal_File_Name);
								var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
								var contentType = "application/octet-stream";
								if (!tiffPath.IsNullOrEmpty())
								{
									return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = tiffPath, originalFileName = fileName });
								}
								return File(fileBytes, contentType, fileName);
							}


						}
					}
					catch (MagickCoderErrorException ex)
					{
						//throw;
						using (var document = new PdfSharpCore.Pdf.PdfDocument())
						{
							string fileName = Path.ChangeExtension(attachment.Orginal_File_Name, ".pdf");
							foreach (var imagePath in documentsViewModel.TiffToPngConvertedLocation)
							{
								// Add a new page to the PDF document
								var page = document.AddPage();

								// Set the page size to A4 (optional, you can adjust this if needed)
								page.Width = imagePath.Item2;
								page.Height = imagePath.Item3;
								string downloadfolder = "/Attachments/Downloads/";
								downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
								string pngLocation = Path.Combine(downloadfolder, imagePath.Item1).Replace("\\", "/");

								// Create graphics object for drawing
								using (var gfx = XGraphics.FromPdfPage(page))
								using (var image = XImage.FromFile(pngLocation))
								{
									// Draw the image on the page, resize to fit the page if necessary
									gfx.DrawImage(image, 0, 0, page.Width, page.Height);
								}
							}

							// Save the document to a MemoryStream
							using (var stream = new MemoryStream())
							{
								document.Save(stream, false);
								var pdfBytes = stream.ToArray();

								// Return the PDF as a downloadable file
								return File(pdfBytes, "application/octet-stream", fileName);
							}
						}
					}

				}
				return PartialView(documentsViewModel);

			}
			return PartialView(new DocumentsViewModel());
		}
        catch (Exception ex)
        {
            ErrorHandler.LogException(ex);
			return RedirectToAction("Index", "Error");

		}
        		
    }


    public string ConvertToMultiPageTiff(List<string> pngFilePaths, string filetype)
    {
        try
        {
            // Create a MagickImageCollection
            var collection = new MagickImageCollection();

            // Read each PNG file and add it to the collection
            foreach (var pngFilePath in pngFilePaths)
            {
                var image = new MagickImage(pngFilePath);
                collection.Add(image);
            }

            // Write the collection as a multi-page TIFF
            var tiffFilePath = Path.ChangeExtension(pngFilePaths[0], filetype);
            collection.Write(tiffFilePath);

            // Dispose of the collection to release resources
            collection.Dispose();

            return tiffFilePath;
        }
        catch (Exception ex)
        {
            // Handle exceptions
            ErrorHandler.LogException(ex);
            throw new Exception($"An error occurred while converting to multi-page TIFF: {ex.Message}");
        }
    }

    public string AddWatermark_View(string path, string fileName, string originalFileName)
    {

        string extension = System.IO.Path.GetExtension(fileName);
        if (extension == ".pdf")
        {
			WatermarkConfiguration watermarkConfiguration = _dbContext.WatermarkConfigurations.Where(x => x.ConfigurationFor == "Documents").FirstOrDefault();
			if (watermarkConfiguration == null)
			{
				// No watermark config found — return original file without watermark
				return string.Empty;
			}
			var pdfBytes = System.IO.File.ReadAllBytes(Path.Combine(path, fileName));
            var oldFile = new iTextSharp.text.pdf.PdfReader(pdfBytes);

            string username = User.FindFirst(ClaimTypes.GivenName).Value;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Guid guid = Guid.NewGuid();
            string watermarkedFileName = guid.ToString() + fileName;
            string downloadfolder = "/Attachments/Downloads/";
            downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
            string watermarkedFilePath = System.IO.Path.Combine(downloadfolder, watermarkedFileName);

            if (!Directory.Exists(downloadfolder))
            {
                Directory.CreateDirectory(downloadfolder);
            }


            //	Response.ContentType = "application/pdf";
            originalFileName = originalFileName == "" ? fileName : originalFileName;
            //Response.Headers.Add("Content-Disposition", "attachment; filename=" + originalFileName);
            using (FileStream outputStream = new FileStream(watermarkedFilePath, FileMode.Create))
            {

                using (var ms = new MemoryStream())
                {
                    // Setup PdfStamper
                    using (var stamper = new PdfStamper(oldFile, ms))
                    {
                        var font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        var brush = new PdfGState();
						// brush.FillOpacity = 0.3f;
						brush.FillOpacity = (float)watermarkConfiguration.FontOpacity;
						// Iterate through the pages in the original file
						for (var i = 1; i <= oldFile.NumberOfPages; i++)
                        {
                            var page = stamper.GetOverContent(i);
                            page.BeginText();
                            page.SetFontAndSize(font, (float)watermarkConfiguration.FontSize);
                            page.SetGState(brush);


                            var width = oldFile.GetPageSize(i).Width;
                            var height = oldFile.GetPageSize(i).Height;

                            // Calculate the position for the first text (30% from the top)
                            var x1 = width / 2;
                            //var y1 = height * 0.7f; // 30% from the top
                            var y1 = height / 2;
                            page.SetTextMatrix(1, 0, 0, 1, x1, y1);
                            page.ShowTextAligned(Element.ALIGN_CENTER, $"Downloaded by: {username}, Date: {timestamp}", x1, y1, (float)watermarkConfiguration.Rotation);


                            // Calculate the position for the second text (60% from the top)
                            //var x2 = width / 2;
                            //var y2 = height * 0.4f; // 60% from the top
                            //page.SetTextMatrix(1, 0, 0, 1, x2, y2);
                            //page.ShowTextAligned(Element.ALIGN_CENTER, $"Downloaded by: {username}, Date: {timestamp}", x2, y2, 30);

                            page.EndText();
                        }
                     }


                    // Create a copy of the MemoryStream
                    var copy = new MemoryStream(ms.ToArray());
                    copy.CopyTo(outputStream);
                    outputStream.Close();

                    // Write the copy of the MemoryStream directly to the response body
                    //copy.CopyTo(Response.Body);
                }

            }
            return watermarkedFileName;
        }
        else
        {
            return string.Empty;
        }
    }

    //  public List<(string, int, int)> AddWatermarkToTiff(string path, string fileName, string originalFileName)
    //  {

    //WatermarkConfiguration watermarkConfiguration = _dbContext.WatermarkConfigurations.Where(x => x.ConfigurationFor == "Drawings").FirstOrDefault();
    //string extension = System.IO.Path.GetExtension(fileName);
    //      string inputFilePath = Path.Combine(path, fileName);
    //      string UserName = User.FindFirst(ClaimTypes.GivenName).Value;
    //      string text = $"Downloaded by: {UserName}, Date: {DateTime.Now}";
    //      string watermarkedFileName = "";
    //      string downloadfolder = "/Attachments/Downloads/";
    //      downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
    //      string watermarkedFilePath = Path.Combine(downloadfolder, watermarkedFileName);
    //      if (!Directory.Exists(downloadfolder))
    //      {
    //          Directory.CreateDirectory(downloadfolder);
    //      }
    //      originalFileName = ((originalFileName == "") ? fileName : originalFileName);

    //      List<(string, int, int)> tiffConvertedToImage = new List<(string, int, int)>();

    //MagickReadSettings settings = new MagickReadSettings
    //{
    //	Format = MagickFormat.Tiff
    //};

    //using (MagickImageCollection images = new MagickImageCollection())
    //{
    //	images.Read(inputFilePath, settings);
    //}

    //using (MagickImageCollection images = new MagickImageCollection(inputFilePath))
    //      {

    //          foreach (MagickImage image in images)
    //          {
    //              // Get the dimensions of the image
    //              int width = image.Width;
    //              int height = image.Height;

    //              // Calculate the position to center the text
    //              int textX = width / 2;
    //              int textY = height / 2;
    //              // Annotate the text on the image

    //              // Set the text properties
    //              //image.Settings.FontPointsize = height * 0.045; // Increase text size to 36
    //              image.Settings.FontPointsize = height * (float)watermarkConfiguration.FontSize; // Increase text size to 36
    //              image.Settings.FillColor = new MagickColor(MagickColors.Gray) { A = (ushort)(Quantum.Max / watermarkConfiguration.FontOpacity) }; 
    //              image.Settings.TextGravity = Gravity.Center; // Set text gravity to center
    //              image.Settings.TextInterlineSpacing = 10; // Set text interline spacing

    //              image.Annotate(text, new MagickGeometry(textX, (int)(height * 0.5), 20, 20), Gravity.Center, (float)watermarkConfiguration.Rotation);
    //              // image.Annotate(text, new MagickGeometry(textX, (int)(height * 0.66), 20, 20), Gravity.Center, 30);

    //              // Write the modified images to the output file
    //              string pngName = Guid.NewGuid().ToString() + ".jpeg";
    //              image.Write(downloadfolder + pngName);
    //              //tiffConvertedToImage.Add((pngName+"-"+counter+".png", width, height));
    //              tiffConvertedToImage.Add((pngName, width, height));

    //          }
    //      }

    //      // Read the modified image into a byte array
    //      // byte[] fileBytes = System.IO.File.ReadAllBytes(watermarkedFilePath);

    //      // Set the content type for TIFF files
    //      //  string contentType = "image/tiff";

    //      // Return the modified image in the response body
    //      return tiffConvertedToImage;

    //  }

    public List<(string, int, int)> AddWatermarkToTiff(string path, string fileName, string originalFileName)
    {
        WatermarkConfiguration watermarkConfiguration = _dbContext.WatermarkConfigurations.Where(x => x.ConfigurationFor == "Drawings").FirstOrDefault();
        string extension = System.IO.Path.GetExtension(fileName);
        string inputFilePath = Path.Combine(path, fileName);
        string UserName = User.FindFirst(ClaimTypes.GivenName).Value;
        string text = $"Downloaded by: {UserName}, Date: {DateTime.Now}";
        string watermarkedFileName = "";
        string downloadfolder = "/Attachments/Downloads/";
        downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
        string watermarkedFilePath = Path.Combine(downloadfolder, watermarkedFileName);

        if (!Directory.Exists(downloadfolder))
        {
            Directory.CreateDirectory(downloadfolder);
        }

        originalFileName = ((originalFileName == "") ? fileName : originalFileName);

        List<(string, int, int)> tiffConvertedToImage = new List<(string, int, int)>();

        try
        {
            using (MagickImageCollection images = new MagickImageCollection(inputFilePath))
            {
                foreach (MagickImage image in images)
                {
                    int width = image.Width;
                    int height = image.Height;

                    image.Settings.FontPointsize = height * (float)watermarkConfiguration.FontSize;
                    image.Settings.FillColor = new MagickColor(MagickColors.Gray) { A = (ushort)(Quantum.Max / watermarkConfiguration.FontOpacity) };
                    image.Settings.TextGravity = Gravity.Center;
                    image.Settings.TextInterlineSpacing = 10;

                    image.Annotate(text, new MagickGeometry(width / 2, height / 2, 20, 20), Gravity.Center, (float)watermarkConfiguration.Rotation);

                    string pngName = Guid.NewGuid().ToString() + ".jpeg";
                    image.Write(Path.Combine(downloadfolder, pngName));

                    tiffConvertedToImage.Add((pngName, width, height));
                }
            }
            return tiffConvertedToImage;

		}
        catch (Exception ex)
        {
            // Attempt to use LibTiff.Net

            //using (Tiff tiff = Tiff.Open(inputFilePath, "r"))
            //{
            //	int pageCount = tiff.NumberOfDirectories();
            //	for (int i = 0; i < pageCount; i++)
            //	{
            //		tiff.SetDirectory((short)i);

            //		int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            //		int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            //		// LibTiff.Net does not directly support drawing text; use your watermark logic here
            //		// Example: Generate image and annotate text using another library

            //		string pngName = Guid.NewGuid().ToString() + ".jpeg";
            //		tiffConvertedToImage.Add((pngName, width, height));
            //	}
            //}
            try
            {
				using (Tiff tiff = Tiff.Open(inputFilePath, "r"))
				{
					int pageCount = tiff.NumberOfDirectories();

					for (int i = 0; i < pageCount; i++)
					{
						tiff.SetDirectory((short)i);

						int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
						int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

						// Read the TIFF image data
						int[] raster = new int[width * height];
						if (!tiff.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
							throw new Exception("Could not read TIFF image");

						// Create a Bitmap from the raster data
						using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
						{
							// Convert raster data to Bitmap
							for (int y = 0; y < height; y++)
							{
								for (int x = 0; x < width; x++)
								{
									int rgba = raster[y * width + x];
									Color color = Color.FromArgb(
										(rgba >> 24) & 0xFF, // Alpha
										(rgba >> 0) & 0xFF,  // Red
										(rgba >> 8) & 0xFF,  // Green
										(rgba >> 16) & 0xFF  // Blue
									);

									// Flip the Y-axis: map to (x, height - y - 1)
									bitmap.SetPixel(x, y, color);
								}
							}

							// Add watermark
							using (Graphics graphics = Graphics.FromImage(bitmap))
							{
								string watermarkText = $"Downloaded by: {UserName}, Date: {DateTime.Now}";
								float fontSize = height * 0.02f; // 5% of the image height
								double fontOpacity = 0.5; // 50% opacity
								int alpha = (int)(255 * fontOpacity);
								Color color = Color.FromArgb(alpha, Color.Gray);
								Font font = new Font("Arial", fontSize, FontStyle.Bold);
								Brush brush = new SolidBrush(color);

								StringFormat stringFormat = new StringFormat
								{
									Alignment = StringAlignment.Center,
									LineAlignment = StringAlignment.Center
								};

								System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0, width, height);

								// Apply a slight rotation for the watermark
								graphics.TranslateTransform(width / 2, height / 2);
								graphics.RotateTransform(-30); // 15 degrees rotation
								graphics.TranslateTransform(-width / 2, -height / 2);

								graphics.DrawString(watermarkText, font, brush, rect, stringFormat);

								// Reset the transformation
								graphics.ResetTransform();
							}

							// Save the image as JPEG
							string pngName = Path.Combine(downloadfolder, Guid.NewGuid().ToString() + ".jpeg");
							bitmap.Save(pngName, ImageFormat.Jpeg);
							tiffConvertedToImage.Add((pngName, width, height));
						}
					}
				}
			}
            catch (Exception excep)
            {
				ErrorHandler.LogException(excep);
				
            }	

			return tiffConvertedToImage;

		}
	}

	public IActionResult DownloadExtractedFile(string Path, string fileName,int docId) 
    {
		MPCRS.Models.Attachment attachment = _dbContext.Attachments.Where(x => x.Attachment_Db_Key == docId).FirstOrDefault();
        if (attachment != null)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            Audit_log audit_Log = new Audit_log();
            audit_Log.table_name = "Attachment";
            audit_Log.Primary_key = docId;
            audit_Log.Event_Description = "Download File - " + fileName;
            audit_Log.Remarks = "From - MPL Documents - " + attachment.Orginal_File_Name;
            audit_Log.Updated_By = userId;
		    audit_Log.Updated_On = DateTime.Now;
            _dbContext.Add(audit_Log);
            _dbContext.SaveChanges();
        }
		string Extension = System.IO.Path.GetExtension(fileName);
		DocumentsViewModel documentsViewModel = new();
        if(Extension == ".tiff" || Extension == ".tif")
        {
			documentsViewModel.TiffToPngConvertedLocation = AddWatermarkToTiff(Path, fileName, fileName);
			List<string> pngFiles = new List<string>();

			string downloadfolder = "/Attachments/Downloads/";
			downloadfolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);

			foreach (var item in documentsViewModel.TiffToPngConvertedLocation)
			{
				pngFiles.Add(System.IO.Path.Combine(downloadfolder, item.Item1).Replace("\\", "/"));
			}

			if (Extension.ToLower().Contains(".tiff") == false)
			{
				var tiffPath = ConvertToMultiPageTiff(pngFiles, ".tif");
				var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
				var contentType = "application/octet-stream";
				if (!tiffPath.IsNullOrEmpty())
				{
					return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = tiffPath, originalFileName = fileName });
				}
				return File(fileBytes, contentType, fileName);
			}
			else
			{
				var tiffPath = ConvertToMultiPageTiff(pngFiles, "tiff");
				var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
				var contentType = "application/octet-stream";
				if (!tiffPath.IsNullOrEmpty())
				{
					return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = tiffPath, originalFileName = fileName });
				}
				return File(fileBytes, contentType, fileName);
			}
		}
        else if(Extension == ".pdf")
        {
			//href = "/DownloadFiles/AddWatermark/?path=@file.FilePath&fileName=@file.FileName&originalFileName=@file.FileName"
			return RedirectToAction("AddWatermark", new { path = Path , fileName = fileName, originalFileName = fileName }); 
			//AddWatermark(Path, fileName, fileName);
		}
		byte[] File_Bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Path,fileName));
		return File(File_Bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
	}

    public IActionResult ViewExtractedFiles(string path, string fileName, int docID)
    {
		MPCRS.Models.Attachment attachment = _dbContext.Attachments.Where(x => x.Attachment_Db_Key == docID).FirstOrDefault();
		if (attachment != null)
		{
			int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
			Audit_log audit_Log = new Audit_log();
			audit_Log.table_name = "Attachment";
			audit_Log.Primary_key = docID;
			audit_Log.Event_Description = "View File - " + fileName;
			audit_Log.Remarks = "From - MPL Documents - " + attachment.Orginal_File_Name;
			audit_Log.Updated_By = userId;
			audit_Log.Updated_On = DateTime.Now;
			_dbContext.Add(audit_Log);
			_dbContext.SaveChanges();
		}
		string extension = System.IO.Path.GetExtension(fileName);
		DocumentsViewModel documentsViewModel = new();
        documentsViewModel.File_type = extension;
		if (extension == ".pdf")
        {
			documentsViewModel.System_File_Name = AddWatermark_View(path, fileName, fileName);
		}
		else if(extension == ".tif")
        {
			documentsViewModel.TiffToPngConvertedLocation = AddWatermarkToTiff(path, fileName, fileName);
		}
		return PartialView(documentsViewModel);
    }

    public IActionResult DownloadFileWithoutWatermark(string path, string fileName, string originalFileName)
    {
        try
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(path, fileName));
			return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, originalFileName);

		}
        catch (Exception ex)
        {
            ErrorHandler.LogException(ex);
            throw;
        }
    }
	public IActionResult DownloadExtractedFileWithoutWaterMark(string Path, string fileName, int docId)
	{
		downloadFileData fileData = GetAttachmentFileInfo(docId, "Attachment", true);

		int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
		Audit_log audit_Log = new Audit_log();
		audit_Log.table_name = fileData.SourceTable;
		audit_Log.Primary_key = docId;
		//audit_Log.Event_Description = fileData.DisplayFileDesc +" - "+ fileName;
        audit_Log.Event_Description = "Download File without Watermark - " + fileName; 
        audit_Log.Remarks = fileData.DisplayFolderName +" - "+ fileData.DisplayFileName;
		audit_Log.Updated_By = userId;
		audit_Log.Updated_On = DateTime.Now;
		_dbContext.Add(audit_Log);
		_dbContext.SaveChanges();
		byte[] File_Bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(Path, fileName));
		return File(File_Bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
	}


    //  public IActionResult AddWatermarkToSinglePageTif(string inputFilePath, string originalFileName)
    //  {
    //      WatermarkConfiguration TiffwatermarkConfiguration = _dbContext.WatermarkConfigurations.Where(x => x.ConfigurationFor == "SinglePageDrawing").FirstOrDefault();
    //      string UserName = User.FindFirst(ClaimTypes.GivenName).Value;
    //      string watermarkText = $"Downloaded by: {UserName}, Date: {DateTime.Now}";
    //      using (Image image = Image.FromFile(inputFilePath))
    //      {

    //          using (Graphics graphics = Graphics.FromImage(image))
    //          {
    //              // Set watermark font and color
    //              Font font = new Font("Arial", (float)TiffwatermarkConfiguration.FontSize, FontStyle.Bold, GraphicsUnit.Pixel);
    //              int alpha = (int)TiffwatermarkConfiguration.FontOpacity; // Change this value to set the desired opacity
    //              Color color = Color.FromArgb(alpha, 255, 255, 255); // Semi-transparent white


    //              using (SolidBrush brush = new SolidBrush(color))
    //              {
    //                  // Determine position of the watermark to center it
    //                  SizeF textSize = graphics.MeasureString(watermarkText, font);
    //                  float x = (image.Width - textSize.Width) / 2; // Center horizontally
    //                  float y = (image.Height - textSize.Height) / 2; // Center vertically

    //                  // Set rotation angle (in degrees)
    //                  float angle = -(float)TiffwatermarkConfiguration.Rotation; // Change this value to your desired angle

    //                  // Rotate the graphics context
    //                  graphics.TranslateTransform(x + textSize.Width / 2, y + textSize.Height / 2); // Move to the center of the text
    //                  graphics.RotateTransform(angle); // Rotate the graphics

    //                  // Draw the watermark on the image
    //                  graphics.DrawString(watermarkText, font, brush, -textSize.Width / 2, -textSize.Height / 2); // Adjust position back
    //                  graphics.ResetTransform(); // Reset the transformation for future drawing

    //              }
    //          }


    //	string downloadfolder = "/Attachments/Downloads/";
    //	downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
    //          string completePath = Path.Combine(downloadfolder, originalFileName);
    //	image.Save(completePath, ImageFormat.Tiff);
    //	return RedirectToAction("CompressTiff", new { WatermarkedtiffPath = completePath, originalFileName  = originalFileName });
    //}
    //  }


    public IActionResult CompressTiff(string WatermarkedtiffPath, string originalFileName)
    {

        WatermarkConfiguration watermarkConfiguration = _dbContext.WatermarkConfigurations.FirstOrDefault(x => x.ConfigurationFor == "Drawings");

        using (Image image = Image.FromFile(WatermarkedtiffPath))
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Get JPEG encoder
                var jpegEncoder = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

                // Create encoder parameters for quality
                var jpegEncoderParameters = new EncoderParameters(1);

                // Get compression quality, defaulting to 75 if null
                long qualityValue = watermarkConfiguration?.Compression ?? 75; // Default to 75
                jpegEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)qualityValue); // Cast to long

                // Save the image as JPEG to the memory stream
                image.Save(memoryStream, jpegEncoder, jpegEncoderParameters);

                // Convert MemoryStream to byte array
                byte[] fileBytes = memoryStream.ToArray();

                // Set the content type
                string contentType = "application/octet-stream";

                // Return the file for download
                return File(fileBytes, contentType, originalFileName);
            }
        }

    }
}


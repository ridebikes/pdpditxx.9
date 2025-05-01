using System;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Forms;
using iText.Commons.Actions;


namespace pdpditxx
{
    internal class PdfProcess
    {
        #region Add Merge Annotation to Single PDF
        /// <summary>
        /// This method takes in text you want to write to the comment annots as breakText, the name of your input PDF and the name of the output pdf
        /// </summary>
        /// <param name="breakText"></param>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public static void AddMergeAnnot(string breakText, string inputFile, string outputFile)
        {
            //disable AGPL messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            //add annot to every first page

            using (PdfWriter outputWriter = new PdfWriter(outputFile))
            {
                using (PdfReader inputReader = new PdfReader(inputFile))
                {
                    using (PdfDocument annotPdfFile = new PdfDocument(inputReader, outputWriter))
                    {
                        annotPdfFile.GetFirstPage().AddAnnotation(new PdfTextAnnotation(new Rectangle(0, 0, 0, 0))
                            .SetTitle(new PdfString(breakText))
                            .SetContents(System.IO.Path.GetFileName(inputFile)));
                        annotPdfFile.Close();
                    }
                }
            }
        }
        #endregion

        #region Concatenate a directory of PDF's
        /// <summary>
        /// Takes an index of PDF files and concatenates them together in that order.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="mergeIndex"></param>
        /// <param name="inputFile"></param>
        /// <param name="zipWorkDir"></param>
        /// <exception cref="InvalidDataException"></exception>
        public static void Concat(AppSettings.Root appSettings, List<string> mergeIndex, FileInfo inputFile, string zipWorkDir)
        {
            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            //code to concat PDF's
            DateTime actionStartTime = DateTime.Now;
            string outputFile = $"{zipWorkDir}zipOutput{System.IO.Path.DirectorySeparatorChar}{System.IO.Path.GetFileNameWithoutExtension(inputFile.FullName)}.pdf";

            using (PdfWriter thisWriter = new PdfWriter(outputFile, new WriterProperties().UseSmartMode()))
            {
                using (PdfDocument thisDocument = new PdfDocument(thisWriter))
                {
                    PdfMerger mergedPDF = new PdfMerger(thisDocument);
                    for (int i = 0; i < mergeIndex.Count; i++)
                    {
                        // keep track of what PDF we are running in case it crashes
                        string currentPdf = mergeIndex[i];
                        string annotPdfFile = $@"{zipWorkDir}{System.IO.Path.GetFileNameWithoutExtension(mergeIndex[i])}.annot.pdf";

                        if (appSettings.Settings.Concatenation.AddDocBreak)
                        {
                            try
                            {
                                //add annot to every first page
                                AddMergeAnnot(appSettings.Settings.Concatenation.BreakText, currentPdf, annotPdfFile);
                                currentPdf = annotPdfFile;
                            }
                            catch (Exception e)
                            {
                                mergedPDF.Close();
                                throw new InvalidDataException($"Annotation|{i + 1}|{System.IO.Path.GetFileName(mergeIndex[i])}", e);
                            }
                        }

                        try
                        {
                            using (PdfReader currentPdfReader = new PdfReader(currentPdf))
                            {
                                using (PdfDocument currentDocument = new PdfDocument(currentPdfReader))
                                {
                                    mergedPDF.Merge(currentDocument, 1, currentDocument.GetNumberOfPages());
                                    currentDocument.Close();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mergedPDF.Close();
                            throw new InvalidDataException($"Merging|{i + 1}|{System.IO.Path.GetFileName(mergeIndex[i])}", e);
                        }

                        if (appSettings.Settings.Concatenation.AddDocBreak)
                        {
                            File.Delete(annotPdfFile);
                        }
                        File.Delete(mergeIndex[i]);
                    }
                    mergedPDF.Close();
                }
            }

            Console.WriteLine($"iText PDF Merge completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");

        }
        #endregion

        #region Copy a single PDF
        /// <summary>
        /// Makes copies of a single pdf into a new PDF. Handy to make larger input for testing.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="inputFile"></param>
        /// <param name="zipWorkDir"></param>
        /// <exception cref="InvalidDataException"></exception>
        public static void MakeCopies(AppSettings.Root appSettings, FileInfo inputFile, string zipWorkDir)
        {
            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            //code to copy PDF's
            DateTime actionStartTime = DateTime.Now;
            string outputFile = $"{zipWorkDir}zipOutput{System.IO.Path.DirectorySeparatorChar}{System.IO.Path.GetFileNameWithoutExtension(inputFile.FullName)}.pdf";

            using (PdfWriter thisWriter = new PdfWriter(outputFile, new WriterProperties().UseSmartMode()))
            {
                using (PdfDocument thisDocument = new PdfDocument(thisWriter))
                {
                    PdfMerger mergedPDF = new PdfMerger(thisDocument);
                    for (int i = 0; i < appSettings.Settings.MakeCopies.NumberOfCopies; i++)
                    {
                        // keep track of what PDF we are running in case it crashes
                        string currentPdf = $"{zipWorkDir}{inputFile.Name}";
                        string annotPdfFile = $@"{zipWorkDir}{System.IO.Path.GetFileNameWithoutExtension(inputFile.Name)}.annot.pdf";

                        if (appSettings.Settings.Concatenation.AddDocBreak)
                        {
                            try
                            {
                                //add annot to every first page
                                AddMergeAnnot(appSettings.Settings.Concatenation.BreakText, currentPdf, annotPdfFile);
                                currentPdf = annotPdfFile;
                            }
                            catch (Exception e)
                            {
                                mergedPDF.Close();
                                throw new InvalidDataException($"Annotation|{i + 1}|{inputFile.Name}", e);
                            }
                        }

                        try
                        {
                            using (PdfReader currentPdfReader = new PdfReader(currentPdf))
                            {
                                using (PdfDocument currentDocument = new PdfDocument(currentPdfReader))
                                {
                                    mergedPDF.Merge(currentDocument, 1, currentDocument.GetNumberOfPages());
                                    currentDocument.Close();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mergedPDF.Close();
                            throw new InvalidDataException($"Merging|{i + 1}|{inputFile.Name}", e);
                        }
                    }
                    mergedPDF.Close();
                }
            }

            Console.WriteLine($"iText PDF Merge completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");

        }
        #endregion

        #region SmartSave a directory of PDF's

        public static void SmartSave(AppSettings.Root appSettings, FileInfo inputFile, string zipWorkDir)
        {
            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            string outputFile = $"{zipWorkDir}zipOutput{System.IO.Path.DirectorySeparatorChar}{System.IO.Path.GetFileNameWithoutExtension(inputFile.FullName)}.pdf";
            string currentPdf = inputFile.FullName;
            string annotPdfFile = $"{zipWorkDir}{System.IO.Path.GetFileNameWithoutExtension(inputFile.FullName)}.annot.pdf";
            string flatPdfFile = $"{zipWorkDir}{System.IO.Path.GetFileNameWithoutExtension(inputFile.FullName)}.flat.pdf";
            // In smart mode when resources (such as fonts, images,...) are encountered, a reference to these resources is saved in a cache,
            // so that they can be reused. This requires more memory, but reduces the file size of the resulting PDF document.

            // also available : setFullCompressionMode
            // set CompressionLevel (Defines the level of compression for the document.
            // See CompressionConstants: BEST_COMPRESSION, BEST_SPEED, DEFAULT_COMPRESSION, NO_COMPRESSION, UNDEFINED_COMPRESSION

            if (appSettings.Settings.SmartSaving.FlattenAcroforms)
            {
                using (PdfWriter flatWriter = new PdfWriter(flatPdfFile))
                {
                    using (PdfReader thisReader = new PdfReader(currentPdf))
                    {
                        if (appSettings.Settings.SmartSaving.RemovePassword)
                        {
                            thisReader.SetUnethicalReading(true);
                        }
                        using (PdfDocument pdfFlattener = new PdfDocument(thisReader, flatWriter))
                        {
                            PdfAcroForm thisForm = PdfAcroForm.GetAcroForm(pdfFlattener, true);
                            thisForm.FlattenFields();
                            pdfFlattener.Close();
                            currentPdf = flatPdfFile;
                        }
                    }
                }
            }

            if (appSettings.Settings.SmartSaving.StripComments)
            {
                //Strip annot from every page
                using (PdfWriter annotWriter = new PdfWriter(annotPdfFile))
                {
                    using (PdfReader thisReader = new PdfReader(currentPdf))
                    {
                        if (appSettings.Settings.SmartSaving.RemovePassword)
                        {
                            thisReader.SetUnethicalReading(true);
                        }

                        using (PdfDocument pdfAnnotStripper = new PdfDocument(thisReader, annotWriter))
                        {
                            for (int i = 1; i <= pdfAnnotStripper.GetNumberOfPages(); i++)
                            {
                                foreach (PdfAnnotation annotation in pdfAnnotStripper.GetPage(i).GetAnnotations())
                                {
                                    pdfAnnotStripper.GetPage(i).RemoveAnnotation(annotation);
                                }
                            }
                            pdfAnnotStripper.GetOutlines(true).RemoveOutline();
                            pdfAnnotStripper.Close();
                            currentPdf = annotPdfFile;
                        }
                    }
                }
            }


            using (PdfWriter thisWriter = new PdfWriter(outputFile, new WriterProperties().UseSmartMode()))
            {
                using (PdfReader thisReader = new PdfReader(currentPdf))
                {
                    if (appSettings.Settings.SmartSaving.RemovePassword)
                    {
                        thisReader.SetUnethicalReading(true);
                    }

                    using (PdfDocument thisDocument = new PdfDocument(thisWriter))
                    {
                        PdfMerger mergedPDF = new PdfMerger(thisDocument);

                        using (PdfDocument sourceDocument = new PdfDocument(thisReader))
                        {
                            mergedPDF.Merge(sourceDocument, 1, sourceDocument.GetNumberOfPages());
                            sourceDocument.Close();
                            mergedPDF.Close();
                        }
                    }
                }
            }
        }

        #endregion

        #region Split a PDF with an external index
        /// <summary>
        /// This method takes in a String zip working director, a list of the index you intend to split in SplitIndex form, and an inputfile in FileInfo format
        /// </summary>
        /// <param name="zipWorkDir"></param>
        /// <param name="splitIndex"></param>
        /// <param name="inputFile"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Split(string zipWorkDir, List<SplitIndex> splitIndex, FileInfo inputFile)
        {
            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            DateTime actionStartTime = DateTime.Now;

            if (Directory.GetFiles(zipWorkDir, "*.pdf").Length > 1)
            {
                Console.Error.WriteLine($"Zip contains more than one PDF to split.");
                throw new InvalidOperationException($"Zip contains more than one PDF to split.");
            }

            using (PdfDocument inputPdf = new PdfDocument(new PdfReader(inputFile.FullName)))
            {
                foreach (SplitIndex index in splitIndex)
                {
                    try
                    {
                        var split = new ImprovedSplitter(inputPdf, PageRange => new PdfWriter(index.FileName));
                        var result = split.ExtractPageRange(new PageRange($"{index.FirstPage}-{index.LastPage}"));
                        result.Close();
                    }
                    catch (Exception e)
                    {
                        if (index.FirstPage > inputPdf.GetNumberOfPages() || index.PageRange + index.LastPage < inputPdf.GetNumberOfPages())
                        {
                            throw new ArgumentOutOfRangeException($"Split|{index.Counter}|{System.IO.Path.GetFileName(index.FileName)}|{inputPdf.GetNumberOfPages()}", e);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            Console.WriteLine($"iText PDF Split completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");
        }

        public class ImprovedSplitter : PdfSplitter
        {
            private Func<PageRange, PdfWriter> nextWriter;
            public ImprovedSplitter(PdfDocument pdfDocument, Func<PageRange, PdfWriter> nextWriter) : base(pdfDocument)
            {
                this.nextWriter = nextWriter;
            }

            protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
            {
                return nextWriter.Invoke(documentPageRange);
            }
        }

        #endregion

#region Scale Shift Rotate - acts on a trigger of all, even, odd or xydiff
/// <summary>
/// This takes in a pdf and goes through the pages on a trigger - all, odd, even or xydiff. It will scale and shift the pages as asked.
/// </summary>
/// <param name="appSettings"></param>
/// <param name="inputFile"></param>
/// <param name="outputFile"></param>
/// <param name="pageX"></param>
/// <param name="pageY"></param>
/// <param name="scaleX"></param>
/// <param name="scaleY"></param>
/// <param name="shiftX"></param>
/// <param name="shiftY"></param>
/// <param name="degreesRotation"></param>
/// <param name="trigger"></param>
/// <exception cref="InvalidOperationException"></exception>
public static void ScaleShiftRotate(AppSettings.Root appSettings, FileInfo inputFile, string outputFile, int pageX, int pageY, double scaleX, double scaleY, double shiftX, double shiftY, int degreesRotation, string trigger)
{
    DateTime actionStartTime = DateTime.Now;

    // disable AGPL license messaging
    EventManager.AcknowledgeAgplUsageDisableWarningMessage();

    // using iText 9.1.0 with Affine Transform
    // https://api.itextpdf.com/iText/java/9.1.0/com/itextpdf/kernel/geom/AffineTransform.html

    using (PdfReader inputReader = new PdfReader(inputFile.FullName))
    {
        using (PdfDocument inputDocument = new PdfDocument(inputReader))
        {
            using (PdfWriter outputWriter = new PdfWriter(outputFile))
            {
                using (PdfDocument outputDocument = new PdfDocument(outputWriter))
                {
                    int totalPages = inputDocument.GetNumberOfPages();

                    // what direction do they want - portrait or landscape - compare requested values to find out
                    bool isLandscape = true ? (pageX > pageY) : false;

                    // do they want it autoscaled
                    bool autoScale = true ? scaleX == 0 && scaleY == 0 : false;

                    for (int i = 1; i <= totalPages; i++)
                    {
                        bool processPage = false;

                        // page changes would be based on the value of i + 1 since that is the page counter (first page is 0)
                        if (trigger.ToLower() == "odd")
                        {
                            //only set processPage true for even pages 
                            if (i % 2 != 0)
                            {
                                processPage = true;
                            }
                        }
                        else if (trigger.ToLower() == "even")
                        {
                            //only set processPage true for odd pages
                            if (i % 2 == 0)
                            {
                                processPage = true;
                            }
                        }
                        else if (trigger.ToLower() == "all")
                        {
                            processPage = true;
                        }
                        else if (trigger.ToLower() == "xydiff")
                        {
                            //need the page dimentions to check
                            PdfPage inputPage = inputDocument.GetPage(i);

                            // do they want us to look for portrait or landscape pages to fix
                            // landscape pages
                            if (isLandscape) 
                            {
                                // rotate portait pages to landscape
                                if (inputPage.GetMediaBox().GetHeight() > inputPage.GetMediaBox().GetWidth())
                                {
                                    processPage = true;
                                }
                            }

                            // portrait pages
                            if (!isLandscape)
                            {
                                // rotate the landscape pages to portrait
                                if (inputPage.GetMediaBox().GetWidth() > inputPage.GetMediaBox().GetHeight())
                                {
                                    processPage = true;
                                }
                            }
                        }

                        if (processPage)
                        {
                            // Get page
                            PdfPage inputPage = inputDocument.GetPage(i);
                            // Set new page output size
                            PageSize outputSize = new PageSize(new Rectangle(pageX, pageY));
                            // create new page of that size
                            PdfPage outputPage = outputDocument.AddNewPage(outputSize);
                            int backupRotation = degreesRotation;

                            double tX = 0.0;
                            double tY = 0.0;
                            double newX = (double)pageX;
                            double newY = (double)pageY;

                            switch (degreesRotation)
                            {
                                case 0:
                                    if (inputPage.GetRotation() == 0)
                                    {
                                        break;
                                    }

                                    if (inputPage.GetRotation() == 90)
                                    {
                                        degreesRotation = 90;
                                        tY = newY;
                                        break;
                                    }

                                    if (inputPage.GetRotation() == 180)
                                    {
                                        degreesRotation = 180;
                                        tX = newX;
                                        tY = newY;
                                        break;
                                    }

                                    if (inputPage.GetRotation() == 270)
                                    {
                                        degreesRotation = 270;
                                        tX = newX;
                                        break;
                                    }

                                    break;
                                case 90:
                                    tY = newY;
                                    break;
                                case 180:
                                    if (trigger.ToLower() == "xydiff")
                                    {
                                        throw new InvalidOperationException("cannot rotate 180 with xydiff");
                                    }
                                    tX = newX;
                                    tY = newY;
                                    break;
                                case 270:
                                    tX = newX;
                                    break;
                                default:
                                    throw new InvalidOperationException("Allowed rotation values : 0, 90, 180, or 270");
                            }

                            if (autoScale && (degreesRotation == 0 || degreesRotation == 180))
                            {
                                scaleX = pageX / inputPage.GetMediaBox().GetWidth();
                                scaleY = pageY / inputPage.GetMediaBox().GetHeight();
                            }

                            if (autoScale && (degreesRotation == 90 || degreesRotation == 270))
                            {
                                scaleX = pageY / inputPage.GetMediaBox().GetWidth();
                                scaleY = pageX / inputPage.GetMediaBox().GetHeight();
                            }

                            // setup page transform Matrix
                            AffineTransform transformationMatrix = new AffineTransform(scaleX, 0, 0, scaleY, shiftX, shiftY);
                            //create canvas
                            PdfCanvas destCanvas = new PdfCanvas(outputPage);

                            // apply the rotation (in radians)
                            double rotRadians = degreesRotation * (Math.PI / 180);
                            //transformationMatrix = AffineTransform.GetRotateInstance(-rotRadians, 320, 396);

                            transformationMatrix.Translate(tX, tY);
                            transformationMatrix.Rotate(-rotRadians);

                            // concat matrix to page
                            destCanvas.ConcatMatrix(transformationMatrix);

                            // grab page data as FormXObject
                            PdfFormXObject origCopy = inputPage.CopyAsFormXObject(outputDocument);

                            // add to our canvas
                            destCanvas.AddXObject(origCopy);

                            // finalize canvas for next iteration
                            destCanvas = new PdfCanvas(outputPage);

                            degreesRotation = backupRotation;
                        }
                        else 
                        {
                            PdfPage inputPage = inputDocument.GetPage(i);
                            // Set new page output size
                            PageSize outputSize = new PageSize(new Rectangle(pageX, pageY));
                            // create new page of that size
                            PdfPage outputPage = outputDocument.AddNewPage(outputSize);
                            PdfCanvas destCanvas = new PdfCanvas(outputPage);
                            // grab page data as FormXObject
                            PdfFormXObject origCopy = inputPage.CopyAsFormXObject(outputDocument);
                            // add to our canvas
                            destCanvas.AddXObject(origCopy);
                            // finalize canvas for next iteration
                            destCanvas = new PdfCanvas(outputPage);

                            //PdfMerger mergedPDF = new PdfMerger(outputDocument);
                            //mergedPDF.Merge(inputDocument, i, i);
                        }
                    }
                }
            }
        }
    }
    Console.WriteLine($"iText Scale and Rotate completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");
}

#endregion
        #region Scale Shift Rotate - acts on a trigger of all, even, odd or xydiff
        /// <summary>
        /// This takes in a pdf and goes through the pages on a trigger - all, odd, even or xydiff. It will scale and shift the pages as asked.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="pageX"></param>
        /// <param name="pageY"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        /// <param name="degreesRotation"></param>
        /// <param name="trigger"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ScaleShiftRotate(AppSettings.Root appSettings, FileInfo inputFile, string outputFile, int pageX, int pageY, double scaleX, double scaleY, double shiftX, double shiftY, int degreesRotation, string trigger)
        {
            DateTime actionStartTime = DateTime.Now;

            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            // using iText 9.1.0 with Affine Transform
            // https://api.itextpdf.com/iText/java/9.1.0/com/itextpdf/kernel/geom/AffineTransform.html

            using (PdfReader inputReader = new PdfReader(inputFile.FullName))
            {
                using (PdfDocument inputDocument = new PdfDocument(inputReader))
                {
                    using (PdfWriter outputWriter = new PdfWriter(outputFile))
                    {
                        using (PdfDocument outputDocument = new PdfDocument(outputWriter))
                        {
                            int totalPages = inputDocument.GetNumberOfPages();

                            for (int i = 1; i <= totalPages; i++)
                            {
                                bool processPage = false;

                                // page changes would be based on the value of i + 1 since that is the page counter (first page is 0)
                                if (trigger.ToLower() == "odd")
                                {
                                    //only set processPage true for even pages 
                                    if (i % 2 != 0)
                                    {
                                        processPage = true;
                                    }
                                }
                                else if (trigger.ToLower() == "even")
                                {
                                    //only set processPage true for odd pages
                                    if (i % 2 == 0)
                                    {
                                        processPage = true;
                                    }
                                }
                                else if (trigger.ToLower() == "all")
                                {
                                    processPage = true;
                                }
                                else if (trigger.ToLower() == "xydiff")
                                {
                                    //need the page dimentions to check
                                    PdfPage inputPage = inputDocument.GetPage(i);
                                    if (inputPage.GetMediaBox().GetWidth() != pageX || inputPage.GetMediaBox().GetHeight() != pageY)
                                    {
                                        processPage = true;
                                    }

                                }

                                if (processPage)
                                {
                                    // Get page
                                    PdfPage inputPage = inputDocument.GetPage(i);
                                    // Set new page output size
                                    PageSize outputSize = new PageSize(new Rectangle(pageX, pageY));
                                    // create new page of that size
                                    PdfPage outputPage = outputDocument.AddNewPage(outputSize);
                                    // setup page transform Matrix
                                    AffineTransform transformationMatrix = new AffineTransform(scaleX, 0, 0, scaleY, shiftX, shiftY);
                                    //create canvas
                                    PdfCanvas destCanvas = new PdfCanvas(outputPage);
                                    // concat matrix to page
                                    destCanvas.ConcatMatrix(transformationMatrix);
                                    //grab page data as FormXObject
                                    PdfFormXObject origCopy = inputPage.CopyAsFormXObject(outputDocument);
                                    //add to our canvas
                                    destCanvas.AddXObject(origCopy);
                                    // finalize canvas for next iteration
                                    destCanvas = new PdfCanvas(outputPage);

                                    // Set page rotation
                                    switch (degreesRotation)
                                    {
                                        case 0:
                                            outputPage.SetRotation(0);
                                            break;
                                        case 90:
                                            outputPage.SetRotation(90);
                                            break;
                                        case 180:
                                            outputPage.SetRotation(180);
                                            break;
                                        case 270:
                                            outputPage.SetRotation(270);
                                            break;
                                        default:
                                            throw new InvalidOperationException("Cannot rotate to anything other than 0, 90, 180, or 270");
                                    }
                                }
                                else 
                                {
                                    PdfMerger mergedPDF = new PdfMerger(outputDocument);
                                    mergedPDF.Merge(inputDocument, i, i);
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"iText Scale and Rotate completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");
        }

        #endregion

 #region Scale Shift Rotate - acts on an external index
 /// <summary>
 /// This takes in a pdf and goes through the pages on a trigger - all, odd, even or xydiff. It will scale and shift the pages as asked.
 /// </summary>
 /// <param name="appSettings"></param>
 /// <param name="inputFile"></param>
 /// <param name="outputFile"></param>
 /// <param name="pageX"></param>
 /// <param name="pageY"></param>
 /// <param name="scaleX"></param>
 /// <param name="scaleY"></param>
 /// <param name="shiftX"></param>
 /// <param name="shiftY"></param>
 /// <param name="degreesRotation"></param>
 /// <param name="trigger"></param>
 /// <exception cref="InvalidOperationException"></exception>
 public static void ScaleShiftRotate(AppSettings.Root appSettings, string zipWorkDir, List<SRIndex> thisIndex, FileInfo inputFile)
 {
     //DateTime actionStartTime = DateTime.Now;

     if (Directory.GetFiles(zipWorkDir, "*.pdf").Length > 1)
     {
         Console.WriteLine($"Zip contains more than one PDF to rotate.");
         throw new InvalidOperationException($"Zip contains more than one PDF to rotate.");
     }

     string outputFile = $"{zipWorkDir}zipOutput{System.IO.Path.DirectorySeparatorChar}{inputFile.Name}";

     // disable AGPL license messaging
     EventManager.AcknowledgeAgplUsageDisableWarningMessage();

     // using iText 9.1.0 with Affine Transform
     // https://api.itextpdf.com/iText/java/9.1.0/com/itextpdf/kernel/geom/AffineTransform.html

     using (PdfReader inputReader = new PdfReader(inputFile.FullName))
     {
         using (PdfDocument inputDocument = new PdfDocument(inputReader))
         {
             using (PdfWriter outputWriter = new PdfWriter(outputFile))
             {
                 using (PdfDocument outputDocument = new PdfDocument(outputWriter))
                 {
                     bool found = false;
                     int totalPages = inputDocument.GetNumberOfPages();
                     
                     // NOTE : The first page in iText is 1, it does not count from 0
                     for (int i = 1; i <= totalPages; i++)
                     {
                         foreach (SRIndex index in thisIndex)
                         {
                             if (i == index.Page && !found)
                             {
                                 found = true;
                                 // Get page
                                 PdfPage inputPage = inputDocument.GetPage(i);
                                 // Set new page output size
                                 PageSize outputSize = new PageSize(new Rectangle(index.PageSizeX, index.PageSizeY));
                                 // create new page of that size
                                 PdfPage outputPage = outputDocument.AddNewPage(outputSize);

                                 double tX = 0.0;
                                 double tY = 0.0;
                                 double newX = (double)index.PageSizeX;
                                 double newY = (double)index.PageSizeY;

                                 switch (index.DegreesRotation)
                                 {
                                     case 0:
                                         if (inputPage.GetRotation() == 0)
                                         {
                                             break;
                                         }

                                         if (inputPage.GetRotation() == 90)
                                         {
                                             index.DegreesRotation = 90;
                                             tY = newY;
                                             break;
                                         }

                                         if (inputPage.GetRotation() == 180)
                                         {
                                             index.DegreesRotation = 180;
                                             tX = newX;
                                             tY = newY;
                                             break;
                                         }

                                         if (inputPage.GetRotation() == 270)
                                         {
                                             index.DegreesRotation = 270;
                                             tX = newX;
                                             break;
                                         }

                                         break;
                                     case 90:
                                         tY = newY;
                                         break;
                                     case 180:
                                         tX = newX;
                                         tY = newY;
                                         break;
                                     case 270:
                                         tX = newX;
                                         break;
                                     default:
                                         throw new InvalidOperationException("Allowed rotation values : 0, 90, 180, or 270");
                                 }

                                 // setup page transform Matrix
                                 AffineTransform transformationMatrix = new AffineTransform(index.ScaleX, 0, 0, index.ScaleY, index.ShiftX, index.ShiftY);

                                 // apply the rotation (in radians)
                                 double rotRadians = index.DegreesRotation * (Math.PI / 180);
                                 transformationMatrix.Translate(tX, tY);
                                 transformationMatrix.Rotate(-rotRadians);

                                 // create canvas
                                 PdfCanvas destCanvas = new PdfCanvas(outputPage);

                                 // concat matrix to page
                                 destCanvas.ConcatMatrix(transformationMatrix);

                                 // grab page data as FormXObject
                                 PdfFormXObject origCopy = inputPage.CopyAsFormXObject(outputDocument);

                                 // add to our canvas
                                 destCanvas.AddXObject(origCopy);

                                 // finalize canvas for next iteration
                                 destCanvas = new PdfCanvas(outputPage);

                             }
                         }
                         if (!found)
                         {
                             PdfPage inputPage = inputDocument.GetPage(i);
                             // Set new page output size
                             PageSize outputSize = new PageSize(new Rectangle(inputPage.GetMediaBox().GetWidth(), inputPage.GetMediaBox().GetHeight()));
                             // create new page of that size
                             PdfPage outputPage = outputDocument.AddNewPage(outputSize);
                             PdfCanvas destCanvas = new PdfCanvas(outputPage);
                             // grab page data as FormXObject
                             PdfFormXObject origCopy = inputPage.CopyAsFormXObject(outputDocument);
                             // add to our canvas
                             destCanvas.AddXObject(origCopy);
                             // finalize canvas for next iteration
                             destCanvas = new PdfCanvas(outputPage);

                             //PdfMerger mergedPDF = new PdfMerger(outputDocument);
                             //mergedPDF.Merge(inputDocument, i, i);
                         }
                         found = false;
                     }
                 }
             }
         }
     }
     //Console.WriteLine($"iText Scale and Rotate completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");
 }

 #endregion
        #region Convert PDF's to Text
        /// <summary>
        /// Takes in a PDF and converts it to a text file.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="zipWorkDir"></param>
        public static void TextConvert(FileInfo inputFile, string zipWorkDir)
        {
            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            string outputFile = $@"{zipWorkDir}zipOutput{System.IO.Path.DirectorySeparatorChar}{System.IO.Path.GetFileNameWithoutExtension(inputFile.FullName)}.txt";

            using (PdfDocument inputPDF = new PdfDocument(new PdfReader(inputFile.FullName)))
            {
                using (StreamWriter outputText = new StreamWriter(outputFile))
                {
                    for (int i = 1; i <= inputPDF.GetNumberOfPages(); i++)
                    {
                        var page = inputPDF.GetPage(i);
                        outputText.WriteLine();
                        outputText.WriteLine($"||P{i:0000000000}||");
                        outputText.WriteLine();
                        outputText.WriteLine(PdfTextExtractor.GetTextFromPage(page));
                    }

                    outputText.Close();
                }
            }

        }
        #endregion
    }
}

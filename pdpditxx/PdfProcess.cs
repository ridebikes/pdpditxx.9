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

        #region Scale and Rotate a PDF
        /// <summary>
        /// This takes in a PDF and page size you want and scales it, and also rotates pages if needed.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="pageWidth"></param>
        /// <param name="pageHeight"></param>
        public static void ScaleAndRotate(AppSettings.Root appSettings, FileInfo inputFile, FileInfo outputFile, float pageWidth, float pageHeight)
        {
            DateTime actionStartTime = DateTime.Now;

            // disable AGPL license messaging
            EventManager.AcknowledgeAgplUsageDisableWarningMessage();

            // using iText 9.1.0 with Affine Transform
            // https://api.itextpdf.com/iText/java/9.1.0/com/itextpdf/kernel/geom/AffineTransform.html

            PageSize targetSize = new PageSize(new Rectangle(pageWidth, pageHeight));

            using (PdfReader inputReader = new PdfReader(inputFile.FullName))
            {
                using (PdfDocument inputDocument = new PdfDocument(inputReader))
                {
                    using (PdfWriter outputWriter = new PdfWriter(outputFile.FullName))
                    {
                        using (PdfDocument outputDocument = new PdfDocument(outputWriter))
                        {
                            for (int i = 1; i <= inputDocument.GetNumberOfPages(); i++)
                            {
                                PdfPage origPage = inputDocument.GetPage(i);

                                Rectangle thisRectangle = origPage.GetPageSize();

                                PageSize thisSize = new PageSize(thisRectangle);

                                bool needsRotated = (thisSize.GetHeight() >= thisSize.GetWidth()) != (targetSize.GetHeight() >= targetSize.GetWidth());

                                PageSize rotatedSize = thisSize;

                                if (needsRotated)
                                {
                                    rotatedSize = new PageSize(new Rectangle(thisSize.GetHeight(), thisSize.GetWidth()));
                                }

                                PdfPage destPage = outputDocument.AddNewPage(targetSize);
                                AffineTransform transformationMatrix = new AffineTransform();

                                //double scale = Math.Min(targetSize.GetWidth() / rotatedSize.GetWidth(), targetSize.GetHeight() / rotatedSize.GetHeight());
                                double scaleX = targetSize.GetWidth() / rotatedSize.GetWidth();
                                double scaleY = targetSize.GetHeight() / rotatedSize.GetHeight();

                                //solimar runs scale and shift, so we are going to scale, but to nothing
                                transformationMatrix = AffineTransform.GetScaleInstance(scaleX, scaleY);

                                if (needsRotated)
                                {
                                    //rotate the page (in Radians)
                                    transformationMatrix = AffineTransform.GetRotateInstance(Math.PI / 2, thisSize.GetHeight() / 2, thisSize.GetWidth() / 2);
                                }


                                //create canvas
                                PdfCanvas destCanvas = new PdfCanvas(destPage);

                                //run Affine config
                                destCanvas.ConcatMatrix(transformationMatrix);

                                //grab page data as FormXObject
                                PdfFormXObject origCopy = origPage.CopyAsFormXObject(outputDocument);

                                float X = (float)((targetSize.GetWidth() - thisSize.GetWidth() * scaleX) / 2);
                                float Y = (float)((targetSize.GetHeight() - thisSize.GetHeight() * scaleY) / 2);

                                //add to our canvas
                                destCanvas.AddXObjectAt(origCopy, X, Y);

                                //finalize canvas for next iteration
                                destCanvas = new PdfCanvas(destPage);
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
            DateTime actionStartTime = DateTime.Now;

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
                            bool tookAction = false;
                            int totalPages = inputDocument.GetNumberOfPages();
                            
                            // NOTE : The first page in iText is 1, it does not count from 0
                            for (int i = 1; i <= totalPages; i++)
                            {
                                tookAction = false;

                                foreach (SRIndex index in thisIndex)
                                {
                                    if (i != index.Page && !tookAction)
                                    {
                                        PdfMerger mergedPDF = new PdfMerger(outputDocument);
                                        mergedPDF.Merge(inputDocument, i, i);
                                        tookAction = true;
                                    }
                                    else if (i == index.Page && !tookAction)
                                    {
                                        // Get page
                                        PdfPage inputPage = inputDocument.GetPage(i);
                                        // Set new page output size
                                        PageSize outputSize = new PageSize(new Rectangle(index.PageSizeX, index.PageSizeY));
                                        // create new page of that size
                                        PdfPage outputPage = outputDocument.AddNewPage(outputSize);
                                        // setup page transform Matrix
                                        AffineTransform transformationMatrix = new AffineTransform(index.ScaleX, 0, 0, index.ScaleY, index.ShiftX, index.ShiftY);
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
                                        switch (index.DegreesRotation)
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
                                        tookAction = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"iText Scale and Rotate completed in {DateTime.Now.Subtract(actionStartTime):c} for file {inputFile.Name}");
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

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace pdpditxx
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Check Args from command line

            if (args.Length < 1)
            {
                Console.Error.WriteLine("usage : <command> <arg1 input filename>");
                Environment.Exit(1);
            }

            #endregion

            #region Get server settings and set variables

            // Console.WriteLine("Starting iText processing");
            FileInfo inputFile = new FileInfo($"{Path.GetFullPath(args[0])}");
            Console.WriteLine($"Processing file : {inputFile.FullName}");

            //Get settings from json file
            IConfigurationBuilder serverBuilder = new ConfigurationBuilder().AddJsonFile($"serverconfig.json", false, true);
            IConfigurationRoot serverConfig = serverBuilder.Build();
            ServerConfig.Root thisServerConfig = serverConfig.Get<ServerConfig.Root>();

            string outDir = thisServerConfig.Directories.OutDir;
            string workDir = thisServerConfig.Directories.WorkDir;

            // make place to unzip file to
            Guid zipGuid = Guid.NewGuid();
            string zipWorkDir = $"{workDir}{zipGuid}{Path.DirectorySeparatorChar}";
            string zipOutDir = $"{zipWorkDir}zipOutput{Path.DirectorySeparatorChar}";
            Directory.CreateDirectory(zipOutDir);
            string appConfigDir = $"{zipWorkDir}jsonAppConfig{Path.DirectorySeparatorChar}";
            Directory.CreateDirectory(appConfigDir);
            string errorDir = $"{zipWorkDir}errorDir{Path.DirectorySeparatorChar}";
            Directory.CreateDirectory(errorDir);

            // create the messagelist for our errors
            List<ErrorMessage> messageList = new List<ErrorMessage>();

            // create appSettings class to put JSON config data into
            AppSettings.Root appSettings = new AppSettings.Root();

            #endregion

            #region Make sure input file exists

            if (!inputFile.Exists)
            {
                Console.Error.WriteLine("Input file does not exist");
                Environment.Exit(1);
            }

            #endregion

            #region Unzip the input and deserialize the JSON to AppSettings Class

            try
            {
                appSettings = ZipProcess.ProcessInputFile(appSettings, appConfigDir, zipWorkDir, inputFile);
                Console.WriteLine($"Notes : {appSettings.Notes}");
            }
            catch (Exception e)
            {
                AppSettings.SetAllProcessingActionsFalse(appSettings);
                ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "ProcessInputFile");
            }

            #endregion

            #region Validate ProcessingActions

            if (messageList.Count == 0)
            {
                try
                {
                    AppSettings.ValidateConfig(appSettings, messageList);
                }
                catch (Exception e)
                {
                    AppSettings.SetAllProcessingActionsFalse(appSettings);
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "AppSettings", "ValidateConfig");
                }
            }

            #endregion

            #region Concatenate a zip full of pdf's in order of an external index

            if (appSettings.ProcessingActions.Concatenate)
            {
                List<string> mergeIndex = new List<string>();

                try
                {
                    mergeIndex = ZipProcess.SortToConcat(inputFile, zipWorkDir);
                }
                catch (Exception e)
                {
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "SortToConcat");
                }
                if (messageList.Count == 0)
                {
                    try
                    {
                        PdfProcess.Concat(appSettings, mergeIndex, inputFile, zipWorkDir);
                    }
                    catch (InvalidDataException idx)
                    {
                        string[] messageArray = idx.Message.Split("|");
                        ErrorHandler.ConcatErrorHandler(messageList, $"{messageArray[1]}:{messageArray[2]}", messageArray[0], $"0x{idx.InnerException.HResult:x}", idx.InnerException.Message, idx.InnerException.StackTrace);
                        //do not return bad output
                        File.Delete($"{zipOutDir}{Path.GetFileNameWithoutExtension(inputFile.FullName)}.pdf");
                    }
                    catch (Exception e)
                    {
                        ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "PdfProcess", "Concatenate");
                        //do not return bad output
                        File.Delete($"{zipOutDir}{Path.GetFileNameWithoutExtension(inputFile.FullName)}.pdf");
                    }
                }
            }

            #endregion

            #region Copy a single PDF n times

            if (appSettings.ProcessingActions.MakeCopies)
            {
                List<string> pdfFilesInZip = new List<string>();

                try
                {
                    pdfFilesInZip = ZipProcess.SortToProcess(zipWorkDir, zipOutDir);
                }
                catch (Exception e)
                {
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "SortToProcess");
                }

                if (messageList.Count == 0)
                {
                    foreach (string pdfFile in pdfFilesInZip)
                    {
                        try
                        {
                            PdfProcess.MakeCopies(appSettings, new FileInfo(pdfFile), zipWorkDir);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler.SmartSaveErrorHandler(messageList, pdfFile, "pdpditxx.MakeCopies", $"0x{e.HResult:x}", e.Message, e.StackTrace);
                            //do not return bad output
                            File.Delete($"{zipOutDir}{Path.GetFileName(pdfFile)}");
                        }
                        // delete the input PDF files as we have completed each one
                        File.Delete(pdfFile);
                    }
                }
            }

            #endregion

            #region Split a pdf with an external index file

            // split a zip with an index file and pdf
            if (appSettings.ProcessingActions.Split)
            {
                List<SplitIndex> splitIndex = new List<SplitIndex>();

                try
                {
                    splitIndex = ZipProcess.SortToSplit(inputFile, zipWorkDir, zipOutDir);
                }
                catch (Exception e)
                {
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "SortToSplit");
                }

                if (messageList.Count == 0)
                {
                    FileInfo thisSplitPdf = new FileInfo(Directory.GetFiles(zipWorkDir, "*.pdf").FirstOrDefault());

                    try
                    {
                        PdfProcess.Split(zipWorkDir, splitIndex, thisSplitPdf);
                    }
                    catch (ArgumentOutOfRangeException oorx)
                    {
                        string[] messageArray = oorx.Message.Split("|");
                        ErrorHandler.SplitErrorHandler(messageList, $"PDFpages:{messageArray[3]}:RequestIndex:{messageArray[1]}:{messageArray[2]}", messageArray[0], $"0x{oorx.InnerException.HResult:x}", oorx.InnerException.Message, oorx.InnerException.StackTrace);
                        //do not return bad output
                        File.Delete($"{zipOutDir}{Path.GetFileNameWithoutExtension(inputFile.FullName)}.pdf");
                    }
                    catch (Exception e)
                    {
                        ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "PdfProcess", "Split");
                    }
                }
            }

            #endregion

            #region Scale and Auto-Rotate a PDF

            if (appSettings.ProcessingActions.ScaleAndRotate)
            {
                List<string> pdfFilesInZip = new List<string>();

                try
                {
                    pdfFilesInZip = ZipProcess.SortToProcess(zipWorkDir, zipOutDir);
                }
                catch (Exception e)
                {
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "SortToProcess");
                }
                if (messageList.Count == 0)
                {
                    float pageWidth = appSettings.Settings.TargetPageSize.PageWidth;
                    float pageHeight = appSettings.Settings.TargetPageSize.PageHeight;
                    foreach (string pdfFile in pdfFilesInZip)
                    {
                        try
                        {
                            PdfProcess.ScaleAndRotate(appSettings, new FileInfo(pdfFile), new FileInfo($"{zipOutDir}{Path.GetFileName(pdfFile)}"), pageWidth, pageHeight);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler.ScaleAndRotateErrorHandler(messageList, pdfFile, "pdpditxx.ScaleAndRotate", $"0x{e.HResult:x}", e.Message, e.StackTrace);
                            //do not return bad output
                            File.Delete($"{zipOutDir}{Path.GetFileName(pdfFile)}");
                        }
                        // delete the input PDF files as we have completed each one
                        File.Delete(pdfFile);
                    }

                }
            }

            #endregion

            #region SmartSave to optimize PDF's

            if (appSettings.ProcessingActions.SmartSave)
            {
                List<string> pdfFilesInZip = new List<string>();

                try
                {
                    pdfFilesInZip = ZipProcess.SortToProcess(zipWorkDir, zipOutDir);
                }
                catch (Exception e)
                {
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "SortToProcess");
                }

                if (messageList.Count == 0)
                {
                    foreach (string pdfFile in pdfFilesInZip)
                    {
                        try
                        {
                            PdfProcess.SmartSave(appSettings, new FileInfo(pdfFile), zipWorkDir);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler.SmartSaveErrorHandler(messageList, pdfFile, "pdpditxx.SmartSave", $"0x{e.HResult:x}", e.Message, e.StackTrace);
                            //do not return bad output
                            File.Delete($"{zipOutDir}{Path.GetFileName(pdfFile)}");
                        }
                        // delete the input PDF files as we have completed each one
                        File.Delete(pdfFile);
                    }
                }
            }


            #endregion

            #region Convert a PDF to a Text File

            if (appSettings.ProcessingActions.TextConvert)
            {
                List<string> pdfFilesInZip = new List<string>();

                try
                {
                    pdfFilesInZip = ZipProcess.SortToProcess(zipWorkDir, zipOutDir);
                }
                catch (Exception e)
                {
                    ErrorHandler.OtherFailureErrorHandler(messageList, e, inputFile, "ZipProcess", "SortToProcess");
                }

                if (messageList.Count == 0)
                {
                    foreach (string pdfFile in pdfFilesInZip)
                    {
                        string currentPdf;
                        try
                        {
                            currentPdf = pdfFile;
                            PdfProcess.TextConvert(new FileInfo(pdfFile), zipWorkDir);
                        }
                        catch (Exception e)
                        {
                            ErrorHandler.TextConvertErrorHandler(messageList, pdfFile, "pdpditxx.TextConvert", $"0x{e.HResult:x}", e.Message, e.StackTrace);
                            //do not return bad output
                            File.Delete($"{zipOutDir}{Path.GetFileName(pdfFile)}");
                        }
                        // delete the input PDF files as we have completed each one
                        File.Delete(pdfFile);
                    }
                }
            }

            #endregion

            #region Write out any errors, zip output, cleanup

            // write error list to file if there are any
            if (messageList.Count > 0)
            {
                if (!appSettings.EnableDebug)
                {
                    foreach (var message in messageList)
                    {
                        message.StackTrace = string.Empty;
                    }
                }
                ErrorMessage.ErrorListToJson(messageList, $"{zipOutDir}{Path.GetFileNameWithoutExtension(inputFile.FullName)}.error.json");
            }

            // zip all output
            ZipProcess.ZipProcessedOutput(zipOutDir, zipWorkDir, inputFile, outDir);

            //cleanup
            CleanupProcess.Cleanup(zipWorkDir);

            #endregion

        }
    }
}
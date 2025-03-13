using System;
using System.IO;
using System.Collections.Generic;

namespace pdpditxx
{
    internal class ErrorHandler
    {
        public static List<ErrorMessage> ConcatErrorHandler(List<ErrorMessage> messageList, string pdfFile, string activeStep, string errorCode, string errorMessage, string stackTrace)
        {
            Console.WriteLine($"Error while concatenating {pdfFile}. {errorCode} : {errorMessage}. ");

            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.ActiveStep = activeStep;
            thisMessage.FileName = pdfFile;
            thisMessage.Code = errorCode;
            thisMessage.Message = errorMessage;
            thisMessage.StackTrace = stackTrace;
            thisMessage.Module = $"pdpditxx.Concatenate";
            messageList.Add(thisMessage);

            return messageList;
        }

        public static List<ErrorMessage> OtherFailureErrorHandler(List<ErrorMessage> messageList, Exception e, FileInfo inputFile, string activeModule, string activeStep)
        {
            Console.WriteLine($"This job failed. Error Code : 0x{e.HResult:x} Error Message : {e.Message}");
            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.FileName = inputFile.Name;
            thisMessage.ActiveStep = activeStep;
            thisMessage.Code = $"0x{e.HResult:x}";
            thisMessage.Message = e.Message;
            thisMessage.Module = activeModule;
            thisMessage.StackTrace = e.StackTrace;
            messageList.Add(thisMessage);

            return messageList;
        }

        public static List<ErrorMessage> ValidationErrorHandler(List<ErrorMessage> messageList, string message, FileInfo inputFile, string activeModule, string activeStep)
        {
            Console.WriteLine($"This job failed validation. Error Message : {message}");
            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.FileName = inputFile.Name;
            thisMessage.ActiveStep = activeStep;
            thisMessage.Code = null;
            thisMessage.Message = message;
            thisMessage.Module = activeModule;
            thisMessage.StackTrace = null;
            messageList.Add(thisMessage);

            return messageList;
        }

        public static List<ErrorMessage> SmartSaveErrorHandler(List<ErrorMessage> messageList, string pdfFile, string activeStep, string errorCode, string errorMessage, string stackTrace)
        {
            Console.Error.WriteLine($"PDF crashed SmartSave : {pdfFile}.");
            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.ActiveStep = activeStep;
            thisMessage.FileName = pdfFile;
            thisMessage.Code = errorCode;
            thisMessage.Message = errorMessage;
            thisMessage.StackTrace = stackTrace;
            thisMessage.Module = $"pdpditxx.SmartSave";
            messageList.Add(thisMessage);

            return messageList;
        }

        public static List<ErrorMessage> ScaleAndRotateErrorHandler(List<ErrorMessage> messageList, string pdfFile, string activeStep, string errorCode, string errorMessage, string stackTrace)
        {
            Console.Error.WriteLine($"Error while rotating pdf : {pdfFile}.");

            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.ActiveStep = activeStep;
            thisMessage.FileName = pdfFile;
            thisMessage.Code = errorCode;
            thisMessage.Message = errorMessage;
            thisMessage.StackTrace = stackTrace;
            thisMessage.Module = $"pdpditxx.ScaleAndRotate";
            messageList.Add(thisMessage);

            return messageList;
        }

        public static List<ErrorMessage> SplitErrorHandler(List<ErrorMessage> messageList, string pdfFile, string activeStep, string errorCode, string errorMessage, string stackTrace)
        {
            Console.WriteLine($"Error while splitting {pdfFile}. {errorCode} : {errorMessage}. ");

            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.ActiveStep = activeStep;
            thisMessage.FileName = pdfFile;
            thisMessage.Code = errorCode;
            thisMessage.Message = errorMessage;
            thisMessage.StackTrace = stackTrace;
            thisMessage.Module = $"pdpditxx.Split";
            messageList.Add(thisMessage);

            return messageList;
        }

        public static List<ErrorMessage> TextConvertErrorHandler(List<ErrorMessage> messageList, string pdfFile, string activeStep, string errorCode, string errorMessage, string stackTrace)
        {
            Console.Error.WriteLine($"Error while converting to text : {pdfFile}.");

            ErrorMessage thisMessage = new ErrorMessage();
            thisMessage.TimeStamp = DateTime.Now.ToString();
            thisMessage.ActiveStep = activeStep;
            thisMessage.FileName = pdfFile;
            thisMessage.Code = errorCode;
            thisMessage.Message = errorMessage;
            thisMessage.StackTrace = stackTrace;
            thisMessage.Module = $"pdpditxx.TextConvert";
            messageList.Add(thisMessage);

            return messageList;
        }
    }
}

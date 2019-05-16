using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace CsvMerger
{
    class Program
    {
        private static IOrderedEnumerable<QuestionRecord> _orderedQuestionList;

        static void Main(string[] args)
        {
            string file1 = args[0];
            string file2 = args[1];
            //file1 = @"C:\Users\rkwiecie\Desktop\Patent\Nauka\Prawo Karne - Fiszki.csv";
            //file2 = @"C:\Users\rkwiecie\Desktop\Patent\Nauka\Prawo Karne - Test.csv";

            GenerateCsvFile(file1, file2);
            GenerateHtmlFile(file1);
            Console.ReadKey();
        }

        private static void GenerateHtmlFile(string file)
        {
            var projectName = file.Substring(file.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase) + 1).Split('-')[0].Trim();
            var htmlContent = $@"<html>
<head>
<title>{projectName}</title>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"" />
<style type=""text/css"">
@page {{
    size: auto;   /* auto is the initial value */
    margin: 0;  /* this affects the margin in the printer settings */
}}
.questionBox {{ margin-bottom: 14px; page-break-inside: avoid; }}
.question {{ font-weight: bold; }}
.answer {{ font-weight: bold; display: inline; }}
.legalBasis {{ display: inline-block; }}
.title {{ font-size: 1.5em; text-align: center; text-transform: uppercase; margin-bottom: 4px; }}
</style >
</head>
<body>
";
            htmlContent += $@"  <div class=""title"">{projectName}</div>{Environment.NewLine}";

            htmlContent += "\t<div class=\"questionList\">\r\n";

            foreach (var questionRecord in _orderedQuestionList)
            {
                htmlContent += "\t\t<div class=\"questionBox\">\r\n";
                htmlContent += "\t\t\t<div class=\"question\">" + questionRecord.Question + "</div>\r\n";
                htmlContent += "\t\t\t<div class=\"answer\">" + questionRecord.Answer + "</div>\r\n";
                htmlContent += "\t\t\t<div class=\"legalBasis\">" + questionRecord.LegalBasis + "</div>\r\n";
                htmlContent += "\t\t</div>\r\n";
            }

            htmlContent += @"   </div>
</body>
</html>";

            var outputFilePath = file.Split('-')[0].Trim() + ".html";

            File.WriteAllText(outputFilePath, htmlContent);
        }

        private static void GenerateCsvFile(string file1, string file2)
        {
            var sr1 = new StreamReader(file1);
            var sr2 = new StreamReader(file2);
            var cutIndexFrom = file1.LastIndexOf(" - ", StringComparison.CurrentCultureIgnoreCase);
            var cutIndexTo = file1.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase);
            var outputFile = file1.Remove(cutIndexFrom, cutIndexTo - cutIndexFrom);
            
            var stream = File.OpenWrite(outputFile);
            var write = new StreamWriter(stream, new UTF8Encoding(true));

            var csvread = new CsvReader(sr1);
            var csvread2 = new CsvReader(sr2);

            csvread.Configuration.RegisterClassMap<QuestionRecordMap>();
            csvread2.Configuration.RegisterClassMap<QuestionRecordMap>();
            csvread.Configuration.Delimiter = ";";
            csvread2.Configuration.Delimiter = ";";

            csvread.Configuration.HeaderValidated = null;
            csvread2.Configuration.HeaderValidated = null;

            csvread.Configuration.MissingFieldFound = null;
            csvread2.Configuration.MissingFieldFound = null;

            var csw = new CsvWriter(write);
            csw.Configuration.RegisterClassMap<QuestionRecordMap>();
            csw.Configuration.Delimiter = ";";

            var records1 = csvread.GetRecords<QuestionRecord>().ToList();
            var records2 = csvread2.GetRecords<QuestionRecord>().ToList();

            var list = new List<QuestionRecord>();

            foreach (var questionRecord in records1)
            {
                var doubledQuestionRecord =
                    records2.FirstOrDefault(q => q.Question == questionRecord.Question && q.Answer == questionRecord.Answer);
                list.Add(string.IsNullOrWhiteSpace(doubledQuestionRecord?.LegalBasis)
                    ? questionRecord
                    : doubledQuestionRecord);
            }

            foreach (var questionRecord in records2)
            {
                if (!list.Any(q => q.Question == questionRecord.Question && q.Answer == questionRecord.Answer))
                {
                    list.Add(questionRecord);
                }
            }

            _orderedQuestionList = list.OrderBy(q => q.Question.ToLowerInvariant()).ThenBy(q => q.Answer.ToLowerInvariant());

            csw.WriteRecords(_orderedQuestionList);

            sr1.Close();
            write.Close();
        }
    }
}

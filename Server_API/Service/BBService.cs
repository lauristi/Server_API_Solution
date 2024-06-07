using OfficeOpenXml;
using Server_API.Model;
using Server_API.Service.Interface;
using System.Globalization;
using System.Text;

namespace Server_API.Service
{
    public class BBService : IBBService
    {
        public string ConvertCsvToXls(string csvFilePath, string xlsFilePath)
        {
            FileInfo csvFile = new FileInfo(csvFilePath);
            FileInfo xlsFile = new FileInfo(xlsFilePath);

            using (ExcelPackage package = new ExcelPackage(xlsFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                int row = 1;

                foreach (string line in File.ReadLines(csvFilePath))
                {
                    string[] fields = line.Split(',');
                    int col = 1;

                    foreach (string field in fields)
                    {
                        worksheet.Cells[row, col].Value = field;
                        col++;
                    }

                    row++;
                }

                // Example: Setting font size and color for column A
                //using (ExcelRange colA = worksheet.Cells[1, 1, row - 1, 1])
                //{
                //    colA.Style.Font.Size = 12;
                //    colA.Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                //    // Additional formatting options can be set, like bold, italic, underline, etc.
                //}

                package.Save();
            }

            return xlsFilePath;
        }

        public string? ProcessStatment(string statementFilePath, string expenseFilePath, string finalFilePath)
        {
            try
            {
                //01 CARREGO A LISTA DE DESPESAS
                List<Expense> expenses = LoadConversions(expenseFilePath);

                //02 CARREGO OS DADOS DO EXTRATO
                List<StatementData> statementDatas = new List<StatementData>();

                string[] lines = File.ReadAllLines(statementFilePath, Encoding.Latin1);

                int cabecalho = 0;
                //string csvName = null;

                string? xlsName = null;

                foreach (string line in lines)
                {
                    //--0---------1-----------------2-------------3--------------------4----------------5----
                    //Data","Dependencia Origem","Histórico","Data do Balancete","Número do documento","Valor",

                    StatementData statementData = new StatementData();

                    if (cabecalho == 0)
                    {
                        statementData.Date = "DATA";
                        statementData.Subject = "CASA";
                        statementData.Value = "VALOR";
                        statementData.Type = "TIPO";
                        statementData.Score = "SCORE";
                    }
                    else
                    {
                        string cleanLine = line.Replace("\"", "");
                        string[] aItem = cleanLine.Split(',');
                        statementData.Date = aItem[0];
                        statementData.Subject = aItem[2].ToUpper();
                        statementData.Value = NormalizeValue(aItem[5]);
                        statementData.Score = ExpenseAnalysis(statementData.Value);
                        statementData.IsCredit = !aItem[5].Contains("-");

                        statementData.Type = ProcessSubject(statementData, expenses);
                        //------------------------------------------------------

                        if (string.IsNullOrEmpty(xlsName))
                        {
                            //csvName = CreateArchiveName(statementData.Date, "csv");
                            xlsName = CreateArchiveName(statementData.Date, "xlsx");
                        }
                    }

                    statementDatas.Add(statementData);
                    cabecalho++;
                }

                //string csvFilePath = Path.Combine(finalFilePath, csvName);
                xlsName = xlsName ?? "";
                string xlsFilePath = Path.Combine(finalFilePath, xlsName);

                if (CreateNewFileXLS(xlsFilePath, statementDatas))
                {
                    return xlsFilePath;
                }
                else
                {
                    return null;
                }

                //if (CreateNewFileCSV(finalFilePath, statementDatas))
                //{
                //    return ConvertCsvToXls(csvFilePath, xlsFilePath); ;
                //}
                //else
                //{
                //    return null;
                //};
            }
            catch (Exception)
            {
                return null;
            }
        }

        public decimal ProcessMonthSpending(string statementFilePath)
        {
            string[] lines = File.ReadAllLines(statementFilePath, Encoding.Latin1);

            int cabecalho = 0;
            decimal totalSpending = 0.0m;

            foreach (string line in lines)
            {
                if (cabecalho > 0)
                {
                    string cleanLine = line.Replace("\"", "");
                    string[] aItem = cleanLine.Split(',');

                    string subject = aItem[2].ToUpper();
                    string grossValue = aItem[5].ToUpper();

                    //alguns items negativos devem ser ignorados
                    List<string> terms = new List<string> {"Aplicação",
                                                           "Ágil",
                                                           "Transferido"};

                    bool devolvido = terms.Any(termo => subject.Contains("DEVOLVIDO"));
                    bool aplicacao = terms.Any(termo => subject.Contains(termo.ToUpper()));

                    if (devolvido)
                    {
                        totalSpending = totalSpending - NormalizeToDecimal(grossValue);
                    }
                    else
                    {
                        if (grossValue.Contains("-") && !aplicacao)
                        {
                            totalSpending = totalSpending + NormalizeToDecimal(grossValue);
                        }
                    }
                }
                cabecalho++;
            }
            return totalSpending;
        }

        #region "Private functions"

        private List<Expense> LoadConversions(string convertionFile)
        {
            List<Expense> expenses = new List<Expense>();

            try
            {
                //CARREGO AS CONVERSOES
                string[] lines = File.ReadAllLines(convertionFile);

                foreach (string line in lines)
                {
                    Expense expense = new Expense();

                    string cleanLine = line.Replace("\"", "");
                    string[] aItem = cleanLine.Split(';');

                    expense.Origin = aItem[0];
                    expense.Owner = aItem[1];
                    //------------------------------------------------------
                    expenses.Add(expense);
                }

                return expenses;
            }
            catch (Exception)
            {
                return expenses;
            }
        }

        private string? ProcessSubject(StatementData statementData, List<Expense> expenses)
        {
            // Procura na lista de despesas o primeiro elemento que corresponde à condição
            // Verifica se o campo Origin da despesa não é nulo
            // Verifica se o campo Origin da despesa está contido na string subject, ignorando maiúsculas e minúsculas
            // Se encontrar um elemento que corresponde à condição, retorna o valor do campo Owner da despesa correspondente

            string? result = null;
            if (statementData.Subject != null)
            {
                var found = expenses.FirstOrDefault(e => e.Origin != null
                                                    && statementData.Subject.IndexOf(e.Origin, StringComparison.OrdinalIgnoreCase) >= 0);
                result = found?.Owner;

                if (result == "" && statementData.IsCredit)
                {
                    result = "XXX - Credito";
                }
            }

            return result;
        }

        private string NormalizeString(string text)
        {
            // Remover acentos e converter para minúsculas
            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) !=
                                          UnicodeCategory.NonSpacingMark)
                             .ToArray();
            string normalizedText = new string(chars).Normalize(NormalizationForm.FormC).ToLowerInvariant();
            return normalizedText;
        }

        private string NormalizeStringSize(string text, int size)
        {
            string normalized = text;
            try
            {
                if (text.Length > size)
                {
                    normalized = text.Substring(0, size);
                }
                else
                {
                    normalized = text + new string(' ', size - text.Length);
                }
                return normalized;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        private string NormalizeValue(string value)
        {
            string normalizedValue = value.Replace(".", ",")
                                     .Replace("-", "");
            return normalizedValue;
        }

        private decimal NormalizeToDecimal(string value)
        {
            if (decimal.TryParse(value.Replace(".", ",")
                                      .Replace("-", ""), out decimal decimalValue))
            {
                return decimalValue;
            }
            return 0.0m;
        }

        private bool CreateNewFileCSV(string finalFilePath, List<StatementData> statementData)
        {
            // CRIA NOVO ARQUIVO
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(finalFilePath, FileMode.CreateNew), Encoding.Latin1))
                {
                    foreach (StatementData line in statementData)
                    {
                        writer.WriteLine(line.LineCSV());
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CreateNewFileXLS(string xlsFilePath, List<StatementData> statementData)
        {
            try
            {
                FileInfo xlsFile = new FileInfo(xlsFilePath);

                using (ExcelPackage package = new ExcelPackage(xlsFile))
                {
                    // Definir compatibilidade com Excel 97-2003
                    package.Compatibility.IsWorksheets1Based = true;

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    int row = 1;

                    foreach (StatementData field in statementData)
                    {
                        worksheet.Cells[row, 1].Value = field.Date;
                        worksheet.Cells[row, 2].Value = field.Subject;
                        worksheet.Cells[row, 3].Value = NormalizeToDecimal(field.Value);
                        worksheet.Cells[row, 4].Value = field.Type;
                        worksheet.Cells[row, 5].Value = field.Score;

                        if (double.TryParse(field.Value.ToString(), out double numericValue))
                        {
                            worksheet.Cells[row, 3].Value = numericValue;
                        }

                        row++;
                    }

                    // Definir formato da coluna 3 como moeda brasileira sem o símbolo de moeda
                    // linha inicial, coluna inicial, linha(s) finais, coluna final
                    var colC = worksheet.Cells[1, 3, row - 1, 3];
                    colC.Style.Numberformat.Format = "#,##0.00"; // Formato sem símbolo de moeda

                    package.Save();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string ExpenseAnalysis(string? value)
        {
            if (decimal.TryParse(value, out decimal Decimalvalue))
            {
                if (Decimalvalue <= 50)
                {
                    return "BAIXO";
                }
                else if (Decimalvalue >= 50 && Decimalvalue <= 100)
                {
                    return "MÉDIO";
                }
                else if (Decimalvalue > 100)
                {
                    return "ALTO";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private string CreateArchiveName(string dateString, string extension)
        {
            if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                                                 DateTimeStyles.None,
                                                                 out DateTime dateTime))
            {
                //A Primeira linha é os Saldo do mês anterior.
                dateTime = dateTime.AddMonths(1);

                return $"{dateTime.Month:00}-{dateTime.ToString("MMMM").ToUpper()}-{dateTime.ToString("yyyy")}.{extension}";
            }

            return "00 MONTH.csv";
        }

        #endregion "Private functions"
    }
}
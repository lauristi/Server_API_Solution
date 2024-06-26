using Server_API.Domain.Model.BB;
using Server_API.Domain.Model.BB.BLL;
using Server_API.Domain.Model.BB.Spending;
using Server_API.Domain.Service.BBService.Interface;
using Server_API.Domain.Service.InfrastrutureService.Interface;
using System.Text;
using static Server_API.Domain.Model.BB.Enumeradores;

namespace Server_API.Domain.Service.BBService
{
    public class BBService : IBBService
    {
        private readonly IExpenseService _expenseService;
        private readonly IXlsService _xlsService;
        private readonly INormalizeService _normalizeService;

        public BBService(IExpenseService expenseService,
                         IXlsService xlsService,
                         INormalizeService normalizeService
                         )
        {
            _expenseService = expenseService;
            _xlsService = xlsService;
            _normalizeService = normalizeService;
        }

        public ProcessedData ProcessBBStatment(string statementFilePath, string expenseFilePath, string finalFilePath)
        {
            try
            {
                ProcessedData processedData = new ProcessedData();

                //01 CARREGO A LISTA DE DESPESAS CONHECIDAS
                List<Expense> expenses = _expenseService.LoadExpensesList(expenseFilePath);

                //02 CARREGO OS DADOS DO EXTRATO
                List<SpendingData> spendingDataList = new List<SpendingData>();

                string[] lines = File.ReadAllLines(statementFilePath, Encoding.Latin1);

                int cabecalho = 0;
                string? xlsName = null;

                foreach (string line in lines)
                {
                    //--0---------1-----------------2-------------3--------------------4----------------5----
                    //Data","Dependencia Origem","Histórico","Data do Balancete","Número do documento","Valor",

                    SpendingData spendingData = new SpendingData();

                    if (cabecalho == 0)
                    {
                        spendingData.Date = "DATA";
                        spendingData.Subject = "CASA";
                        spendingData.StringValue = "VALOR";
                        spendingData.Type = "TIPO";
                        spendingData.Score = "SCORE";
                    }
                    else
                    {
                        string cleanLine = line.Replace("\"", "");
                        string[] aItem = cleanLine.Split(',');

                        spendingData.Date = aItem[0];
                        spendingData.Subject = aItem[2].ToUpper();

                        spendingData.GrossValue = aItem[5];
                        spendingData.StringValue = _normalizeService.NormalizeValue(aItem[5]);
                        spendingData.DecimalValue = _normalizeService.NormalizeToDecimal(spendingData.StringValue);
                        spendingData.IsCredit = !aItem[5].Contains("-");

                        spendingData = ProcessSubject(spendingData, expenses);

                        //------------------------------------------------------

                        if (string.IsNullOrEmpty(xlsName))
                        {
                            xlsName = _xlsService.CreateXlsArchiveName(spendingData.Date, "xlsx");
                        }
                    }

                    spendingDataList.Add(spendingData);
                    cabecalho++;
                }

                //--------------------------------------------------------------------------
                // PROCESSA DESPESAS FIXAS E SOMADORES
                //--------------------------------------------------------------------------

                processedData = ProcessTotalKnowSpending(spendingDataList);

                //--------------------------------------------------------------------------
                //cria xls
                //--------------------------------------------------------------------------
                xlsName = xlsName ?? "";
                string xlsFilePath = Path.Combine(finalFilePath, xlsName);

                if (_xlsService.CreateNewFileXLS(xlsFilePath, spendingDataList))
                {
                    processedData.FilePath = xlsFilePath;
                    return processedData;
                }
                else
                {
                    return processedData;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private SpendingData ProcessSubject(SpendingData spendingData, List<Expense> expenses)
        {
            if (spendingData.Subject != null)
            {
                if (spendingData.IsCredit)
                {
                    //===============================================================================================================================
                    // CREDITO
                    //===============================================================================================================================

                    // Alguns itens são de devolucao e devem ser somados pois anulam debitos
                    bool devolvido = spendingData.Subject.Contains("DEVOLVIDO");
                    if (devolvido)
                    {
                        spendingData.FinancialType = FINANCIAL_TYPE.UNKNOW_CREDIT;
                    }
                    else
                    {
                        spendingData.FinancialType = FINANCIAL_TYPE.IGNORE;
                    }
                }
                else
                {
                    //===============================================================================================================================
                    // DEBITO
                    //===============================================================================================================================
                    string? result = null;

                    // Alguns items negativos devem ser ignorados pois sao movimentacao interna
                    List<string> termList = new List<string> { "Aplicação", "Ágil", "Transferido", "Saldo", "S A L D O", "Enviada" };
                    bool aplicacao = termList.Any(term => spendingData.Subject.Contains(term.ToUpper()));

                    if (aplicacao)
                    {
                        spendingData.Type = "XXX - Credito/Aplicação";
                        spendingData.FinancialType = FINANCIAL_TYPE.IGNORE;
                    }
                    else
                    {
                        // Se encontrar um elemento que corresponde à condição, retorna o valor do campo Owner da despesa correspondente
                        var found = expenses.FirstOrDefault(e => e.Origin != null
                                                        && spendingData.Subject.IndexOf(e.Origin, StringComparison.OrdinalIgnoreCase) >= 0);
                        result = found?.Owner;

                        switch (result)
                        {
                            case "MERCADO":
                                spendingData.FinancialType = FINANCIAL_TYPE.SUPERMARKET_DEBIT;
                                break;

                            case "FARMACIA":
                                spendingData.FinancialType = FINANCIAL_TYPE.PHARMACY_DEBIT;
                                break;

                            default:
                                spendingData.FinancialType = FINANCIAL_TYPE.UNKNOW_DEBIT;
                                break;
                        }
                    }

                    spendingData.Type = result;

                    //====================================================================================================================
                    // PROCESSA O SCORE
                    //====================================================================================================================

                    if (spendingData.DecimalValue <= 50)
                    {
                        spendingData.Score = "BAIXO";
                    }
                    else if (spendingData.DecimalValue >= 50 && spendingData.DecimalValue <= 100)
                    {
                        spendingData.Score = "MÉDIO";
                    }
                    else if (spendingData.DecimalValue > 100)
                    {
                        spendingData.Score = "ALTO";
                    }
                    else
                    {
                        spendingData.Score = "";
                    }
                }
            }

            return spendingData;
        }

        private ProcessedData ProcessTotalKnowSpending(List<SpendingData> spendingDataList)
        {
            ProcessedData processedData = new ProcessedData();

            foreach (var spending in spendingDataList)
            {
                switch (spending.FinancialType)
                {
                    case FINANCIAL_TYPE.EXTRA_DEBIT:
                        processedData.Extra += spending.DecimalValue;
                        processedData.TotalDebit += spending.DecimalValue;
                        break;

                    case FINANCIAL_TYPE.SUPERMARKET_DEBIT:
                        processedData.SuperMarket += spending.DecimalValue;
                        processedData.TotalDebit += spending.DecimalValue;
                        break;

                    case FINANCIAL_TYPE.PHARMACY_DEBIT:
                        processedData.Pharmacy += spending.DecimalValue;
                        processedData.TotalDebit += spending.DecimalValue;
                        break;

                    case FINANCIAL_TYPE.UNKNOW_CREDIT:
                        processedData.TotalCredit += spending.DecimalValue;
                        break;

                    case FINANCIAL_TYPE.UNKNOW_DEBIT:
                        processedData.TotalDebit += spending.DecimalValue;
                        break;
                }
            }

            return processedData;
        }
    }
}
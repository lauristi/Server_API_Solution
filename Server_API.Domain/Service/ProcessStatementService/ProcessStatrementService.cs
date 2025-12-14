using Server_API.Domain.Model.BB;
using Server_API.Domain.Model.BB.BLL;
using Server_API.Domain.Model.BB.Spending;
using Server_API.Domain.Service.ExpenseService.Inrterface;
using Server_API.Domain.Service.InfrastrutureService.Interface;
using static Server_API.Domain.Model.BB.Enumeradores;

namespace Server_API.Domain.Service.ProcessStatementService
{
    public class ProcessStatrementService : Interface.IProcessStatementService
    {
        private readonly IExpenseService _expenseService;
        private readonly IXlsService _xlsService;
        private readonly INormalizeService _normalizeService;

        public ProcessStatrementService(IExpenseService expenseService,
                         IXlsService xlsService,
                         INormalizeService normalizeService
                         )
        {
            _expenseService = expenseService;
            _xlsService = xlsService;
            _normalizeService = normalizeService;
        }

        public SpendingData ProcessSubject(SpendingData spendingData, List<Expense> expenses)
        {
            if (spendingData.Subject != null)
            {
                string? result = null;

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

                    // Se encontrar um elemento que corresponde à condição, retorna o valor do campo Owner da despesa correspondente
                    var found = expenses.FirstOrDefault(e => e.Origin != null
                                                    && spendingData.Subject.IndexOf(e.Origin, StringComparison.OrdinalIgnoreCase) >= 0);
                    result = found?.Owner;
                    spendingData.Type = result;
                }
                else
                {
                    //===============================================================================================================================
                    // DEBITO
                    //===============================================================================================================================

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

        public ProcessedData ProcessTotalKnowSpending(List<SpendingData> spendingDataList)
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
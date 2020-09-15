using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace API_IBW.Business_Classes
{
    public class Invoice
    {
        private InvoicesDataContext _invDB;
        private QuotesDataContext _quotesDB;
        private AdminDataContext _adminDB;
        private QuotesExtensionDataContext _quotesExtensionDB;
        public Invoice()
        {
            _invDB = new InvoicesDataContext();
            _quotesDB = new QuotesDataContext();
            _adminDB = new AdminDataContext();
            _quotesExtensionDB = new QuotesExtensionDataContext();
        }
        //author:Vamsi 
        //date: 06/03/2020
        //To get all quote related invoice details
        public List<InvSites> GetQuoteInvoicesDetails(long QuoteId, SelectedActions selectedactions, long? invoiceId)
        {
            try
            {
                var invoicedetails = _invDB.getquoterelatedinvoicedetails(QuoteId,invoiceId).Where(x => x.dec_remaining_unbilled > 0 || x.dec_billed>0).ToList();
                //if (selectedactions != null && ((selectedactions.actionloghourids != null && selectedactions.actionloghourids.Count() > 0) ||
                //    (selectedactions.actionexpenseids != null && selectedactions.actionexpenseids.Count() > 0) ||
                //    (selectedactions.timesheetids != null && selectedactions.timesheetids.Count() > 0) ||
                //    (selectedactions.expensesids != null && selectedactions.expensesids.Count() > 0) ||
                //    (selectedactions.budgetsowids != null && selectedactions.budgetsowids.Count() > 0)))
                //{
                //    invoicedetails = invoicedetails.Where(x => (selectedactions.actionloghourids != null && selectedactions.actionloghourids.Contains(x.int_action_log_hour_id)) ||
                //    (selectedactions.actionexpenseids != null && selectedactions.actionexpenseids.Contains(x.int_action_expense_id)) ||
                //     (selectedactions.timesheetids != null && selectedactions.timesheetids.Contains(x.int_timesheet_id)) ||
                //     (selectedactions.expensesids != null && selectedactions.expensesids.Contains(x.int_expense_id)) ||
                //     (selectedactions.budgetsowids != null && selectedactions.budgetsowids.Contains(x.int_budget_sow_id))).ToList();
                //}
                //var sites=(dynamic)null;
                List<InvSites> Sites = new List<InvSites>();
                List<getquoterelatedinvoicedetailsResult> sitedetails = new List<getquoterelatedinvoicedetailsResult>();
                foreach (var site in invoicedetails.Select(x => new { x.int_site_id, x.vc_site_name }).Distinct())
                {
                    sitedetails = invoicedetails.Where(x => x.int_site_id == site.int_site_id).ToList();
                    InvSites Site = new InvSites();
                    Site.name = site.vc_site_name;

                    Site.rowspan = sitedetails.Count();
                    List<InvSows> Sows = new List<InvSows>();
                    InvSows Sow;
                    foreach (var sow in sitedetails.Select(x => new { x.int_sow_id, x.vc_sow_name, x.vc_fee_structure, x.quoted_total, x.budget_total }).Distinct())
                    {
                        sitedetails = invoicedetails.Where(x => x.int_site_id == site.int_site_id && x.int_sow_id == sow.int_sow_id).ToList();
                        Sow = new InvSows();
                        Sow.name = sow.vc_sow_name;
                        Sow.budgetAmount = sow.budget_total;
                        Sow.quotedAmount = sow.quoted_total;
                        Sow.feestructure = sow.vc_fee_structure;
                        Sow.rowspan = sitedetails.Count();
                        if (sitedetails.Where(x => x.vc_fee_structure == "Fixed Rate").Count() > 0)
                        {
                            foreach (var budgetsow in sitedetails.Where(x => x.vc_fee_structure == "Fixed Rate" && x.int_budget_sow_id != null).Select(x => x.int_budget_sow_id).Distinct())
                            {
                                var budgetsows = sitedetails.Where(x => x.int_budget_sow_id == budgetsow).FirstOrDefault();
                                List<InvTasks> budgetsowTask = new List<InvTasks>();
                                InvTasks budgetsowTasks = new InvTasks();
                                budgetsowTasks.name = "-";
                                budgetsowTasks.rowspan = 1;
                                List<InvActions> budgetsowAction = new List<InvActions>();
                                InvActions budgetsowActions = new InvActions();
                                budgetsowActions.int_invoice_item_id = budgetsows.int_invoice_item_id;
                                budgetsowActions.int_budget_sow_id = budgetsow;
                                budgetsowActions.name = "-";
                                budgetsowActions.jobcode = "-";
                                budgetsowActions.jobtitle = "-";
                                budgetsowActions.expensecode = "-";
                                budgetsowActions.isselect = (budgetsows.int_invoice_item_id != null ||((selectedactions != null && selectedactions.budgetsowids != null && selectedactions.budgetsowids.Count() > 0 && selectedactions.budgetsowids.Contains(budgetsow)))) ? true : false;
                                budgetsowActions.username = "-";
                                budgetsowActions.date = null;
                                budgetsowActions.percentinvoiced = (selectedactions == null && invoiceId== null) ? 0 : 100;
                                budgetsowActions.invoiced = ((Sow.quotedAmount == null ? 0 : Sow.quotedAmount) / 100) * budgetsowActions.percentinvoiced;
                                budgetsowActions.billed = budgetsows.dec_billed;
                                budgetsowActions.totalinvoiced = ((Sow.quotedAmount == null ? 0 : Sow.quotedAmount) / 100) - budgetsows.dec_remaining_unbilled;
                                budgetsowActions.remainingunbilled = (Sow.quotedAmount == null ? 0 : Sow.quotedAmount) - budgetsowActions.invoiced;
                                budgetsowActions.totalremainingunbilled = budgetsows.dec_remaining_unbilled;
                                budgetsowAction.Add(budgetsowActions);
                                budgetsowTasks.actions = budgetsowAction;
                                budgetsowTask.Add(budgetsowTasks);
                                Sow.tasks = budgetsowTask;
                                Sows.Add(Sow);
                            }
                        }
                        else
                        {
                            List<InvTasks> Tasks = new List<InvTasks>();
                            InvTasks Task;
                            foreach (var task in sitedetails.Where(x => x.vc_fee_structure == "Time & Expense" && x.int_budget_sow_id == null && x.int_task_id != null).Select(x => new { x.int_task_id, x.vc_task_name }).Distinct())
                            {
                                var actions = sitedetails.Where(x => x.int_task_id == task.int_task_id).ToList();
                                Task = new InvTasks();
                                List<InvActions> Actions = new List<InvActions>();
                                Task.name = task.vc_task_name;
                                Task.rowspan = actions.Count();
                                InvActions Action;
                                foreach (var action in actions.Where(x => x.int_budget_sow_id == null).Distinct())
                                {
                                    Action = new InvActions();
                                    Action.int_invoice_item_id = action.int_invoice_item_id;
                                    Action.int_action_log_hour_id = action.int_action_log_hour_id != null ? action.int_action_log_hour_id : null;
                                    Action.int_action_expense_id = action.int_action_expense_id != null ? action.int_action_expense_id : null;
                                    Action.int_timesheet_id = action.int_timesheet_id != null ? action.int_timesheet_id : null;
                                    Action.int_expense_id = action.int_expense_id != null ? action.int_expense_id : null;
                                    Action.int_budget_sow_id = null;
                                    Action.name = action.vc_action_name;
                                    Action.jobcode = action.vc_job_code;
                                    Action.jobtitle = action.vc_job_code != "-" ? action.vc_job_code + " (" + action.vc_job_title + ")" : "-";
                                    Action.expensecode = action.vc_expense_code;
                                    Action.worked = action.dec_worked_hours;
                                    Action.total = action.total;
                                    Action.expenses = action.expenses != null ? action.expenses : null;
                                    Action.isselect = (action.int_invoice_item_id != null ||((selectedactions != null && ((action.int_action_log_hour_id != null && selectedactions.actionloghourids != null && selectedactions.actionloghourids.Count() > 0 && selectedactions.actionloghourids.Contains(action.int_action_log_hour_id)) ||
                                        (action.int_action_expense_id != null && selectedactions.actionexpenseids != null && selectedactions.actionexpenseids.Count() > 0 && selectedactions.actionexpenseids.Contains(action.int_action_expense_id)) ||
                                        (action.int_timesheet_id != null && selectedactions.timesheetids != null && selectedactions.timesheetids.Count() > 0 && selectedactions.timesheetids.Contains(action.int_timesheet_id)) ||
                                        (action.int_expense_id != null && selectedactions.expensesids != null && selectedactions.expensesids.Count() > 0 && selectedactions.expensesids.Contains(action.int_expense_id)))
                                        ))) ? true : false;
                                    Action.username = action.vc_user_name;
                                    Action.date = action.dt_date;
                                    Action.percentinvoiced = 100;
                                    Action.invoiced = (((action.total == null ? 0 : action.total) + (action.expenses == null ? 0 : action.expenses)) / 100) * Action.percentinvoiced;
                                    Action.billed = action.dec_billed;
                                    Action.totalinvoiced = (((action.total == null ? 0 : action.total) + (action.expenses == null ? 0 : action.expenses)) / 100) - action.dec_remaining_unbilled;
                                    Action.remainingunbilled = ((action.total == null ? 0 : action.total) + (action.expenses == null ? 0 : action.expenses)) - Action.invoiced;
                                    Action.totalremainingunbilled = action.dec_remaining_unbilled;
                                    Actions.Add(Action);
                                }
                                Task.actions = Actions;
                                Tasks.Add(Task);
                            }
                            Sow.tasks = Tasks;
                            Sows.Add(Sow);
                        }
                        Site.sows = Sows;
                    }
                    Sites.Add(Site);
                }

                return Sites;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<InvSites> GetQuoteInvoicesDetailsForEdit(long QuoteId, SelectedActions selectedactions, long? invoiceId)
        {
            try
            {
                var invoicedetails = _invDB.getquoterelatedinvoicedetails(QuoteId, invoiceId).Where(x => x.dec_remaining_unbilled > 0 || x.dec_billed > 0).ToList();
                
                List<InvSites> Sites = new List<InvSites>();
                List<getquoterelatedinvoicedetailsResult> sitedetails = new List<getquoterelatedinvoicedetailsResult>();
                foreach (var site in invoicedetails.Select(x => new { x.int_site_id, x.vc_site_name }).Distinct())
                {
                    sitedetails = invoicedetails.Where(x => x.int_site_id == site.int_site_id).ToList();
                    InvSites Site = new InvSites();
                    Site.name = site.vc_site_name;

                    Site.rowspan = sitedetails.Count();
                    List<InvSows> Sows = new List<InvSows>();
                    InvSows Sow;
                    foreach (var sow in sitedetails.Select(x => new { x.int_sow_id, x.vc_sow_name, x.vc_fee_structure, x.quoted_total, x.budget_total }).Distinct())
                    {
                        sitedetails = invoicedetails.Where(x => x.int_site_id == site.int_site_id && x.int_sow_id == sow.int_sow_id).ToList();
                        Sow = new InvSows();
                        Sow.name = sow.vc_sow_name;
                        Sow.budgetAmount = sow.budget_total;
                        Sow.quotedAmount = sow.quoted_total;
                        Sow.feestructure = sow.vc_fee_structure;
                        Sow.rowspan = sitedetails.Count();
                        if (sitedetails.Where(x => x.vc_fee_structure == "Fixed Rate").Count() > 0)
                        {
                            foreach (var budgetsow in sitedetails.Where(x => x.vc_fee_structure == "Fixed Rate" && x.int_budget_sow_id != null).Select(x => x.int_budget_sow_id).Distinct())
                            {
                                var budgetsows = sitedetails.Where(x => x.int_budget_sow_id == budgetsow).FirstOrDefault();
                                List<InvTasks> budgetsowTask = new List<InvTasks>();
                                InvTasks budgetsowTasks = new InvTasks();
                                budgetsowTasks.name = "-";
                                budgetsowTasks.rowspan = 1;
                                List<InvActions> budgetsowAction = new List<InvActions>();
                                InvActions budgetsowActions = new InvActions();
                                budgetsowActions.int_invoice_item_id = budgetsows.int_invoice_item_id;
                                budgetsowActions.int_budget_sow_id = budgetsow;
                                budgetsowActions.name = "-";
                                budgetsowActions.jobcode = "-";
                                budgetsowActions.jobtitle = "-";
                                budgetsowActions.expensecode = "-";
                                budgetsowActions.isselect = (budgetsows.int_invoice_item_id != null || ((selectedactions != null && selectedactions.budgetsowids != null && selectedactions.budgetsowids.Count() > 0 && selectedactions.budgetsowids.Contains(budgetsow)))) ? true : false;
                                budgetsowActions.username = "-";
                                budgetsowActions.date = null;
                                budgetsowActions.percentinvoiced = (selectedactions == null && invoiceId == null) ? 0 : 100;
                                budgetsowActions.invoiced = ((Sow.quotedAmount == null ? 0 : Sow.quotedAmount) / 100) * budgetsowActions.percentinvoiced;
                                budgetsowActions.billed = budgetsows.dec_billed;
                                budgetsowActions.totalinvoiced = ((Sow.quotedAmount == null ? 0 : Sow.quotedAmount) / 100) - budgetsows.dec_remaining_unbilled;
                                budgetsowActions.remainingunbilled = (Sow.quotedAmount == null ? 0 : Sow.quotedAmount) - budgetsowActions.invoiced;
                                budgetsowActions.totalremainingunbilled = budgetsows.dec_remaining_unbilled;

                                if (budgetsows.int_invoice_item_id != null && budgetsows.int_invoice_item_id != 0 && budgetsows.dec_remaining_unbilled > 0)
                                {
                                    budgetsowActions.invoiced = 0;
                                    budgetsowActions.remainingunbilled = 0;
                                    budgetsowAction.Add(budgetsowActions);
                                    InvActions budgetsowActionsPartial = new InvActions()
                                    {
                                        int_invoice_item_id = null,
                                        int_budget_sow_id = budgetsowActions.int_budget_sow_id,
                                        name = budgetsowActions.name,
                                        jobcode = budgetsowActions.jobcode,
                                        jobtitle = budgetsowActions.jobtitle,
                                        expensecode = budgetsowActions.expensecode,
                                        isselect = false,
                                        username = budgetsowActions.username,
                                        date = budgetsowActions.date,
                                        percentinvoiced = budgetsowActions.percentinvoiced,
                                        invoiced = budgetsowActions.invoiced,
                                        billed = 0,
                                        totalinvoiced = budgetsowActions.totalinvoiced,
                                        remainingunbilled = budgetsowActions.remainingunbilled,
                                        totalremainingunbilled = budgetsowActions.totalremainingunbilled
                                    };

                                    budgetsowAction.Add(budgetsowActionsPartial);
                                    ++budgetsowTasks.rowspan;
                                    ++Site.rowspan;
                                    ++Sow.rowspan;
                                }
                                else
                                {
                                    budgetsowAction.Add(budgetsowActions);
                                }
                                budgetsowTasks.actions = budgetsowAction;
                                budgetsowTask.Add(budgetsowTasks);
                                Sow.tasks = budgetsowTask;
                                Sows.Add(Sow);
                            }
                        }
                        else
                        {
                            List<InvTasks> Tasks = new List<InvTasks>();
                            InvTasks Task;
                            foreach (var task in sitedetails.Where(x => x.vc_fee_structure == "Time & Expense" && x.int_budget_sow_id == null && x.int_task_id != null).Select(x => new { x.int_task_id, x.vc_task_name }).Distinct())
                            {
                                var actions = sitedetails.Where(x => x.int_task_id == task.int_task_id).ToList();
                                Task = new InvTasks();
                                List<InvActions> Actions = new List<InvActions>();
                                Task.name = task.vc_task_name;
                                Task.rowspan = actions.Count();
                                InvActions Action;
                                foreach (var action in actions.Where(x => x.int_budget_sow_id == null).Distinct())
                                {
                                    Action = new InvActions();
                                    Action.int_invoice_item_id = action.int_invoice_item_id;
                                    Action.int_action_log_hour_id = action.int_action_log_hour_id != null ? action.int_action_log_hour_id : null;
                                    Action.int_action_expense_id = action.int_action_expense_id != null ? action.int_action_expense_id : null;
                                    Action.int_timesheet_id = action.int_timesheet_id != null ? action.int_timesheet_id : null;
                                    Action.int_expense_id = action.int_expense_id != null ? action.int_expense_id : null;
                                    Action.int_budget_sow_id = null;
                                    Action.name = action.vc_action_name;
                                    Action.jobcode = action.vc_job_code;
                                    Action.jobtitle = action.vc_job_code != "-" ? action.vc_job_code + " (" + action.vc_job_title + ")" : "-";
                                    Action.expensecode = action.vc_expense_code;
                                    Action.worked = action.dec_worked_hours;
                                    Action.total = action.total;
                                    Action.expenses = action.expenses != null ? action.expenses : null;
                                    Action.isselect = (action.int_invoice_item_id != null || ((selectedactions != null && ((action.int_action_log_hour_id != null && selectedactions.actionloghourids != null && selectedactions.actionloghourids.Count() > 0 && selectedactions.actionloghourids.Contains(action.int_action_log_hour_id)) ||
                                        (action.int_action_expense_id != null && selectedactions.actionexpenseids != null && selectedactions.actionexpenseids.Count() > 0 && selectedactions.actionexpenseids.Contains(action.int_action_expense_id)) ||
                                        (action.int_timesheet_id != null && selectedactions.timesheetids != null && selectedactions.timesheetids.Count() > 0 && selectedactions.timesheetids.Contains(action.int_timesheet_id)) ||
                                        (action.int_expense_id != null && selectedactions.expensesids != null && selectedactions.expensesids.Count() > 0 && selectedactions.expensesids.Contains(action.int_expense_id)))
                                        ))) ? true : false;
                                    Action.username = action.vc_user_name;
                                    Action.date = action.dt_date;
                                    Action.percentinvoiced = 100;
                                    Action.invoiced = (((action.total == null ? 0 : action.total) + (action.expenses == null ? 0 : action.expenses)) / 100) * Action.percentinvoiced;
                                    Action.billed = action.dec_billed;
                                    Action.totalinvoiced = (((action.total == null ? 0 : action.total) + (action.expenses == null ? 0 : action.expenses)) / 100) - action.dec_remaining_unbilled;
                                    Action.remainingunbilled = ((action.total == null ? 0 : action.total) + (action.expenses == null ? 0 : action.expenses)) - Action.invoiced;
                                    Action.totalremainingunbilled = action.dec_remaining_unbilled;

                                    if (action.int_invoice_item_id != null && action.int_invoice_item_id != 0 && action.dec_remaining_unbilled > 0)
                                    {
                                        Action.invoiced = 0;
                                        Action.remainingunbilled = 0;
                                        Actions.Add(Action);
                                        InvActions actionPartial = new InvActions()
                                        {
                                            int_invoice_item_id = null,
                                            int_action_log_hour_id = Action.int_action_log_hour_id,
                                            int_action_expense_id = Action.int_action_expense_id,
                                            int_timesheet_id = Action.int_timesheet_id,
                                            int_budget_sow_id = Action.int_budget_sow_id,
                                            int_expense_id = Action.int_expense_id,
                                            name = Action.name,
                                            jobcode = Action.jobcode,
                                            jobtitle = Action.jobtitle,
                                            expensecode = Action.expensecode,
                                            worked = Action.worked,
                                            total = Action.total,
                                            isselect = false,
                                            expenses = Action.expenses,
                                            username = Action.username,
                                            date = Action.date,
                                            percentinvoiced = Action.percentinvoiced,
                                            invoiced = Action.invoiced,
                                            billed = 0,
                                            totalinvoiced = Action.totalinvoiced,
                                            remainingunbilled = Action.remainingunbilled,
                                            totalremainingunbilled = Action.totalremainingunbilled
                                        };

                                        Actions.Add(actionPartial);
                                        ++Task.rowspan;
                                        ++Site.rowspan;
                                        ++Sow.rowspan;
                                    }
                                    else
                                    {
                                        Actions.Add(Action);
                                    }
                                }
                                Task.actions = Actions;
                                Tasks.Add(Task);
                            }
                            Sow.tasks = Tasks;
                            Sows.Add(Sow);
                        }
                        Site.sows = Sows;
                    }
                    Sites.Add(Site);
                }

                return Sites;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //author:Vamsi 
        //date: 10/03/2020
        //To get all invoices & their item details
        public List<Invoices> GetInvoiceItemsDetails(long QuoteId)
        {
            try
            {
                var invoicedetails = _invDB.getinvoiceitemdetails(QuoteId).OrderBy(x => x.int_invoice_id).ToList();
                //var sites=(dynamic)null;
                List<Invoices> Invoices = new List<Invoices>();
                List<getinvoiceitemdetailsResult> invoiceitemsdetails = new List<getinvoiceitemdetailsResult>();
                foreach (var invoice in invoicedetails.Select(x => new { x.int_invoice_id,x.dec_HST_percent, x.vc_invoice_number, x.vc_manual_invoice_number, x.dt_invoice_date, x.dec_invoice_total, x.dec_balance_amount, x.int_days_old, x.vc_google_drive_file_id, x.vc_invoice_access_link,x.vc_sent_by,x.vc_sent_to,x.dt_sent_date,x.vc_invoice_status }).Distinct())
                {
                    invoiceitemsdetails = invoicedetails.Where(x => x.int_invoice_id == invoice.int_invoice_id).ToList();
                    Invoices Invoice = new Invoices();
                    Invoice.invoice_id = invoice.int_invoice_id;
                    Invoice.invoice_number = invoice.vc_invoice_number;
                    Invoice.manual_invoice_number = invoice.vc_manual_invoice_number;
                    Invoice.invoice_google_drive_file_id = invoice.vc_google_drive_file_id;
                    Invoice.invoice_access_link = invoice.vc_invoice_access_link;
                    Invoice.invoice_total = invoice.dec_invoice_total+((invoice.dec_HST_percent / 100)*invoice.dec_invoice_total);
                    Invoice.balance_amount = invoice.dec_balance_amount;
                    Invoice.invoice_date = invoice.dt_invoice_date;
                    Invoice.days_old = invoice.int_days_old;
                    Invoice.sent_to = invoice.vc_sent_to;
                    Invoice.sent_date = invoice.dt_sent_date==null?"-" : invoice.dt_sent_date.Value.ToString("yyyy-MM-dd");
                    Invoice.invoice_status = invoice.vc_invoice_status;
                    Invoice.sent_by = invoice.vc_sent_by;
                    Invoice.rowspan = invoiceitemsdetails.Count();
                    List<InvSites> Sites = new List<InvSites>();
                    InvSites Site;
                    foreach (var site in invoicedetails.Select(x => new { x.int_site_id, x.vc_site_name }).Distinct())
                    {
                        invoiceitemsdetails = invoicedetails.Where(x => x.int_site_id == site.int_site_id && x.int_invoice_id == invoice.int_invoice_id).ToList();
                        Site = new InvSites();
                        Site.name = site.vc_site_name;
                        Site.rowspan = invoiceitemsdetails.Count();
                        List<InvSows> Sows = new List<InvSows>();
                        InvSows Sow;
                        foreach (var sow in invoiceitemsdetails.Select(x => new { x.int_sow_id, x.vc_sow_name, x.vc_fee_structure, x.quoted_total }).Distinct())
                        {
                            invoiceitemsdetails = invoicedetails.Where(x => x.int_site_id == site.int_site_id && x.int_sow_id == sow.int_sow_id && x.int_invoice_id == invoice.int_invoice_id).ToList();
                            Sow = new InvSows();
                            Sow.name = sow.vc_sow_name;
                            Sow.quotedAmount = sow.quoted_total;
                            Sow.feestructure = sow.vc_fee_structure;
                            Sow.rowspan = invoiceitemsdetails.Count();
                            if (invoiceitemsdetails.Where(x => x.vc_fee_structure == "Fixed Rate").Count() > 0)
                            {
                                foreach (var budgetsow in invoiceitemsdetails.Where(x => x.vc_fee_structure == "Fixed Rate" && x.int_budget_sow_id != null).Select(x => x.int_budget_sow_id).Distinct())
                                {
                                    var budgetsows = invoicedetails.Where(x => x.int_budget_sow_id == budgetsow && x.int_invoice_id == invoice.int_invoice_id).FirstOrDefault();
                                    List<InvTasks> budgetsowTask = new List<InvTasks>();
                                    InvTasks budgetsowTasks = new InvTasks();
                                    budgetsowTasks.name = "-";
                                    budgetsowTasks.rowspan = 1;
                                    List<InvActions> budgetsowAction = new List<InvActions>();
                                    InvActions budgetsowActions = new InvActions();
                                    budgetsowActions.int_invoice_item_id = budgetsows.int_invoice_item_id;
                                    budgetsowActions.int_budget_sow_id = budgetsow;
                                    budgetsowActions.name = "-";
                                    budgetsowActions.jobcode = "-";
                                    budgetsowActions.jobtitle = "-";
                                    budgetsowActions.expensecode = "-";
                                    budgetsowAction.Add(budgetsowActions);
                                    budgetsowTasks.actions = budgetsowAction;
                                    budgetsowTask.Add(budgetsowTasks);
                                    Sow.tasks = budgetsowTask;
                                    Sows.Add(Sow);
                                }
                            }
                            else
                            {
                                List<InvTasks> Tasks = new List<InvTasks>();
                                InvTasks Task;
                                foreach (var task in invoiceitemsdetails.Where(x => x.vc_fee_structure == "Time & Expense" && x.int_budget_sow_id == null && x.int_task_id != null).Select(x => new { x.int_task_id, x.vc_task_name }).Distinct())
                                {
                                    var actions = invoicedetails.Where(x => x.int_task_id == task.int_task_id && x.int_invoice_id == invoice.int_invoice_id).ToList();
                                    Task = new InvTasks();
                                    List<InvActions> Actions = new List<InvActions>();
                                    Task.name = task.vc_task_name;
                                    Task.rowspan = actions.Count();
                                    InvActions Action;
                                    foreach (var action in actions.Where(x => x.int_budget_sow_id == null).Distinct())
                                    {
                                        Action = new InvActions();
                                        Action.int_invoice_item_id = action.int_invoice_item_id;
                                        Action.int_action_log_hour_id = action.int_action_log_hour_id != null ? action.int_action_log_hour_id : null;
                                        Action.int_action_expense_id = action.int_action_expense_id != null ? action.int_action_expense_id : null;
                                        Action.int_timesheet_id = action.int_timesheet_id != null ? action.int_timesheet_id : null;
                                        Action.int_expense_id = action.int_expense_id != null ? action.int_expense_id : null;
                                        Action.name = action.vc_action_name;
                                        Action.jobcode = action.vc_job_code;
                                        Action.jobtitle = action.vc_job_code != "-" ? action.vc_job_code + " (" + action.vc_job_title + ")" : "-"; ;
                                        Action.expensecode = action.vc_expense_code;
                                        Action.worked = action.dec_worked_hours;
                                        Action.total = action.total;
                                        Action.expenses = action.expenses != null ? action.expenses : null;
                                        Actions.Add(Action);
                                    }
                                    Task.actions = Actions;
                                    Tasks.Add(Task);
                                }
                                Sow.tasks = Tasks;
                                Sows.Add(Sow);
                            }
                            Site.sows = Sows;
                        }
                        Sites.Add(Site);
                        Invoice.sites = Sites;
                    }
                    Invoices.Add(Invoice);
                }
                return Invoices;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //author:Vamsi 
        //date: 09/03/2020
        //To get all details of a specific client
        public InvQuoteDetails GetQuoteDetailsbyId(long? quoteId, long? invoiceId)
        {
            InvQuoteDetails invquoteDetails = new InvQuoteDetails();
            var invoicedetails = _invDB.getinvoicedetailsbyinvoiceid(invoiceId).FirstOrDefault();
            var Quote = _quotesExtensionDB.GetQuoteDetailsById(quoteId).FirstOrDefault();
            if (Quote != null)
            {
                invquoteDetails.int_quote_id = Quote.quoteId;
                invquoteDetails.int_client_id = Quote.clientId;
                invquoteDetails.client_name = Quote.clientName;
                invquoteDetails.client_email = Quote.clientEmail;
                invquoteDetails.client_phone = Quote.clientPhone;
                invquoteDetails.project_manager_name = Quote.projectManager;
                invquoteDetails.account_manager_name = Quote.accountManager;
                invquoteDetails.int_contact_id = invoiceId != null ? invoicedetails.int_contact_id : null;
                invquoteDetails.dt_invoice_date = invoiceId != null ? invoicedetails.dt_created_date : DateTime.Now;
                invquoteDetails.is_existing_client = (invoiceId != null && invoicedetails.int_contact_id ==null) ? false : true;
                invquoteDetails.int_invoice_id = invoiceId != null ? invoiceId : null;
                invquoteDetails.auto_invoice_number = invoiceId != null ? invoicedetails.vc_invoice_number : GetLatestInvoiceNumber(quoteId);
                invquoteDetails.manual_invoice_number = invoiceId != null ? invoicedetails.vc_manual_invoice_number : "";
                invquoteDetails.purchase_order_number = invoiceId != null ? invoicedetails.vc_purchase_order_number : "";
                invquoteDetails.invoice_total = invoiceId != null ? invoicedetails.dec_invoice_total : 0; ;
                invquoteDetails.actual_total = 0;
                invquoteDetails.description = invoiceId!=null? invoicedetails.vc_description : "";
            }
            return invquoteDetails;
        }
        //author:Vamsi 
        //date: 09/03/2020
        //To get all contacts associated to a particular client
        public List<ClientContacts> GetContactDetailsbyClientId(long? clientId)
        {
            InvQuoteDetails invquoteDetails = new InvQuoteDetails();
            List<ClientContacts> contacts = _invDB.getcontactdetailsbyclientid(clientId).Select(x => new ClientContacts
            {
                int_contact_id = x.int_contact_id,
                contact_name = x.vc_contact_name,
                job_title = x.vc_job_title,
                contact_email = x.vc_contact_email,
                contact_phone = x.vc_contact_phone_number
            }).OrderBy(x => x.contact_name).ToList();
            return contacts;
        }
        //author:Vamsi 
        //date: 10/03/2020
        //To get latest invoice number for a quote
        public string GetLatestInvoiceNumber(long? quoteId)
        {
            return _invDB.generatelatestinvoicenumberbyquoteid(quoteId).Select(x => x.vc_invoice_number).FirstOrDefault();
        }
        //author:Vamsi 
        //date: 10/03/2020
        //To insert or update invoice and invoice items for a particular quote
        public long? UpsertInvoiceItems(InvQuoteDetails quotedetails, List<InvoiceItems> InvoiceItems, long? userid)
        {
            try
            {
                long? result = null;
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[8] {
                    new DataColumn("int_invoice_item_id", typeof(int)),
                    new DataColumn("int_action_log_hour_id", typeof(int)),
                    new DataColumn("int_action_expense_id", typeof(int)),
                    new DataColumn("int_budget_sow_id", typeof(int)),
                    new DataColumn("int_timesheet_id", typeof(int)),
                    new DataColumn("int_expense_id", typeof(int)),
                    new DataColumn("dec_invoiced_percent", typeof(decimal)),
                    new DataColumn("dec_invoiced_total", typeof(decimal))
                });
                foreach (var row in InvoiceItems)
                {
                    int? int_invoice_item_id = (row.int_invoice_item_id != null) ? (int?)Convert.ToInt32(row.int_invoice_item_id) : null;
                    int? int_action_log_hour_id = (row.int_action_log_hour_id != null) ? (int?)Convert.ToInt32(row.int_action_log_hour_id) : null;
                    int? int_action_expense_id = (row.int_action_expense_id != null) ? (int?)Convert.ToInt32(row.int_action_expense_id) : null;
                    int? int_budget_sow_id = (row.int_budget_sow_id != null) ? (int?)Convert.ToInt32(row.int_budget_sow_id) : null;
                    int? int_timesheet_id = (row.int_timesheet_id != null) ? (int?)Convert.ToInt32(row.int_timesheet_id) : null;
                    int? int_expense_id = (row.int_expense_id != null) ? (int?)Convert.ToInt32(row.int_expense_id) : null;
                    decimal dec_invoiced_percent = Convert.ToDecimal(row.dec_invoiced_percent);
                    decimal dec_invoiced_total = Convert.ToDecimal(row.dec_invoiced_total);
                    dt.Rows.Add(int_invoice_item_id, int_action_log_hour_id, int_action_expense_id, int_budget_sow_id, int_timesheet_id, int_expense_id, dec_invoiced_percent, dec_invoiced_total);
                }
                using (var con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("upsert_invoice_and_invoice_items_details", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@vc_invoice_number", quotedetails.auto_invoice_number);
                        if (!string.IsNullOrEmpty(quotedetails.manual_invoice_number))
                        {
                            cmd.Parameters.AddWithValue("@vc_manual_invoice_number", quotedetails.manual_invoice_number);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@vc_manual_invoice_number", DBNull.Value);
                        }
                        if (!string.IsNullOrEmpty(quotedetails.purchase_order_number))
                        {
                            cmd.Parameters.AddWithValue("@vc_purchase_order_number", quotedetails.purchase_order_number);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@vc_purchase_order_number", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@dec_invoice_total", quotedetails.invoice_total);
                        if (!string.IsNullOrEmpty(quotedetails.description))
                        {
                            cmd.Parameters.AddWithValue("@vc_description", quotedetails.description);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@vc_description", DBNull.Value);
                        }
                        if (quotedetails.int_invoice_id != null)
                        {
                            cmd.Parameters.AddWithValue("@int_invoice_id", quotedetails.int_invoice_id);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@int_invoice_id", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@int_quote_id", quotedetails.int_quote_id);
                        if (quotedetails.int_contact_id != null)
                        {
                            cmd.Parameters.AddWithValue("@int_contact_id", quotedetails.int_contact_id);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@int_contact_id", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@invoice_items", dt);
                        cmd.Parameters.AddWithValue("@user_id", userid);
                        //Add the output parameter to the command object
                        SqlParameter outPutParameter = new SqlParameter();
                        outPutParameter.ParameterName = "@result";
                        outPutParameter.SqlDbType = System.Data.SqlDbType.Int;
                        outPutParameter.Direction = System.Data.ParameterDirection.Output;
                        cmd.Parameters.Add(outPutParameter);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        result = Convert.ToInt32(outPutParameter.Value);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //author:Vamsi 
        //date: 10/03/2020
        //To get latest invoice number for a quote
        public string CheckInvoiceNumberUniquness(long? invoiceId, long? quoteId, string vc_invoice_number, string vc_manual_invoice_number)
        {
            string result = "";
            result =_invDB.checkinvoicenumberuniqueness(quoteId, invoiceId, vc_invoice_number, vc_manual_invoice_number).Select(x=>x.message).FirstOrDefault();
            return result;
        }
        //author:Vamsi 
        //date: 17/03/2020
        //To get invoice terms and conditions
        public InvoiceTermsandConditions GetInvoiceTandC()
        {
            InvoiceTermsandConditions InvoiceTandC = new InvoiceTermsandConditions();
            if (_invDB.tbl_ibw_invoice_terms_and_conditions.Count() > 0)
                InvoiceTandC.termsandconditions = _invDB.tbl_ibw_invoice_terms_and_conditions.Select(x => x.vc_terms_and_conditions).FirstOrDefault();
            return InvoiceTandC;
        }
        //author:Vamsi 
        //date: 17/03/2020
        //To update invoice terms and conditions
        public bool UpdateInvoiceTandC(string InvoiceTandC, long? userId)
        {
            SqlCommand cmd;
            bool issuccess = false;
            try
            {
                SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                conn.Open();
                if (_invDB.tbl_ibw_invoice_terms_and_conditions.Count() > 0)
                {
                    cmd = new SqlCommand("update tbl_ibw_invoice_terms_and_conditions set vc_terms_and_conditions=@TandC, int_last_modified_by=@userid, dt_last_modified_date=@modifiedDate", conn);
                }
                else
                {
                    cmd = new SqlCommand("insert into tbl_ibw_invoice_terms_and_conditions values(@TandC,@userid, @modifiedDate)", conn);
                }
                cmd.Parameters.AddWithValue("@TandC", InvoiceTandC);
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@modifiedDate", DateTime.Now);
                cmd.ExecuteNonQuery();
                conn.Close();
                issuccess = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            return issuccess;
        }
        public void DeleteInvoicePdfFromGDrive(long? invoiceId)
        {
            var invoicedetails = _invDB.getinvoicedetailsbyinvoiceid(invoiceId).FirstOrDefault();
            if (invoicedetails != null)
            {
                if (invoicedetails.vc_google_drive_file_id != null)
                    GoogleDriveFilesRepository.DeleteFile(invoicedetails.vc_google_drive_file_id);
            }
        }
        //author:Vamsi 
        //date: 18/03/2020
        //To delete invoice details
        public bool DeleteInvoiceDetails(long? invoiceId, long? userId)
        {
            //var invoicedetails = _invDB.getinvoicedetailsbyinvoiceid(invoiceId).FirstOrDefault();
            //if(invoicedetails != null)
            //{
            //    GoogleDriveFilesRepository.DeleteFile(invoicedetails.vc_google_drive_file_id);
            //}
            SqlCommand cmd;
            userId = 1;
            bool issuccess = false;
            try
            {
                SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                conn.Open();
                cmd = new SqlCommand("delete from tbl_ibw_invoice_items where int_invoice_id=@invoiceId; update tbl_ibw_invoices set bt_delete=1, int_modified_by_id=@userid, dt_modified_date=@modifiedDate where int_invoice_id=@invoiceId;", conn);

                cmd.Parameters.AddWithValue("@invoiceId", invoiceId);
                cmd.Parameters.AddWithValue("@userid", userId);
                cmd.Parameters.AddWithValue("@modifiedDate", DateTime.Now);
                cmd.ExecuteNonQuery();
                conn.Close();
                List<string> fileids = GetInvoiceFileIds(invoiceId).Select(x=>x.vc_file_id).ToList();
                if(fileids!=null && fileids.Count() > 0)
                {
                    foreach(var vc_file_id in fileids)
                    {
                        GoogleDriveFilesRepository.DeleteFile(vc_file_id);
                    }
                }
                issuccess = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            return issuccess;
        }
        //author:Vamsi 
        //date: 26/03/2020
        //To get all invoice file ids
        public List<getinvoicefileidsResult> GetInvoiceFileIds(long? invoiceId)
        {
            return _invDB.getinvoicefileids(invoiceId).ToList();
        }
        //author:Vamsi 
        //date: 19/03/2020
        //To get all invoice fee details
        public List<getinvoicefeedetailsResult> GetInvoiceFeeDetails(long? invoiceId)
        {
            return _invDB.getinvoicefeedetails(invoiceId).ToList();
        }
        //author:Vamsi 
        //date: 20/03/2020
        //To get client details by client id
        public getinvoiceclientdetailsbyclientidResult GetClientDetailsbyClientId(long? clientId)
        {
            return _invDB.getinvoiceclientdetailsbyclientid(clientId).FirstOrDefault();
        }
        //author:Vamsi 
        //date: 20/03/2020
        //To update invoice google drive file id and access link
        public void UpdateInvoiceGoogleDriveDetails(long? invoiceId, string vc_google_drive_file_id, string vc_invoice_access_link)
        {
            _invDB.updateinvoicegoogledrivedetails(invoiceId, vc_google_drive_file_id, vc_invoice_access_link);
        }
        //author:Vamsi 
        //date: 22/03/2020
        //To update invoice payment details
        public long? UpdateInvoicePaymentDetails(long? invoiceId, long? int_payment_type_id, decimal? dec_amount, string comments, long? userid, InvoicepaymentDetails data, HttpPostedFile attachment)
        {
            long? result = null;
            long? attachmentresult = null;
            _invDB.updateinvoicepaymentdetails(invoiceId, int_payment_type_id, dec_amount, comments, userid, ref result);
            if (result > 0)
            {
                string fileName = attachment.FileName;
                fileName = data.vc_invoice_number + "-" + fileName + "-"+ data.vc_invoice_date + " - " + DateTime.Now.ToString("hh:mm tt");
                string GoogleFileId = GoogleDriveFilesRepository.FileUploadInFolder(HelperMethods.Literals.GDriveInvoicegFolderId, attachment, fileName, attachment.ContentType);
                if (!string.IsNullOrEmpty(GoogleFileId))
                {
                    string invoiceaccesslink = GoogleDriveFilesRepository.GetSharableLink(GoogleFileId);
                    if (!string.IsNullOrEmpty(invoiceaccesslink))
                        attachmentresult = UpdateInvoicePaymentDetailsAttachment(result, GoogleFileId, invoiceaccesslink);
                }
            }
            return attachmentresult;
        }
        //author:Vamsi 
        //date: 22/03/2020
        //To update invoice payment details attachment
        public long? UpdateInvoicePaymentDetailsAttachment(long? invoicepaymentdetailsId, string vc_google_drive_file_id, string vc_file_access_link)
        {
            long? result = null;
            _invDB.updateinvoicepaymentdetailsattachmentinfo(invoicepaymentdetailsId, vc_google_drive_file_id, vc_file_access_link, ref result);
            return result;
        }
        //author:Vamsi 
        //date: 22/03/2020
        //To get invoice details by invoice id
        public InvoicepaymentDetails GetInvoiceDetails(long InvoiceId)
        {
            try
            {
                InvoicepaymentDetails invoicedetails = new InvoicepaymentDetails();
                var invoice = _invDB.getinvoicedetailsbyinvoiceid(InvoiceId).FirstOrDefault();
                invoicedetails.InvoiceId = InvoiceId;
                invoicedetails.vc_invoice_number = invoice.vc_invoice_number;
                invoicedetails.vc_manual_invoice_number = invoice.vc_manual_invoice_number;
                invoicedetails.dec_invoice_total =(invoice.dec_invoice_total+((invoice.dec_HST_percent.Value/100)*invoice.dec_invoice_total));
                invoicedetails.Amount = invoice.dec_balance_amount;
                invoicedetails.vc_invoice_date = invoice.dt_created_date.ToString("yyyy-MM-dd");
                invoicedetails.PaymentTypeId = _adminDB.GetLookupOptions().Where(x=>x.vc_lookup_name=="Full").Select(x=>x.int_lookup_id).FirstOrDefault();
                invoicedetails.InvoiceFileId = invoice.vc_google_drive_file_id;
                invoicedetails.ClientName = invoice.client_name;
                invoicedetails.ContactName = invoice.contact_name == null ? "-" : invoice.contact_name;
                invoicedetails.InvoiceStatus = invoice.vc_invoice_status;
                invoicedetails.FromEmail = "";
                invoicedetails.ToEmail = invoice.vc_send_to_email;
                invoicedetails.vc_sent_to = invoice.contact_name == null ? invoice.client_name : invoice.contact_name;
                invoicedetails.vc_project_manager_email = invoice.vc_project_manager_email;
                invoicedetails.vc_financial_email = invoice.vc_financial_email;
                invoicedetails.vc_access_link = invoice.vc_invoice_access_link;
                invoicedetails.ProjectNumber = invoice.projectNumber;
                return invoicedetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //author:Vamsi 
        //date: 23/03/2020
        //To get invoice payment details
        public List<InvoicePaymentHistory> GetInvoicePaymentDetails(long? invoiceId)
        {
            List<InvoicePaymentHistory> details = _invDB.getinvoicepaymentdetails(invoiceId).Select(x => new InvoicePaymentHistory {
                vc_posted_date = (x.dt_created_on.Value).ToString("yyyy-MM-dd, h:mm tt"),
                vc_posted_by=x.created_by,
                vc_payment_type=x.payment_type,
                Amount=x.dec_amount,
                Comments=x.vc_comments,
                vc_access_link=x.vc_invoice_payment_details_attachment_access_link
            }).ToList();
            return details;
        }
        //author:Vamsi 
        //date: 27/03/2020
        //To update sent invoice details
        public void UpdateSentInvoice(long? invoiceId, string vc_sent_to, string vc_sent_by, long? userId)
        {
            long? result = null;
            _invDB.updatesentinvoicedetails(invoiceId, vc_sent_by, vc_sent_to, userId, ref result);
        }
    }
}
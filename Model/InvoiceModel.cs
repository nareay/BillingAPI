namespace BillingAPI.Models
{
    public class InvoiceModel
    {
        public int RefNo { get; set; }
        public string InvoiceNo { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public string BillType { get; set; } = "";
        public string OrderNo { get; set; } = "";
        public DateTime? OrderDate { get; set; }
        public string TermsPayment { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string AddressOne { get; set; } = "";
        public string AddressTwo { get; set; } = "";
        public string AddressThree { get; set; } = "";
        public string AddressFour { get; set; } = "";
        public string CustomerPhone { get; set; } = "";

        public string DeliveryName { get; set; } = "";
        public string DelAddressOne { get; set; } = "";
        public string DelAddressTwo { get; set; } = "";
        public string DelAddressThree { get; set; } = "";
        public string DelAddressFour { get; set; } = "";
        public string DeliveryPhone { get; set; } = "";


        public string CustomerGSTNo { get; set; } = "";
        public string GSTState { get; set; } = "";
        public string ItemNo { get; set; } = "";
        public string Description { get; set; } = "";
        public string HSNSAC { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public string PER { get; set; } = "";
        public decimal GSTPC { get; set; }
        public string RupeesOne { get; set; } = "";
        public string RupeesTwo { get; set; } = "";
    }
}

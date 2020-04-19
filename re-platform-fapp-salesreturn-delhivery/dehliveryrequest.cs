using System;
using System.Collections.Generic;
using System.Text;

namespace re_platform_fapp_salesreturn_delhivery
{



    public class Response
    {
        public bool success { get; set; }
        public string msg { get; set; }
     //   public string magento_order_id { get; set; }
     //   public string magento_return_id { get; set; }
     //   public string sap_uniq_no { get; set; }
        public string sap_return_id { get; set; }
        public string awb_no { get; set; }
        public string carrier_name { get; set; }
        public string carrier_code { get; set; }
    }








    public class PickupLocation
    {
        public string add { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string pin { get; set; }
    }

    public class Shipment
    {
        public string waybill { get; set; }
        public string client { get; set; }
        public string name { get; set; }
        public string order { get; set; }
        public string products_desc { get; set; }
        public DateTime order_date { get; set; }
        public string payment_mode { get; set; }
        public int total_amount { get; set; }
        public int cod_amount { get; set; }
        public string add { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public string pin { get; set; }
        public string return_add { get; set; }
        public string return_city { get; set; }
        public string return_country { get; set; }
        public string return_name { get; set; }
        public string return_phone { get; set; }
        public string return_pin { get; set; }
        public string return_state { get; set; }
        public object supplier { get; set; }
        public object extra_parameters { get; set; }
        public object shipment_width { get; set; }
        public object shipment_height { get; set; }
        public string weight { get; set; }
        public int quantity { get; set; }
        public object seller_inv { get; set; }
        public DateTime seller_inv_date { get; set; }
        public string seller_name { get; set; }
        public string seller_add { get; set; }
        public object seller_cst { get; set; }
        public object seller_tin { get; set; }
        public object consignee_tin { get; set; }
        public object commodity_value { get; set; }
        public object tax_value { get; set; }
        public object sales_tax_form_ack_no { get; set; }
        public string category_of_goods { get; set; }
        public string seller_gst_tin { get; set; }
        public object client_gst_tin { get; set; }
        public string consignee_gst_tin { get; set; }
        public string invoice_reference { get; set; }
        public object hsn_code { get; set; }
    }



    public class Sales_Return
    {

        public string MAGENTO_UNIQ_NO { get; set; }
        public string MAGENTO_ORDER_NO { get; set; }
        public string SAP_SALE_ORDER_NO { get; set; }
        public string SAP_INVOICE_NO { get; set; }


    }

    public class RootObject
    {
        public string issuccess { get; set; }
        public PickupLocation pickup_location { get; set; }
        public List<Shipment> shipments { get; set; }
        public string MAGENTO_UNIQ_NO { get; set; }
        public string MAGENTO_ORDER_NO { get; set; }
        public string SAP_SALE_ORDER_NO { get; set; }
        public string SAP_INVOICE_NO { get; set; }
    }

    public class dehliveryrequest
    {
     



        public PickupLocation pickup_location { get; set; }
        public List<Shipment> shipments { get; set; }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Net;
using System.Xml;

namespace re_platform_fapp_salesreturn_delhivery
{
    public static class re_platform_fapp_salesreturn_delhivery
    {
        public static string mysqlconnectionstring { get; set; }


        [FunctionName("salesreturn")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Response response = new Response();
            string res = string.Empty;
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                string connectionstring = Environment.GetEnvironmentVariable("connectionstring");
                var salesreturnrequestobj = JsonConvert.DeserializeObject<RootObject>(requestBody);

                //Temprory code for success
                if (!string.IsNullOrEmpty(salesreturnrequestobj.issuccess))
                {
                    res = Dehlivey_POST_ReverseManifest(salesreturnrequestobj);
                    var responsedehilvery = JsonConvert.DeserializeObject<delhiveryresponse>(res);

                    if (responsedehilvery.packages == null)
                    {
                        response.awb_no = "";
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";

                        response.msg = res;
                        response.sap_return_id = "";
                        // response.sap_uniq_no = "";
                        response.success = false;


                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                        };

                    }

                    if (responsedehilvery.packages.Count == 0)
                    {

                        response.awb_no = "";
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";

                        response.msg = res;
                        response.sap_return_id = "";
                        // response.sap_uniq_no = "";
                        response.success = false;


                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                        };

                    }

                    if (responsedehilvery.packages[0].status.ToLower() == "success")
                    {

                        Random generator = new Random();
                        response.awb_no = responsedehilvery.packages[0].waybill;
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";

                        response.msg = "return created successfully";
                        response.sap_return_id = generator.Next(1, 999999).ToString("D5");

                        response.success = true;

                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                        };
                    }
                    else
                    {


                        response.awb_no = responsedehilvery.packages[0].waybill;
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";

                        response.msg = responsedehilvery.packages[0].remarks[0];
                        response.sap_return_id = "";
                        // response.sap_uniq_no = "";
                        response.success = false;


                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                        };

                    }

                }



                Sales_Return saprequest = new Sales_Return();

                // sap request param
                saprequest.MAGENTO_ORDER_NO = salesreturnrequestobj.MAGENTO_ORDER_NO;
                saprequest.MAGENTO_UNIQ_NO = salesreturnrequestobj.MAGENTO_UNIQ_NO;
                saprequest.SAP_INVOICE_NO = salesreturnrequestobj.SAP_INVOICE_NO;
                saprequest.SAP_SALE_ORDER_NO = salesreturnrequestobj.SAP_SALE_ORDER_NO;

                var sapres = SAP_POST_SalesReturn(saprequest);




                if (sapres.MSG_TYP == "e")
                {

                    response.awb_no = "";
                    response.carrier_code = "delhivery";
                    response.carrier_name = "Delhivery";
                    response.msg = sapres.message;
                    response.sap_return_id = "";
                    response.success = false;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                    };

                }
                if (sapres.MSG_TYP == "s")
                {
                    res = Dehlivey_POST_ReverseManifest(salesreturnrequestobj);
                    var responsedehilvery = JsonConvert.DeserializeObject<delhiveryresponse>(res);
                    if (responsedehilvery.packages[0].status.ToLower() == "success")
                    {

                        response.awb_no = responsedehilvery.packages[0].waybill;
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";
                        response.msg = "return created successfully";
                        response.sap_return_id = sapres.ordernumber;
                        response.success = true;


                    }
                    else
                    {


                        response.awb_no = responsedehilvery.packages[0].waybill;
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";

                        response.msg = responsedehilvery.packages[0].remarks[0];
                        response.sap_return_id = sapres.ordernumber;
                        response.success = false;

                    }

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                    };

                }

                response.awb_no = "";
                response.carrier_code = "delhivery";
                response.carrier_name = "Delhivery";
                response.msg = "";
                response.sap_return_id = "";
                response.success = false;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                };

            }
            catch (Exception ex)
            {
                response.awb_no = "";
                response.carrier_code = "delhivery";
                response.carrier_name = "Delhivery";
                response.msg = ex.Message;
                response.sap_return_id = "";
                response.success = false;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                };
            }



        }



        public static sapresponse SAP_POST_SalesReturn(Sales_Return sapRequest)
        {
            sapresponse obj = new sapresponse();
            var sss = new StringBuilder("<?xml version='1.0' encoding='UTF-8'?>");
            sss.Append("<ZBAPI_MGN_SALES_RETURN xmlns='http://Microsoft.LobServices.Sap/2007/03/Rfc/'><IT_TABLE1><ZSTR_MGN_SALES_RETURN_IT xmlns='http://Microsoft.LobServices.Sap/2007/03/Types/Rfc/'><MAGENTO_UNIQ_NO>" + sapRequest.MAGENTO_UNIQ_NO + "</MAGENTO_UNIQ_NO><MAGENTO_ORDER_NO>" + sapRequest.MAGENTO_ORDER_NO + "</MAGENTO_ORDER_NO><SAP_SALE_ORDER_NO>" + sapRequest.SAP_SALE_ORDER_NO + "</SAP_SALE_ORDER_NO><SAP_INVOICE_NO>" + sapRequest.SAP_INVOICE_NO + "</SAP_INVOICE_NO></ZSTR_MGN_SALES_RETURN_IT></IT_TABLE1></ZBAPI_MGN_SALES_RETURN>");
            var lapp_salesreturnurl = Environment.GetEnvironmentVariable("lapp_salesreturnurl");
            var content = new StringContent(sss.ToString());

            using (var client = new HttpClient())
            {


                var result = client.PostAsync(lapp_salesreturnurl, content).Result;

                if (result.IsSuccessStatusCode)
                {

                    XmlDocument xmlDoc = new XmlDocument();
                    string p = result.Content.ReadAsStringAsync().Result;
                    var t = p.Replace("xmlns", "name");
                    xmlDoc.LoadXml(t);
                    string MSG_TYP = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MSG_TYP").InnerText;
                    if (MSG_TYP.ToLower() == "s")
                    {
                        string MESSAGE = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MESSAGE").InnerText;
                        string MAGENTO_UNIQ_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MAGENTO_UNIQ_NO").InnerText;
                        string RETURN_ORD_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/RETURN_ORD_NO").InnerText;
                        string SAP_INVOICE_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/SAP_INVOICE_NO").InnerText;

                        obj.ordernumber = RETURN_ORD_NO;
                        obj.SAP_INVOICE_NO = SAP_INVOICE_NO;
                        obj.message = MESSAGE;
                        obj.MSG_TYP = MSG_TYP.ToLower();

                    }

                    if (string.IsNullOrEmpty(MSG_TYP) || MSG_TYP.ToLower() == "e")
                    {
                        string MESSAGE = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MESSAGE").InnerText;

                        string MAGENTO_UNIQ_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MAGENTO_UNIQ_NO").InnerText;
                        string RETURN_ORD_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/RETURN_ORD_NO").InnerText;
                        string SAP_INVOICE_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/SAP_INVOICE_NO").InnerText;
                        obj.message = MESSAGE;
                        obj.MSG_TYP = "e";

                    }
                }

                return obj;
            }
        }

        public static string Dehlivey_POST_ReverseManifest(RootObject salesreturnrequestobj)
        {


            dehliveryrequest objrequest = new dehliveryrequest();
            objrequest.shipments = new List<Shipment>();

            objrequest.shipments = salesreturnrequestobj.shipments;
            objrequest.pickup_location = salesreturnrequestobj.pickup_location;


            var dehlivery_Reversemenifesturl = Environment.GetEnvironmentVariable("dehlivery_Reversemenifesturl");

            var jsonserialize = JsonConvert.SerializeObject(objrequest);

            string token = Environment.GetEnvironmentVariable("token");
            string dehliveryapipassword = Environment.GetEnvironmentVariable("dehliveryapipassword");
            using (var client = new HttpClient())
            {



                string str = "format=json&data=" + System.Net.WebUtility.UrlEncode(jsonserialize);
                StringContent stringContent = new StringContent(str);

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);



                var result = client.PostAsync(dehlivery_Reversemenifesturl, stringContent).Result;

                return result.Content.ReadAsStringAsync().Result;




            }
        }
    }


    public class waybillresponse
    {
        public int reference_id { get; set; }
        public string success { get; set; }
        public List<int> awb { get; set; }
    }

    public class sapresponse
    {

        public string ordernumber { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string SAP_INVOICE_NO { get; set; }
        public string MSG_TYP { get; set; }

    }

}

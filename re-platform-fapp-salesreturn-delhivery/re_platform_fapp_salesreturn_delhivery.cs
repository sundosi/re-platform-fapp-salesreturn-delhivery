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
          //  var responsedehilvery = new delhiveryresponse();
        mysqlconnectionstring = Environment.GetEnvironmentVariable("mysqlconnectionstring");

         
            string res = string.Empty;
            //SAP_POST_SalesReturn();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                string connectionstring = Environment.GetEnvironmentVariable("connectionstring");

            
                var salesreturnrequestobj = JsonConvert.DeserializeObject<RootObject>(requestBody);




               

                if (!string.IsNullOrEmpty(salesreturnrequestobj.issuccess))
                {
                    res = Dehlivey_POST_ReverseManifest(salesreturnrequestobj);
                  var  responsedehilvery = JsonConvert.DeserializeObject<delhiveryresponse>(res);

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



                Sales_Return saprequest = new Sales_Return();

                // sap request param
                saprequest.MAGENTO_ORDER_NO = salesreturnrequestobj.MAGENTO_ORDER_NO;
                saprequest.MAGENTO_UNIQ_NO = salesreturnrequestobj.MAGENTO_UNIQ_NO;
                saprequest.SAP_INVOICE_NO = salesreturnrequestobj.SAP_INVOICE_NO;
                saprequest.SAP_SALE_ORDER_NO = salesreturnrequestobj.SAP_SALE_ORDER_NO;

         var sapres =       SAP_POST_SalesReturn(saprequest);




                if (sapres.MSG_TYP == "e")
                {
 
                    response.awb_no = "";
                    response.carrier_code = "delhivery";
                    response.carrier_name = "Delhivery";
                  //  response.magento_order_id = "";
                  //  response.magento_return_id = "";
                    response.msg = sapres.message;
                    response.sap_return_id = "";
                   // response.sap_uniq_no = "";
                    response.success = false;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
                    };



                }
                if (sapres.MSG_TYP == "s")
                {
                    res = Dehlivey_POST_ReverseManifest(salesreturnrequestobj);
                  var  responsedehilvery = JsonConvert.DeserializeObject<delhiveryresponse>(res);
                    if (responsedehilvery.packages[0].status.ToLower() == "success")
                    {
                      
                        response.awb_no = responsedehilvery.packages[0].waybill;
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";
                     
                        response.msg = "return created successfully";
                        response.sap_return_id = sapres.ordernumber;
                      
                        response.success = true; 


                    }else
                    {

                        
                        response.awb_no = responsedehilvery.packages[0].waybill;
                        response.carrier_code = "delhivery";
                        response.carrier_name = "Delhivery";
                       
                        response.msg = responsedehilvery.packages[0].remarks[0];
                        response.sap_return_id = sapres.ordernumber;
                        // response.sap_uniq_no = "";
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

                    //  var q = XDocument.Parse(res, LoadOptions.PreserveWhitespace);
                    //var or = resq.Replace("\"", "'");
                    string p = result.Content.ReadAsStringAsync().Result;
                    var t = p.Replace("xmlns", "name");
                    xmlDoc.LoadXml(t);


                    string MSG_TYP = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MSG_TYP").InnerText;



                    if (MSG_TYP.ToLower() =="s")
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

                    if ( string.IsNullOrEmpty(MSG_TYP) || MSG_TYP.ToLower()  == "e")
                    {
                        string MESSAGE = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MESSAGE").InnerText;
                        //string MSG_TYP = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MSG_TYP").InnerText;
                        string MAGENTO_UNIQ_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/MAGENTO_UNIQ_NO").InnerText;
                        string RETURN_ORD_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/RETURN_ORD_NO").InnerText;
                        string SAP_INVOICE_NO = xmlDoc.SelectSingleNode("ZBAPI_MGN_SALES_RETURNResponse/RETURN/ZSTR_MGN_SALES_RETURN_ET/SAP_INVOICE_NO").InnerText;

                      //  obj.ordernumber = RETURN_ORD_NO;
                       // obj.SAP_INVOICE_NO = SAP_INVOICE_NO;
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
                


                string str = "format=json&data=" + System.Net.WebUtility.UrlEncode( jsonserialize);
                StringContent stringContent = new StringContent(str);

                client.DefaultRequestHeaders.Authorization =  new System.Net.Http.Headers.AuthenticationHeaderValue("Token" ,token);

               

                var result = client.PostAsync(dehlivery_Reversemenifesturl, stringContent).Result;

                return result.Content.ReadAsStringAsync().Result;
            
            
            
            
            }
        }






        public static void GetAWBnumber(out string waybillid, out string awbnumber)
        {
            waybillid = string.Empty;
            awbnumber = string.Empty;
            var tbawbnumber = GetPreAllocatedAWB();

            if (tbawbnumber.Rows.Count < 6)
            {

                Dehlivery_GET_AWB();

                var tbawbnumbernew = GetPreAllocatedAWB();

                if (tbawbnumbernew.Rows.Count < 6)
                {
                    string result = "error in fetching awb number";
                }
                else
                {
                    awbnumber = tbawbnumbernew.Rows[0]["awbnumber"].ToString();
                    waybillid = tbawbnumbernew.Rows[0]["waybillid"].ToString();

                }

            }
            else if (tbawbnumber.Rows.Count >= 6)
            {
                awbnumber = tbawbnumber.Rows[0]["awbnumber"].ToString();
                waybillid = tbawbnumber.Rows[0]["waybillid"].ToString();

            }

        }

        public static void updateawbstatus(string waybillid)
        {
            //string connetionString = Environment.GetEnvironmentVariable("mysqlconnectionstring");

            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(mysqlconnectionstring);
                connection.Close();
                connection.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;

                cmd.CommandText = "update dehliverywaybillnumber set isused=@isused, updatedate =@updatedate where waybillid = @waybillid";
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@waybillid", waybillid);
                cmd.Parameters.AddWithValue("@isused", "1");
                cmd.Parameters.AddWithValue("@updatedate", DateTime.Now);


                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }

        }



        public static DataTable GetPreAllocatedAWB()
        {

            // string mysqlconnectionstring = Environment.GetEnvironmentVariable("mysqlconnectionstring");
            MySqlCommand cmd = new MySqlCommand();
            MySqlConnection connection = new MySqlConnection(mysqlconnectionstring);
            connection.Close();
            connection.Open();
            var command = connection.CreateCommand();

            command.CommandText = "SELECT *,DATEDIFF(CURDATE(),insertdate) FROM dehliverywaybillnumber WHERE isused = 0 AND DATEDIFF(CURDATE(),insertdate) < 90";
            var reader = command.ExecuteNonQuery();

            DataTable _dataTable = new DataTable();

            var _da = new MySqlDataAdapter(command);
            _da.Fill(_dataTable);

            return _dataTable;

        }

        public static async Task<string> Dehlivery_GET_AWB()
        {
            MySqlConnection connection = null;
            string awbnumber = string.Empty;
            DateTime insertdate = DateTime.Now;
            DateTime updatedate = DateTime.Now;
            string dehliveryapiusername = Environment.GetEnvironmentVariable("dehliveryapiusername");
            string token = Environment.GetEnvironmentVariable("token");
            string dehliveryapipassword = Environment.GetEnvironmentVariable("dehliveryapipassword");
            string count = Environment.GetEnvironmentVariable("count");
            string fetchawburl = Environment.GetEnvironmentVariable("fetchawburl");
            try
            {
               using (var client = new HttpClient())
               {

                   var formContent = new MultipartFormDataContent
    {

        {new StringContent(System.Net.WebUtility.UrlEncode(dehliveryapiusername)),"username"},
        {new StringContent(System.Net.WebUtility.UrlEncode(dehliveryapipassword)),"password" },
         {new StringContent(count),"count" },
          {new StringContent("rev"),"type" },

    };
                    token = System.Net.WebUtility.UrlEncode(token);



                   var result = client.GetAsync(fetchawburl+ "waybill/api/bulk/json/?cl=client_name&token="+ token + "&count="+count).Result;
    //                MySqlCommand cmd = new MySqlCommand();
                    var resultContent =  result.Content.ReadAsStringAsync().Result;
                    return awbnumber =  JsonConvert.DeserializeObject(resultContent).ToString();


               //  return   awbnumber = resultContent.ToString();
                    //                List<string> Rows = new List<string>();
                    //                string currentdate = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
                    //                StringBuilder sCommand = new StringBuilder("INSERT INTO dehliverywaybillnumber(awbnumber, isused, insertdate, updatedate, isdeleted,reference_id) VALUES ");
                    //                for (int i = 0; i < resultContent.awb.Count; i++)
                    //                {
                    //                    Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}')", MySqlHelper.EscapeString(resultContent.awb[i].ToString()), MySqlHelper.EscapeString("0"), MySqlHelper.EscapeString(currentdate), MySqlHelper.EscapeString(currentdate), MySqlHelper.EscapeString("0"), MySqlHelper.EscapeString(resultContent.reference_id.ToString())));
                    //                }
                    //                sCommand.Append(string.Join(",", Rows));
                    //                sCommand.Append(";");
                    //                connection = new MySqlConnection(mysqlconnectionstring);
                    //                connection.Open();
                    //                cmd.Connection = connection;
                    //                cmd.CommandText = sCommand.ToString();
                    //                cmd.ExecuteNonQuery();
                              }

                    return awbnumber;

                }
            catch (Exception ex)
            {
                return awbnumber;
            }
            finally
            {

               // return awbnumber ;
                //if (connection != null)
                //    connection.Close();
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

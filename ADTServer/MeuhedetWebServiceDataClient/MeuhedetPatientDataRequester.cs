using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using AdtSvrCmn;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using MeuhedetWebServiceDataClient.MeuhedetPatientInfoWebService;
using AdtSvrCmn.EventArguments;
using System.Xml.Serialization;
using System.IO;

namespace MeuhedetWebServiceDataClient
{
  public  class MeuhedetPatientDataRequester : IPatientInfoSoapSource
    {
        Logger logger;
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;
        private ApplicationConfiguration _config;

        public MeuhedetPatientDataRequester(ApplicationConfiguration config)
        {
            _config = config;
            logger = LogManager.GetCurrentClassLogger();
        }

        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            var pInfo = new CompletePatientInformation();
            getCustomerInfo_Response res = new getCustomerInfo_Response();
            try
            {
                using (var client = new CustomersClient("PortType.CustomersEndpoint1"))
                {
                    logger.Trace($"inside MeuhedetWebServiceData.GetPatientInfo() ");

                    var esb = new esbData()
                    {
                        request_id = DateTime.Now.ToString(),
                        sourceSystem = "45"
                    };
                   
                    logger.Info($"Asking for patient info for patient : {CustumerId} ");
                    try
                    {
                        logger.Debug("Triyng to get data from web service");
                        var req = new getCustomerInfo_Request()
                        {
                            ActionCode = 1,
                            CustId = CustumerId,
                            CustIdType = 1,
                            esbData = esb

                        };

                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(getCustomerInfo_Request));
                        using (StringWriter textWriter = new StringWriter())
                        {
                            xmlSerializer.Serialize(textWriter, req);
                            logger.Debug($"Message to send : {textWriter.ToString()}");
                        }


                        res = client.getCustomerInfo(req);
                        
                        
                        logger.Debug($"Got reply status : {res.statusReply.description} , {res.statusReply.errorCode} , {res.statusReply.errorSource} ,{res.statusReply.status}");
                        logger.Info(res.CustomerInfo.CustFirstName);
                        logger.Info(res.CustomerInfo.CustSecName);
                    }
                    catch (Exception ex)
                    {
                        NlogHelper.CreateLogEntry(ex.Message,"200",LogLevel.Error,logger);
                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                            NlogHelper.CreateLogEntry(ex.Message, "200", LogLevel.Error, logger);
                            
                        }
                    }


                    if (res.statusReply.errorCode != _config.BadSoapResponseCode)
                    {
                        // good response
                        RecevingPatientInfo?.Invoke(null, new PatientInfoEventArgs() { Source = "Meuhedet Data Requester", Message = "recieved data..." });
                        logger.Debug("Got Succssfull reponse from web service");
                        pInfo.ResponseStatus = res.statusReply.errorCode;
                        pInfo.ResponseStatusMessage = res.CustomerInfo.CustStatusDesc;
                        pInfo.CompleteResponseStatusMessage = $"Error Code : {res.statusReply.errorCode}{Environment.NewLine}" +
                                                              $"Error Message : {res.statusReply.description}{Environment.NewLine}" +
                                                              $"Error Source : {res.statusReply.errorSource}" +
                                                              $"Error Status : {res.statusReply.status}";
                    }
                    else if (res.statusReply.errorCode == _config.BadSoapResponseCode)
                    {
                        ErrorGettingPatientInfo?.Invoke(this, new PatientInfoEventArgs() { Source = "Meuhedet Data Requester", Message = $"Recived erorr code {_config.BadSoapResponseCode}" });
                        pInfo.ResponseStatus = res.statusReply.errorCode;
                        pInfo.ResponseStatusMessage = res.CustomerInfo.CustStatusDesc;
                        pInfo.CompleteResponseStatusMessage = $"Error Code : {res.statusReply.errorCode}{Environment.NewLine}" +
                                                              $"Error Message : {res.statusReply.description}{Environment.NewLine}" +
                                                              $"Error Source : {res.statusReply.errorSource}" +
                                                              $"Error Status : {res.statusReply.status}";
                        // no data
                        return pInfo;
                    }



                    pInfo.Age = (DateTime.Now.Year - res.CustomerInfo.CustBirthDate.Year).ToString();
                    pInfo.DOB = res.CustomerInfo.CustBirthDate.ToString();
                    pInfo.FirstName = res.CustomerInfo.CustFirstName;
                    pInfo.LastName = res.CustomerInfo.CustSecName;
                    pInfo.Gender = ((Gender)(int.Parse(res.CustomerInfo.CustGender))).ToString();
                    pInfo.GenderDesc = res.CustomerInfo.CustGenderDesc;
                    pInfo.Height = "";
                    logger.Info($"Got Good data ,{pInfo.LastName}, {pInfo.FirstName}");
                    // good data
                    return pInfo;

                }
            }
            catch (Exception ex)
            {
                logger.Debug("inside catch block ");
                logger.Debug(ex, ex.Message, null);
                while (ex.InnerException != null)
                {
                    logger.Debug(ex.InnerException.Message, ex.InnerException, null);
                    ex = ex.InnerException;
                }

                logger.Debug($"Error Getting Patient Data From Web Service for patient id : {CustumerId}");
                return new CompletePatientInformation() { ResponseStatus = _config.BadSoapResponseCode,
                                                          ResponseStatusMessage = "Fatal error :"+ex.Message};
            }

        }

        public CompletePatientInformation GetPatientInfo(string CustumerId, string pidType)
        {
            return GetPatientInfo(CustumerId);
        }

        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            return GetPatientInfo(patientId.ID);
        }
    }
}

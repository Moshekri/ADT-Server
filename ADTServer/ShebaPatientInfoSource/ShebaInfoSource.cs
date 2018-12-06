using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using ShebaPatientInfoSource.ShebaDemographicsProxy;
using NLog;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Configuration;

namespace ShebaPatientInfoSource
{
    public class ShebaInfoSource : IPatientInfoSoapSource
    {
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;
        Logger logger;
        ApplicationConfiguration _config;


        public ShebaInfoSource()
        {

        }
        public ShebaInfoSource(ApplicationConfiguration config)
        {
            _config = config;
        }
        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Debug("inside get patient info");
            logger.Debug($"Getting information for {CustumerId}");
            CompletePatientInformation completePatientInformation = new CompletePatientInformation();
            using (var client = new ShebaDemographicsProxy.ShebaCoreInterfacesWSDemogBothServiceSoapClient("Sheba.CoreInterfaces.WS.DemogBoth.ServiceSoap"))
            {
                client.ClientCredentials.Windows.ClientCredential.UserName = "EldanUser";
                client.ClientCredentials.Windows.ClientCredential.Password = "ELDAN951";
                var name  = client.ClientCredentials.UserName;
                name.UserName = "EldanUser";
                name.Password = "ELDAN951";
                logger.Debug($"Client Details : Address : {client.Endpoint.Address}" +
                             $"Binding Name : {client.Endpoint.Binding.Name}" +
                             $"Listen Uri : {client.Endpoint.ListenUri}" +
                             $"Connection State : {client.State.ToString()}" +
                             $"User Name : {client.ClientCredentials.UserName.UserName}" +
                             $"Password {client.ClientCredentials.UserName.Password}");
                    //$"{client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation}");

                try
                {
                    long id = long.Parse(CustumerId);

                    logger.Debug("building request");
                    GetPatientDetailsRequest req = new GetPatientDetailsRequest();
                    req.Body = new GetPatientDetailsRequestBody();
                    req.Body.AppUserName = "Museadmin";
                    req.Body.DataSource = DataSource.MF;
                    req.Body.PatientID = id;
                    req.Body.QueryType = QueryType.ImutID;
                    req.Body.SendingApp = "ECG";

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetPatientDetailsRequest));
                    using (StringWriter textWriter = new StringWriter())
                    {
                        xmlSerializer.Serialize(textWriter, req);
                        logger.Debug($"XML Message to send : {textWriter.ToString()}");
                    }

                    logger.Debug("Sending req via factory");
                    var res = client.ChannelFactory.CreateChannel().GetPatientDetails(req);

                    client.Open();
                    Output respone = client.GetPatientDetails("ECG", DataSource.MF, QueryType.ImutID, id, "", "", "", "MuseAdmin");
                    logger.Debug("Got Response");
                    logger.Debug($"{ res.Body.GetPatientDetailsResult.PatDetails.engpname},{ res.Body.GetPatientDetailsResult.ErrMessage}");
                    logger.Debug($"{respone.PatDetails.engfname} , {respone.PatDetails.engpname}");
                    var datestring = respone.PatDetails.birthdate.ToString();
                    datestring = datestring.Insert(4, "-");
                    datestring = datestring.Insert(7, "-");


                    var age = DateTime.Now.Year - DateTime.Parse(datestring).Year;
                    completePatientInformation.Age = respone.PatDetails.birthdate.ToString();
                    completePatientInformation.LastName = respone.PatDetails.engfname;
                    completePatientInformation.FirstName = respone.PatDetails.engpname;
                    completePatientInformation.Gender = respone.PatDetails.sex;
                    if (completePatientInformation.Gender == "ז")
                    {
                        completePatientInformation.Gender = "M";
                    }
                    else
                    {
                        completePatientInformation.Gender = "F";
                    }
                    completePatientInformation.ResponseStatus = respone.Status;
                    completePatientInformation.ResponseStatusMessage = respone.ErrMessage;
                    completePatientInformation.DOB = datestring;
                    logger.Debug($"{completePatientInformation.Age}," +
                        $"{completePatientInformation.CompleteResponseStatusMessage}," +
                        $"{completePatientInformation.DOB}," +
                        $"{completePatientInformation.FirstName}," +
                        $"{completePatientInformation.Gender}," +
                        $"{completePatientInformation.GenderDesc}," +
                        $"{completePatientInformation.Height}," +
                        $"{completePatientInformation.LastName}," +
                        $"{completePatientInformation.PatientId}," +
                        $"Response ststus code : {completePatientInformation.ResponseStatus}," +
                        $"{completePatientInformation.ResponseStatusMessage}");

                    return completePatientInformation;

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        logger.Error(ex.Message);
                    }
                    client.Close();
                    return null;
                }

            }

        }

        [Obsolete("Use GetPatientInfo(PatientId patientId) instead")]
        public CompletePatientInformation GetPatientInfo(string CustumerId, string pidType)
        {
            throw new NotImplementedException();
        }

        



        public CompletePatientInformation GetPatientInfo(PatientId patientId)
        {
            return GetPatientInfo(patientId.ID);
        }
       
    }
}


//http://ensembleprodsrv/csp/sheba/Sheba.CoreInterfaces.BS.Demog.InputSecure.cls


//שם משתמש EldanUser
//b.סיסמא ELDAN951
//c.      SendingApp = ECG
//c.DataSource = MF
//d.QueryType ו- BirthDate (לא רלוונטי כשפונים ל-MF)
//e.PatientID - ספרות בלבד, כולל ס.ב.באורך מקסימלי של 9.  שדה חובה בכל פניה למרשם או למחשב מרכזי.
//f.FirstName – שדה חובה בכל פניה מסוג ImutNames. (לא רלוונטי כאשר פונים רק למחשב המרכזי)
//g.LastName– שדה חובה בכל פניה מסוג ImutNames. (לא רלוונטי כאשר פונים רק למחשב המרכזי)
//h.BirthDate – תאריך לידה בפורמט DD/MM/YYYY.שדה חובה בכל פניה מסוג ImutDOB.  (לא רלוונטי כאשר DataSource = MF)
//i.AppUserName – שם המשתמש שהפעיל את השאילתא.חד ערכי במערכת ששולחת את הבקשה ושממנו ניתן להגיע בצורה חד ערכית למשתמש שהפעיל את השאילתא.



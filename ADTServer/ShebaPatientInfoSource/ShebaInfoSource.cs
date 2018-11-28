using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using ShebaPatientInfoSource.ShebaDemographicsProxy;


namespace ShebaPatientInfoSource
{
    public class ShebaInfoSource : IPatientInfoSoapSource
    {
        public event EventHandler ErrorGettingPatientInfo;
        public event EventHandler RecevingPatientInfo;
        public event EventHandler RequestingPatientInfo;

        public CompletePatientInformation GetPatientInfo(string CustumerId)
        {
            CompletePatientInformation completePatientInformation = new CompletePatientInformation();
            using (var client = new ShebaDemographicsProxy.ShebaCoreInterfacesWSDemogBothServiceSoapClient("Sheba.CoreInterfaces.WS.DemogBoth.ServiceSoap"))
            {
                try
                {
                    long id = long.Parse(CustumerId);
                    var respone = client.GetPatientDetails("ECG", DataSource.MF, QueryType.ImutID, id, "", "", "", "EldaUser");
                    var datestring = respone.PatDetails.birthdate.ToString();
                    datestring = datestring.Insert(4, "-");
                    datestring = datestring.Insert(7, "-");
                  

                    var age =DateTime.Now.Year - DateTime.Parse(datestring).Year;
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
                    completePatientInformation.ResponseStatus = "0";
                    completePatientInformation.DOB = datestring;
                    return completePatientInformation;

                }
                catch (Exception ex)
                {
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



using NHapi.Base.Parser;
using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapi.Model.V25.Datatype;

namespace GateEntryExit.HL7
{
    // Example Output(HL7 Message)
    // MSH|^~\&|RefApp|ClinicA|SpecialistSystem|HospitalX|20260323143000||ORU^R01|MSG20001|P|2.3
    // PID|||123456||Doe^John||19800101|M
    // PV1||O|||^^^ClinicA
    // ORC|NW|REF001|||^Smith^Alice|^Johnson^Bob
    // OBR|1|REF001||REF001^Specialist Referral||20260323143000
    // OBX|1|NM|WBC^White Blood Cells||5.4|10^9/L|4.0-11.0|N|||F
    // OBX|2|NM|RBC^Red Blood Cells||4.7|10^12/L|4.0-6.0|N|||F
    public class ORU
    {
        public string Build()
        {
            // Create ORU^R01 message
            ORU_R01 message = new ORU_R01();

            // MSH
            MSH msh = message.MSH;
            msh.FieldSeparator.Value = "|";
            msh.EncodingCharacters.Value = "^~\\&";
            msh.SendingApplication.NamespaceID.Value = "LabSystem";
            msh.SendingFacility.NamespaceID.Value = "ClinicA";
            msh.ReceivingApplication.NamespaceID.Value = "HospitalSystem";
            msh.ReceivingFacility.NamespaceID.Value = "HospitalX";
            msh.DateTimeOfMessage.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");
            msh.MessageType.MessageCode.Value = "ORU";
            msh.MessageType.TriggerEvent.Value = "R01";
            msh.MessageControlID.Value = "MSG10001";
            msh.ProcessingID.ProcessingID.Value = "P";
            msh.VersionID.VersionID.Value = "2.5";

            // PID
            PID pid = message.GetPATIENT_RESULT().PATIENT.PID;
            pid.SetIDPID.Value = "1";
            pid.PatientID.IDNumber.Value = "123456";
            pid.GetPatientName(0).FamilyName.Surname.Value = "Doe";
            pid.GetPatientName(0).GivenName.Value = "John";
            pid.DateTimeOfBirth.Time.Value = "19800101";
            pid.AdministrativeSex.Value = "M";

            // PV1
            PV1 pv1 = message.GetPATIENT_RESULT().PATIENT.VISIT.PV1;
            pv1.PatientClass.Value = "O"; // Outpatient
            pv1.AssignedPatientLocation.Facility.NamespaceID.Value = "ClinicA";

            // ORC
            ORC orc = message.GetPATIENT_RESULT().GetORDER_OBSERVATION().ORC;
            orc.OrderControl.Value = "RE"; // Result
            orc.PlacerOrderNumber.EntityIdentifier.Value = "ORD12345";

            // OBR
            OBR obr = message.GetPATIENT_RESULT().GetORDER_OBSERVATION().OBR;
            obr.SetIDOBR.Value = "1";
            obr.PlacerOrderNumber.EntityIdentifier.Value = "ORD12345";
            obr.UniversalServiceIdentifier.Identifier.Value = "BLD001";
            obr.UniversalServiceIdentifier.Text.Value = "Complete Blood Count";
            obr.ObservationDateTime.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");

            // First recipient
            var copyTo1 = obr.GetResultCopiesTo(0);
            copyTo1.IDNumber.Value = "SPEC001";
            copyTo1.FamilyName.Surname.Value = "Taylor";
            copyTo1.GivenName.Value = "Emma";

            // Second recipient
            var copyTo2 = obr.GetResultCopiesTo(1);
            copyTo2.IDNumber.Value = "SPEC002";
            copyTo2.FamilyName.Surname.Value = "Lee";
            copyTo2.GivenName.Value = "John";

            // Third recipient
            var copyTo3 = obr.GetResultCopiesTo(2);
            copyTo3.IDNumber.Value = "SPEC003";
            copyTo3.FamilyName.Surname.Value = "Patel";
            copyTo3.GivenName.Value = "Maya";

            // OBX - WBC
            OBX obx1 = message.GetPATIENT_RESULT().GetORDER_OBSERVATION().GetOBSERVATION().OBX;
            obx1.SetIDOBX.Value = "1";
            obx1.ValueType.Value = "NM";
            obx1.ObservationIdentifier.Identifier.Value = "WBC";
            obx1.ObservationIdentifier.Text.Value = "White Blood Cells";
            NM wbcValue = new NM(message);
            wbcValue.Value = "5.4";
            obx1.GetObservationValue(0).Data = wbcValue;
            obx1.Units.Identifier.Value = "10^9/L";
            obx1.ReferencesRange.Value = "4.0-11.0";
            obx1.ObservationResultStatus.Value = "F";

            // OBX - RBC
            OBX obx2 = message.GetPATIENT_RESULT().GetORDER_OBSERVATION().GetOBSERVATION(1).OBX;
            obx2.SetIDOBX.Value = "2";
            obx2.ValueType.Value = "NM";
            obx2.ObservationIdentifier.Identifier.Value = "RBC";
            obx2.ObservationIdentifier.Text.Value = "Red Blood Cells";
            NM val2 = new NM(message);
            val2.Value = "4.7";
            obx2.GetObservationValue(0).Data = val2;
            obx2.Units.Identifier.Value = "10^12/L";
            obx2.ReferencesRange.Value = "4.0-6.0";
            obx2.GetAbnormalFlags(0).Value = "N";
            obx2.ObservationResultStatus.Value = "F";

            // Encode HL7 message
            PipeParser parser = new PipeParser();
            string hl7 = parser.Encode(message);
            return hl7;
        }
    }
}

using NHapi.Base.Parser;
using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;

namespace GateEntryExit.HL7
{
    // Example Output(HL7 Message)
    // MSH|^~\&|RefApp|ClinicA|SpecialistSystem|HospitalX|20260323150000||ORM^O01|MSG30001|P|2.3
    // PID|||123456||Doe^John||19800101|M
    // PV1||O|||^^^ClinicA
    // ORC|NW|REF001|||^Smith^Alice|^Johnson^Bob
    // OBR|1|REF001||REF001^Specialist Referral||20260323150000|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|^^^|SPEC001^Taylor^Emma

    public class ORM
    {
        public string Build()
        {
            ORM_O01 orm = new ORM_O01();

            // MSH
            orm.MSH.FieldSeparator.Value = "|";
            orm.MSH.EncodingCharacters.Value = "^~\\&";
            orm.MSH.SendingApplication.NamespaceID.Value = "ClinicA";
            orm.MSH.SendingFacility.NamespaceID.Value = "RefApp";
            orm.MSH.ReceivingApplication.NamespaceID.Value = "HospitalX";
            orm.MSH.ReceivingFacility.NamespaceID.Value = "SpecSys";
            orm.MSH.DateTimeOfMessage.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");
            orm.MSH.MessageType.MessageCode.Value = "ORM";
            orm.MSH.MessageType.TriggerEvent.Value = "O01";
            orm.MSH.MessageControlID.Value = "MSG20001";
            orm.MSH.ProcessingID.ProcessingID.Value = "P";
            orm.MSH.VersionID.VersionID.Value = "2.5";

            // PID
            PID pid = orm.PATIENT.PID;
            pid.SetIDPID.Value = "1";
            pid.PatientID.IDNumber.Value = "123456";
            pid.GetPatientName(0).FamilyName.Surname.Value = "Doe";
            pid.GetPatientName(0).GivenName.Value = "John";
            pid.DateTimeOfBirth.Time.Value = "19800101";
            pid.AdministrativeSex.Value = "M";

            // PV1
            PV1 pv1 = orm.PATIENT.PATIENT_VISIT.PV1;
            pv1.PatientClass.Value = "O";
            pv1.AssignedPatientLocation.Facility.NamespaceID.Value = "ClinicA";

            // ORC (Referral header)
            ORC orc = orm.GetORDER().ORC;
            orc.OrderControl.Value = "NW"; // New referral
            orc.PlacerOrderNumber.EntityIdentifier.Value = "REF001";

            // Referring doctor (Ordering Provider)
            var referring = orc.GetOrderingProvider(0);
            referring.IDNumber.Value = "DOC001";
            referring.FamilyName.Surname.Value = "Smith";
            referring.GivenName.Value = "Alice";

            var provider2 = orc.GetOrderingProvider(1);
            provider2.IDNumber.Value = "DOC002";
            provider2.FamilyName.Surname.Value = "Johnson";
            provider2.GivenName.Value = "Bob";

            // OBR (Referral details)
            OBR obr = orm.GetORDER().ORDER_DETAIL.OBR;
            obr.SetIDOBR.Value = "1";
            obr.PlacerOrderNumber.EntityIdentifier.Value = "REF001";

            // Referral type
            obr.UniversalServiceIdentifier.Identifier.Value = "REF";
            obr.UniversalServiceIdentifier.Text.Value = "Specialist Referral";

            // Requested date
            obr.ObservationDateTime.Time.Value = "20260324100000";

            // Receiving specialists (multiple)
            var spec1 = obr.GetResultCopiesTo(0);
            spec1.IDNumber.Value = "SPEC001";
            spec1.FamilyName.Surname.Value = "Taylor";
            spec1.GivenName.Value = "Emma";

            var spec2 = obr.GetResultCopiesTo(1);
            spec2.IDNumber.Value = "SPEC002";
            spec2.FamilyName.Surname.Value = "Lee";
            spec2.GivenName.Value = "John";

            var spec3 = obr.GetResultCopiesTo(2);
            spec3.IDNumber.Value = "SPEC003";
            spec3.FamilyName.Surname.Value = "Patel";
            spec3.GivenName.Value = "Maya";

            // Generate HL7
            PipeParser parser = new PipeParser();
            string ormHl7 = parser.Encode(orm);
            return ormHl7;
        }
    }
}
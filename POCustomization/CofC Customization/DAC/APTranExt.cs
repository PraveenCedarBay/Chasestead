using PX.Data;
using PX.Objects.AP;

namespace POCustomization
{
    public sealed class APTranExt : PXCacheExtension<APTran>
    { 
        public static bool IsActive() => true;

        #region UsrPOQualityRequirement
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PO Quality Required",IsReadOnly =true)]
        public bool? UsrPOQualityRequirement { get; set; }
        public abstract class usrPOQualityRequirement : PX.Data.BQL.BqlBool.Field<usrPOQualityRequirement> { }
        #endregion

        #region UsrReceiptQualityDocsReceived
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Receipt Quality Docs Received",IsReadOnly =true)]
        public bool? UsrReceiptQualityDocsReceived { get; set; }
        public abstract class usrReceiptQualityDocsReceived : PX.Data.BQL.BqlBool.Field<usrReceiptQualityDocsReceived> { }
        #endregion

        #region UsrLateDocumentSubmission
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Late Document Submission")]
        public bool? UsrLateDocumentSubmission { get; set; }
        public abstract class usrLateDocumentSubmission : PX.Data.BQL.BqlBool.Field<usrLateDocumentSubmission> { }
        #endregion

        #region UsrNonComplianceHold
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Non-compliance Hold", IsReadOnly = true)]
        public bool? UsrNonComplianceHold { get; set; }
        public abstract class usrNonComplianceHold : PX.Data.BQL.BqlBool.Field<usrNonComplianceHold> { }
        #endregion

        #region UsrDocumentsRequired
        [PXDBString(1024, IsUnicode = true)]
        [PXUIField(DisplayName = "Documents Required")]
        public string UsrDocumentsRequired { get; set; }
        public abstract class usrDocumentsRequired : PX.Data.BQL.BqlString.Field<usrDocumentsRequired> { }
        #endregion
    }
}

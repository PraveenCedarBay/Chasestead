using PX.Data;
using PX.Objects.PO; 

namespace POCustomization
{
    public class POReceiptLineExt : PXCacheExtension<POReceiptLine>
    { 
        public static bool IsActive() => true;

        #region UsrQualityDocumentsReceived
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Quality Documents Received")]
        public bool? UsrQualityDocumentsReceived { get; set; }
        public abstract class usrQualityDocumentsReceived : PX.Data.BQL.BqlBool.Field<usrQualityDocumentsReceived> { }
        #endregion 

        #region UsrRequireQualityDocuments
        [PXDBBool()]
        [PXDefault(typeof(Search<POLineExt.usrRequireQualityDocuments, Where<POLine.orderType, Equal<Current<POReceiptLine.pOType>>,
            And<POLine.orderNbr, Equal<Current<POReceiptLine.pONbr>>,
                And<POLine.lineNbr, Equal<Current<POReceiptLine.pOLineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Require Quality Documents")]
        public bool? UsrRequireQualityDocuments { get; set; }
        public abstract class usrRequireQualityDocuments : PX.Data.BQL.BqlBool.Field<usrRequireQualityDocuments> { }
        #endregion

        #region UsrDocumentsRequired
        [PXDBString(1024,IsUnicode =true)]
        [PXDefault(typeof(Search<POLineExt.usrDocumentsRequired, Where<POLine.orderType, Equal<Current<POReceiptLine.pOType>>,
            And<POLine.orderNbr, Equal<Current<POReceiptLine.pONbr>>,
                And<POLine.lineNbr, Equal<Current<POReceiptLine.pOLineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Documents Required")]
        public string UsrDocumentsRequired { get; set; }
        public abstract class usrDocumentsRequired : PX.Data.BQL.BqlString.Field<usrDocumentsRequired> { }
        #endregion


    }
}
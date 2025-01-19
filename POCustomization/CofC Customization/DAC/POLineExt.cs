using PX.Data;
using PX.Objects.PO;

namespace POCustomization
{
    public sealed class POLineExt : PXCacheExtension<POLine>
    {
        public static bool IsActive() => true;

        #region UsrRequireQualityDocuments
        [PXDBBool()]
        [PXDefault(false,PersistingCheck =PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Require Quality Documents")]
        public bool? UsrRequireQualityDocuments { get; set; }
        public abstract class usrRequireQualityDocuments : PX.Data.BQL.BqlBool.Field<usrRequireQualityDocuments> { }
        #endregion

        #region UsrDocumentsRequired
        [PXDBString(1024, IsUnicode = true)]
         [PXUIField(DisplayName = "Documents Required")]
        public string UsrDocumentsRequired { get; set; }
        public abstract class usrDocumentsRequired : PX.Data.BQL.BqlString.Field<usrDocumentsRequired> { }
        #endregion
    }
}

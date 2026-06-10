using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Coupon : IDisposable
    {
        public Coupon()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public string StoreNo { get; set; }
        [DataMember]
        public string Barcode { get; set; }
        [DataMember]
        public DateTime FirstValidDate { get; set; }
        [DataMember]
        public DateTime LastValidDate { get; set; }
    }
}

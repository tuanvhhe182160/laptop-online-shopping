namespace WebAPI.Services
{
    //quy doi usd sang vnd va cac cau hinh khac: so luong item tren 1 trang, thoi gian song cua token, ...
    public class SystemConfigService 
    {
        public decimal UsdToVndRate { get; private set; }
        public int DefaultPageSize { get; private set; }
        public int ResetTokenExpiryMinutes { get; private set; }

        public SystemConfigService()
        {
            UsdToVndRate = 25000m;
            DefaultPageSize = 10;
            ResetTokenExpiryMinutes = 15;
        }

        public void UpdateRate(decimal newRate)
        {
            if (newRate > 0)
                UsdToVndRate = newRate;
        }
    }
}

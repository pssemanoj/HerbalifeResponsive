namespace MyHerbalife3.Ordering.Providers.RulesManagement
{
	public class HLRulesManager
	{
		/// <summary>Simple access property to shield singleton access</summary>
		public static ServerRulesManager Manager
		{
			get { return ServerRulesManager.Instance; }
		}
	}
}

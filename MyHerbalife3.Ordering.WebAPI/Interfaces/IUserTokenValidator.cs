namespace MyHerbalife3.Ordering.WebAPI.Interfaces
{
    public interface IUserTokenValidator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="validateUserId"></param>
        /// <param name="userId"></param>
        /// <param name="locale"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        bool ValidateToken(string token, bool validateUserId, string userId, string locale, string client);
    }
}

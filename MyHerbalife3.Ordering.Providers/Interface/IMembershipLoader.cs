using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IMembershipLoader
    {
        string ApplicationName { get; set; }

        bool EnablePasswordReset { get; }

        bool EnablePasswordRetrieval { get; }

        string HashAlgorithmType { get; }

        int MaxInvalidPasswordAttempts { get; }

        int MinRequiredNonAlphanumericCharacters { get; }

        int MinRequiredPasswordLength { get; }

        int PasswordAttemptWindow { get; }

        string PasswordStrengthRegularExpression { get; }

        MembershipProvider Provider { get; }

        MembershipProviderCollection Providers { get; }

        bool RequiresQuestionAndAnswer { get; }

        int UserIsOnlineTimeWindow { get; }

        MembershipUser CreateUser(string username, string password);

        MembershipUser CreateUser(string username, string password, string email);

        MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, out MembershipCreateStatus status);

        MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status);

        bool DeleteUser(string username);

        bool DeleteUser(string username, bool deleteAllRelatedData);

        MembershipUserCollection FindUsersByEmail(string emailToMatch);

        MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords);

        MembershipUserCollection FindUsersByName(string usernameToMatch);

        MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords);

        string GeneratePassword(int length, int numberOfNonAlphanumericCharacters);

        MembershipUserCollection GetAllUsers();

        MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords);

        int GetNumberOfUsersOnline();

        MembershipUser GetUser();

        MembershipUser GetUser(string username);

        MembershipUser GetUser(object providerUserKey);

        MembershipUser GetUser(bool userIsOnline);

        MembershipUser GetUser(object providerUserKey, bool userIsOnline);

        MembershipUser GetUser(string username, bool userIsOnline);

        string GetUserNameByEmail(string emailToMatch);

        void UpdateUser(MembershipUser user);

        bool ValidateUser(string username, string password);
    }
}
